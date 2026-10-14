namespace DucHanhTiepTan.Api.Entities;

// Keyless entities mapped to the SQL views in Sql/views.sql (read-only).

public class TreatmentAdapter
{
    public long PrescriptionId { get; set; }
    public int AppointmentNo { get; set; }
    public int SubId { get; set; }
    public int ServiceId { get; set; }
    public int? LaboId { get; set; }
    public long OriginalPrice { get; set; }
    public long Price { get; set; }
    public DateTime CreatedDate { get; set; }
    public long Paid { get; set; }
    public string? DoctorName { get; set; }
    public string? AssistantName { get; set; }
    public string? Comment { get; set; }
    public int zQuantity { get; set; }
    public string? ToothPositions { get; set; }
    public long PreviousDebt { get; set; }
    public bool HasDone { get; set; }
    public bool IsCancelled { get; set; }
    public long Remain { get; set; }
}

public class PrescriptionAdapter
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public bool HasDone { get; set; }
    public string? Treatment { get; set; }
    public string? Diagnostic { get; set; }
    public string? Symptoms { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastAppointmentDate { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public string? AppointmentReason { get; set; }
    public string? AppointmentAdvised { get; set; }
    public int AppointmentNo { get; set; }
    public long Paid { get; set; }
    public long Remain { get; set; }
    public long CustomerVisiableId { get; set; }
}

public class AppointmentAdapter
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public long PrescriptionId { get; set; }
    public int No { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool HasDone { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public string? AppointmentReason { get; set; }
    public string? Advised { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public long CustomerId { get; set; }
    public string? DoctorFullName { get; set; }
    public int? NextAppointmentNo { get; set; }
    public long Paid { get; set; }
    public string? DoctorName { get; set; }
    public long CustomerVisiableId { get; set; }
}

public class EquipmentInvoiceView
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public int Quantity { get; set; }
    public long Total { get; set; }
}

public class ExpenseView
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string AccountantName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string? AccountantFullName { get; set; }
    public long Total { get; set; }
    public int BranchId { get; set; }
}

public class ExportInvoiceView
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public int BranchId { get; set; }
    public int Quantity { get; set; }
    public string BranchName { get; set; } = string.Empty;
}

public class InvoiceView
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public int Quantity { get; set; }
    public long Total { get; set; }
}
