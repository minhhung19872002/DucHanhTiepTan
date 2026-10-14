using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DucHanhTiepTan.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Branch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Tel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configuration",
                columns: table => new
                {
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuration", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "CustomerMoreInformation",
                columns: table => new
                {
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    WhyYouSeeMe = table.Column<int>(type: "integer", nullable: false),
                    TheReasonYouSeeMe = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    SubcribeByPhone = table.Column<bool>(type: "boolean", nullable: false),
                    SubcribeByEmail = table.Column<bool>(type: "boolean", nullable: false),
                    SubcribeByMail = table.Column<bool>(type: "boolean", nullable: false),
                    PhoneToSubcribe = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EmailToSubcribe = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MailToSubcribe = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    YesQuestions = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerMoreInformation", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "District",
                columns: table => new
                {
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProvinceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_District", x => new { x.ProvinceName, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "Drug",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    OutPrice = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drug", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Serial = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Position = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Usage = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Note = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Vendor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsVarious = table.Column<bool>(type: "boolean", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LaboType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Disassembly = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaboType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LaboVendor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Tel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaboVendor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Province",
                columns: table => new
                {
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Province", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "ServiceGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsedReceipt",
                columns: table => new
                {
                    PrescriptionId = table.Column<long>(type: "bigint", nullable: false),
                    AppointmentNo = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsedReceipt", x => new { x.PrescriptionId, x.AppointmentNo });
                });

            migrationBuilder.CreateTable(
                name: "Ward",
                columns: table => new
                {
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DistrictName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProvinceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ward", x => new { x.ProvinceName, x.DistrictName, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "YesNoQuestion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Header = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YesNoQuestion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Sex = table.Column<int>(type: "integer", nullable: false),
                    BirthYear = table.Column<int>(type: "integer", nullable: false),
                    Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Job = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Image = table.Column<byte[]>(type: "bytea", nullable: true),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    ProvinceName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DistrictName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    WardName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VisiableId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customer_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentExportInvoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentExportInvoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentExportInvoice_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentInvoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentInvoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentInvoice_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberShipGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IsFixed = table.Column<bool>(type: "boolean", nullable: false),
                    FixedValue = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberShipGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberShipGroup_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MonthYear",
                columns: table => new
                {
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    WorkDaysInMonth = table.Column<int>(type: "integer", nullable: false),
                    BaseLunchSubsidy = table.Column<long>(type: "bigint", nullable: false),
                    BasePhoneSubsidy = table.Column<long>(type: "bigint", nullable: false),
                    BaseToxicSubsidy = table.Column<long>(type: "bigint", nullable: false),
                    BaseSalary = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthYear", x => new { x.Month, x.Year, x.BranchId });
                    table.ForeignKey(
                        name: "FK_MonthYear_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MonthYearDoctor",
                columns: table => new
                {
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthYearDoctor", x => new { x.Year, x.Month, x.BranchId });
                    table.ForeignKey(
                        name: "FK_MonthYearDoctor_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DrugInBranch",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugInBranch", x => new { x.BranchId, x.DrugId });
                    table.ForeignKey(
                        name: "FK_DrugInBranch_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DrugInBranch_Drug_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EmployeeTypeId = table.Column<int>(type: "integer", nullable: false),
                    DefaultSalary = table.Column<long>(type: "bigint", nullable: false),
                    DefaultRatio = table.Column<double>(type: "double precision", nullable: true),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    JoinDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Sex = table.Column<int>(type: "integer", nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Rank = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OtherEmployeeTypeName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BaseLunchSubsidy = table.Column<long>(type: "bigint", nullable: false),
                    BasePhoneSubsidy = table.Column<long>(type: "bigint", nullable: false),
                    BaseToxicSubsidy = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.UserName);
                    table.ForeignKey(
                        name: "FK_Employee_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employee_EmployeeType_EmployeeTypeId",
                        column: x => x.EmployeeTypeId,
                        principalTable: "EmployeeType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EquimentInBranch",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquimentInBranch", x => new { x.BranchId, x.EquipmentId });
                    table.ForeignKey(
                        name: "FK_EquimentInBranch_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquimentInBranch_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContainedConsultingLabo",
                columns: table => new
                {
                    PrescriptionId = table.Column<long>(type: "bigint", nullable: false),
                    SubId = table.Column<int>(type: "integer", nullable: false),
                    LaboVendorId = table.Column<int>(type: "integer", nullable: false),
                    LaboTypeId = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReceiverDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeliverDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Delivered = table.Column<bool>(type: "boolean", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainedConsultingLabo", x => new { x.PrescriptionId, x.SubId });
                    table.ForeignKey(
                        name: "FK_ContainedConsultingLabo_LaboType_LaboTypeId",
                        column: x => x.LaboTypeId,
                        principalTable: "LaboType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContainedConsultingLabo_LaboVendor_LaboVendorId",
                        column: x => x.LaboVendorId,
                        principalTable: "LaboVendor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Service",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    GroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Service", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Service_ServiceGroup_GroupId",
                        column: x => x.GroupId,
                        principalTable: "ServiceGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContainedUsedReceipt",
                columns: table => new
                {
                    PrescriptionId = table.Column<long>(type: "bigint", nullable: false),
                    AppointmentNo = table.Column<int>(type: "integer", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainedUsedReceipt", x => new { x.PrescriptionId, x.AppointmentNo, x.DrugId });
                    table.ForeignKey(
                        name: "FK_ContainedUsedReceipt_Drug_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContainedUsedReceipt_UsedReceipt_PrescriptionId_Appointment~",
                        columns: x => new { x.PrescriptionId, x.AppointmentNo },
                        principalTable: "UsedReceipt",
                        principalColumns: new[] { "PrescriptionId", "AppointmentNo" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContainedEquipmentExportInvoice",
                columns: table => new
                {
                    EquipmentExportInvoiceId = table.Column<int>(type: "integer", nullable: false),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainedEquipmentExportInvoice", x => new { x.EquipmentExportInvoiceId, x.EquipmentId });
                    table.ForeignKey(
                        name: "FK_ContainedEquipmentExportInvoice_EquipmentExportInvoice_Equi~",
                        column: x => x.EquipmentExportInvoiceId,
                        principalTable: "EquipmentExportInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContainedEquipmentExportInvoice_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContainedEquipmentInvoice",
                columns: table => new
                {
                    InvoiceId = table.Column<int>(type: "integer", nullable: false),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainedEquipmentInvoice", x => new { x.InvoiceId, x.EquipmentId });
                    table.ForeignKey(
                        name: "FK_ContainedEquipmentInvoice_EquipmentInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "EquipmentInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContainedEquipmentInvoice_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Membership",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiredDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    GroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Membership", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Membership_MemberShipGroup_GroupId",
                        column: x => x.GroupId,
                        principalTable: "MemberShipGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsAdministrator = table.Column<bool>(type: "boolean", nullable: false),
                    Password = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    FullName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EmployeeUserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.UserName);
                    table.ForeignKey(
                        name: "FK_Account_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Account_Employee_EmployeeUserName",
                        column: x => x.EmployeeUserName,
                        principalTable: "Employee",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Assistant",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assistant", x => x.UserName);
                    table.ForeignKey(
                        name: "FK_Assistant_Employee_UserName",
                        column: x => x.UserName,
                        principalTable: "Employee",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Doctor",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctor", x => x.UserName);
                    table.ForeignKey(
                        name: "FK_Doctor_Employee_UserName",
                        column: x => x.UserName,
                        principalTable: "Employee",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtherEmployee",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtherEmployee", x => x.UserName);
                    table.ForeignKey(
                        name: "FK_OtherEmployee_Employee_UserName",
                        column: x => x.UserName,
                        principalTable: "Employee",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Receptionist",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receptionist", x => x.UserName);
                    table.ForeignKey(
                        name: "FK_Receptionist_Employee_UserName",
                        column: x => x.UserName,
                        principalTable: "Employee",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Salary",
                columns: table => new
                {
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Rank = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Ratio = table.Column<double>(type: "double precision", nullable: false),
                    WorkDays = table.Column<int>(type: "integer", nullable: false),
                    OvertimeInMinute = table.Column<int>(type: "integer", nullable: false),
                    DegreeAllowance = table.Column<long>(type: "bigint", nullable: false),
                    HeathAllowance = table.Column<long>(type: "bigint", nullable: false),
                    DayOffAllowance = table.Column<long>(type: "bigint", nullable: false),
                    RiskAllowance = table.Column<long>(type: "bigint", nullable: false),
                    RetireAllowance = table.Column<long>(type: "bigint", nullable: false),
                    Advance = table.Column<long>(type: "bigint", nullable: false),
                    BasePhoneSubsidy = table.Column<long>(type: "bigint", nullable: false),
                    BaseToxicSubsidy = table.Column<long>(type: "bigint", nullable: false),
                    BaseLunchSubsidy = table.Column<long>(type: "bigint", nullable: false),
                    TowHoursOvertimeDays = table.Column<int>(type: "integer", nullable: false),
                    Absent = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Fee = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salary", x => new { x.Month, x.Year, x.BranchId, x.EmployeeName });
                    table.ForeignKey(
                        name: "FK_Salary_Employee_EmployeeName",
                        column: x => x.EmployeeName,
                        principalTable: "Employee",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Salary_MonthYear_Month_Year_BranchId",
                        columns: x => new { x.Month, x.Year, x.BranchId },
                        principalTable: "MonthYear",
                        principalColumns: new[] { "Month", "Year", "BranchId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContainedConsulting",
                columns: table => new
                {
                    PrescriptionId = table.Column<long>(type: "bigint", nullable: false),
                    SubId = table.Column<int>(type: "integer", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    OriginalPrice = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Comment = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    zQuantity = table.Column<int>(type: "integer", nullable: false),
                    ToothPositions = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainedConsulting", x => new { x.PrescriptionId, x.SubId });
                    table.ForeignKey(
                        name: "FK_ContainedConsulting_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceDiscount",
                columns: table => new
                {
                    MemberShipGroupId = table.Column<int>(type: "integer", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceDiscount", x => new { x.MemberShipGroupId, x.ServiceId });
                    table.ForeignKey(
                        name: "FK_ServiceDiscount_MemberShipGroup_MemberShipGroupId",
                        column: x => x.MemberShipGroupId,
                        principalTable: "MemberShipGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceDiscount_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Prescription",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    HasDone = table.Column<bool>(type: "boolean", nullable: false),
                    Treatment = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Diagnostic = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Symptoms = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    MembershipId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DiscountValue = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescription_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescription_Membership_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "Membership",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Expense",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AccountantName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expense", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expense_Account_AccountantName",
                        column: x => x.AccountantName,
                        principalTable: "Account",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoice_Account_UserName",
                        column: x => x.UserName,
                        principalTable: "Account",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoice_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DoctorSalary",
                columns: table => new
                {
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    UserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BaseSalary = table.Column<long>(type: "bigint", nullable: false),
                    LunchSubsidy = table.Column<long>(type: "bigint", nullable: false),
                    RespondSubsidy = table.Column<long>(type: "bigint", nullable: false),
                    Advance = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorSalary", x => new { x.Year, x.Month, x.BranchId, x.UserName });
                    table.ForeignKey(
                        name: "FK_DoctorSalary_Doctor_UserName",
                        column: x => x.UserName,
                        principalTable: "Doctor",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DoctorSalary_MonthYearDoctor_Year_Month_BranchId",
                        columns: x => new { x.Year, x.Month, x.BranchId },
                        principalTable: "MonthYearDoctor",
                        principalColumns: new[] { "Year", "Month", "BranchId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Appointment",
                columns: table => new
                {
                    PrescriptionId = table.Column<long>(type: "bigint", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    HasDone = table.Column<bool>(type: "boolean", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AppointmentReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Advised = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DoctorName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointment", x => new { x.PrescriptionId, x.No });
                    table.ForeignKey(
                        name: "FK_Appointment_Doctor_DoctorName",
                        column: x => x.DoctorName,
                        principalTable: "Doctor",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointment_Prescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContainedDiscount",
                columns: table => new
                {
                    PrescriptionId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainedDiscount", x => new { x.PrescriptionId, x.ServiceId });
                    table.ForeignKey(
                        name: "FK_ContainedDiscount_Prescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContainedDiscount_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrescriptionId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    ReceptionistName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payment_Account_ReceptionistName",
                        column: x => x.ReceptionistName,
                        principalTable: "Account",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payment_Prescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContainedExpense",
                columns: table => new
                {
                    ExpenseId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainedExpense", x => new { x.ExpenseId, x.Description });
                    table.ForeignKey(
                        name: "FK_ContainedExpense_Expense_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expense",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContainedInvoice",
                columns: table => new
                {
                    InvoiceId = table.Column<int>(type: "integer", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainedInvoice", x => new { x.InvoiceId, x.DrugId });
                    table.ForeignKey(
                        name: "FK_ContainedInvoice_Drug_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContainedInvoice_Invoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContainedPicture",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Image = table.Column<byte[]>(type: "bytea", nullable: false),
                    PrescriptionId = table.Column<long>(type: "bigint", nullable: false),
                    AppointmentNo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainedPicture", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainedPicture_Appointment_PrescriptionId_AppointmentNo",
                        columns: x => new { x.PrescriptionId, x.AppointmentNo },
                        principalTable: "Appointment",
                        principalColumns: new[] { "PrescriptionId", "No" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContainedReceipt",
                columns: table => new
                {
                    PrescriptionId = table.Column<long>(type: "bigint", nullable: false),
                    AppointmentNo = table.Column<int>(type: "integer", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Instruction = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PerDay = table.Column<double>(type: "double precision", nullable: false),
                    PerTimes = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainedReceipt", x => new { x.PrescriptionId, x.AppointmentNo, x.DrugId });
                    table.ForeignKey(
                        name: "FK_ContainedReceipt_Appointment_PrescriptionId_AppointmentNo",
                        columns: x => new { x.PrescriptionId, x.AppointmentNo },
                        principalTable: "Appointment",
                        principalColumns: new[] { "PrescriptionId", "No" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContainedReceipt_Drug_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContainedTreatment",
                columns: table => new
                {
                    PrescriptionId = table.Column<long>(type: "bigint", nullable: false),
                    AppointmentNo = table.Column<int>(type: "integer", nullable: false),
                    SubId = table.Column<int>(type: "integer", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    LaboId = table.Column<int>(type: "integer", nullable: true),
                    OriginalPrice = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Paid = table.Column<long>(type: "bigint", nullable: false),
                    DoctorName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AssistantName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Comment = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    zQuantity = table.Column<int>(type: "integer", nullable: false),
                    ToothPositions = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PreviousDebt = table.Column<long>(type: "bigint", nullable: false),
                    IsCancelled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainedTreatment", x => new { x.PrescriptionId, x.AppointmentNo, x.SubId });
                    table.ForeignKey(
                        name: "FK_ContainedTreatment_Appointment_PrescriptionId_AppointmentNo",
                        columns: x => new { x.PrescriptionId, x.AppointmentNo },
                        principalTable: "Appointment",
                        principalColumns: new[] { "PrescriptionId", "No" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContainedTreatment_Assistant_AssistantName",
                        column: x => x.AssistantName,
                        principalTable: "Assistant",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContainedTreatment_Doctor_DoctorName",
                        column: x => x.DoctorName,
                        principalTable: "Doctor",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContainedTreatment_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContainedUsedEquipment",
                columns: table => new
                {
                    PrescriptionId = table.Column<long>(type: "bigint", nullable: false),
                    AppointmentNo = table.Column<int>(type: "integer", nullable: false),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainedUsedEquipment", x => new { x.PrescriptionId, x.AppointmentNo, x.EquipmentId });
                    table.ForeignKey(
                        name: "FK_ContainedUsedEquipment_Appointment_PrescriptionId_Appointme~",
                        columns: x => new { x.PrescriptionId, x.AppointmentNo },
                        principalTable: "Appointment",
                        principalColumns: new[] { "PrescriptionId", "No" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContainedUsedEquipment_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContainedLabo",
                columns: table => new
                {
                    PrescriptionId = table.Column<long>(type: "bigint", nullable: false),
                    AppointmentNo = table.Column<int>(type: "integer", nullable: false),
                    SubId = table.Column<int>(type: "integer", nullable: false),
                    LaboVendorId = table.Column<int>(type: "integer", nullable: false),
                    LaboTypeId = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReceiverDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeliverDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Delivered = table.Column<bool>(type: "boolean", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainedLabo", x => new { x.PrescriptionId, x.AppointmentNo, x.SubId });
                    table.ForeignKey(
                        name: "FK_ContainedLabo_ContainedTreatment_PrescriptionId_Appointment~",
                        columns: x => new { x.PrescriptionId, x.AppointmentNo, x.SubId },
                        principalTable: "ContainedTreatment",
                        principalColumns: new[] { "PrescriptionId", "AppointmentNo", "SubId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContainedLabo_LaboType_LaboTypeId",
                        column: x => x.LaboTypeId,
                        principalTable: "LaboType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContainedLabo_LaboVendor_LaboVendorId",
                        column: x => x.LaboVendorId,
                        principalTable: "LaboVendor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_BranchId",
                table: "Account",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Account_EmployeeUserName",
                table: "Account",
                column: "EmployeeUserName");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_DoctorName",
                table: "Appointment",
                column: "DoctorName");

            migrationBuilder.CreateIndex(
                name: "IX_ContainedConsulting_ServiceId",
                table: "ContainedConsulting",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainedConsultingLabo_LaboTypeId",
                table: "ContainedConsultingLabo",
                column: "LaboTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainedConsultingLabo_LaboVendorId",
                table: "ContainedConsultingLabo",
                column: "LaboVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainedDiscount_ServiceId",
                table: "ContainedDiscount",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainedEquipmentExportInvoice_EquipmentId",
                table: "ContainedEquipmentExportInvoice",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainedEquipmentInvoice_EquipmentId",
                table: "ContainedEquipmentInvoice",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainedInvoice_DrugId",
                table: "ContainedInvoice",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainedLabo_LaboTypeId",
                table: "ContainedLabo",
                column: "LaboTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainedLabo_LaboVendorId",
                table: "ContainedLabo",
                column: "LaboVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainedPicture_PrescriptionId_AppointmentNo",
                table: "ContainedPicture",
                columns: new[] { "PrescriptionId", "AppointmentNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ContainedReceipt_DrugId",
                table: "ContainedReceipt",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainedTreatment_AssistantName",
                table: "ContainedTreatment",
                column: "AssistantName");

            migrationBuilder.CreateIndex(
                name: "IX_ContainedTreatment_DoctorName",
                table: "ContainedTreatment",
                column: "DoctorName");

            migrationBuilder.CreateIndex(
                name: "IX_ContainedTreatment_ServiceId",
                table: "ContainedTreatment",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainedUsedEquipment_EquipmentId",
                table: "ContainedUsedEquipment",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainedUsedReceipt_DrugId",
                table: "ContainedUsedReceipt",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_BranchId",
                table: "Customer",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSalary_UserName",
                table: "DoctorSalary",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_DrugInBranch_DrugId",
                table: "DrugInBranch",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_BranchId",
                table: "Employee",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_EmployeeTypeId",
                table: "Employee",
                column: "EmployeeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EquimentInBranch_EquipmentId",
                table: "EquimentInBranch",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentExportInvoice_BranchId",
                table: "EquipmentExportInvoice",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentInvoice_BranchId",
                table: "EquipmentInvoice",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Expense_AccountantName",
                table: "Expense",
                column: "AccountantName");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_BranchId",
                table: "Invoice",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_UserName",
                table: "Invoice",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_Membership_GroupId",
                table: "Membership",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberShipGroup_BranchId",
                table: "MemberShipGroup",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthYear_BranchId",
                table: "MonthYear",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthYearDoctor_BranchId",
                table: "MonthYearDoctor",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PrescriptionId",
                table: "Payment",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_ReceptionistName",
                table: "Payment",
                column: "ReceptionistName");

            migrationBuilder.CreateIndex(
                name: "IX_Prescription_CustomerId",
                table: "Prescription",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescription_MembershipId",
                table: "Prescription",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_Salary_EmployeeName",
                table: "Salary",
                column: "EmployeeName");

            migrationBuilder.CreateIndex(
                name: "IX_Service_GroupId",
                table: "Service",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDiscount_ServiceId",
                table: "ServiceDiscount",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configuration");

            migrationBuilder.DropTable(
                name: "ContainedConsulting");

            migrationBuilder.DropTable(
                name: "ContainedConsultingLabo");

            migrationBuilder.DropTable(
                name: "ContainedDiscount");

            migrationBuilder.DropTable(
                name: "ContainedEquipmentExportInvoice");

            migrationBuilder.DropTable(
                name: "ContainedEquipmentInvoice");

            migrationBuilder.DropTable(
                name: "ContainedExpense");

            migrationBuilder.DropTable(
                name: "ContainedInvoice");

            migrationBuilder.DropTable(
                name: "ContainedLabo");

            migrationBuilder.DropTable(
                name: "ContainedPicture");

            migrationBuilder.DropTable(
                name: "ContainedReceipt");

            migrationBuilder.DropTable(
                name: "ContainedUsedEquipment");

            migrationBuilder.DropTable(
                name: "ContainedUsedReceipt");

            migrationBuilder.DropTable(
                name: "CustomerMoreInformation");

            migrationBuilder.DropTable(
                name: "District");

            migrationBuilder.DropTable(
                name: "DoctorSalary");

            migrationBuilder.DropTable(
                name: "DrugInBranch");

            migrationBuilder.DropTable(
                name: "EquimentInBranch");

            migrationBuilder.DropTable(
                name: "OtherEmployee");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "Province");

            migrationBuilder.DropTable(
                name: "Receptionist");

            migrationBuilder.DropTable(
                name: "Salary");

            migrationBuilder.DropTable(
                name: "ServiceDiscount");

            migrationBuilder.DropTable(
                name: "Ward");

            migrationBuilder.DropTable(
                name: "YesNoQuestion");

            migrationBuilder.DropTable(
                name: "EquipmentExportInvoice");

            migrationBuilder.DropTable(
                name: "EquipmentInvoice");

            migrationBuilder.DropTable(
                name: "Expense");

            migrationBuilder.DropTable(
                name: "Invoice");

            migrationBuilder.DropTable(
                name: "ContainedTreatment");

            migrationBuilder.DropTable(
                name: "LaboType");

            migrationBuilder.DropTable(
                name: "LaboVendor");

            migrationBuilder.DropTable(
                name: "UsedReceipt");

            migrationBuilder.DropTable(
                name: "MonthYearDoctor");

            migrationBuilder.DropTable(
                name: "Drug");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "MonthYear");

            migrationBuilder.DropTable(
                name: "Account");

            migrationBuilder.DropTable(
                name: "Appointment");

            migrationBuilder.DropTable(
                name: "Assistant");

            migrationBuilder.DropTable(
                name: "Service");

            migrationBuilder.DropTable(
                name: "Doctor");

            migrationBuilder.DropTable(
                name: "Prescription");

            migrationBuilder.DropTable(
                name: "ServiceGroup");

            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "Membership");

            migrationBuilder.DropTable(
                name: "EmployeeType");

            migrationBuilder.DropTable(
                name: "MemberShipGroup");

            migrationBuilder.DropTable(
                name: "Branch");
        }
    }
}
