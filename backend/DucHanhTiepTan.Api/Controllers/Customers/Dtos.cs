namespace DucHanhTiepTan.Api.Controllers.Customers;

// ----- Customers -----------------------------------------------------------

public record CustomerListDto(
    long Id, long VisiableId, string FormattedId, string Name, int Sex, int BirthYear, string? Address,
    string? ProvinceName, string? DistrictName, string? WardName, string AddressRepresentation,
    DateTime CreatedDate, string? Phone, string? Email, string? Job, int BranchId, string BranchName, bool HasImage);

public record CustomerMoreInformationDto(
    int WhyYouSeeMe, string? TheReasonYouSeeMe,
    bool SubcribeByPhone, string? PhoneToSubcribe,
    bool SubcribeByEmail, string? EmailToSubcribe,
    bool SubcribeByMail, string? MailToSubcribe,
    int YesQuestions);

public record CustomerDetailDto(
    long Id, long VisiableId, string FormattedId, string Name, int Sex, int BirthYear, int Age, string? Address,
    string? ProvinceName, string? DistrictName, string? WardName, string AddressRepresentation,
    DateTime CreatedDate, string? Phone, string? Email, string? Job, int BranchId, string BranchName, bool HasImage,
    int Times, CustomerMoreInformationDto? MoreInformation);

public record CustomerInput(
    long? VisiableId, string Name, int Sex, int BirthYear, string? Address, string? Phone, string? Email, string? Job,
    DateTime? CreatedDate, int? BranchId, string? ProvinceName, string? DistrictName, string? WardName,
    CustomerMoreInformationDto? MoreInformation,
    /// <summary>true when the photo was changed; then <see cref="ImageBase64"/> holds the new image (null = cleared).</summary>
    bool ImageChanged, string? ImageBase64);

public record YesNoQuestionDto(int Id, string Header);

// ----- Prescriptions (history) ---------------------------------------------

public record PrescriptionRowDto(
    long Id, int BranchId, string BranchName, long CustomerId, string CustomerName, long CustomerVisiableId, string? CustomerPhone,
    bool HasDone, string? Treatment, string? Diagnostic, string? Symptoms, DateTime CreatedDate,
    DateTime LastAppointmentDate, DateTime? LastNextAppointmentDate, string? LastAppointmentReason, string? AppointmentAdvised,
    int AppointmentNo, long Total, long Paid, long Remain);

// ----- Appointments ----------------------------------------------------------

public record AppointmentRowDto(
    long PrescriptionId, int No, int BranchId, string BranchName, DateTime CreatedDate, bool HasDone,
    DateTime? AppointmentDate, string? AppointmentReason, string? Advised,
    long CustomerId, long CustomerVisiableId, string CustomerName, string? CustomerPhone,
    string? DoctorName, string? DoctorFullName, int? NextAppointmentNo, long Paid, bool IsAppointed, string Status);

public record ChangeAppointmentDateInput(DateTime AppointmentDate);

public record DashboardItemDto(
    long PrescriptionId, int No, string BranchName, DateTime CreatedDate, long CustomerId, long VisiableId, string CustomerName,
    int Sex, int BirthYear, string? Address, string? Phone, string Content, long Total, long TotalIfNotDiscount, long Paid, long Remain,
    bool Finished, string? DoctorFullName, string Comment, bool IsNew);

public record DashboardDto(List<DashboardItemDto> Items, long Total, long TotalIfNotDiscount, long Paid, long Remain);
