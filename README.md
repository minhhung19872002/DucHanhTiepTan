# Nha khoa Đức Hạnh – Web (DucHanhTiepTan)

Bản web của phần mềm quản lý phòng khám nha khoa Đức Hạnh (app gốc: `MainApp.exe`, WinForms .NET Framework 4.0, Krypton Ribbon, LINQ to SQL, SQL Server, RDLC ReportViewer).

**Hoàn toàn độc lập với phần mềm và DB SQL Server đang chạy**: web dùng PostgreSQL riêng trong Docker, dữ liệu mẫu lấy từ file dump `NhaKhoaBienHoa01102013.sql` (đã chuyển đổi sang `db/seed/`).

## Stack
| Thành phần | Công nghệ |
|---|---|
| Backend | ASP.NET Core 9 Web API, EF Core 9, Npgsql, JWT, ClosedXML (xuất Excel), Swagger |
| Frontend | React 19, Vite, TypeScript, Ant Design 6, react-router 7, axios, dayjs |
| Database | PostgreSQL 16 (schema giữ nguyên tên bảng/cột của app gốc) |
| Triển khai | Docker Compose (db + api + web/nginx) |

## Chạy bằng Docker (khuyên dùng)
```bash
docker compose up -d --build
```
- Web: http://localhost:3091
- API + Swagger: http://localhost:5091/swagger
- PostgreSQL: `localhost:5491` (user `duchanh` / pass `duchanh_dev_pw`, db `duchanh_tieptan`)

Lần chạy đầu API tự: apply migration → tạo views → nạp `db/seed/*.sql` (chỉ khi DB trống).

**Tài khoản demo** (mật khẩu `123456` – đặt bởi `db/seed/003_dev_accounts.sql`, chỉ cho DB local):
- Quản trị: `admin`, `quanly01`…`quanly07`
- Tiếp tân: `tieptan01`…`tieptan07`

Đổi `JWT_KEY` trong `.env` khi triển khai thật.

## Chạy dev (không Docker cho code)
```bash
docker compose up -d db                     # chỉ Postgres
cd backend/DucHanhTiepTan.Api && dotnet run # API http://localhost:5091
cd frontend && npm install && npm run dev   # UI http://localhost:5173 (proxy /api)
```

## Cấu trúc
```
backend/DucHanhTiepTan.Api/   ASP.NET Core API (Controllers/<Module>, Entities, Data, Sql/views.sql)
frontend/                     React app (src/modules/<module>, src/layout, src/components)
db/seed/                      Dữ liệu mẫu PostgreSQL (chuyển từ dump SQL Server)
tools/                        Script chuyển đổi dump SQL Server → PostgreSQL, sinh entity
docker-compose.yml
CONVENTIONS.md                Quy ước code cho từng module
```

## Ánh xạ màn hình gốc → web
| Ribbon gốc | Web |
|---|---|
| Khách hàng (Tìm kiếm/Tạo mới/Chỉnh sửa/Xóa) | `/customers` |
| Khám bệnh (Tạo mới/Đang khám/Lịch sử) | `/examination`, `/prescriptions` |
| Lịch hẹn (Lịch hẹn/Khám/Hủy/Đổi) | `/appointments`, admin: `/appointments/dashboard` |
| Thu phí (Khách hàng đang nợ/Lịch sử thu) | `/debts`, `/payments` |
| Labo chờ | `/labo-delivering` |
| Chi phí (Tạo phiếu chi/Đã chi) | `/expenses` |
| Danh mục (Thuốc/Labo/Dịch vụ/Tài khoản/Tài sản/Vật tư/Mã VIP/Nhân viên/Chi nhánh) | `/catalog/*` |
| Quản lí thuốc (HĐ nhập/HĐ xuất/Tồn kho) | `/drug-invoices`, `/drug-receipts`, `/drug-stock` |
| Xuất nhập thiết bị | `/equipment-invoices`, `/equipment-exports`, `/equipment-stock` |
| Thống kê doanh thu / Thống kê | `/reports/*` (+ xuất Excel) |
| Lương (Khai báo/Đã khai báo/Lương bác sĩ) | `/salary`, `/salary/doctors` |

## Nạp dữ liệu thật
Xuất dump mới từ SQL Server (SSMS → Generate Scripts, schema + data, UTF-16) rồi chạy:
```bash
python tools/convert_seed.py <dump-utf8.sql> db/seed/002_data.sql
docker compose down -v && docker compose up -d --build
```
(Chuyển file dump về UTF-8 trước: `iconv -f UTF-16 -t UTF-8`.)
