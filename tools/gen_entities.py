"""Generate EF Core entity classes + DbContext from the SQL Server dump schema."""
import re, sys, os
src, outdir = sys.argv[1], sys.argv[2]
sql = open(src, encoding="utf-8").read()
TYPE = {"nvarchar":"string","varchar":"string","bigint":"long","int":"int","bit":"bool","datetime":"DateTime","date":"DateOnly","float":"double","image":"byte[]"}
tables = {}
for m in re.finditer(r'CREATE TABLE \[dbo\]\.\[(\w+)\]\((.*?)\n\) ON \[PRIMARY\]', sql, re.S):
    name, body = m.group(1), m.group(2)
    cols = []
    for c in re.finditer(r'^\s*\[(\w+)\] \[(\w+)\](?:\((\d+|max)\))?( IDENTITY\(1,1\))? (NOT NULL|NULL)', body, re.M):
        cols.append(dict(name=c.group(1), type=c.group(2), len=c.group(3), identity=bool(c.group(4)), nullable=c.group(5)=="NULL"))
    pk = re.search(r'PRIMARY KEY CLUSTERED\s*\((.*?)\)', body, re.S)
    pk = re.findall(r'\[(\w+)\] ASC', pk.group(1)) if pk else []
    tables[name] = dict(cols=cols, pk=pk, fks=[])

def col(name, type, len=None, nullable=False):
    return dict(name=name, type=type, len=len, identity=False, nullable=nullable)

# extra columns the app expects but the 2013 dump lacks
tables["Salary"]["cols"] += [dict(col("Absent", "int"), default=0), dict(col("Fee", "bigint"), default="0L")]
# address tables (from the separate ADDRESS database used by the app)
tables["Province"] = dict(cols=[col("Name","nvarchar","100")], pk=["Name"], fks=[])
tables["District"] = dict(cols=[col("Name","nvarchar","100"), col("ProvinceName","nvarchar","100")], pk=["ProvinceName","Name"], fks=[])
tables["Ward"] = dict(cols=[col("Name","nvarchar","100"), col("DistrictName","nvarchar","100"), col("ProvinceName","nvarchar","100")], pk=["ProvinceName","DistrictName","Name"], fks=[])

fkre = re.compile(r'ALTER TABLE \[dbo\]\.\[(\w+)\]\s+WITH CHECK ADD\s+CONSTRAINT \[(\w+)\] FOREIGN KEY\((.*?)\)\s*REFERENCES \[dbo\]\.\[(\w+)\] \((.*?)\)', re.S)
for m in fkre.finditer(sql):
    child, cname, ccols, parent, pcols = m.groups()
    ccols = re.findall(r'\[(\w+)\]', ccols); pcols = re.findall(r'\[(\w+)\]', pcols)
    tables[child]["fks"].append(dict(name=cname, cols=ccols, parent=parent, pcols=pcols))

def plural(n):
    if n.endswith("y"): return n[:-1] + "ies"
    if n.endswith(("ch", "sh", "s", "x")): return n + "es"
    return n + "s"

def navname(fk):
    """Reference navigation property name on the child entity."""
    c = fk["cols"][0]
    if len(fk["cols"]) == 1 and c.endswith("Id") and c != "Id":
        return c[:-2]
    if len(fk["cols"]) == 1 and c.endswith("Name") and c != "Name" and c[:-4] != fk["parent"]:
        return c[:-4]
    return fk["parent"]

# classify FKs: one-to-one when child's PK is exactly the FK
for t, info in tables.items():
    for fk in info["fks"]:
        fk["nav"] = navname(fk)
        fk["one"] = (fk["cols"] == info["pk"] and len(info["pk"]) == 1 and fk["parent"] != t)
        same_parent = sum(1 for f in info["fks"] if f["parent"] == fk["parent"])
        fk["coll"] = plural(t) if (same_parent == 1 or fk["nav"] == fk["parent"]) else plural(t) + "As" + fk["nav"]

parent_navs = {t: [] for t in tables}
for t, info in tables.items():
    for fk in info["fks"]:
        parent_navs[fk["parent"]].append((fk, t))

os.makedirs(os.path.join(outdir, "Entities"), exist_ok=True)
fluent = []
for t, info in tables.items():
    lines = ["namespace DucHanhTiepTan.Api.Entities;", "", f"public class {t}", "{"]
    for c in info["cols"]:
        ty = TYPE[c["type"]]
        decl = ty + ("?" if c["nullable"] else "")
        init = ""
        if not c["nullable"] and ty == "string": init = " = string.Empty;"
        if not c["nullable"] and ty == "byte[]": init = " = Array.Empty<byte>();"
        if c["len"] and c["len"] != "max":
            lines.append(f"    [MaxLength({c['len']})]")
        lines.append(f"    public {decl} {c['name']} {{ get; set; }}{init}")
    lines.append("")
    seen = set()
    for fk in info["fks"]:
        if fk["nav"] in seen: fk["nav"] = fk["parent"]
        seen.add(fk["nav"])
        lines.append(f"    public {fk['parent']}? {fk['nav']} {{ get; set; }}")
    for fk, child in parent_navs[t]:
        if fk["one"]:
            lines.append(f"    public {child}? {child} {{ get; set; }}")
        else:
            lines.append(f"    public ICollection<{child}> {fk['coll']} {{ get; set; }} = new List<{child}>();")
    lines.append("}")
    src_cs = "\n".join(lines) + "\n"
    if "[MaxLength" in src_cs:
        src_cs = "using System.ComponentModel.DataAnnotations;\n\n" + src_cs
    open(os.path.join(outdir, "Entities", t + ".cs"), "w", encoding="utf-8").write(src_cs)

    f = [f"        modelBuilder.Entity<{t}>(e =>", "        {", f"            e.ToTable(\"{t}\");"]
    key = "x." + info["pk"][0] if len(info["pk"]) == 1 else "new { " + ", ".join("x." + k for k in info["pk"]) + " }"
    f.append(f"            e.HasKey(x => {key});")
    for c in info["cols"]:
        if c["identity"]:
            f.append(f"            e.Property(x => x.{c['name']}).UseIdentityByDefaultColumn();")
        elif c["name"] in info["pk"] and c["type"] in ("int", "bigint") and len(info["pk"]) == 1:
            f.append(f"            e.Property(x => x.{c['name']}).ValueGeneratedNever();")
        if c["type"] == "datetime":
            f.append(f"            e.Property(x => x.{c['name']}).HasColumnType(\"timestamp without time zone\");")
        if c.get("default") is not None:
            f.append(f"            e.Property(x => x.{c['name']}).HasDefaultValue({c['default']});")
    for fk in info["fks"]:
        keys = "x." + fk["cols"][0] if len(fk["cols"]) == 1 else "new { " + ", ".join("x." + k for k in fk["cols"]) + " }"
        if fk["one"]:
            f.append(f"            e.HasOne(x => x.{fk['nav']}).WithOne(x => x.{t}).HasForeignKey<{t}>(x => {keys}).OnDelete(DeleteBehavior.Cascade);")
        else:
            f.append(f"            e.HasOne(x => x.{fk['nav']}).WithMany(x => x.{fk['coll']}).HasForeignKey(x => {keys}).OnDelete(DeleteBehavior.Restrict);")
    f.append("        });")
    fluent.append("\n".join(f))

ctx = ["using DucHanhTiepTan.Api.Entities;", "using Microsoft.EntityFrameworkCore;", "", "namespace DucHanhTiepTan.Api.Data;", "",
       "public partial class AppDbContext : DbContext", "{",
       "    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }", ""]
for t in tables:
    ctx.append(f"    public DbSet<{t}> {plural(t)} => Set<{t}>();")
ctx += ["", "    protected override void OnModelCreating(ModelBuilder modelBuilder)", "    {"]
ctx.append("\n".join(fluent))
ctx += ["        OnModelCreatingViews(modelBuilder);", "    }", "}", ""]
os.makedirs(os.path.join(outdir, "Data"), exist_ok=True)
open(os.path.join(outdir, "Data", "AppDbContext.cs"), "w", encoding="utf-8").write("\n".join(ctx))
print("tables:", len(tables))
print("one-to-one:", [(t, fk["parent"]) for t, i in tables.items() for fk in i["fks"] if fk["one"]])
for t, info in tables.items():
    pars = [fk["parent"] for fk in info["fks"]]
    dup = {p for p in pars if pars.count(p) > 1}
    if dup: print("multi-FK to same parent:", t, dup)
