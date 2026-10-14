# Quy ước phát triển – DucHanhTiepTan Web

Web clone của phần mềm quản lý nha khoa Đức Hạnh (WinForms `MainApp.exe`, namespace `Tmt.PharmacyRibbon`).

## Stack
- **Backend**: ASP.NET Core 9 Web API + EF Core 9 + Npgsql (`backend/DucHanhTiepTan.Api`)
- **Frontend**: React 19 + Vite + TypeScript + Ant Design 6 + react-router 7 (`frontend`)
- **DB**: PostgreSQL 16 (Docker, cổng host 5491). Schema giữ nguyên tên bảng/cột PascalCase của SQL Server gốc (quoted identifiers).
- **Docker**: `docker compose up -d` → web `http://localhost:3091`, API `http://localhost:5091` (dev: `dotnet run` cũng chạy 5091, Vite dev `5173` proxy `/api`).

## Backend
- Entities sinh từ schema gốc: `Entities/*.cs` (KHÔNG sửa tay – chạy lại generator nếu cần; thêm navigation thì sửa generator).
- Views SQL gốc (`TreatmentAdapter`, `PrescriptionAdapter`, `AppointmentAdapter`, `InvoiceView`, `ExpenseView`, `ExportInvoiceView`, `EquipmentInvoiceView`) là keyless entity trong `Entities/Views.cs`, DDL ở `Sql/views.sql` (chạy mỗi lần khởi động).
- Mỗi module = 1 hoặc nhiều controller trong `Controllers/<Module>/`, route `api/<resource>` (kebab-case, số nhiều).
  DTO là `record` khai báo ngay trong controller hoặc `Controllers/<Module>/Dtos.cs`. Không trả entity trực tiếp (tránh vòng lặp/`Image` bytes).
- Auth: `[Authorize]` mặc định; thao tác chỉ admin → `[Authorize(Policy = "Admin")]`. Lấy user hiện tại qua `ICurrentUser` (UserName, IsAdministrator, BranchId).
- Lọc chi nhánh: `BranchScope.Resolve(_user, branchId)`; 0 = tất cả (chỉ admin). Query param `branchId`.
- Lỗi nghiệp vụ: `throw new ApiException("thông báo tiếng Việt")` → HTTP 400 `{ message }`; `NotFoundException` → 404.
- Phân trang: `query.ToPagedAsync(page, pageSize)` → `PagedResult<T>`; tìm kiếm không dấu: `query.WhereUnaccentLike(x => x.Name, keyword)`.
- Xuất Excel: `ExcelExporter.Build(...)` trả `File(bytes, ExcelExporter.ContentType, "ten-file.xlsx")`, endpoint `GET .../export`.
- DateTime trong DB là `timestamp without time zone` (giờ địa phương, không convert UTC). Nhận `DateTime` từ client dạng `yyyy-MM-ddTHH:mm:ss` (không có Z).
- Mật khẩu: MD5 hex (tương thích app cũ) – `Md5Hasher`.

## Frontend
- `src/api/client.ts`: axios instance (`api`), `errorMessage(e)`, `downloadFile`, `PagedResult<T>`.
- `src/auth/AuthContext.tsx`: `useAuth()` → `user`, `branchId` (bộ lọc chi nhánh toàn cục), `setBranchId`.
- `src/components/common.tsx`: `PageTitle`, `ExcelButton`, `DeleteButton`, `BranchSelect`, `useBranches`, `useLoad`.
- `src/utils/format.ts`: `money`, `fmtDate`, `fmtDateTime`, `toLocalIso`, `SEX_OPTIONS`.
- Mỗi module ở `src/modules/<module>/`: `*Page.tsx` (đã đăng ký trong `src/routes.tsx`), `api.ts` (hàm gọi API + types), các component con/modal.
- Bảng dữ liệu: AntD `<Table size="small" rowKey=... pagination=...>`; form nhập liệu: `<Modal>` + `<Form layout="vertical">`.
- Nhãn, thông báo: tiếng Việt, giữ đúng tên như app gốc (VD "Phiếu chi", "Labo chờ", "Khách hàng đang nợ").
- Đường dẫn route và nhãn menu: `src/layout/menu.ts` + `src/routes.tsx`.

## Mã nguồn gốc (tham chiếu khi port nghiệp vụ)
Mã C# decompile của app cũ nằm ở (máy dev):
`C:\Users\ADMIN\AppData\Local\Temp\claude\D--Source-DucHanhTiepTan\9f40bbe8-e5dc-4a76-a8e4-935b546e3166\scratchpad\decomp\main\Tmt.PharmacyRibbon\*.cs`
Mỗi màn hình = 1 `*Control.cs`/`*Dialog.cs`; truy vấn LINQ to SQL trong đó chuyển gần như 1:1 sang EF Core.
