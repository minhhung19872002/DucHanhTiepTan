# Handoff – trạng thái công việc (2026-10-14)

## Mục tiêu
Clone phần mềm nha khoa Đức Hạnh (WinForms `C:\Users\ADMIN\Downloads\test\test\MainApp.exe`) thành web app:
**ASP.NET Core 9 API + PostgreSQL + React (Vite/TS/AntD) + Docker**. Không đụng tới phần mềm và DB SQL Server đang chạy.
Repo: https://github.com/minhhung19872002/DucHanhTiepTan (nhánh `main`).

## Đã xong (đã push)
- Decompile app gốc (ilspycmd) → mã C# ở scratchpad:
  `C:\Users\ADMIN\AppData\Local\Temp\claude\D--Source-DucHanhTiepTan\9f40bbe8-e5dc-4a76-a8e4-935b546e3166\scratchpad\decomp\main\Tmt.PharmacyRibbon\*.cs`
  (nếu mất, chạy lại: `ilspycmd -p -o main MainApp.exe`).
- Backend skeleton build OK: entities + DbContext sinh từ schema (`tools/gen_entities.py`), migration InitialCreate, views (`Sql/views.sql`), seed (`db/seed/`), JWT auth (MD5 cũ), `BranchScope`, `ExcelExporter`, `BranchesController` mẫu.
- Frontend skeleton build OK: login, layout menu theo ribbon gốc, `routes.tsx` đã đăng ký đủ 37 trang (placeholder).
- Postgres đang chạy: container `duchanh_db`, cổng 5491, đã nạp seed; tài khoản demo `admin`/`123456`, `tieptan01`/`123456`.
- Quy ước: `CONVENTIONS.md`. Chạy: xem `README.md`.

## Đang dở – 6 agent port module đang chạy song song khi tạm dừng
Mỗi agent tự tạo file trong `backend/.../Controllers/<Module>/` và `frontend/src/modules/<module>/`:
customers, examination, finance, catalog, inventory, reports. Có thể một số đã xong, một số dở.

## Việc tiếp theo cho model mới
1. `git status` để xem file mới; kiểm tra từng module còn trang nào là placeholder (`grep -rl "Placeholder" frontend/src/modules`).
2. Build: `cd backend/DucHanhTiepTan.Api && dotnet build`; `cd frontend && npx tsc --noEmit && npm run build`. Sửa lỗi tích hợp (trùng tên DTO/route giữa module...).
3. Chạy thử: `dotnet run` (5091) + `npm run dev` (5173), đăng nhập, bấm qua các màn hình; hoặc `docker compose up -d --build` (web 3091).
4. Port nốt module nào còn thiếu theo `CONVENTIONS.md`.
5. Commit + push lên `main` (attribution Co-Authored-By theo quy ước session).
