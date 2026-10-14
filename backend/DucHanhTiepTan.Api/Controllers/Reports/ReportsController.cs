using System.Text;
using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Reports;

/// <summary>
/// "Thống kê doanh thu" + "Thống kê" ribbon groups of AdministratorForm.
/// Every endpoint accepts: from, to (yyyy-MM-dd, both empty = Tất cả) and branchId (0 = tất cả chi nhánh).
/// Each report has a twin "/export" endpoint producing the legacy "Lưu Excel" workbook.
/// </summary>
[ApiController]
[Route("api/reports")]
[Authorize(Policy = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public ReportsController(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    private int Branch(int? branchId) => BranchScope.Resolve(_user, branchId);

    private static FileContentResult Xlsx(byte[] bytes, string name) =>
        new(bytes, ExcelExporter.ContentType) { FileDownloadName = name + ".xlsx" };

    private static int CompareDateKeys(string a, string b) => string.CompareOrdinal(a, b);

    // ---------------------------------------------------------------- Doanh thu bác sĩ theo ngày (DoctorIncomeControl)

    private async Task<PivotResult> DoctorIncomeAsync(ReportRange range, int branch)
    {
        var all = range.IsAll; var f = range.FromDateTime; var t = range.ToExclusive;
        var baseQuery = _db.ContainedTreatments.Where(r => r.Appointment!.HasDone
            && (branch == 0 || r.Appointment.Prescription!.Customer!.BranchId == branch)
            && (all || (r.CreatedDate >= f && r.CreatedDate < t)));

        // Column set = every doctor having treatments in range (legacy includes cancelled ones for columns).
        var doctors = await baseQuery.Select(r => new { r.DoctorName, FullName = (string?)r.Doctor!.User!.FullName }).Distinct().ToListAsync();
        var columns = doctors.Select(d => new PivotColumn(d.DoctorName ?? Pivot.EmptyDoctorKey, d.FullName ?? Pivot.EmptyDoctorLabel)).DistinctBy(c => c.Key);

        var cells = await baseQuery.Where(r => !r.IsCancelled)
            .GroupBy(r => new { r.DoctorName, FullName = (string?)r.Doctor!.User!.FullName, Date = r.CreatedDate.Date })
            .Select(g => new { g.Key.DoctorName, g.Key.FullName, g.Key.Date, Price = g.Sum(r => (long)r.zQuantity * r.Price) })
            .ToListAsync();

        return Pivot.Build(cells.Select(c => (c.Date.ToString("yyyy-MM-dd"), c.Date.ToString("dd/MM/yy"), c.DoctorName ?? Pivot.EmptyDoctorKey, c.FullName ?? Pivot.EmptyDoctorLabel, c.Price)), columns, CompareDateKeys);
    }

    [HttpGet("doctor-income")]
    public Task<PivotResult> DoctorIncome(DateOnly? from, DateOnly? to, int? branchId) => DoctorIncomeAsync(ReportRange.Parse(from, to), Branch(branchId));

    [HttpGet("doctor-income/export")]
    public async Task<IActionResult> DoctorIncomeExport(DateOnly? from, DateOnly? to, int? branchId)
    {
        var range = ReportRange.Parse(from, to);
        var pivot = await DoctorIncomeAsync(range, Branch(branchId));
        return Xlsx(Pivot.ToExcel(pivot, "Doanh thu bac si", "Ngày", "Doanh thu bác sĩ theo ngày - " + range.Label), "doanh-thu-bac-si");
    }

    // ---------------------------------------------------------------- Doanh thu chi nhánh theo ngày (BranchIncomeControl)

    private async Task<PivotResult> BranchIncomeAsync(ReportRange range, int branch, bool byPayment)
    {
        var all = range.IsAll; var f = range.FromDateTime; var t = range.ToExclusive;
        List<(string, string, string, string, long)> cells;
        if (byPayment)
        {
            var rows = await _db.Payments.Where(r => (branch == 0 || r.Receptionist!.BranchId == branch) && (all || (r.CreatedDate >= f && r.CreatedDate < t)))
                .GroupBy(r => new { r.Receptionist!.BranchId, BranchName = r.Receptionist.Branch!.Name, Date = r.CreatedDate.Date })
                .Select(g => new { g.Key.BranchId, g.Key.BranchName, g.Key.Date, Price = g.Sum(r => r.Amount) })
                .ToListAsync();
            cells = rows.Select(c => (c.Date.ToString("yyyy-MM-dd"), c.Date.ToString("dd/MM/yy"), c.BranchId.ToString(), c.BranchName, c.Price)).ToList();
        }
        else
        {
            var rows = await _db.ContainedTreatments.Where(r => r.Appointment!.HasDone && !r.IsCancelled
                    && (branch == 0 || r.Appointment.Prescription!.Customer!.BranchId == branch) && (all || (r.CreatedDate >= f && r.CreatedDate < t)))
                .GroupBy(r => new { r.Appointment!.Prescription!.Customer!.BranchId, BranchName = r.Appointment.Prescription.Customer.Branch!.Name, Date = r.CreatedDate.Date })
                .Select(g => new { g.Key.BranchId, g.Key.BranchName, g.Key.Date, Price = g.Sum(r => (long)r.zQuantity * r.Price) })
                .ToListAsync();
            cells = rows.Select(c => (c.Date.ToString("yyyy-MM-dd"), c.Date.ToString("dd/MM/yy"), c.BranchId.ToString(), c.BranchName, c.Price)).ToList();
        }
        return Pivot.Build(cells, rowSort: CompareDateKeys);
    }

    /// <param name="mode">treatment (doanh thu điều trị, default) | payment (theo tiền thu)</param>
    [HttpGet("branch-income")]
    public Task<PivotResult> BranchIncome(DateOnly? from, DateOnly? to, int? branchId, string? mode) =>
        BranchIncomeAsync(ReportRange.Parse(from, to), Branch(branchId), mode == "payment");

    [HttpGet("branch-income/export")]
    public async Task<IActionResult> BranchIncomeExport(DateOnly? from, DateOnly? to, int? branchId, string? mode)
    {
        var range = ReportRange.Parse(from, to);
        var pivot = await BranchIncomeAsync(range, Branch(branchId), mode == "payment");
        return Xlsx(Pivot.ToExcel(pivot, "Doanh thu chi nhanh", "Ngày", $"Doanh thu chi nhánh theo ngày ({(mode == "payment" ? "theo tiền thu" : "theo điều trị")}) - {range.Label}"), "doanh-thu-chi-nhanh");
    }

    // ---------------------------------------------------------------- Doanh thu theo khách hàng (CustomerIncomeControl)

    public record CustomerIncomeRow(string BranchName, long CustomerId, long CustomerVisiableId, string CustomerName, DateTime CreatedDate,
        long Total, long Paid, long Remain, int PrescriptionNo, string? Province, string? District, string? Ward);
    public record CustomerIncomeResult(List<CustomerIncomeRow> Items, long Total, long Paid, long Remain);

    private async Task<CustomerIncomeResult> CustomerIncomeAsync(ReportRange range, int branch, string? province, string? district, string? ward)
    {
        var all = range.IsAll; var f = range.FromDateTime; var t = range.ToExclusive;
        if (string.IsNullOrWhiteSpace(province)) { province = null; district = null; ward = null; }
        if (string.IsNullOrWhiteSpace(district)) { district = null; ward = null; }
        if (string.IsNullOrWhiteSpace(ward)) ward = null;

        var rows = await _db.ContainedTreatments.Where(r =>
                (branch == 0 || r.Appointment!.Prescription!.Customer!.BranchId == branch)
                && (province == null || r.Appointment!.Prescription!.Customer!.ProvinceName == province)
                && (district == null || r.Appointment!.Prescription!.Customer!.DistrictName == district)
                && (ward == null || r.Appointment!.Prescription!.Customer!.WardName == ward)
                && (all || (r.Appointment!.CreatedDate >= f && r.Appointment.CreatedDate < t)))
            .GroupBy(r => new
            {
                r.Appointment!.Prescription!.Customer!.Id,
                r.Appointment.Prescription.Customer.VisiableId,
                r.Appointment.Prescription.Customer.Name,
                r.Appointment.Prescription.Customer.CreatedDate,
                BranchName = r.Appointment.Prescription.Customer.Branch!.Name,
                r.Appointment.Prescription.Customer.ProvinceName,
                r.Appointment.Prescription.Customer.DistrictName,
                r.Appointment.Prescription.Customer.WardName,
            })
            .Select(g => new
            {
                g.Key,
                Paid = g.Sum(r => r.Paid),
                Total = g.Sum(r => r.IsCancelled ? 0L : (long)r.zQuantity * r.Price),
            })
            .ToListAsync();

        var customerIds = rows.Select(r => r.Key.Id).ToList();
        var prescriptionCounts = await _db.Prescriptions.Where(p => customerIds.Contains(p.CustomerId))
            .GroupBy(p => p.CustomerId).Select(g => new { CustomerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Count);

        var items = rows.Select(r => new CustomerIncomeRow(r.Key.BranchName, r.Key.Id, r.Key.VisiableId, r.Key.Name, r.Key.CreatedDate,
                r.Total, r.Paid, r.Total - r.Paid, prescriptionCounts.GetValueOrDefault(r.Key.Id), r.Key.ProvinceName, r.Key.DistrictName, r.Key.WardName))
            .OrderBy(r => r.CustomerName).ToList();
        var total = items.Sum(i => i.Total);
        var paid = items.Sum(i => i.Paid);
        return new CustomerIncomeResult(items, total, paid, total - paid);
    }

    [HttpGet("customer-income")]
    public Task<CustomerIncomeResult> CustomerIncome(DateOnly? from, DateOnly? to, int? branchId, string? province, string? district, string? ward) =>
        CustomerIncomeAsync(ReportRange.Parse(from, to), Branch(branchId), province, district, ward);

    [HttpGet("customer-income/export")]
    public async Task<IActionResult> CustomerIncomeExport(DateOnly? from, DateOnly? to, int? branchId, string? province, string? district, string? ward)
    {
        var range = ReportRange.Parse(from, to);
        var data = await CustomerIncomeAsync(range, Branch(branchId), province, district, ward);
        var cols = new List<ExcelExporter.Column<CustomerIncomeRow>>
        {
            new("Chi nhánh", r => r.BranchName), new("Mã KH", r => r.CustomerVisiableId), new("Khách hàng", r => r.CustomerName),
            new("Ngày tạo", r => r.CreatedDate), new("Số phiếu khám", r => r.PrescriptionNo), new("Tổng tiền", r => r.Total),
            new("Đã trả", r => r.Paid), new("Còn nợ", r => r.Remain), new("Tỉnh/TP", r => r.Province), new("Quận/Huyện", r => r.District), new("Phường/Xã", r => r.Ward),
        };
        return Xlsx(ExcelExporter.Build("Doanh thu khach hang", data.Items, cols, "Doanh thu theo khách hàng - " + range.Label), "doanh-thu-khach-hang");
    }

    // ---------------------------------------------------------------- Dịch vụ theo chi nhánh (ServiceInBranchControl)

    private async Task<PivotResult> ServiceIncomeAsync(ReportRange range, int branch)
    {
        var all = range.IsAll; var f = range.FromDateTime; var t = range.ToExclusive;
        var rows = await _db.ContainedTreatments.Where(r => r.Appointment!.HasDone && !r.IsCancelled
                && (branch == 0 || r.Appointment.Prescription!.Customer!.BranchId == branch) && (all || (r.CreatedDate >= f && r.CreatedDate < t)))
            .GroupBy(r => new
            {
                r.Appointment!.Prescription!.Customer!.BranchId,
                BranchName = r.Appointment.Prescription.Customer.Branch!.Name,
                GroupId = r.Service!.GroupId,
                GroupName = (string?)r.Service.Group!.Name,
            })
            .Select(g => new { g.Key.BranchId, g.Key.BranchName, g.Key.GroupId, g.Key.GroupName, Price = g.Sum(r => (long)r.zQuantity * r.Price) })
            .ToListAsync();
        return Pivot.Build(rows.Select(c => (c.GroupId.ToString("D6"), c.GroupName ?? "(Không nhóm)", c.BranchId.ToString(), c.BranchName, c.Price)));
    }

    [HttpGet("service-income")]
    public Task<PivotResult> ServiceIncome(DateOnly? from, DateOnly? to, int? branchId) => ServiceIncomeAsync(ReportRange.Parse(from, to), Branch(branchId));

    [HttpGet("service-income/export")]
    public async Task<IActionResult> ServiceIncomeExport(DateOnly? from, DateOnly? to, int? branchId)
    {
        var range = ReportRange.Parse(from, to);
        var pivot = await ServiceIncomeAsync(range, Branch(branchId));
        return Xlsx(Pivot.ToExcel(pivot, "Dich vu theo chi nhanh", "Dịch vụ", "Doanh thu dịch vụ theo chi nhánh - " + range.Label), "doanh-thu-dich-vu");
    }

    // ---------------------------------------------------------------- Doanh thu và chi phí Labo của bác sĩ (DoctorExpenseControl)

    public record DoctorExpenseRow(string? BranchName, string DoctorName, string DoctorFullName, long Income, long Expense, long IncomeWoExpense);
    public record DoctorExpenseResult(List<DoctorExpenseRow> Items, long TotalIncome, long TotalExpense, long TotalIncomeWoExpense);

    private async Task<DoctorExpenseResult> DoctorExpenseAsync(ReportRange range, int branch)
    {
        var all = range.IsAll; var f = range.FromDateTime; var t = range.ToExclusive;
        var query = _db.ContainedTreatments.Where(r => !r.IsCancelled && r.Appointment!.HasDone
            && (branch == 0 || r.Appointment.Prescription!.Customer!.BranchId == branch) && (all || (r.CreatedDate >= f && r.CreatedDate < t)));
        var rows = await query
            .GroupBy(r => new { r.DoctorName, FullName = (string?)r.Doctor!.User!.FullName, BranchName = (string?)r.Doctor.User.Branch!.Name })
            .Select(g => new
            {
                g.Key.DoctorName, g.Key.FullName, g.Key.BranchName,
                Income = g.Sum(r => (long)r.zQuantity * r.Price),
                Expense = g.Sum(r => r.ContainedLabos.Sum(l => l.Price) * (long)r.zQuantity),
            })
            .ToListAsync();
        var items = rows.Select(r => new DoctorExpenseRow(r.BranchName, r.DoctorName ?? Pivot.EmptyDoctorKey, r.FullName ?? Pivot.EmptyDoctorLabel, r.Income, r.Expense, r.Income - r.Expense))
            .OrderBy(r => r.DoctorFullName).ToList();
        var income = items.Sum(i => i.Income);
        var expense = items.Sum(i => i.Expense);
        return new DoctorExpenseResult(items, income, expense, income - expense);
    }

    [HttpGet("doctor-expense")]
    public Task<DoctorExpenseResult> DoctorExpense(DateOnly? from, DateOnly? to, int? branchId) => DoctorExpenseAsync(ReportRange.Parse(from, to), Branch(branchId));

    [HttpGet("doctor-expense/export")]
    public async Task<IActionResult> DoctorExpenseExport(DateOnly? from, DateOnly? to, int? branchId)
    {
        var range = ReportRange.Parse(from, to);
        var data = await DoctorExpenseAsync(range, Branch(branchId));
        var cols = new List<ExcelExporter.Column<DoctorExpenseRow>>
        {
            new("Chi nhánh", r => r.BranchName), new("Bác sĩ", r => r.DoctorFullName), new("Doanh thu", r => r.Income),
            new("Chi phí Labo", r => r.Expense), new("Doanh thu - Chi phí", r => r.IncomeWoExpense),
        };
        return Xlsx(ExcelExporter.Build("Chi phi bac si", data.Items, cols, "Doanh thu và chi phí Labo của bác sĩ - " + range.Label), "chi-phi-bac-si");
    }

    // ---------------------------------------------------------------- Doanh thu theo phiếu khám (MembershipStatisticsControl)

    /// <summary>Legacy DiscountType: normal | free | discount | all.</summary>
    public record MembershipRow(long Id, string BranchName, long VisiableId, long CustomerId, DateTime CreatedDate, string CustomerName,
        string? MemberCode, string Discount, long TotalIfNotDiscount, long Total, long Paid, long Remain);
    public record MembershipResult(List<MembershipRow> Items, long Total, long TotalIfNotDiscount, long Paid, long Remain);

    private async Task<MembershipResult> MembershipAsync(ReportRange range, int branch, string? discount, string? memberCode)
    {
        var all = range.IsAll; var f = range.FromDateTime; var t = range.ToExclusive;
        discount = (discount ?? "all").ToLowerInvariant();
        var prescriptions = await _db.Prescriptions.Where(r =>
                (branch == 0 || r.Customer!.BranchId == branch)
                && (all || (r.CreatedDate >= f && r.CreatedDate < t))
                && (discount == "all"
                    || (discount == "discount" && r.MembershipId != null)
                    || (discount == "free" && r.DiscountValue == 1.0 && r.MembershipId == null)
                    || (discount == "normal" && r.DiscountValue == 0.0 && r.MembershipId == null)))
            .Select(r => new
            {
                r.Id, BranchName = r.Customer!.Branch!.Name, r.Customer.VisiableId, r.CustomerId, r.CreatedDate, CustomerName = r.Customer.Name,
                r.MembershipId, r.DiscountValue,
            })
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(memberCode))
        {
            var key = VietnameseText.Normalize(memberCode);
            prescriptions = prescriptions.Where(p => p.MembershipId != null && VietnameseText.Normalize(p.MembershipId).Contains(key)).ToList();
        }

        var ids = prescriptions.Select(p => p.Id).ToList();
        var treatments = await _db.TreatmentAdapters.Where(x => ids.Contains(x.PrescriptionId))
            .Select(x => new { x.PrescriptionId, x.SubId, x.HasDone, x.IsCancelled, x.Paid, x.Remain, x.Price, x.OriginalPrice, x.zQuantity })
            .ToListAsync();
        var byPrescription = treatments.GroupBy(x => x.PrescriptionId).ToDictionary(g => g.Key, g => g.ToList());

        var items = new List<MembershipRow>();
        foreach (var p in prescriptions)
        {
            var list = byPrescription.GetValueOrDefault(p.Id) ?? [];
            var done = list.Where(x => x.HasDone).ToList();
            var paid = done.Sum(x => x.Paid);
            var last = done.Count == 0 ? null : list.FirstOrDefault(x => x.SubId == done.Max(d => d.SubId));
            var remain = last?.Remain ?? 0;
            var active = done.Where(x => !x.IsCancelled).ToList();
            var total = active.Sum(x => x.Price * x.zQuantity);
            var totalIfNotDiscount = active.Sum(x => x.OriginalPrice * x.zQuantity);
            var kind = p.MembershipId != null ? "discount" : p.DiscountValue == 1.0 ? "free" : "normal";
            items.Add(new MembershipRow(p.Id, p.BranchName, p.VisiableId, p.CustomerId, p.CreatedDate, p.CustomerName, p.MembershipId, kind, totalIfNotDiscount, total, paid, remain));
        }
        items = items.OrderByDescending(i => i.CreatedDate).ToList();
        return new MembershipResult(items, items.Sum(i => i.Total), items.Sum(i => i.TotalIfNotDiscount), items.Sum(i => i.Paid), items.Sum(i => i.Remain));
    }

    [HttpGet("membership")]
    public Task<MembershipResult> Membership(DateOnly? from, DateOnly? to, int? branchId, string? discount, string? memberCode) =>
        MembershipAsync(ReportRange.Parse(from, to), Branch(branchId), discount, memberCode);

    [HttpGet("membership/export")]
    public async Task<IActionResult> MembershipExport(DateOnly? from, DateOnly? to, int? branchId, string? discount, string? memberCode)
    {
        var range = ReportRange.Parse(from, to);
        var data = await MembershipAsync(range, Branch(branchId), discount, memberCode);
        var cols = new List<ExcelExporter.Column<MembershipRow>>
        {
            new("Số phiếu", r => r.Id), new("Chi nhánh", r => r.BranchName), new("Mã KH", r => r.VisiableId), new("Khách hàng", r => r.CustomerName),
            new("Ngày lập", r => r.CreatedDate), new("Mã VIP", r => r.MemberCode), new("Loại", r => DiscountLabel(r.Discount)),
            new("Tổng chưa giảm", r => r.TotalIfNotDiscount), new("Tổng tiền", r => r.Total), new("Đã trả", r => r.Paid), new("Còn nợ", r => r.Remain),
        };
        return Xlsx(ExcelExporter.Build("Phieu kham", data.Items, cols, "Doanh thu theo phiếu khám - " + range.Label), "doanh-thu-phieu-kham");
    }

    public static string DiscountLabel(string kind) => kind switch { "free" => "Miễn phí", "discount" => "Khuyến mãi", "normal" => "Bình thường", _ => "Tất cả" };

    // ---------------------------------------------------------------- Thống kê sử dụng (UsedStatisticsControl): thuốc / nhập thiết bị / xuất thiết bị

    public record UsedDrugRow(int Id, string Name, string Unit, int Sum);
    public record UsedImportRow(int Id, string Name, string Serial, string? Unit, int Quantity, long Total, long Average);
    public record UsedExportRow(int Id, string Name, string Serial, string? Unit, int Quantity);
    public record UsedResult(List<UsedDrugRow> Drugs, List<UsedImportRow> Imports, List<UsedExportRow> Exports, long ImportTotal);

    private async Task<UsedResult> UsedAsync(ReportRange range, int branch)
    {
        var all = range.IsAll; var f = range.FromDateTime; var t = range.ToExclusive;
        var drugs = await _db.ContainedReceipts.Where(r => (all || (r.Appointment!.CreatedDate >= f && r.Appointment.CreatedDate < t))
                && (branch == 0 || r.Appointment!.Prescription!.Customer!.BranchId == branch) && r.Appointment!.HasDone)
            .GroupBy(r => new { r.Drug!.Id, r.Drug.Name, r.Drug.Unit })
            .Select(g => new { g.Key.Id, g.Key.Name, g.Key.Unit, Sum = g.Sum(r => r.Amount) })
            .OrderBy(r => r.Name).ToListAsync();
        var imports = await _db.ContainedEquipmentInvoices.Where(r => (all || (r.Invoice!.CreatedDate >= f && r.Invoice.CreatedDate < t))
                && (branch == 0 || r.Invoice!.BranchId == branch))
            .GroupBy(r => new { r.Equipment!.Id, r.Equipment.Name, r.Equipment.Serial, r.Equipment.Unit })
            .Select(g => new { g.Key.Id, g.Key.Name, g.Key.Serial, g.Key.Unit, Quantity = g.Sum(r => r.Quantity), Total = g.Sum(r => r.Price * (long)r.Quantity) })
            .OrderBy(r => r.Name).ToListAsync();
        var exports = await _db.ContainedEquipmentExportInvoices.Where(r => (all || (r.EquipmentExportInvoice!.CreatedDate >= f && r.EquipmentExportInvoice.CreatedDate < t))
                && (branch == 0 || r.EquipmentExportInvoice!.BranchId == branch))
            .GroupBy(r => new { r.Equipment!.Id, r.Equipment.Name, r.Equipment.Serial, r.Equipment.Unit })
            .Select(g => new { g.Key.Id, g.Key.Name, g.Key.Serial, g.Key.Unit, Quantity = g.Sum(r => r.Quantity) })
            .OrderBy(r => r.Name).ToListAsync();
        var drugRows = drugs.Select(d => new UsedDrugRow(d.Id, d.Name, d.Unit, d.Sum)).ToList();
        var importRows = imports.Select(i => new UsedImportRow(i.Id, i.Name, i.Serial, i.Unit, i.Quantity, i.Total, i.Quantity == 0 ? 0 : i.Total / i.Quantity)).ToList();
        var exportRows = exports.Select(e => new UsedExportRow(e.Id, e.Name, e.Serial, e.Unit, e.Quantity)).ToList();
        return new UsedResult(drugRows, importRows, exportRows, importRows.Sum(i => i.Total));
    }

    [HttpGet("used")]
    public Task<UsedResult> Used(DateOnly? from, DateOnly? to, int? branchId) => UsedAsync(ReportRange.Parse(from, to), Branch(branchId));

    /// <param name="tab">drugs | imports | exports</param>
    [HttpGet("used/export")]
    public async Task<IActionResult> UsedExport(DateOnly? from, DateOnly? to, int? branchId, string? tab)
    {
        var range = ReportRange.Parse(from, to);
        var data = await UsedAsync(range, Branch(branchId));
        return tab switch
        {
            "imports" => Xlsx(ExcelExporter.Build("Nhap thiet bi", data.Imports, new List<ExcelExporter.Column<UsedImportRow>>
            {
                new("Thiết bị", r => r.Name), new("Serial", r => r.Serial), new("Đơn vị", r => r.Unit), new("Số lượng", r => r.Quantity), new("Thành tiền", r => r.Total), new("Giá trung bình", r => r.Average),
            }, "Thống kê nhập thiết bị - " + range.Label), "thong-ke-nhap-thiet-bi"),
            "exports" => Xlsx(ExcelExporter.Build("Xuat thiet bi", data.Exports, new List<ExcelExporter.Column<UsedExportRow>>
            {
                new("Thiết bị", r => r.Name), new("Serial", r => r.Serial), new("Đơn vị", r => r.Unit), new("Số lượng", r => r.Quantity),
            }, "Thống kê xuất thiết bị - " + range.Label), "thong-ke-xuat-thiet-bi"),
            _ => Xlsx(ExcelExporter.Build("Su dung thuoc", data.Drugs, new List<ExcelExporter.Column<UsedDrugRow>>
            {
                new("Thuốc", r => r.Name), new("Đơn vị", r => r.Unit), new("Số lượng", r => r.Sum),
            }, "Thống kê sử dụng thuốc - " + range.Label), "thong-ke-su-dung-thuoc"),
        };
    }

    // ---------------------------------------------------------------- Thống kê chi phí (ExpenseStatisticsControl)

    public record ExpenseStatRow(DateTime? CreatedDate, string? BranchName, string? AccountantFullName, string? Description, long Amount);
    public record ExpenseStatResult(List<ExpenseStatRow> Items, long Total, string Mode);

    /// <param name="mode">detail (default) | content (nhóm theo nội dung) | date (nhóm theo ngày)</param>
    private async Task<ExpenseStatResult> ExpenseStatsAsync(ReportRange range, int branch, string? mode)
    {
        var all = range.IsAll; var f = range.FromDateTime; var t = range.ToExclusive;
        mode = (mode ?? "detail").ToLowerInvariant();
        var query = _db.ContainedExpenses.Where(r => (all || (r.Expense!.CreatedDate >= f && r.Expense.CreatedDate < t)) && (branch == 0 || r.Expense!.Accountant!.BranchId == branch));
        List<ExpenseStatRow> items;
        if (mode == "content")
        {
            var grouped = await query.GroupBy(r => r.Description).Select(g => new { Description = g.Key, Amount = g.Sum(r => r.Amount) }).OrderBy(r => r.Description).ToListAsync();
            items = grouped.Select(g => new ExpenseStatRow(null, null, null, g.Description, g.Amount)).ToList();
        }
        else if (mode == "date")
        {
            var rows = await query.GroupBy(r => r.Expense!.CreatedDate.Date).Select(g => new { Date = g.Key, Amount = g.Sum(r => r.Amount) }).OrderBy(r => r.Date).ToListAsync();
            items = new List<ExpenseStatRow>();
            foreach (var r in rows)
                items.Add(new ExpenseStatRow(r.Date, null, null, await ConcatDescriptionAsync(branch, r.Date), r.Amount));
        }
        else
        {
            items = await query.OrderBy(r => r.Expense!.CreatedDate)
                .Select(r => new ExpenseStatRow(r.Expense!.CreatedDate, r.Expense.Accountant!.Branch!.Name, r.Expense.Accountant.FullName, r.Description, r.Amount))
                .ToListAsync();
        }
        return new ExpenseStatResult(items, items.Sum(i => i.Amount), mode);
    }

    /// <summary>Legacy ConcatDescription: every expense line description of that day (branch filtered), comma separated.</summary>
    private async Task<string> ConcatDescriptionAsync(int branch, DateTime date)
    {
        var next = date.Date.AddDays(1);
        var descriptions = await _db.ContainedExpenses
            .Where(r => (branch == 0 || r.Expense!.Accountant!.BranchId == branch) && r.Expense!.CreatedDate >= date.Date && r.Expense.CreatedDate < next)
            .Select(r => r.Description).ToListAsync();
        return string.Join(", ", descriptions);
    }

    [HttpGet("expenses")]
    public Task<ExpenseStatResult> ExpenseStats(DateOnly? from, DateOnly? to, int? branchId, string? mode) => ExpenseStatsAsync(ReportRange.Parse(from, to), Branch(branchId), mode);

    [HttpGet("expenses/export")]
    public async Task<IActionResult> ExpenseStatsExport(DateOnly? from, DateOnly? to, int? branchId, string? mode)
    {
        var range = ReportRange.Parse(from, to);
        var data = await ExpenseStatsAsync(range, Branch(branchId), mode);
        var cols = new List<ExcelExporter.Column<ExpenseStatRow>>();
        if (data.Mode != "content") cols.Add(new("Ngày", r => r.CreatedDate));
        if (data.Mode == "detail") { cols.Add(new("Chi nhánh", r => r.BranchName)); cols.Add(new("Nhân viên", r => r.AccountantFullName)); }
        cols.Add(new("Nội dung", r => r.Description));
        cols.Add(new("Số tiền", r => r.Amount));
        return Xlsx(ExcelExporter.Build("Chi phi", data.Items, cols, "Thống kê chi phí - " + range.Label), "thong-ke-chi-phi");
    }

    // ---------------------------------------------------------------- Doanh thu - Chi phí theo ngày (IncomeExpenseStatisticsControl)

    public record IncomeExpenseRow(DateTime CreatedDate, long Income, long Expense, long Subtract, string? Description);
    public record IncomeExpenseResult(List<IncomeExpenseRow> Items, long Income, long Expense, long IncomeWoExpense);

    private async Task<IncomeExpenseResult> IncomeExpenseAsync(ReportRange range, int branch)
    {
        var all = range.IsAll; var f = range.FromDateTime; var t = range.ToExclusive;
        var payments = await _db.Payments.Where(r => (all || (r.CreatedDate >= f && r.CreatedDate < t)) && (branch == 0 || r.Receptionist!.BranchId == branch))
            .GroupBy(r => r.CreatedDate.Date).Select(g => new { Date = g.Key, Amount = g.Sum(r => r.Amount) }).ToListAsync();
        var expenses = await _db.ContainedExpenses.Where(r => (all || (r.Expense!.CreatedDate >= f && r.Expense.CreatedDate < t)) && (branch == 0 || r.Expense!.Accountant!.BranchId == branch))
            .GroupBy(r => r.Expense!.CreatedDate.Date).Select(g => new { Date = g.Key, Amount = g.Sum(r => r.Amount) }).ToListAsync();

        var dates = payments.Select(p => p.Date).Union(expenses.Select(e => e.Date)).OrderBy(d => d).ToList();
        var incomeByDate = payments.ToDictionary(p => p.Date, p => p.Amount);
        var expenseByDate = expenses.ToDictionary(e => e.Date, e => e.Amount);
        var items = new List<IncomeExpenseRow>();
        foreach (var d in dates)
        {
            var income = incomeByDate.GetValueOrDefault(d);
            var expense = expenseByDate.GetValueOrDefault(d);
            var description = expense > 0 ? await ConcatDescriptionAsync(branch, d) : null;
            items.Add(new IncomeExpenseRow(d, income, expense, income - expense, description));
        }
        var totalIncome = items.Sum(i => i.Income);
        var totalExpense = items.Sum(i => i.Expense);
        return new IncomeExpenseResult(items, totalIncome, totalExpense, totalIncome - totalExpense);
    }

    [HttpGet("income-expense")]
    public Task<IncomeExpenseResult> IncomeExpense(DateOnly? from, DateOnly? to, int? branchId) => IncomeExpenseAsync(ReportRange.Parse(from, to), Branch(branchId));

    [HttpGet("income-expense/export")]
    public async Task<IActionResult> IncomeExpenseExport(DateOnly? from, DateOnly? to, int? branchId)
    {
        var range = ReportRange.Parse(from, to);
        var data = await IncomeExpenseAsync(range, Branch(branchId));
        var cols = new List<ExcelExporter.Column<IncomeExpenseRow>>
        {
            new("Ngày", r => r.CreatedDate.Date), new("Doanh thu", r => r.Income), new("Chi phí", r => r.Expense), new("Doanh thu - Chi phí", r => r.Subtract), new("Nội dung chi", r => r.Description),
        };
        return Xlsx(ExcelExporter.Build("Doanh thu - Chi phi", data.Items, cols, "Thống kê doanh thu - chi phí - " + range.Label), "doanh-thu-chi-phi");
    }

    // ---------------------------------------------------------------- Thống kê sử dụng Labo (LaboStatisticsControl)

    public record LaboStatRow(int Id, string LaboName, string LaboType, int Quantity, long Total, long AveragePrice);
    public record LaboStatResult(List<LaboStatRow> Items, long TotalFixed, long QuantityFixed, long TotalDisassembly, long QuantityDisassembly, long TotalPrice, long TotalQuantity);

    private async Task<LaboStatResult> LaboStatsAsync(ReportRange range, int branch)
    {
        var all = range.IsAll; var f = range.FromDateTime; var t = range.ToExclusive;
        var rows = await _db.ContainedLabos.Where(r => (all || (r.ContainedTreatment!.CreatedDate >= f && r.ContainedTreatment.CreatedDate < t))
                && (branch == 0 || r.ContainedTreatment!.Appointment!.Prescription!.Customer!.BranchId == branch)
                && r.ContainedTreatment!.Appointment!.HasDone && !r.ContainedTreatment.IsCancelled)
            .GroupBy(r => new { r.LaboType!.Id, r.LaboType.Name, r.LaboType.Disassembly })
            .Select(g => new { g.Key.Id, g.Key.Name, g.Key.Disassembly, Quantity = g.Sum(r => r.ContainedTreatment!.zQuantity), Total = g.Sum(r => (long)r.ContainedTreatment!.zQuantity * r.Price) })
            .OrderBy(r => r.Name).ToListAsync();
        var items = rows.Select(r => new LaboStatRow(r.Id, r.Name, r.Disassembly ? "Tháo lắp" : "Cố định", r.Quantity, r.Total, r.Quantity == 0 ? 0 : (long)((float)r.Total / r.Quantity))).ToList();

        // Legacy summary labels use ContainedLabo.Query(branch, deliverDate range, disassembly): filtered by DeliverDate.
        var summary = await _db.ContainedLabos.Where(r => (branch == 0 || r.ContainedTreatment!.Appointment!.Prescription!.Customer!.BranchId == branch)
                && (all || (r.DeliverDate >= f && r.DeliverDate < t))
                && r.ContainedTreatment!.Appointment!.HasDone && !r.ContainedTreatment.IsCancelled)
            .GroupBy(r => r.LaboType!.Disassembly)
            .Select(g => new { Disassembly = g.Key, Total = g.Sum(r => r.Price * (long)r.ContainedTreatment!.zQuantity), Quantity = g.Sum(r => (long)r.ContainedTreatment!.zQuantity) })
            .ToListAsync();
        var fixedRow = summary.FirstOrDefault(s => !s.Disassembly);
        var dynRow = summary.FirstOrDefault(s => s.Disassembly);
        long tf = fixedRow?.Total ?? 0, qf = fixedRow?.Quantity ?? 0, td = dynRow?.Total ?? 0, qd = dynRow?.Quantity ?? 0;
        return new LaboStatResult(items, tf, qf, td, qd, tf + td, qf + qd);
    }

    [HttpGet("labo")]
    public Task<LaboStatResult> LaboStats(DateOnly? from, DateOnly? to, int? branchId) => LaboStatsAsync(ReportRange.Parse(from, to), Branch(branchId));

    [HttpGet("labo/export")]
    public async Task<IActionResult> LaboStatsExport(DateOnly? from, DateOnly? to, int? branchId)
    {
        var range = ReportRange.Parse(from, to);
        var data = await LaboStatsAsync(range, Branch(branchId));
        var cols = new List<ExcelExporter.Column<LaboStatRow>>
        {
            new("Loại Labo", r => r.LaboName), new("Kiểu", r => r.LaboType), new("Số lượng", r => r.Quantity), new("Thành tiền", r => r.Total), new("Giá trung bình", r => r.AveragePrice),
        };
        return Xlsx(ExcelExporter.Build("Labo", data.Items, cols, "Thống kê sử dụng Labo - " + range.Label), "thong-ke-labo");
    }

    // ---------------------------------------------------------------- Labo của bác sĩ theo ngày (LaboOfDoctorControl)

    private async Task<PivotResult> LaboByDoctorAsync(ReportRange range, int branch)
    {
        var all = range.IsAll; var f = range.FromDateTime; var t = range.ToExclusive;
        var doctors = await _db.ContainedTreatments.Where(r => r.Appointment!.HasDone
                && (branch == 0 || r.Appointment.Prescription!.Customer!.BranchId == branch) && (all || (r.CreatedDate >= f && r.CreatedDate < t)))
            .Select(r => new { r.DoctorName, FullName = (string?)r.Doctor!.User!.FullName }).Distinct().ToListAsync();
        var columns = doctors.Select(d => new PivotColumn(d.DoctorName ?? Pivot.EmptyDoctorKey, d.FullName ?? Pivot.EmptyDoctorLabel)).DistinctBy(c => c.Key);

        var cells = await _db.ContainedLabos.Where(r => r.ContainedTreatment!.Appointment!.HasDone && !r.ContainedTreatment.IsCancelled
                && (branch == 0 || r.ContainedTreatment.Appointment.Prescription!.Customer!.BranchId == branch)
                && (all || (r.ContainedTreatment.CreatedDate >= f && r.ContainedTreatment.CreatedDate < t)))
            .GroupBy(r => new { r.ContainedTreatment!.DoctorName, FullName = (string?)r.ContainedTreatment.Doctor!.User!.FullName, Date = r.ContainedTreatment.CreatedDate.Date })
            .Select(g => new { g.Key.DoctorName, g.Key.FullName, g.Key.Date, Price = g.Sum(r => (long)r.ContainedTreatment!.zQuantity * r.Price) })
            .ToListAsync();
        return Pivot.Build(cells.Select(c => (c.Date.ToString("yyyy-MM-dd"), c.Date.ToString("dd/MM/yy"), c.DoctorName ?? Pivot.EmptyDoctorKey, c.FullName ?? Pivot.EmptyDoctorLabel, c.Price)), columns, CompareDateKeys);
    }

    [HttpGet("labo-by-doctor")]
    public Task<PivotResult> LaboByDoctor(DateOnly? from, DateOnly? to, int? branchId) => LaboByDoctorAsync(ReportRange.Parse(from, to), Branch(branchId));

    [HttpGet("labo-by-doctor/export")]
    public async Task<IActionResult> LaboByDoctorExport(DateOnly? from, DateOnly? to, int? branchId)
    {
        var range = ReportRange.Parse(from, to);
        var pivot = await LaboByDoctorAsync(range, Branch(branchId));
        return Xlsx(Pivot.ToExcel(pivot, "Labo theo bac si", "Ngày", "Labo của bác sĩ theo ngày - " + range.Label), "labo-theo-bac-si");
    }

    // ---------------------------------------------------------------- lookups used by the report filters

    public record AddressOption(string Name);

    [HttpGet("lookups/provinces")]
    public async Task<List<string>> Provinces() =>
        await _db.Customers.Where(c => c.ProvinceName != null && c.ProvinceName != "").Select(c => c.ProvinceName!).Distinct().OrderBy(x => x).ToListAsync();

    [HttpGet("lookups/districts")]
    public async Task<List<string>> Districts(string province) =>
        await _db.Customers.Where(c => c.ProvinceName == province && c.DistrictName != null && c.DistrictName != "").Select(c => c.DistrictName!).Distinct().OrderBy(x => x).ToListAsync();

    [HttpGet("lookups/wards")]
    public async Task<List<string>> Wards(string province, string district) =>
        await _db.Customers.Where(c => c.ProvinceName == province && c.DistrictName == district && c.WardName != null && c.WardName != "").Select(c => c.WardName!).Distinct().OrderBy(x => x).ToListAsync();
}
