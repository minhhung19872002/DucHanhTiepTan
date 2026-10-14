namespace DucHanhTiepTan.Api.Controllers.Examination;

// ---------- lookups ----------
public record ServiceLookup(int Id, string Name, long Price, int GroupId, string? GroupName);
public record DrugLookup(int Id, string Name, string Unit, long Price, long OutPrice);
public record EquipmentLookup(int Id, string Name, string Serial, string? Unit, string Vendor, string? Position, string? Usage, string? Note, int StockQuantity);
public record IdName(int Id, string Name);
public record EmployeeLookup(string UserName, string FullName);
public record LookupsDto(
    List<ServiceLookup> Services, List<DrugLookup> Drugs, List<EquipmentLookup> Equipment,
    List<IdName> LaboTypes, List<IdName> LaboVendors, List<EmployeeLookup> Doctors, List<EmployeeLookup> Assistants);

// ---------- in-progress list ("Đang khám") ----------
public record InProgressItem(long PrescriptionId, int AppointmentNo, DateTime CreatedDate, long CustomerId, string CustomerName,
    string FormattedId, int CustomerBirthYear, string CustomerSex, string? DoctorName, string? DoctorFullName, int BranchId);

public record StartRequest(long CustomerId);
public record StartResult(long PrescriptionId, int AppointmentNo);

// ---------- detail ----------
public record LaboDto(int? LaboVendorId, int? LaboTypeId, string? Color, DateTime? ReceiverDate, DateTime? DeliverDate, bool Delivered, long Price);

public record ConsultingDto(int SubId, int ServiceId, long OriginalPrice, long Price, DateTime CreatedDate, string? Comment, int Quantity, string? ToothPositions, LaboDto? Labo);

public record TreatmentDto(int SubId, int AppointmentNo, int ServiceId, long OriginalPrice, long Price, DateTime CreatedDate, long Paid,
    string? DoctorName, string? AssistantName, string? Comment, int Quantity, string? ToothPositions, long PreviousDebt, bool IsCancelled, LaboDto? Labo, int? LaboId);

public record ReceiptDto(int DrugId, int Amount, string? Instruction, double PerDay, double PerTimes);
public record UsedEquipmentDto(int EquipmentId, int Quantity);
public record DiscountItemDto(int ServiceId, string? ServiceName, double Value);
public record PaymentDto(long Id, DateTime CreatedDate, long Amount, string ReceptionistName);

public record CustomerInfo(long Id, string Name, string FormattedId, int Sex, int BirthYear, string? Address, string? Phone, int BranchId);
public record BranchInfo(int Id, string Name, string? Address, string? Tel);

public record PrescriptionInfo(long Id, long CustomerId, bool HasDone, string? Treatment, string? Diagnostic, string? Symptoms, DateTime CreatedDate,
    string? MembershipId, double DiscountValue, string DiscountType /* normal|free|discount */, List<DiscountItemDto> Discounts, long Remain);

public record AppointmentInfo(long PrescriptionId, int No, DateTime CreatedDate, bool HasDone, DateTime? AppointmentDate, string? AppointmentReason, string? Advised, string? DoctorName, int MaxNo);

public record MembershipInfo(string Id, DateTime CreatedDate, DateTime? ExpiredDate, string? Description, int GroupId, string GroupName, bool IsFixed, double FixedValue, List<DiscountItemDto> Discounts);

public record ExaminationDetail(
    CustomerInfo Customer, BranchInfo Branch, PrescriptionInfo Prescription, AppointmentInfo Appointment,
    List<ConsultingDto> Consultings, List<TreatmentDto> Treatments, List<TreatmentDto> ElderTreatments, long PreviousDebt,
    List<ReceiptDto> Receipts, List<UsedEquipmentDto> UsedEquipments, List<long> PictureIds, List<PaymentDto> Payments,
    MembershipInfo? Membership, bool IsSuperUser);

// ---------- save ----------
public record DiscountInput(string Type /* normal|free|discount */, string? MembershipId);
public record ReceiptInput(int DrugId, int Amount, string? Instruction, double PerDay, double PerTimes);

public record SaveRequest(
    string? Diagnostic, string? Symptoms, string? Treatment, DateTime? PrescriptionCreatedDate,
    string? DoctorName, DateTime? AppointmentCreatedDate /* null = now */,
    string Status /* notyet|appoint|done */, DateTime? AppointmentDate, string? AppointmentReason, string? Advised,
    List<ConsultingDto> Consultings, List<TreatmentDto> Treatments, List<ReceiptInput> Receipts, List<UsedEquipmentDto> UsedEquipments,
    DiscountInput? Discount);

public record PrescriptionSummary(long Id, long CustomerId, string CustomerName, DateTime CreatedDate, bool HasDone, List<int> AppointmentNos);
