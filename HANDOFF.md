# Trạng thái dự án (2026-10-14)

Clone web của phần mềm nha khoa Đức Hạnh (WinForms `MainApp.exe`) đã hoàn tất phiên bản đầu:
**ASP.NET Core 9 API + PostgreSQL 16 + React (Vite/TS/AntD) + Docker Compose**. Không đụng tới phần mềm và DB SQL Server đang chạy.

## Đã xong
- Backend 6 module (khách hàng/lịch hẹn, khám bệnh, thu chi/labo/VIP, danh mục, kho, báo cáo/lương): build sạch, 34 endpoint GET đã gọi thử với dữ liệu thật.
- Frontend: 37 trang, `tsc -b` + `vite build` sạch; đã duyệt cả 39 route trong trình duyệt qua Docker (cổng 3091), không có lỗi console.
- Phân quyền: tiếp tân bị chặn báo cáo/lương/tài khoản; chỉ thấy dữ liệu chi nhánh của mình.
- Xuất Excel cho các báo cáo; in phiếu khám bằng layout in của trình duyệt.

## Chưa kiểm thử kỹ / việc tiếp theo
- Chưa bấm thử toàn bộ luồng ghi (tạo/sửa/xóa) trên giao diện; các agent chỉ thử qua API.
- Chưa có luồng "trả nợ" riêng ngoài cột Thanh toán trong bảng điều trị (giống app gốc).
- Báo cáo chưa đối chiếu số liệu với app gốc trên cùng bộ dữ liệu.
- Seed chỉ là dump năm 2013 (chi nhánh Biên Hòa). Muốn dữ liệu thật: xem mục "Nạp dữ liệu thật" trong README.
- Mật khẩu demo `123456` chỉ dành cho DB local; đổi `JWT_KEY` khi triển khai.
