using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Inventory;

// DTOs shared by the inventory controllers ("Quản lí thuốc" + "Xuất nhập thiết bị").

public record DrugLookupDto(int Id, string Name, string Unit, long Price, long OutPrice, string? Description);

public record EquipmentLookupDto(int Id, string Name, string Serial, string? Unit, long Price, bool IsVarious, string Vendor,
    string? Position, string? Usage, string? Note, int Quantity);

public record InvoiceLineInput(int ItemId, int Quantity, long Price);

public record ExportLineInput(int EquipmentId, int Quantity);

public record CreateInvoiceInput(DateTime? CreatedDate, int? BranchId, List<InvoiceLineInput> Lines);

public record CreateExportInput(DateTime? CreatedDate, int? BranchId, List<ExportLineInput> Lines);

public record DrugInvoiceDto(int Id, DateTime CreatedDate, string UserName, string? FullName, int BranchId, string BranchName, int Quantity, long Total);

public record DrugInvoiceLineDto(int DrugId, string DrugName, string Unit, long Price, int Quantity, long Total);

public record DrugInvoiceDetailDto(DrugInvoiceDto Invoice, List<DrugInvoiceLineDto> Lines);

public record EquipmentInvoiceDto(int Id, DateTime CreatedDate, int BranchId, string BranchName, int Quantity, long Total);

public record EquipmentInvoiceLineDto(int EquipmentId, string EquipmentName, string Serial, string EquipmentType, string Vendor, string? Unit, long Price, int Quantity, long Total);

public record EquipmentInvoiceDetailDto(EquipmentInvoiceDto Invoice, List<EquipmentInvoiceLineDto> Lines);

public record ExportInvoiceDto(int Id, DateTime CreatedDate, int BranchId, string BranchName, int Quantity);

public record ExportInvoiceLineDto(int EquipmentId, string EquipmentName, string Serial, string EquipmentType, string Vendor, string? Unit, int Quantity);

public record ExportInvoiceDetailDto(ExportInvoiceDto Invoice, List<ExportInvoiceLineDto> Lines);

public record DrugReceiptDto(long PrescriptionId, int AppointmentNo, DateTime CreatedDate, long CustomerId, string CustomerName, long CustomerVisiableId,
    int BranchId, string BranchName, int Quantity, long Total);

public record DrugReceiptLineDto(int DrugId, string DrugName, string Unit, long Price, int Quantity, long Total);

public record DrugReceiptDetailDto(DrugReceiptDto Receipt, List<DrugReceiptLineDto> Lines);

public record DrugStockDto(int DrugId, string Name, string Unit, long Price, long OutPrice, string? Description, int BranchId, int Quantity);

public record EquipmentStockDto(int Id, string Name, string Serial, string? Unit, long Price, bool IsVarious, string EquipmentType, string Vendor,
    string? Position, string? Usage, string? Note, int BranchId, int Quantity);

public record StockAdjustInput(int BranchId, int Quantity);

public record ListResult<T>(List<T> Items, long Total, int Count);

internal static class InventoryHelpers
{
    /// <summary>Legacy date filter: fromDate == null means "all"; otherwise CreatedDate.Date between from and to (inclusive).</summary>
    public static (DateTime? from, DateTime? to) Range(DateTime? from, DateTime? to)
    {
        if (from is null) return (null, null);
        var f = from.Value.Date;
        var t = (to ?? from.Value).Date;
        if (t < f) (f, t) = (t, f);
        return (f, t.AddDays(1)); // exclusive upper bound
    }

    /// <summary>Branch for a new voucher: receptionists use their own branch, admins must pick a concrete one.</summary>
    public static int RequireBranch(ICurrentUser user, int? requested)
    {
        var id = BranchScope.Resolve(user, requested);
        if (id == 0) throw new ApiException("Vui lòng chọn chi nhánh để lập phiếu");
        return id;
    }

    public static string EquipmentType(bool isVarious) => isVarious ? "Hao mòn" : "Cố định";

    public static async Task AdjustDrugStockAsync(AppDbContext db, int branchId, int drugId, int delta)
    {
        var row = await db.DrugInBranches.FindAsync(branchId, drugId);
        if (row is null)
        {
            db.DrugInBranches.Add(new DrugInBranch { BranchId = branchId, DrugId = drugId, Quantity = delta });
        }
        else
        {
            row.Quantity += delta;
        }
    }

    public static async Task AdjustEquipmentStockAsync(AppDbContext db, int branchId, int equipmentId, int delta, bool validate)
    {
        var row = await db.EquimentInBranches.FindAsync(branchId, equipmentId);
        var current = row?.Quantity ?? 0;
        if (validate && current + delta < 0)
        {
            var name = await db.Equipments.Where(e => e.Id == equipmentId).Select(e => e.Name).FirstOrDefaultAsync() ?? equipmentId.ToString();
            throw new ApiException($"\"{name}\" không đủ tồn kho (còn {current:n0})");
        }
        if (row is null)
        {
            db.EquimentInBranches.Add(new EquimentInBranch { BranchId = branchId, EquipmentId = equipmentId, Quantity = delta });
        }
        else
        {
            row.Quantity += delta;
        }
    }

    public static void ValidateLines<T>(IReadOnlyCollection<T> lines, Func<T, int> id, Func<T, int> qty, Func<T, long>? price)
    {
        if (lines.Count == 0) throw new ApiException("Phiếu phải có ít nhất một dòng");
        foreach (var l in lines)
        {
            if (id(l) <= 0) throw new ApiException("Vui lòng chọn thiết bị.");
            if (qty(l) <= 0 || (price is not null && price(l) <= 0)) throw new ApiException("Số lượng và giá phải lớn hơn 0.");
        }
        var dup = lines.GroupBy(id).FirstOrDefault(g => g.Count() > 1);
        if (dup is not null) throw new ApiException("Dòng bị trùng thiết bị/thuốc");
    }
}
