using DucHanhTiepTan.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Assistant> Assistants => Set<Assistant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Configuration> Configurations => Set<Configuration>();
    public DbSet<ContainedConsulting> ContainedConsultings => Set<ContainedConsulting>();
    public DbSet<ContainedConsultingLabo> ContainedConsultingLabos => Set<ContainedConsultingLabo>();
    public DbSet<ContainedDiscount> ContainedDiscounts => Set<ContainedDiscount>();
    public DbSet<ContainedEquipmentExportInvoice> ContainedEquipmentExportInvoices => Set<ContainedEquipmentExportInvoice>();
    public DbSet<ContainedEquipmentInvoice> ContainedEquipmentInvoices => Set<ContainedEquipmentInvoice>();
    public DbSet<ContainedExpense> ContainedExpenses => Set<ContainedExpense>();
    public DbSet<ContainedInvoice> ContainedInvoices => Set<ContainedInvoice>();
    public DbSet<ContainedLabo> ContainedLabos => Set<ContainedLabo>();
    public DbSet<ContainedPicture> ContainedPictures => Set<ContainedPicture>();
    public DbSet<ContainedReceipt> ContainedReceipts => Set<ContainedReceipt>();
    public DbSet<ContainedTreatment> ContainedTreatments => Set<ContainedTreatment>();
    public DbSet<ContainedUsedEquipment> ContainedUsedEquipments => Set<ContainedUsedEquipment>();
    public DbSet<ContainedUsedReceipt> ContainedUsedReceipts => Set<ContainedUsedReceipt>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerMoreInformation> CustomerMoreInformations => Set<CustomerMoreInformation>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DoctorSalary> DoctorSalaries => Set<DoctorSalary>();
    public DbSet<Drug> Drugs => Set<Drug>();
    public DbSet<DrugInBranch> DrugInBranches => Set<DrugInBranch>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeType> EmployeeTypes => Set<EmployeeType>();
    public DbSet<EquimentInBranch> EquimentInBranches => Set<EquimentInBranch>();
    public DbSet<Equipment> Equipments => Set<Equipment>();
    public DbSet<EquipmentExportInvoice> EquipmentExportInvoices => Set<EquipmentExportInvoice>();
    public DbSet<EquipmentInvoice> EquipmentInvoices => Set<EquipmentInvoice>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<LaboType> LaboTypes => Set<LaboType>();
    public DbSet<LaboVendor> LaboVendors => Set<LaboVendor>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<MemberShipGroup> MemberShipGroups => Set<MemberShipGroup>();
    public DbSet<MonthYear> MonthYears => Set<MonthYear>();
    public DbSet<MonthYearDoctor> MonthYearDoctors => Set<MonthYearDoctor>();
    public DbSet<OtherEmployee> OtherEmployees => Set<OtherEmployee>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<Receptionist> Receptionists => Set<Receptionist>();
    public DbSet<Salary> Salaries => Set<Salary>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceDiscount> ServiceDiscounts => Set<ServiceDiscount>();
    public DbSet<ServiceGroup> ServiceGroups => Set<ServiceGroup>();
    public DbSet<UsedReceipt> UsedReceipts => Set<UsedReceipt>();
    public DbSet<YesNoQuestion> YesNoQuestions => Set<YesNoQuestion>();
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Ward> Wards => Set<Ward>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(e =>
        {
            e.ToTable("Account");
            e.HasKey(x => x.UserName);
            e.HasOne(x => x.Branch).WithMany(x => x.Accounts).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.EmployeeUser).WithMany(x => x.Accounts).HasForeignKey(x => x.EmployeeUserName).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Appointment>(e =>
        {
            e.ToTable("Appointment");
            e.HasKey(x => new { x.PrescriptionId, x.No });
            e.Property(x => x.CreatedDate).HasColumnType("timestamp without time zone");
            e.Property(x => x.AppointmentDate).HasColumnType("timestamp without time zone");
            e.HasOne(x => x.Doctor).WithMany(x => x.Appointments).HasForeignKey(x => x.DoctorName).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Prescription).WithMany(x => x.Appointments).HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Assistant>(e =>
        {
            e.ToTable("Assistant");
            e.HasKey(x => x.UserName);
            e.HasOne(x => x.User).WithOne(x => x.Assistant).HasForeignKey<Assistant>(x => x.UserName).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Branch>(e =>
        {
            e.ToTable("Branch");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
        });
        modelBuilder.Entity<Configuration>(e =>
        {
            e.ToTable("Configuration");
            e.HasKey(x => x.Name);
        });
        modelBuilder.Entity<ContainedConsulting>(e =>
        {
            e.ToTable("ContainedConsulting");
            e.HasKey(x => new { x.PrescriptionId, x.SubId });
            e.Property(x => x.CreatedDate).HasColumnType("timestamp without time zone");
            e.HasOne(x => x.Service).WithMany(x => x.ContainedConsultings).HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ContainedConsultingLabo>(e =>
        {
            e.ToTable("ContainedConsultingLabo");
            e.HasKey(x => new { x.PrescriptionId, x.SubId });
            e.Property(x => x.ReceiverDate).HasColumnType("timestamp without time zone");
            e.Property(x => x.DeliverDate).HasColumnType("timestamp without time zone");
            e.HasOne(x => x.LaboType).WithMany(x => x.ContainedConsultingLabos).HasForeignKey(x => x.LaboTypeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.LaboVendor).WithMany(x => x.ContainedConsultingLabos).HasForeignKey(x => x.LaboVendorId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ContainedDiscount>(e =>
        {
            e.ToTable("ContainedDiscount");
            e.HasKey(x => new { x.PrescriptionId, x.ServiceId });
            e.HasOne(x => x.Prescription).WithMany(x => x.ContainedDiscounts).HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Service).WithMany(x => x.ContainedDiscounts).HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ContainedEquipmentExportInvoice>(e =>
        {
            e.ToTable("ContainedEquipmentExportInvoice");
            e.HasKey(x => new { x.EquipmentExportInvoiceId, x.EquipmentId });
            e.HasOne(x => x.Equipment).WithMany(x => x.ContainedEquipmentExportInvoices).HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.EquipmentExportInvoice).WithMany(x => x.ContainedEquipmentExportInvoices).HasForeignKey(x => x.EquipmentExportInvoiceId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ContainedEquipmentInvoice>(e =>
        {
            e.ToTable("ContainedEquipmentInvoice");
            e.HasKey(x => new { x.InvoiceId, x.EquipmentId });
            e.HasOne(x => x.Equipment).WithMany(x => x.ContainedEquipmentInvoices).HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Invoice).WithMany(x => x.ContainedEquipmentInvoices).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ContainedExpense>(e =>
        {
            e.ToTable("ContainedExpense");
            e.HasKey(x => new { x.ExpenseId, x.Description });
            e.HasOne(x => x.Expense).WithMany(x => x.ContainedExpenses).HasForeignKey(x => x.ExpenseId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ContainedInvoice>(e =>
        {
            e.ToTable("ContainedInvoice");
            e.HasKey(x => new { x.InvoiceId, x.DrugId });
            e.HasOne(x => x.Drug).WithMany(x => x.ContainedInvoices).HasForeignKey(x => x.DrugId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Invoice).WithMany(x => x.ContainedInvoices).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ContainedLabo>(e =>
        {
            e.ToTable("ContainedLabo");
            e.HasKey(x => new { x.PrescriptionId, x.AppointmentNo, x.SubId });
            e.Property(x => x.ReceiverDate).HasColumnType("timestamp without time zone");
            e.Property(x => x.DeliverDate).HasColumnType("timestamp without time zone");
            e.HasOne(x => x.ContainedTreatment).WithMany(x => x.ContainedLabos).HasForeignKey(x => new { x.PrescriptionId, x.AppointmentNo, x.SubId }).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.LaboType).WithMany(x => x.ContainedLabos).HasForeignKey(x => x.LaboTypeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.LaboVendor).WithMany(x => x.ContainedLabos).HasForeignKey(x => x.LaboVendorId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ContainedPicture>(e =>
        {
            e.ToTable("ContainedPicture");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.HasOne(x => x.Appointment).WithMany(x => x.ContainedPictures).HasForeignKey(x => new { x.PrescriptionId, x.AppointmentNo }).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ContainedReceipt>(e =>
        {
            e.ToTable("ContainedReceipt");
            e.HasKey(x => new { x.PrescriptionId, x.AppointmentNo, x.DrugId });
            e.HasOne(x => x.Appointment).WithMany(x => x.ContainedReceipts).HasForeignKey(x => new { x.PrescriptionId, x.AppointmentNo }).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Drug).WithMany(x => x.ContainedReceipts).HasForeignKey(x => x.DrugId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ContainedTreatment>(e =>
        {
            e.ToTable("ContainedTreatment");
            e.HasKey(x => new { x.PrescriptionId, x.AppointmentNo, x.SubId });
            e.Property(x => x.CreatedDate).HasColumnType("timestamp without time zone");
            e.HasOne(x => x.Appointment).WithMany(x => x.ContainedTreatments).HasForeignKey(x => new { x.PrescriptionId, x.AppointmentNo }).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Assistant).WithMany(x => x.ContainedTreatments).HasForeignKey(x => x.AssistantName).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Doctor).WithMany(x => x.ContainedTreatments).HasForeignKey(x => x.DoctorName).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Service).WithMany(x => x.ContainedTreatments).HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ContainedUsedEquipment>(e =>
        {
            e.ToTable("ContainedUsedEquipment");
            e.HasKey(x => new { x.PrescriptionId, x.AppointmentNo, x.EquipmentId });
            e.HasOne(x => x.Appointment).WithMany(x => x.ContainedUsedEquipments).HasForeignKey(x => new { x.PrescriptionId, x.AppointmentNo }).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Equipment).WithMany(x => x.ContainedUsedEquipments).HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ContainedUsedReceipt>(e =>
        {
            e.ToTable("ContainedUsedReceipt");
            e.HasKey(x => new { x.PrescriptionId, x.AppointmentNo, x.DrugId });
            e.HasOne(x => x.Drug).WithMany(x => x.ContainedUsedReceipts).HasForeignKey(x => x.DrugId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.UsedReceipt).WithMany(x => x.ContainedUsedReceipts).HasForeignKey(x => new { x.PrescriptionId, x.AppointmentNo }).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Customer>(e =>
        {
            e.ToTable("Customer");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.CreatedDate).HasColumnType("timestamp without time zone");
            e.HasOne(x => x.Branch).WithMany(x => x.Customers).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<CustomerMoreInformation>(e =>
        {
            e.ToTable("CustomerMoreInformation");
            e.HasKey(x => x.CustomerId);
            e.Property(x => x.CustomerId).ValueGeneratedNever();
        });
        modelBuilder.Entity<Doctor>(e =>
        {
            e.ToTable("Doctor");
            e.HasKey(x => x.UserName);
            e.HasOne(x => x.User).WithOne(x => x.Doctor).HasForeignKey<Doctor>(x => x.UserName).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<DoctorSalary>(e =>
        {
            e.ToTable("DoctorSalary");
            e.HasKey(x => new { x.Year, x.Month, x.BranchId, x.UserName });
            e.HasOne(x => x.User).WithMany(x => x.DoctorSalaries).HasForeignKey(x => x.UserName).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.MonthYearDoctor).WithMany(x => x.DoctorSalaries).HasForeignKey(x => new { x.Year, x.Month, x.BranchId }).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Drug>(e =>
        {
            e.ToTable("Drug");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
        });
        modelBuilder.Entity<DrugInBranch>(e =>
        {
            e.ToTable("DrugInBranch");
            e.HasKey(x => new { x.BranchId, x.DrugId });
            e.HasOne(x => x.Branch).WithMany(x => x.DrugInBranches).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Drug).WithMany(x => x.DrugInBranches).HasForeignKey(x => x.DrugId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Employee>(e =>
        {
            e.ToTable("Employee");
            e.HasKey(x => x.UserName);
            e.HasOne(x => x.Branch).WithMany(x => x.Employees).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.EmployeeType).WithMany(x => x.Employees).HasForeignKey(x => x.EmployeeTypeId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<EmployeeType>(e =>
        {
            e.ToTable("EmployeeType");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
        });
        modelBuilder.Entity<EquimentInBranch>(e =>
        {
            e.ToTable("EquimentInBranch");
            e.HasKey(x => new { x.BranchId, x.EquipmentId });
            e.HasOne(x => x.Branch).WithMany(x => x.EquimentInBranches).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Equipment).WithMany(x => x.EquimentInBranches).HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Equipment>(e =>
        {
            e.ToTable("Equipment");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
        });
        modelBuilder.Entity<EquipmentExportInvoice>(e =>
        {
            e.ToTable("EquipmentExportInvoice");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.CreatedDate).HasColumnType("timestamp without time zone");
            e.HasOne(x => x.Branch).WithMany(x => x.EquipmentExportInvoices).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<EquipmentInvoice>(e =>
        {
            e.ToTable("EquipmentInvoice");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.CreatedDate).HasColumnType("timestamp without time zone");
            e.HasOne(x => x.Branch).WithMany(x => x.EquipmentInvoices).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Expense>(e =>
        {
            e.ToTable("Expense");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.CreatedDate).HasColumnType("timestamp without time zone");
            e.HasOne(x => x.Accountant).WithMany(x => x.Expenses).HasForeignKey(x => x.AccountantName).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Invoice>(e =>
        {
            e.ToTable("Invoice");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.CreatedDate).HasColumnType("timestamp without time zone");
            e.HasOne(x => x.User).WithMany(x => x.Invoices).HasForeignKey(x => x.UserName).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Branch).WithMany(x => x.Invoices).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<LaboType>(e =>
        {
            e.ToTable("LaboType");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
        });
        modelBuilder.Entity<LaboVendor>(e =>
        {
            e.ToTable("LaboVendor");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
        });
        modelBuilder.Entity<Membership>(e =>
        {
            e.ToTable("Membership");
            e.HasKey(x => x.Id);
            e.Property(x => x.CreatedDate).HasColumnType("timestamp without time zone");
            e.Property(x => x.ExpiredDate).HasColumnType("timestamp without time zone");
            e.HasOne(x => x.Group).WithMany(x => x.Memberships).HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<MemberShipGroup>(e =>
        {
            e.ToTable("MemberShipGroup");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.HasOne(x => x.Branch).WithMany(x => x.MemberShipGroups).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<MonthYear>(e =>
        {
            e.ToTable("MonthYear");
            e.HasKey(x => new { x.Month, x.Year, x.BranchId });
            e.HasOne(x => x.Branch).WithMany(x => x.MonthYears).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<MonthYearDoctor>(e =>
        {
            e.ToTable("MonthYearDoctor");
            e.HasKey(x => new { x.Year, x.Month, x.BranchId });
            e.HasOne(x => x.Branch).WithMany(x => x.MonthYearDoctors).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<OtherEmployee>(e =>
        {
            e.ToTable("OtherEmployee");
            e.HasKey(x => x.UserName);
            e.HasOne(x => x.User).WithOne(x => x.OtherEmployee).HasForeignKey<OtherEmployee>(x => x.UserName).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Payment>(e =>
        {
            e.ToTable("Payment");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.CreatedDate).HasColumnType("timestamp without time zone");
            e.HasOne(x => x.Receptionist).WithMany(x => x.Payments).HasForeignKey(x => x.ReceptionistName).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Prescription).WithMany(x => x.Payments).HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Prescription>(e =>
        {
            e.ToTable("Prescription");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.CreatedDate).HasColumnType("timestamp without time zone");
            e.HasOne(x => x.Customer).WithMany(x => x.Prescriptions).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Membership).WithMany(x => x.Prescriptions).HasForeignKey(x => x.MembershipId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Receptionist>(e =>
        {
            e.ToTable("Receptionist");
            e.HasKey(x => x.UserName);
            e.HasOne(x => x.User).WithOne(x => x.Receptionist).HasForeignKey<Receptionist>(x => x.UserName).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Salary>(e =>
        {
            e.ToTable("Salary");
            e.HasKey(x => new { x.Month, x.Year, x.BranchId, x.EmployeeName });
            e.Property(x => x.Absent).HasDefaultValue(0);
            e.Property(x => x.Fee).HasDefaultValue(0L);
            e.HasOne(x => x.Employee).WithMany(x => x.Salaries).HasForeignKey(x => x.EmployeeName).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.MonthYear).WithMany(x => x.Salaries).HasForeignKey(x => new { x.Month, x.Year, x.BranchId }).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Service>(e =>
        {
            e.ToTable("Service");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.HasOne(x => x.Group).WithMany(x => x.Services).HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ServiceDiscount>(e =>
        {
            e.ToTable("ServiceDiscount");
            e.HasKey(x => new { x.MemberShipGroupId, x.ServiceId });
            e.HasOne(x => x.MemberShipGroup).WithMany(x => x.ServiceDiscounts).HasForeignKey(x => x.MemberShipGroupId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Service).WithMany(x => x.ServiceDiscounts).HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ServiceGroup>(e =>
        {
            e.ToTable("ServiceGroup");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
        });
        modelBuilder.Entity<UsedReceipt>(e =>
        {
            e.ToTable("UsedReceipt");
            e.HasKey(x => new { x.PrescriptionId, x.AppointmentNo });
            e.Property(x => x.CreatedDate).HasColumnType("timestamp without time zone");
        });
        modelBuilder.Entity<YesNoQuestion>(e =>
        {
            e.ToTable("YesNoQuestion");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
        });
        modelBuilder.Entity<Province>(e =>
        {
            e.ToTable("Province");
            e.HasKey(x => x.Name);
        });
        modelBuilder.Entity<District>(e =>
        {
            e.ToTable("District");
            e.HasKey(x => new { x.ProvinceName, x.Name });
        });
        modelBuilder.Entity<Ward>(e =>
        {
            e.ToTable("Ward");
            e.HasKey(x => new { x.ProvinceName, x.DistrictName, x.Name });
        });
        OnModelCreatingViews(modelBuilder);
    }
}
