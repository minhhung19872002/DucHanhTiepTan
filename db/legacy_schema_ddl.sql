USE [TXL_DM_BIENHOA]
GO
/****** Object:  Table [dbo].[Account]    Script Date: 10/1/2013 9:19:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Account](
	[UserName] [nvarchar](50) NOT NULL,
	[IsAdministrator] [bit] NOT NULL,
	[Password] [nvarchar](150) NOT NULL,
	[BranchId] [int] NOT NULL,
	[FullName] [nvarchar](50) NULL,
	[EmployeeUserName] [nvarchar](50) NULL,
 CONSTRAINT [PK__Account__C9F28457F946ED31] PRIMARY KEY CLUSTERED 
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Appointment]    Script Date: 10/1/2013 9:19:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Appointment](
	[PrescriptionId] [bigint] NOT NULL,
	[No] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[HasDone] [bit] NOT NULL,
	[AppointmentDate] [datetime] NULL,
	[AppointmentReason] [nvarchar](250) NULL,
	[Advised] [nvarchar](250) NULL,
	[DoctorName] [nvarchar](50) NULL,
 CONSTRAINT [PK_Appointment] PRIMARY KEY CLUSTERED 
(
	[PrescriptionId] ASC,
	[No] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Assistant]    Script Date: 10/1/2013 9:19:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Assistant](
	[UserName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Assistant] PRIMARY KEY CLUSTERED 
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Branch]    Script Date: 10/1/2013 9:19:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Branch](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Address] [nvarchar](150) NULL,
	[Tel] [nvarchar](50) NULL,
 CONSTRAINT [PK_Branch] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Configuration]    Script Date: 10/1/2013 9:19:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Configuration](
	[Name] [varchar](50) NOT NULL,
	[Value] [nvarchar](150) NULL,
 CONSTRAINT [PK_Configuration] PRIMARY KEY CLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ContainedConsulting]    Script Date: 10/1/2013 9:19:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ContainedConsulting](
	[PrescriptionId] [bigint] NOT NULL,
	[SubId] [int] NOT NULL,
	[ServiceId] [int] NOT NULL,
	[OriginalPrice] [bigint] NOT NULL,
	[Price] [bigint] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[Comment] [nvarchar](250) NULL,
	[zQuantity] [int] NOT NULL,
	[ToothPositions] [varchar](150) NULL,
 CONSTRAINT [PK_ContainedConsulting_1] PRIMARY KEY CLUSTERED 
(
	[PrescriptionId] ASC,
	[SubId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ContainedConsultingLabo]    Script Date: 10/1/2013 9:19:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContainedConsultingLabo](
	[PrescriptionId] [bigint] NOT NULL,
	[SubId] [int] NOT NULL,
	[LaboVendorId] [int] NOT NULL,
	[LaboTypeId] [int] NOT NULL,
	[Color] [nvarchar](50) NULL,
	[ReceiverDate] [datetime] NOT NULL,
	[DeliverDate] [datetime] NOT NULL,
	[Delivered] [bit] NOT NULL,
	[Price] [bigint] NOT NULL,
 CONSTRAINT [PK_ContainedConsultingLabo] PRIMARY KEY CLUSTERED 
(
	[PrescriptionId] ASC,
	[SubId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ContainedDiscount]    Script Date: 10/1/2013 9:19:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContainedDiscount](
	[PrescriptionId] [bigint] NOT NULL,
	[ServiceId] [int] NOT NULL,
	[Value] [float] NOT NULL,
 CONSTRAINT [PK__ServiceD__FC42B33267E737D4] PRIMARY KEY CLUSTERED 
(
	[PrescriptionId] ASC,
	[ServiceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ContainedEquipmentExportInvoice]    Script Date: 10/1/2013 9:19:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContainedEquipmentExportInvoice](
	[EquipmentExportInvoiceId] [int] NOT NULL,
	[EquipmentId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
 CONSTRAINT [PK_ContainedEquipmentExportInvoice] PRIMARY KEY CLUSTERED 
(
	[EquipmentExportInvoiceId] ASC,
	[EquipmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ContainedEquipmentInvoice]    Script Date: 10/1/2013 9:19:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContainedEquipmentInvoice](
	[InvoiceId] [int] NOT NULL,
	[EquipmentId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[Price] [bigint] NOT NULL,
 CONSTRAINT [PK_ContainedEquipmentInvoice] PRIMARY KEY CLUSTERED 
(
	[InvoiceId] ASC,
	[EquipmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ContainedExpense]    Script Date: 10/1/2013 9:19:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContainedExpense](
	[ExpenseId] [int] NOT NULL,
	[Description] [nvarchar](150) NOT NULL,
	[Amount] [bigint] NOT NULL,
 CONSTRAINT [PK_ContainedExpense] PRIMARY KEY CLUSTERED 
(
	[ExpenseId] ASC,
	[Description] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ContainedInvoice]    Script Date: 10/1/2013 9:19:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContainedInvoice](
	[InvoiceId] [int] NOT NULL,
	[DrugId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[Price] [bigint] NOT NULL,
 CONSTRAINT [PK_ContainedInvoice] PRIMARY KEY CLUSTERED 
(
	[InvoiceId] ASC,
	[DrugId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ContainedLabo]    Script Date: 10/1/2013 9:19:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContainedLabo](
	[PrescriptionId] [bigint] NOT NULL,
	[AppointmentNo] [int] NOT NULL,
	[SubId] [int] NOT NULL,
	[LaboVendorId] [int] NOT NULL,
	[LaboTypeId] [int] NOT NULL,
	[Color] [nvarchar](50) NULL,
	[ReceiverDate] [datetime] NOT NULL,
	[DeliverDate] [datetime] NOT NULL,
	[Delivered] [bit] NOT NULL,
	[Price] [bigint] NOT NULL,
 CONSTRAINT [PK_ContainedLabo] PRIMARY KEY CLUSTERED 
(
	[PrescriptionId] ASC,
	[AppointmentNo] ASC,
	[SubId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ContainedPicture]    Script Date: 10/1/2013 9:19:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContainedPicture](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Image] [image] NOT NULL,
	[PrescriptionId] [bigint] NOT NULL,
	[AppointmentNo] [int] NOT NULL,
 CONSTRAINT [PK_ContainedPicture] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ContainedReceipt]    Script Date: 10/1/2013 9:19:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContainedReceipt](
	[PrescriptionId] [bigint] NOT NULL,
	[AppointmentNo] [int] NOT NULL,
	[DrugId] [int] NOT NULL,
	[Amount] [int] NOT NULL,
	[Instruction] [nvarchar](50) NULL,
	[PerDay] [float] NOT NULL,
	[PerTimes] [float] NOT NULL,
 CONSTRAINT [PK_ContainedReceipt] PRIMARY KEY CLUSTERED 
(
	[PrescriptionId] ASC,
	[AppointmentNo] ASC,
	[DrugId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ContainedTreatment]    Script Date: 10/1/2013 9:19:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ContainedTreatment](
	[PrescriptionId] [bigint] NOT NULL,
	[AppointmentNo] [int] NOT NULL,
	[SubId] [int] NOT NULL,
	[ServiceId] [int] NOT NULL,
	[LaboId] [int] NULL,
	[OriginalPrice] [bigint] NOT NULL,
	[Price] [bigint] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[Paid] [bigint] NOT NULL,
	[DoctorName] [nvarchar](50) NULL,
	[AssistantName] [nvarchar](50) NULL,
	[Comment] [nvarchar](250) NULL,
	[zQuantity] [int] NOT NULL,
	[ToothPositions] [varchar](150) NULL,
	[PreviousDebt] [bigint] NOT NULL,
	[IsCancelled] [bit] NOT NULL,
 CONSTRAINT [PK_ContainedTreatment] PRIMARY KEY CLUSTERED 
(
	[PrescriptionId] ASC,
	[AppointmentNo] ASC,
	[SubId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ContainedUsedEquipment]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContainedUsedEquipment](
	[PrescriptionId] [bigint] NOT NULL,
	[AppointmentNo] [int] NOT NULL,
	[EquipmentId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
 CONSTRAINT [PK_ContainedUsedEquipment] PRIMARY KEY CLUSTERED 
(
	[PrescriptionId] ASC,
	[AppointmentNo] ASC,
	[EquipmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ContainedUsedReceipt]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContainedUsedReceipt](
	[PrescriptionId] [bigint] NOT NULL,
	[AppointmentNo] [int] NOT NULL,
	[DrugId] [int] NOT NULL,
	[Price] [bigint] NOT NULL,
	[Quantity] [int] NOT NULL,
 CONSTRAINT [PK_ContainedUsedReceipt] PRIMARY KEY CLUSTERED 
(
	[PrescriptionId] ASC,
	[AppointmentNo] ASC,
	[DrugId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Customer]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Customer](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Sex] [int] NOT NULL,
	[BirthYear] [int] NOT NULL,
	[Address] [nvarchar](250) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[Phone] [varchar](50) NULL,
	[Email] [nvarchar](50) NULL,
	[Job] [nvarchar](50) NULL,
	[Image] [image] NULL,
	[BranchId] [int] NOT NULL,
	[ProvinceName] [nvarchar](50) NULL,
	[DistrictName] [nvarchar](50) NULL,
	[WardName] [nvarchar](50) NULL,
	[VisiableId] [bigint] NOT NULL,
 CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CustomerMoreInformation]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerMoreInformation](
	[WhyYouSeeMe] [int] NOT NULL,
	[TheReasonYouSeeMe] [nvarchar](150) NULL,
	[SubcribeByPhone] [bit] NOT NULL,
	[SubcribeByEmail] [bit] NOT NULL,
	[SubcribeByMail] [bit] NOT NULL,
	[PhoneToSubcribe] [nvarchar](50) NULL,
	[EmailToSubcribe] [nvarchar](50) NULL,
	[MailToSubcribe] [nvarchar](150) NULL,
	[CustomerId] [bigint] NOT NULL,
	[YesQuestions] [int] NOT NULL,
 CONSTRAINT [PK_CustomerMoreInformation] PRIMARY KEY CLUSTERED 
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Doctor]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Doctor](
	[UserName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Doctor] PRIMARY KEY CLUSTERED 
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DoctorSalary]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DoctorSalary](
	[Year] [int] NOT NULL,
	[Month] [int] NOT NULL,
	[BranchId] [int] NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[BaseSalary] [bigint] NOT NULL,
	[LunchSubsidy] [bigint] NOT NULL,
	[RespondSubsidy] [bigint] NOT NULL,
	[Advance] [bigint] NOT NULL,
 CONSTRAINT [PK_DoctorSalary] PRIMARY KEY CLUSTERED 
(
	[Year] ASC,
	[Month] ASC,
	[BranchId] ASC,
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Drug]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Drug](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[Unit] [nvarchar](50) NOT NULL,
	[Price] [bigint] NOT NULL,
	[Description] [nvarchar](250) NULL,
	[OutPrice] [bigint] NOT NULL,
 CONSTRAINT [PK_Drug] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DrugInBranch]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DrugInBranch](
	[BranchId] [int] NOT NULL,
	[DrugId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
 CONSTRAINT [PK_DrugInBranch] PRIMARY KEY CLUSTERED 
(
	[BranchId] ASC,
	[DrugId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Employee]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Employee](
	[UserName] [nvarchar](50) NOT NULL,
	[EmployeeTypeId] [int] NOT NULL,
	[DefaultSalary] [bigint] NOT NULL,
	[DefaultRatio] [float] NULL,
	[BranchId] [int] NOT NULL,
	[IsValid] [bit] NOT NULL,
	[JoinDate] [date] NOT NULL,
	[FullName] [nvarchar](150) NOT NULL,
	[Address] [nvarchar](250) NULL,
	[Sex] [int] NOT NULL,
	[Phone] [varchar](50) NULL,
	[Email] [varchar](50) NULL,
	[Rank] [varchar](50) NULL,
	[OtherEmployeeTypeName] [nvarchar](50) NULL,
	[BaseLunchSubsidy] [bigint] NOT NULL,
	[BasePhoneSubsidy] [bigint] NOT NULL,
	[BaseToxicSubsidy] [bigint] NOT NULL,
 CONSTRAINT [PK_Employee_1] PRIMARY KEY CLUSTERED 
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[EmployeeType]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeType](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_EmployeeType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[EquimentInBranch]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EquimentInBranch](
	[BranchId] [int] NOT NULL,
	[EquipmentId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
 CONSTRAINT [PK_EquimentInBranch] PRIMARY KEY CLUSTERED 
(
	[BranchId] ASC,
	[EquipmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Equipment]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Equipment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Serial] [nvarchar](50) NOT NULL,
	[Position] [nvarchar](50) NULL,
	[Usage] [nvarchar](150) NULL,
	[Note] [nvarchar](150) NULL,
	[Vendor] [nvarchar](50) NOT NULL,
	[IsVarious] [bit] NOT NULL,
	[Price] [bigint] NOT NULL,
	[Unit] [nvarchar](50) NULL,
 CONSTRAINT [PK_Equipment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[EquipmentExportInvoice]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EquipmentExportInvoice](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[BranchId] [int] NOT NULL,
 CONSTRAINT [PK_EquipmentExportInvoice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[EquipmentInvoice]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EquipmentInvoice](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[BranchId] [int] NOT NULL,
 CONSTRAINT [PK_EquipmentInvoice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Expense]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Expense](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[AccountantName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Expense] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Invoice]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Invoice](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[BranchId] [int] NOT NULL,
 CONSTRAINT [PK_Invoice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LaboType]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LaboType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[Description] [nvarchar](250) NULL,
	[Disassembly] [bit] NOT NULL,
 CONSTRAINT [PK_Labo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LaboVendor]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LaboVendor](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[Address] [nvarchar](150) NULL,
	[Tel] [varchar](50) NULL,
	[Description] [nvarchar](250) NULL,
 CONSTRAINT [PK_LaboVendor] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Membership]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Membership](
	[Id] [nvarchar](30) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ExpiredDate] [datetime] NULL,
	[Description] [nvarchar](250) NULL,
	[GroupId] [int] NOT NULL,
 CONSTRAINT [PK_Membership] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MemberShipGroup]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MemberShipGroup](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[BranchId] [int] NOT NULL,
	[Description] [nvarchar](150) NULL,
	[IsFixed] [bit] NOT NULL,
	[FixedValue] [float] NOT NULL,
 CONSTRAINT [PK_MemberShipGroup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MonthYear]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MonthYear](
	[Month] [int] NOT NULL,
	[Year] [int] NOT NULL,
	[BranchId] [int] NOT NULL,
	[WorkDaysInMonth] [int] NOT NULL,
	[BaseLunchSubsidy] [bigint] NOT NULL,
	[BasePhoneSubsidy] [bigint] NOT NULL,
	[BaseToxicSubsidy] [bigint] NOT NULL,
	[BaseSalary] [bigint] NOT NULL,
 CONSTRAINT [PK_MonthYear_1] PRIMARY KEY CLUSTERED 
(
	[Month] ASC,
	[Year] ASC,
	[BranchId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MonthYearDoctor]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MonthYearDoctor](
	[Year] [int] NOT NULL,
	[Month] [int] NOT NULL,
	[BranchId] [int] NOT NULL,
 CONSTRAINT [PK_MonthYearDoctor] PRIMARY KEY CLUSTERED 
(
	[Year] ASC,
	[Month] ASC,
	[BranchId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[OtherEmployee]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OtherEmployee](
	[UserName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_OtherEmployee] PRIMARY KEY CLUSTERED 
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Payment]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payment](
	[PrescriptionId] [bigint] NOT NULL,
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[Amount] [bigint] NOT NULL,
	[ReceptionistName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Payment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Prescription]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Prescription](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomerId] [bigint] NOT NULL,
	[HasDone] [bit] NOT NULL,
	[Treatment] [nvarchar](250) NULL,
	[Diagnostic] [nvarchar](250) NULL,
	[Symptoms] [nvarchar](250) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[MembershipId] [nvarchar](30) NULL,
	[DiscountValue] [float] NOT NULL,
 CONSTRAINT [PK_Prescription] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Receptionist]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Receptionist](
	[UserName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Receptionist] PRIMARY KEY CLUSTERED 
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Salary]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Salary](
	[Month] [int] NOT NULL,
	[Year] [int] NOT NULL,
	[BranchId] [int] NOT NULL,
	[EmployeeName] [nvarchar](50) NOT NULL,
	[Rank] [nvarchar](50) NULL,
	[Ratio] [float] NOT NULL,
	[WorkDays] [int] NOT NULL,
	[OvertimeInMinute] [int] NOT NULL,
	[DegreeAllowance] [bigint] NOT NULL,
	[HeathAllowance] [bigint] NOT NULL,
	[DayOffAllowance] [bigint] NOT NULL,
	[RiskAllowance] [bigint] NOT NULL,
	[RetireAllowance] [bigint] NOT NULL,
	[Advance] [bigint] NOT NULL,
	[BasePhoneSubsidy] [bigint] NOT NULL,
	[BaseToxicSubsidy] [bigint] NOT NULL,
	[BaseLunchSubsidy] [bigint] NOT NULL,
	[TowHoursOvertimeDays] [int] NOT NULL,
 CONSTRAINT [PK_MonthYear] PRIMARY KEY CLUSTERED 
(
	[Month] ASC,
	[Year] ASC,
	[BranchId] ASC,
	[EmployeeName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Service]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Service](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[Price] [bigint] NOT NULL,
	[Description] [nvarchar](250) NULL,
	[GroupId] [int] NOT NULL,
 CONSTRAINT [PK_Service] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ServiceDiscount]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ServiceDiscount](
	[MemberShipGroupId] [int] NOT NULL,
	[ServiceId] [int] NOT NULL,
	[Value] [float] NOT NULL,
 CONSTRAINT [PK_ServiceDiscount] PRIMARY KEY CLUSTERED 
(
	[MemberShipGroupId] ASC,
	[ServiceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ServiceGroup]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ServiceGroup](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](150) NULL,
 CONSTRAINT [PK_ServiceGroup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UsedReceipt]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UsedReceipt](
	[PrescriptionId] [bigint] NOT NULL,
	[AppointmentNo] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_UsedReceipt] PRIMARY KEY CLUSTERED 
(
	[PrescriptionId] ASC,
	[AppointmentNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[YesNoQuestion]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[YesNoQuestion](
	[Id] [int] NOT NULL,
	[Header] [nvarchar](150) NOT NULL,
 CONSTRAINT [PK_YesNoQuestion] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  View [dbo].[TreatmentAdapter]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[TreatmentAdapter]
AS
SELECT        t0.PrescriptionId, t0.AppointmentNo, t0.SubId, t0.ServiceId, t0.LaboId, t0.OriginalPrice, t0.Price, t0.CreatedDate, t0.Paid, t0.DoctorName, t0.AssistantName, 
                         t0.Comment, t0.zQuantity, t0.ToothPositions, t0.PreviousDebt, t1.HasDone, t0.IsCancelled, 
                         (CASE WHEN [t0].[IsCancelled] = 1 THEN [t0].[PreviousDebt] - [t0].[Paid] ELSE ([t0].[PreviousDebt] + ((CONVERT(BigInt, [t0].[zQuantity])) * [t0].[Price])) 
                         - [t0].[Paid] END) AS Remain
FROM            dbo.ContainedTreatment AS t0 INNER JOIN
                         dbo.Appointment AS t1 ON t0.PrescriptionId = t1.PrescriptionId AND t0.AppointmentNo = t1.No

GO
/****** Object:  View [dbo].[PrescriptionAdapter]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[PrescriptionAdapter]
AS
SELECT        t2.Id AS BranchId, t2.Name AS BranchName, t0.Id, t0.CustomerId, t1.Name AS CustomerName, t1.Phone AS CustomerPhone, t0.HasDone, t0.Treatment, 
                         t0.Diagnostic, t0.Symptoms, t0.CreatedDate, t3.CreatedDate AS LastAppointmentDate, t3.AppointmentDate, t3.AppointmentReason, 
                         t3.Advised AS AppointmentAdvised, ISNULL
                             ((SELECT        COUNT(*) AS Expr1
                                 FROM            dbo.Appointment AS t5
                                 WHERE        (HasDone = 1) AND (PrescriptionId = t0.Id)), 0) AS AppointmentNo, ISNULL
                             ((SELECT        SUM(value) AS Expr1
                                 FROM            (SELECT        Paid AS value, PrescriptionId, HasDone
                                                           FROM            dbo.TreatmentAdapter AS t6) AS t7
                                 WHERE        (PrescriptionId = t0.Id) AND (HasDone = 1)), 0) AS Paid, ISNULL
                             ((SELECT        Remain
                                 FROM            dbo.TreatmentAdapter AS t8
                                 WHERE        (PrescriptionId = t0.Id) AND (SubId =
                                                              (SELECT        MAX(SubId) AS Expr1
                                                                FROM            dbo.TreatmentAdapter AS t9
                                                                WHERE        (PrescriptionId = t0.Id) AND (HasDone = 1)))), 0) AS Remain, t1.VisiableId AS CustomerVisiableId
FROM            dbo.Prescription AS t0 INNER JOIN
                         dbo.Customer AS t1 ON t0.CustomerId = t1.Id INNER JOIN
                         dbo.Branch AS t2 ON t1.BranchId = t2.Id INNER JOIN
                         dbo.Appointment AS t3 ON t0.Id = t3.PrescriptionId AND
                             (SELECT        MAX(No) AS Expr1
                               FROM            dbo.Appointment AS t4
                               WHERE        (PrescriptionId = t0.Id)) = t3.No

GO
/****** Object:  View [dbo].[AppointmentAdapter]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[AppointmentAdapter]
AS
SELECT        t3.Id AS BranchId, t3.Name AS BranchName, t0.PrescriptionId, t0.No, t0.CreatedDate, t0.HasDone, t0.AppointmentDate, t0.AppointmentReason, t0.Advised, 
                         t2.Name AS CustomerName, t2.Phone AS CustomerPhone, t2.Id AS CustomerId, t5.FullName AS DoctorFullName,
                             (SELECT        No
                               FROM            dbo.Appointment AS t6
                               WHERE        (PrescriptionId = t0.PrescriptionId) AND (No = t0.No + 1)) AS NextAppointmentNo, ISNULL
                             ((SELECT        SUM(value) AS Expr1
                                 FROM            (SELECT        Paid AS value, PrescriptionId, AppointmentNo
                                                           FROM            dbo.ContainedTreatment AS t7) AS t8
                                 WHERE        (PrescriptionId = t0.PrescriptionId) AND (AppointmentNo = t0.No)), 0) AS Paid, t0.DoctorName, t2.VisiableId AS CustomerVisiableId
FROM            dbo.Appointment AS t0 INNER JOIN
                         dbo.Prescription AS t1 ON t0.PrescriptionId = t1.Id INNER JOIN
                         dbo.Customer AS t2 ON t1.CustomerId = t2.Id INNER JOIN
                         dbo.Branch AS t3 ON t2.BranchId = t3.Id LEFT OUTER JOIN
                         dbo.Doctor AS t4 ON t4.UserName = t0.DoctorName LEFT OUTER JOIN
                         dbo.Employee AS t5 ON t5.UserName = t4.UserName

GO
/****** Object:  View [dbo].[EquipmentInvoiceView]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[EquipmentInvoiceView]
AS
SELECT        dbo.EquipmentInvoice.BranchId, dbo.Branch.Name AS BranchName, dbo.EquipmentInvoice.Id, dbo.EquipmentInvoice.CreatedDate, ISNULL
                             ((SELECT        COUNT(*) AS Expr1
                                 FROM            dbo.ContainedEquipmentInvoice
                                 WHERE        (InvoiceId = dbo.EquipmentInvoice.Id)), 0) AS Quantity, ISNULL
                             ((SELECT        SUM(Quantity * Price) AS Expr1
                                 FROM            dbo.ContainedEquipmentInvoice AS ContainedInvoice_1
                                 WHERE        (InvoiceId = dbo.EquipmentInvoice.Id)), 0) AS Total
FROM            dbo.EquipmentInvoice INNER JOIN
                         dbo.Branch ON dbo.Branch.Id = dbo.EquipmentInvoice.BranchId

GO
/****** Object:  View [dbo].[ExpenseView]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[ExpenseView]
AS
SELECT        dbo.Expense.Id, dbo.Expense.CreatedDate, dbo.Expense.AccountantName, dbo.Branch.Name AS BranchName, 
                         dbo.Account.FullName AS AccountantFullName, ISNULL
                             ((SELECT        SUM(Amount) AS Expr1
                                 FROM            dbo.ContainedExpense
                                 WHERE        (ExpenseId = dbo.Expense.Id)), 0) AS Total, dbo.Account.BranchId
FROM            dbo.Expense INNER JOIN
                         dbo.Account ON dbo.Expense.AccountantName = dbo.Account.UserName INNER JOIN
                         dbo.Branch ON dbo.Account.BranchId = dbo.Branch.Id

GO
/****** Object:  View [dbo].[ExportInvoiceView]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[ExportInvoiceView]
AS
SELECT        TOP (1000) t0.Id, t0.CreatedDate, t0.BranchId, ISNULL
                             ((SELECT        COUNT(*) AS Expr1
                                 FROM            dbo.ContainedEquipmentExportInvoice
                                 WHERE        (EquipmentExportInvoiceId = t0.Id)), 0) AS Quantity, dbo.Branch.Name AS BranchName
FROM            dbo.EquipmentExportInvoice AS t0 INNER JOIN
                         dbo.Branch ON t0.BranchId = dbo.Branch.Id

GO
/****** Object:  View [dbo].[InvoiceView]    Script Date: 10/1/2013 9:19:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[InvoiceView]
AS
SELECT        dbo.Branch.Id AS BranchId, dbo.Branch.Name AS BranchName, dbo.Invoice.Id, dbo.Invoice.CreatedDate, dbo.Invoice.UserName, dbo.Account.FullName, 
                         ISNULL
                             ((SELECT        COUNT(*) AS Expr1
                                 FROM            dbo.ContainedInvoice
                                 WHERE        (InvoiceId = dbo.Invoice.Id)), 0) AS Quantity, ISNULL
                             ((SELECT        SUM(Quantity * Price) AS Expr1
                                 FROM            dbo.ContainedInvoice AS ContainedInvoice_1
                                 WHERE        (InvoiceId = dbo.Invoice.Id)), 0) AS Total
FROM            dbo.Invoice INNER JOIN
                         dbo.Account ON dbo.Account.UserName = dbo.Invoice.UserName INNER JOIN
                         dbo.Branch ON dbo.Branch.Id = dbo.Invoice.BranchId

GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO

GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO

GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO

GO

GO

GO
GO
GO

GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO

GO
GO
GO
GO
ENDO 4R
', N'', N'', CAST(0x0000A223013AB51A AS DateTime), NULL, 0)
', N'', N'Mất men', CAST(0x0000A20E00F5E1DD AS DateTime), NULL, 0)
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO
GO

GO

ALTER TABLE [dbo].[ContainedTreatment] ADD  CONSTRAINT [DF_ContainedTreatment_IsCancelled]  DEFAULT ((0)) FOR [IsCancelled]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_VisiableId]  DEFAULT ((0)) FOR [VisiableId]
GO
ALTER TABLE [dbo].[CustomerMoreInformation] ADD  CONSTRAINT [DF_CustomerMoreInformation_WhyYouSeeMe]  DEFAULT ((0)) FOR [WhyYouSeeMe]
GO
ALTER TABLE [dbo].[CustomerMoreInformation] ADD  CONSTRAINT [DF_CustomerMoreInformation_SubcribeByPhone]  DEFAULT ((0)) FOR [SubcribeByPhone]
GO
ALTER TABLE [dbo].[CustomerMoreInformation] ADD  CONSTRAINT [DF_CustomerMoreInformation_SubcribeByEmail]  DEFAULT ((0)) FOR [SubcribeByEmail]
GO
ALTER TABLE [dbo].[CustomerMoreInformation] ADD  CONSTRAINT [DF_CustomerMoreInformation_SubcribeByMail]  DEFAULT ((0)) FOR [SubcribeByMail]
GO
ALTER TABLE [dbo].[CustomerMoreInformation] ADD  CONSTRAINT [DF_CustomerMoreInformation_YesQuestions]  DEFAULT ((0)) FOR [YesQuestions]
GO
ALTER TABLE [dbo].[Employee] ADD  CONSTRAINT [DF_Employee_DefaultSalary]  DEFAULT ((0)) FOR [DefaultSalary]
GO
ALTER TABLE [dbo].[Employee] ADD  CONSTRAINT [DF_Employee_DefaultRatio]  DEFAULT ((0)) FOR [DefaultRatio]
GO
ALTER TABLE [dbo].[Employee] ADD  CONSTRAINT [DF_Employee_BaseLunchSubsidy]  DEFAULT ((0)) FOR [BaseLunchSubsidy]
GO
ALTER TABLE [dbo].[Employee] ADD  CONSTRAINT [DF_Employee_BasePhoneSubsidy]  DEFAULT ((0)) FOR [BasePhoneSubsidy]
GO
ALTER TABLE [dbo].[Employee] ADD  CONSTRAINT [DF_Employee_BaseToxicSubsidy]  DEFAULT ((0)) FOR [BaseToxicSubsidy]
GO
ALTER TABLE [dbo].[MonthYear] ADD  CONSTRAINT [DF_MonthYear_BranchId]  DEFAULT ((10)) FOR [BranchId]
GO
ALTER TABLE [dbo].[Prescription] ADD  CONSTRAINT [DF_Prescription_ValueDiscount]  DEFAULT ((0)) FOR [DiscountValue]
GO
ALTER TABLE [dbo].[Salary] ADD  CONSTRAINT [DF_Salary_BasePhoneSubsidy]  DEFAULT ((0)) FOR [BasePhoneSubsidy]
GO
ALTER TABLE [dbo].[Salary] ADD  CONSTRAINT [DF_Salary_BaseToxicSubsidy]  DEFAULT ((0)) FOR [BaseToxicSubsidy]
GO
ALTER TABLE [dbo].[Salary] ADD  CONSTRAINT [DF_Salary_BaseLunchSubsidy]  DEFAULT ((0)) FOR [BaseLunchSubsidy]
GO
ALTER TABLE [dbo].[Salary] ADD  CONSTRAINT [DF_Salary_TowHoursOvertimeDays]  DEFAULT ((0)) FOR [TowHoursOvertimeDays]
GO
ALTER TABLE [dbo].[Account]  WITH CHECK ADD  CONSTRAINT [FK_Account_Branch] FOREIGN KEY([BranchId])
REFERENCES [dbo].[Branch] ([Id])
GO
ALTER TABLE [dbo].[Account] CHECK CONSTRAINT [FK_Account_Branch]
GO
ALTER TABLE [dbo].[Account]  WITH CHECK ADD  CONSTRAINT [FK_Account_Employee] FOREIGN KEY([EmployeeUserName])
REFERENCES [dbo].[Employee] ([UserName])
GO
ALTER TABLE [dbo].[Account] CHECK CONSTRAINT [FK_Account_Employee]
GO
ALTER TABLE [dbo].[Appointment]  WITH CHECK ADD  CONSTRAINT [FK_Appointment_Doctor] FOREIGN KEY([DoctorName])
REFERENCES [dbo].[Doctor] ([UserName])
GO
ALTER TABLE [dbo].[Appointment] CHECK CONSTRAINT [FK_Appointment_Doctor]
GO
ALTER TABLE [dbo].[Appointment]  WITH CHECK ADD  CONSTRAINT [FK_Appointment_Prescription] FOREIGN KEY([PrescriptionId])
REFERENCES [dbo].[Prescription] ([Id])
GO
ALTER TABLE [dbo].[Appointment] CHECK CONSTRAINT [FK_Appointment_Prescription]
GO
ALTER TABLE [dbo].[Assistant]  WITH CHECK ADD  CONSTRAINT [FK__Assistant__UserN__1A34DF26] FOREIGN KEY([UserName])
REFERENCES [dbo].[Employee] ([UserName])
GO
ALTER TABLE [dbo].[Assistant] CHECK CONSTRAINT [FK__Assistant__UserN__1A34DF26]
GO
ALTER TABLE [dbo].[ContainedConsulting]  WITH CHECK ADD FOREIGN KEY([PrescriptionId])
REFERENCES [dbo].[Prescription] ([Id])
GO
ALTER TABLE [dbo].[ContainedConsulting]  WITH CHECK ADD  CONSTRAINT [FK_ContainedConsulting_Service] FOREIGN KEY([ServiceId])
REFERENCES [dbo].[Service] ([Id])
GO
ALTER TABLE [dbo].[ContainedConsulting] CHECK CONSTRAINT [FK_ContainedConsulting_Service]
GO
ALTER TABLE [dbo].[ContainedConsultingLabo]  WITH CHECK ADD FOREIGN KEY([PrescriptionId], [SubId])
REFERENCES [dbo].[ContainedConsulting] ([PrescriptionId], [SubId])
GO
ALTER TABLE [dbo].[ContainedConsultingLabo]  WITH CHECK ADD  CONSTRAINT [FK_ContainedConsultingLabo_LaboType] FOREIGN KEY([LaboTypeId])
REFERENCES [dbo].[LaboType] ([Id])
GO
ALTER TABLE [dbo].[ContainedConsultingLabo] CHECK CONSTRAINT [FK_ContainedConsultingLabo_LaboType]
GO
ALTER TABLE [dbo].[ContainedConsultingLabo]  WITH CHECK ADD  CONSTRAINT [FK_ContainedConsultingLabo_LaboVendor] FOREIGN KEY([LaboVendorId])
REFERENCES [dbo].[LaboVendor] ([Id])
GO
ALTER TABLE [dbo].[ContainedConsultingLabo] CHECK CONSTRAINT [FK_ContainedConsultingLabo_LaboVendor]
GO
ALTER TABLE [dbo].[ContainedDiscount]  WITH CHECK ADD  CONSTRAINT [FK_ContainedDiscount_Prescription] FOREIGN KEY([PrescriptionId])
REFERENCES [dbo].[Prescription] ([Id])
GO
ALTER TABLE [dbo].[ContainedDiscount] CHECK CONSTRAINT [FK_ContainedDiscount_Prescription]
GO
ALTER TABLE [dbo].[ContainedDiscount]  WITH CHECK ADD  CONSTRAINT [FK_ContainedDiscount_Service] FOREIGN KEY([ServiceId])
REFERENCES [dbo].[Service] ([Id])
GO
ALTER TABLE [dbo].[ContainedDiscount] CHECK CONSTRAINT [FK_ContainedDiscount_Service]
GO
ALTER TABLE [dbo].[ContainedEquipmentExportInvoice]  WITH CHECK ADD  CONSTRAINT [FK_ContainedEquipmentExportInvoice_Equipment] FOREIGN KEY([EquipmentId])
REFERENCES [dbo].[Equipment] ([Id])
GO
ALTER TABLE [dbo].[ContainedEquipmentExportInvoice] CHECK CONSTRAINT [FK_ContainedEquipmentExportInvoice_Equipment]
GO
ALTER TABLE [dbo].[ContainedEquipmentExportInvoice]  WITH CHECK ADD  CONSTRAINT [FK_ContainedEquipmentExportInvoice_EquipmentExportInvoice] FOREIGN KEY([EquipmentExportInvoiceId])
REFERENCES [dbo].[EquipmentExportInvoice] ([Id])
GO
ALTER TABLE [dbo].[ContainedEquipmentExportInvoice] CHECK CONSTRAINT [FK_ContainedEquipmentExportInvoice_EquipmentExportInvoice]
GO
ALTER TABLE [dbo].[ContainedEquipmentInvoice]  WITH CHECK ADD  CONSTRAINT [FK_ContainedEquipmentInvoice_Equipment] FOREIGN KEY([EquipmentId])
REFERENCES [dbo].[Equipment] ([Id])
GO
ALTER TABLE [dbo].[ContainedEquipmentInvoice] CHECK CONSTRAINT [FK_ContainedEquipmentInvoice_Equipment]
GO
ALTER TABLE [dbo].[ContainedEquipmentInvoice]  WITH CHECK ADD  CONSTRAINT [FK_ContainedEquipmentInvoice_EquipmentInvoice] FOREIGN KEY([InvoiceId])
REFERENCES [dbo].[EquipmentInvoice] ([Id])
GO
ALTER TABLE [dbo].[ContainedEquipmentInvoice] CHECK CONSTRAINT [FK_ContainedEquipmentInvoice_EquipmentInvoice]
GO
ALTER TABLE [dbo].[ContainedExpense]  WITH CHECK ADD  CONSTRAINT [FK_ContainedExpense_Expense] FOREIGN KEY([ExpenseId])
REFERENCES [dbo].[Expense] ([Id])
GO
ALTER TABLE [dbo].[ContainedExpense] CHECK CONSTRAINT [FK_ContainedExpense_Expense]
GO
ALTER TABLE [dbo].[ContainedInvoice]  WITH CHECK ADD  CONSTRAINT [FK_ContainedInvoice_Drug] FOREIGN KEY([DrugId])
REFERENCES [dbo].[Drug] ([Id])
GO
ALTER TABLE [dbo].[ContainedInvoice] CHECK CONSTRAINT [FK_ContainedInvoice_Drug]
GO
ALTER TABLE [dbo].[ContainedInvoice]  WITH CHECK ADD  CONSTRAINT [FK_ContainedInvoice_Invoice] FOREIGN KEY([InvoiceId])
REFERENCES [dbo].[Invoice] ([Id])
GO
ALTER TABLE [dbo].[ContainedInvoice] CHECK CONSTRAINT [FK_ContainedInvoice_Invoice]
GO
ALTER TABLE [dbo].[ContainedLabo]  WITH CHECK ADD  CONSTRAINT [FK__ContainedLabo__7CD98669] FOREIGN KEY([PrescriptionId], [AppointmentNo], [SubId])
REFERENCES [dbo].[ContainedTreatment] ([PrescriptionId], [AppointmentNo], [SubId])
GO
ALTER TABLE [dbo].[ContainedLabo] CHECK CONSTRAINT [FK__ContainedLabo__7CD98669]
GO
ALTER TABLE [dbo].[ContainedLabo]  WITH CHECK ADD  CONSTRAINT [FK_ContainedLabo_LaboType] FOREIGN KEY([LaboTypeId])
REFERENCES [dbo].[LaboType] ([Id])
GO
ALTER TABLE [dbo].[ContainedLabo] CHECK CONSTRAINT [FK_ContainedLabo_LaboType]
GO
ALTER TABLE [dbo].[ContainedLabo]  WITH CHECK ADD  CONSTRAINT [FK_ContainedLabo_LaboVendor] FOREIGN KEY([LaboVendorId])
REFERENCES [dbo].[LaboVendor] ([Id])
GO
ALTER TABLE [dbo].[ContainedLabo] CHECK CONSTRAINT [FK_ContainedLabo_LaboVendor]
GO
ALTER TABLE [dbo].[ContainedPicture]  WITH CHECK ADD  CONSTRAINT [FK_ContainedPicture_Appointment] FOREIGN KEY([PrescriptionId], [AppointmentNo])
REFERENCES [dbo].[Appointment] ([PrescriptionId], [No])
GO
ALTER TABLE [dbo].[ContainedPicture] CHECK CONSTRAINT [FK_ContainedPicture_Appointment]
GO
ALTER TABLE [dbo].[ContainedReceipt]  WITH CHECK ADD  CONSTRAINT [FK_ContainedReceipt_Appointment] FOREIGN KEY([PrescriptionId], [AppointmentNo])
REFERENCES [dbo].[Appointment] ([PrescriptionId], [No])
GO
ALTER TABLE [dbo].[ContainedReceipt] CHECK CONSTRAINT [FK_ContainedReceipt_Appointment]
GO
ALTER TABLE [dbo].[ContainedReceipt]  WITH CHECK ADD  CONSTRAINT [FK_ContainedReceipt_Drug] FOREIGN KEY([DrugId])
REFERENCES [dbo].[Drug] ([Id])
GO
ALTER TABLE [dbo].[ContainedReceipt] CHECK CONSTRAINT [FK_ContainedReceipt_Drug]
GO
ALTER TABLE [dbo].[ContainedTreatment]  WITH CHECK ADD  CONSTRAINT [FK_ContainedTreatment_Appointment] FOREIGN KEY([PrescriptionId], [AppointmentNo])
REFERENCES [dbo].[Appointment] ([PrescriptionId], [No])
GO
ALTER TABLE [dbo].[ContainedTreatment] CHECK CONSTRAINT [FK_ContainedTreatment_Appointment]
GO
ALTER TABLE [dbo].[ContainedTreatment]  WITH CHECK ADD  CONSTRAINT [FK_ContainedTreatment_Assistant] FOREIGN KEY([AssistantName])
REFERENCES [dbo].[Assistant] ([UserName])
GO
ALTER TABLE [dbo].[ContainedTreatment] CHECK CONSTRAINT [FK_ContainedTreatment_Assistant]
GO
ALTER TABLE [dbo].[ContainedTreatment]  WITH CHECK ADD  CONSTRAINT [FK_ContainedTreatment_Doctor] FOREIGN KEY([DoctorName])
REFERENCES [dbo].[Doctor] ([UserName])
GO
ALTER TABLE [dbo].[ContainedTreatment] CHECK CONSTRAINT [FK_ContainedTreatment_Doctor]
GO
ALTER TABLE [dbo].[ContainedTreatment]  WITH CHECK ADD  CONSTRAINT [FK_ContainedTreatment_Service] FOREIGN KEY([ServiceId])
REFERENCES [dbo].[Service] ([Id])
GO
ALTER TABLE [dbo].[ContainedTreatment] CHECK CONSTRAINT [FK_ContainedTreatment_Service]
GO
ALTER TABLE [dbo].[ContainedUsedEquipment]  WITH CHECK ADD  CONSTRAINT [FK_ContainedUsedEquipment_Appointment] FOREIGN KEY([PrescriptionId], [AppointmentNo])
REFERENCES [dbo].[Appointment] ([PrescriptionId], [No])
GO
ALTER TABLE [dbo].[ContainedUsedEquipment] CHECK CONSTRAINT [FK_ContainedUsedEquipment_Appointment]
GO
ALTER TABLE [dbo].[ContainedUsedEquipment]  WITH CHECK ADD  CONSTRAINT [FK_ContainedUsedEquipment_Equipment] FOREIGN KEY([EquipmentId])
REFERENCES [dbo].[Equipment] ([Id])
GO
ALTER TABLE [dbo].[ContainedUsedEquipment] CHECK CONSTRAINT [FK_ContainedUsedEquipment_Equipment]
GO
ALTER TABLE [dbo].[ContainedUsedReceipt]  WITH CHECK ADD  CONSTRAINT [FK_ContainedUsedReceipt_Drug] FOREIGN KEY([DrugId])
REFERENCES [dbo].[Drug] ([Id])
GO
ALTER TABLE [dbo].[ContainedUsedReceipt] CHECK CONSTRAINT [FK_ContainedUsedReceipt_Drug]
GO
ALTER TABLE [dbo].[ContainedUsedReceipt]  WITH CHECK ADD  CONSTRAINT [FK_ContainedUsedReceipt_UsedReceipt] FOREIGN KEY([PrescriptionId], [AppointmentNo])
REFERENCES [dbo].[UsedReceipt] ([PrescriptionId], [AppointmentNo])
GO
ALTER TABLE [dbo].[ContainedUsedReceipt] CHECK CONSTRAINT [FK_ContainedUsedReceipt_UsedReceipt]
GO
ALTER TABLE [dbo].[Customer]  WITH CHECK ADD  CONSTRAINT [FK_Customer_Branch] FOREIGN KEY([BranchId])
REFERENCES [dbo].[Branch] ([Id])
GO
ALTER TABLE [dbo].[Customer] CHECK CONSTRAINT [FK_Customer_Branch]
GO
ALTER TABLE [dbo].[CustomerMoreInformation]  WITH CHECK ADD FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[Doctor]  WITH CHECK ADD  CONSTRAINT [FK__Doctor__UserName__1940BAED] FOREIGN KEY([UserName])
REFERENCES [dbo].[Employee] ([UserName])
GO
ALTER TABLE [dbo].[Doctor] CHECK CONSTRAINT [FK__Doctor__UserName__1940BAED]
GO
ALTER TABLE [dbo].[DoctorSalary]  WITH CHECK ADD  CONSTRAINT [FK_DoctorSalary_Doctor] FOREIGN KEY([UserName])
REFERENCES [dbo].[Doctor] ([UserName])
GO
ALTER TABLE [dbo].[DoctorSalary] CHECK CONSTRAINT [FK_DoctorSalary_Doctor]
GO
ALTER TABLE [dbo].[DoctorSalary]  WITH CHECK ADD  CONSTRAINT [FK_DoctorSalary_MonthYearDoctor] FOREIGN KEY([Year], [Month], [BranchId])
REFERENCES [dbo].[MonthYearDoctor] ([Year], [Month], [BranchId])
GO
ALTER TABLE [dbo].[DoctorSalary] CHECK CONSTRAINT [FK_DoctorSalary_MonthYearDoctor]
GO
ALTER TABLE [dbo].[DrugInBranch]  WITH CHECK ADD  CONSTRAINT [FK_DrugInBranch_Branch] FOREIGN KEY([BranchId])
REFERENCES [dbo].[Branch] ([Id])
GO
ALTER TABLE [dbo].[DrugInBranch] CHECK CONSTRAINT [FK_DrugInBranch_Branch]
GO
ALTER TABLE [dbo].[DrugInBranch]  WITH CHECK ADD  CONSTRAINT [FK_DrugInBranch_Drug] FOREIGN KEY([DrugId])
REFERENCES [dbo].[Drug] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DrugInBranch] CHECK CONSTRAINT [FK_DrugInBranch_Drug]
GO
ALTER TABLE [dbo].[Employee]  WITH CHECK ADD  CONSTRAINT [FK_Employee_Branch] FOREIGN KEY([BranchId])
REFERENCES [dbo].[Branch] ([Id])
GO
ALTER TABLE [dbo].[Employee] CHECK CONSTRAINT [FK_Employee_Branch]
GO
ALTER TABLE [dbo].[Employee]  WITH CHECK ADD  CONSTRAINT [FK_Employee_EmployeeType] FOREIGN KEY([EmployeeTypeId])
REFERENCES [dbo].[EmployeeType] ([Id])
GO
ALTER TABLE [dbo].[Employee] CHECK CONSTRAINT [FK_Employee_EmployeeType]
GO
ALTER TABLE [dbo].[EquimentInBranch]  WITH CHECK ADD  CONSTRAINT [FK_EquimentInBranch_Branch] FOREIGN KEY([BranchId])
REFERENCES [dbo].[Branch] ([Id])
GO
ALTER TABLE [dbo].[EquimentInBranch] CHECK CONSTRAINT [FK_EquimentInBranch_Branch]
GO
ALTER TABLE [dbo].[EquimentInBranch]  WITH CHECK ADD  CONSTRAINT [FK_EquimentInBranch_Equipment] FOREIGN KEY([EquipmentId])
REFERENCES [dbo].[Equipment] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EquimentInBranch] CHECK CONSTRAINT [FK_EquimentInBranch_Equipment]
GO
ALTER TABLE [dbo].[EquipmentExportInvoice]  WITH CHECK ADD  CONSTRAINT [FK_EquipmentExportInvoice_Branch] FOREIGN KEY([BranchId])
REFERENCES [dbo].[Branch] ([Id])
GO
ALTER TABLE [dbo].[EquipmentExportInvoice] CHECK CONSTRAINT [FK_EquipmentExportInvoice_Branch]
GO
ALTER TABLE [dbo].[EquipmentInvoice]  WITH CHECK ADD  CONSTRAINT [FK_EquipmentInvoice_Branch] FOREIGN KEY([BranchId])
REFERENCES [dbo].[Branch] ([Id])
GO
ALTER TABLE [dbo].[EquipmentInvoice] CHECK CONSTRAINT [FK_EquipmentInvoice_Branch]
GO
ALTER TABLE [dbo].[Expense]  WITH CHECK ADD  CONSTRAINT [FK_Expense_Account] FOREIGN KEY([AccountantName])
REFERENCES [dbo].[Account] ([UserName])
GO
ALTER TABLE [dbo].[Expense] CHECK CONSTRAINT [FK_Expense_Account]
GO
ALTER TABLE [dbo].[Invoice]  WITH CHECK ADD  CONSTRAINT [FK_Invoice_Account] FOREIGN KEY([UserName])
REFERENCES [dbo].[Account] ([UserName])
GO
ALTER TABLE [dbo].[Invoice] CHECK CONSTRAINT [FK_Invoice_Account]
GO
ALTER TABLE [dbo].[Invoice]  WITH CHECK ADD  CONSTRAINT [FK_Invoice_Branch] FOREIGN KEY([BranchId])
REFERENCES [dbo].[Branch] ([Id])
GO
ALTER TABLE [dbo].[Invoice] CHECK CONSTRAINT [FK_Invoice_Branch]
GO
ALTER TABLE [dbo].[Membership]  WITH CHECK ADD  CONSTRAINT [FK_Membership_MemberShipGroup] FOREIGN KEY([GroupId])
REFERENCES [dbo].[MemberShipGroup] ([Id])
GO
ALTER TABLE [dbo].[Membership] CHECK CONSTRAINT [FK_Membership_MemberShipGroup]
GO
ALTER TABLE [dbo].[MemberShipGroup]  WITH CHECK ADD  CONSTRAINT [FK_MemberShipGroup_Branch] FOREIGN KEY([BranchId])
REFERENCES [dbo].[Branch] ([Id])
GO
ALTER TABLE [dbo].[MemberShipGroup] CHECK CONSTRAINT [FK_MemberShipGroup_Branch]
GO
ALTER TABLE [dbo].[MonthYear]  WITH CHECK ADD  CONSTRAINT [FK_MonthYear_Branch] FOREIGN KEY([BranchId])
REFERENCES [dbo].[Branch] ([Id])
GO
ALTER TABLE [dbo].[MonthYear] CHECK CONSTRAINT [FK_MonthYear_Branch]
GO
ALTER TABLE [dbo].[MonthYearDoctor]  WITH CHECK ADD  CONSTRAINT [FK_MonthYearDoctor_Branch] FOREIGN KEY([BranchId])
REFERENCES [dbo].[Branch] ([Id])
GO
ALTER TABLE [dbo].[MonthYearDoctor] CHECK CONSTRAINT [FK_MonthYearDoctor_Branch]
GO
ALTER TABLE [dbo].[OtherEmployee]  WITH CHECK ADD  CONSTRAINT [FK__OtherEmpl__UserN__1C1D2798] FOREIGN KEY([UserName])
REFERENCES [dbo].[Employee] ([UserName])
GO
ALTER TABLE [dbo].[OtherEmployee] CHECK CONSTRAINT [FK__OtherEmpl__UserN__1C1D2798]
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD  CONSTRAINT [FK_Payment_Account] FOREIGN KEY([ReceptionistName])
REFERENCES [dbo].[Account] ([UserName])
GO
ALTER TABLE [dbo].[Payment] CHECK CONSTRAINT [FK_Payment_Account]
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD  CONSTRAINT [FK_Payment_Prescription] FOREIGN KEY([PrescriptionId])
REFERENCES [dbo].[Prescription] ([Id])
GO
ALTER TABLE [dbo].[Payment] CHECK CONSTRAINT [FK_Payment_Prescription]
GO
ALTER TABLE [dbo].[Prescription]  WITH CHECK ADD  CONSTRAINT [FK_Prescription_Customer] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[Prescription] CHECK CONSTRAINT [FK_Prescription_Customer]
GO
ALTER TABLE [dbo].[Prescription]  WITH CHECK ADD  CONSTRAINT [FK_Prescription_Membership] FOREIGN KEY([MembershipId])
REFERENCES [dbo].[Membership] ([Id])
GO
ALTER TABLE [dbo].[Prescription] CHECK CONSTRAINT [FK_Prescription_Membership]
GO
ALTER TABLE [dbo].[Receptionist]  WITH CHECK ADD  CONSTRAINT [FK__Reception__UserN__184C96B4] FOREIGN KEY([UserName])
REFERENCES [dbo].[Employee] ([UserName])
GO
ALTER TABLE [dbo].[Receptionist] CHECK CONSTRAINT [FK__Reception__UserN__184C96B4]
GO
ALTER TABLE [dbo].[Salary]  WITH CHECK ADD  CONSTRAINT [FK_Salary_Employee] FOREIGN KEY([EmployeeName])
REFERENCES [dbo].[Employee] ([UserName])
GO
ALTER TABLE [dbo].[Salary] CHECK CONSTRAINT [FK_Salary_Employee]
GO
ALTER TABLE [dbo].[Salary]  WITH CHECK ADD  CONSTRAINT [FK_Salary_MonthYear] FOREIGN KEY([Month], [Year], [BranchId])
REFERENCES [dbo].[MonthYear] ([Month], [Year], [BranchId])
GO
ALTER TABLE [dbo].[Salary] CHECK CONSTRAINT [FK_Salary_MonthYear]
GO
ALTER TABLE [dbo].[Service]  WITH CHECK ADD  CONSTRAINT [FK_Service_ServiceGroup] FOREIGN KEY([GroupId])
REFERENCES [dbo].[ServiceGroup] ([Id])
GO
ALTER TABLE [dbo].[Service] CHECK CONSTRAINT [FK_Service_ServiceGroup]
GO
ALTER TABLE [dbo].[ServiceDiscount]  WITH CHECK ADD  CONSTRAINT [FK_ServiceDiscount_MemberShipGroup] FOREIGN KEY([MemberShipGroupId])
REFERENCES [dbo].[MemberShipGroup] ([Id])
GO
ALTER TABLE [dbo].[ServiceDiscount] CHECK CONSTRAINT [FK_ServiceDiscount_MemberShipGroup]
GO
ALTER TABLE [dbo].[ServiceDiscount]  WITH CHECK ADD  CONSTRAINT [FK_ServiceDiscount_Service] FOREIGN KEY([ServiceId])
REFERENCES [dbo].[Service] ([Id])
GO
ALTER TABLE [dbo].[ServiceDiscount] CHECK CONSTRAINT [FK_ServiceDiscount_Service]
GO
ALTER TABLE [dbo].[UsedReceipt]  WITH CHECK ADD FOREIGN KEY([PrescriptionId], [AppointmentNo])
REFERENCES [dbo].[Appointment] ([PrescriptionId], [No])
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[43] 4[4] 2[28] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "t0"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 236
            End
            DisplayFlags = 280
            TopColumn = 4
         End
         Begin Table = "t1"
            Begin Extent = 
               Top = 6
               Left = 274
               Bottom = 136
               Right = 444
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "t2"
            Begin Extent = 
               Top = 6
               Left = 482
               Bottom = 136
               Right = 652
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "t3"
            Begin Extent = 
               Top = 138
               Left = 38
               Bottom = 268
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "t4"
            Begin Extent = 
               Top = 138
               Left = 246
               Bottom = 268
               Right = 416
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "t5"
            Begin Extent = 
               Top = 138
               Left = 454
               Bottom = 268
               Right = 631
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 18
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'AppointmentAdapter'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N' = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'AppointmentAdapter'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'AppointmentAdapter'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "EquipmentInvoice"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 119
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Branch"
            Begin Extent = 
               Top = 6
               Left = 246
               Bottom = 136
               Right = 416
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 600
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'EquipmentInvoiceView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'EquipmentInvoiceView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[28] 4[20] 2[32] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = -132
         Left = -734
      End
      Begin Tables = 
         Begin Table = "Expense"
            Begin Extent = 
               Top = 6
               Left = 772
               Bottom = 137
               Right = 953
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Account"
            Begin Extent = 
               Top = 124
               Left = 988
               Bottom = 254
               Right = 1158
            End
            DisplayFlags = 280
            TopColumn = 1
         End
         Begin Table = "Branch"
            Begin Extent = 
               Top = 77
               Left = 1243
               Bottom = 207
               Right = 1413
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1890
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1635
         Alias = 1920
         Table = 1680
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ExpenseView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ExpenseView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[41] 4[38] 2[4] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "t0"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 135
               Right = 243
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Branch"
            Begin Extent = 
               Top = 23
               Left = 399
               Bottom = 153
               Right = 569
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ExportInvoiceView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ExportInvoiceView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Invoice"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Account"
            Begin Extent = 
               Top = 6
               Left = 246
               Bottom = 136
               Right = 416
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Branch"
            Begin Extent = 
               Top = 6
               Left = 454
               Bottom = 136
               Right = 624
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'InvoiceView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'InvoiceView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[21] 4[9] 2[45] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "t0"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 241
               Right = 335
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "t1"
            Begin Extent = 
               Top = 6
               Left = 482
               Bottom = 227
               Right = 753
            End
            DisplayFlags = 280
            TopColumn = 2
         End
         Begin Table = "t2"
            Begin Extent = 
               Top = 138
               Left = 38
               Bottom = 268
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "t3"
            Begin Extent = 
               Top = 145
               Left = 347
               Bottom = 306
               Right = 533
            End
            DisplayFlags = 280
            TopColumn = 3
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 12
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 1890
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
   ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'PrescriptionAdapter'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'      Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'PrescriptionAdapter'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'PrescriptionAdapter'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[42] 4[18] 2[22] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "t0"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 215
               Right = 266
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "t1"
            Begin Extent = 
               Top = 6
               Left = 304
               Bottom = 202
               Right = 531
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 3870
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'TreatmentAdapter'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'TreatmentAdapter'
GO
