"""Convert SSMS-generated T-SQL INSERT script (UTF-8) into a PostgreSQL seed script."""
import re, sys, struct, datetime
src, dst = sys.argv[1], sys.argv[2]
sql = open(src, encoding="utf-8").read()

# --- column types per table (to convert bit -> boolean) ---
types = {}
for m in re.finditer(r'CREATE TABLE \[dbo\]\.\[(\w+)\]\((.*?)\n(?: CONSTRAINT|\) ON)', sql, re.S):
    cols = re.findall(r'^\s*\[(\w+)\] \[(\w+)\]', m.group(2), re.M)
    types[m.group(1)] = dict(cols)

ORDER = ["Branch","EmployeeType","Employee","Doctor","Assistant","Receptionist","OtherEmployee","Account",
 "Configuration","YesNoQuestion","ServiceGroup","Service","MemberShipGroup","Membership","ServiceDiscount",
 "Drug","DrugInBranch","Equipment","EquimentInBranch","LaboType","LaboVendor","Customer","CustomerMoreInformation",
 "Prescription","Appointment","ContainedConsulting","ContainedConsultingLabo","ContainedDiscount","ContainedTreatment",
 "ContainedLabo","ContainedPicture","ContainedReceipt","UsedReceipt","ContainedUsedReceipt","ContainedUsedEquipment",
 "Payment","Invoice","ContainedInvoice","EquipmentInvoice","ContainedEquipmentInvoice","EquipmentExportInvoice",
 "ContainedEquipmentExportInvoice","Expense","ContainedExpense","MonthYear","Salary","MonthYearDoctor","DoctorSalary"]

EPOCH = datetime.datetime(1900,1,1)
def dt_from_hex(h):
    b = bytes.fromhex(h)
    days, ticks = struct.unpack(">iI", b)  # big-endian as printed by SSMS
    return (EPOCH + datetime.timedelta(days=days, seconds=ticks/300.0)).strftime("%Y-%m-%d %H:%M:%S.%f")[:-3]
def date_from_hex(h):
    b = bytes.fromhex(h)
    days = int.from_bytes(b[:3], "little") if len(b) >= 3 else int.from_bytes(b, "little")
    return (datetime.date(1,1,1) + datetime.timedelta(days=days)).isoformat()

def split_values(s):
    """Split a VALUES(...) body on top-level commas, respecting quotes."""
    out, cur, inq, i = [], [], False, 0
    while i < len(s):
        c = s[i]
        if inq:
            cur.append(c)
            if c == "'":
                if i+1 < len(s) and s[i+1] == "'":
                    cur.append("'"); i += 1
                else:
                    inq = False
        else:
            if c == "'": inq = True; cur.append(c)
            elif c == ",": out.append("".join(cur).strip()); cur = []
            else: cur.append(c)
        i += 1
    out.append("".join(cur).strip())
    return out

def conv(v, typ):
    if v == "NULL": return "NULL"
    m = re.fullmatch(r"CAST\(0x([0-9A-Fa-f]*) AS DateTime\)", v)
    if m: return "'%s'" % dt_from_hex(m.group(1))
    m = re.fullmatch(r"CAST\(0x([0-9A-Fa-f]*) AS Date\)", v)
    if m: return "'%s'" % date_from_hex(m.group(1))
    m = re.fullmatch(r"CAST\(N?'([^']*)' AS \w+\)", v)
    if m: return "'%s'" % m.group(1)
    if v.startswith("N'"): v = v[1:]
    if v.startswith("0x"):
        return "'" + chr(92) + "x" + v[2:] + "'::bytea"
    if typ == "bit":
        return "true" if v.strip() == "1" else "false"
    return v

rows = {t: [] for t in ORDER}
unknown = set()
stmts, cur = [], None
for line in sql.split(chr(10)):
    if line.startswith("INSERT [dbo]."):
        if cur: stmts.append(cur)
        cur = line
    elif cur is not None and line.strip() != "GO" and not line.startswith(("SET ", "/*", "ALTER ", "CREATE ", "USE ")):
        cur += chr(10) + line
    else:
        if cur: stmts.append(cur)
        cur = None
if cur: stmts.append(cur)
for st in stmts:
    m = re.match(r"^INSERT \[dbo\]\.\[(\w+)\] \((.*?)\) VALUES \((.*)\)\s*$", st, re.S)
    assert m, repr(st[-300:])
    t, cols, vals = m.group(1), m.group(2), m.group(3)
    if t not in rows: unknown.add(t); continue
    cols = re.findall(r"\[(\w+)\]", cols)
    vals = split_values(vals)
    assert len(cols) == len(vals), (t, cols, vals)
    ct = types.get(t, {})
    vals = [conv(v, ct.get(c)) for c, v in zip(cols, vals)]
    rows[t].append('INSERT INTO "%s" (%s) VALUES (%s);' % (t, ", ".join('"%s"' % c for c in cols), ", ".join(vals)))

with open(dst, "w", encoding="utf-8") as f:
    f.write("-- Seed data converted from NhaKhoaBienHoa01102013.sql (SQL Server) to PostgreSQL\n")
    f.write("BEGIN;\n")
    for t in ORDER:
        if rows[t]:
            f.write("\n-- %s (%d rows)\n" % (t, len(rows[t])))
            f.write("\n".join(rows[t]) + "\n")
    f.write("COMMIT;\n")
print("unknown tables:", unknown)
print({t: len(r) for t, r in rows.items() if r})
