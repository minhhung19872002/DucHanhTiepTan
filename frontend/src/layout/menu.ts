/**
 * Navigation mirrors the ribbon of the legacy WinForms app.
 * MainForm (receptionist) tabs: "Home" / "Quản lí".
 * AdministratorForm (admin) tabs: "Lịch sử" / "Danh mục" / "Báo cáo".
 */
export interface MenuLeaf {
  label: string
  path: string
}
export interface MenuGroup {
  label: string
  items: MenuLeaf[]
}
export interface MenuTab {
  key: string
  label: string
  groups: MenuGroup[]
}

export const receptionistMenu: MenuTab[] = [
  {
    key: 'home',
    label: 'Trang chính',
    groups: [
      { label: 'Khách hàng', items: [{ label: 'Tìm kiếm khách hàng', path: '/customers' }] },
      {
        label: 'Khám bệnh',
        items: [
          { label: 'Đang khám', path: '/examination' },
          { label: 'Lịch sử khám', path: '/prescriptions' },
        ],
      },
      { label: 'Lịch hẹn', items: [{ label: 'Danh sách hẹn', path: '/appointments' }] },
      {
        label: 'Thu phí khách hàng',
        items: [
          { label: 'Khách hàng đang nợ', path: '/debts' },
          { label: 'Lịch sử thu', path: '/payments' },
        ],
      },
      { label: 'Labo', items: [{ label: 'Labo chờ', path: '/labo-delivering' }] },
      { label: 'Chi phí', items: [{ label: 'Phiếu chi', path: '/expenses' }] },
    ],
  },
  {
    key: 'manage',
    label: 'Quản lí',
    groups: [
      {
        label: 'Danh mục',
        items: [
          { label: 'Thuốc', path: '/catalog/drugs' },
          { label: 'Labo', path: '/catalog/labo' },
          { label: 'Dịch vụ', path: '/catalog/services' },
          { label: 'Tài khoản', path: '/catalog/accounts' },
          { label: 'Tài sản', path: '/catalog/equipment/fixed' },
          { label: 'Vật tư', path: '/catalog/equipment/various' },
        ],
      },
      {
        label: 'Quản lí thuốc',
        items: [
          { label: 'Hóa đơn nhập thuốc', path: '/drug-invoices' },
          { label: 'Hóa đơn xuất thuốc', path: '/drug-receipts' },
          { label: 'Tồn kho thuốc', path: '/drug-stock' },
        ],
      },
      {
        label: 'Xuất nhập thiết bị',
        items: [
          { label: 'Đã nhập', path: '/equipment-invoices' },
          { label: 'Đã xuất', path: '/equipment-exports' },
          { label: 'Tồn kho', path: '/equipment-stock' },
        ],
      },
    ],
  },
]

export const adminMenu: MenuTab[] = [
  {
    key: 'history',
    label: 'Lịch sử',
    groups: [
      {
        label: 'Khám bệnh',
        items: [
          { label: 'Khách hàng', path: '/customers' },
          { label: 'Khám bệnh', path: '/prescriptions' },
          { label: 'Labo', path: '/labo-delivering' },
          { label: 'Lịch hẹn theo ngày', path: '/appointments/dashboard' },
        ],
      },
      {
        label: 'Thiết bị',
        items: [
          { label: 'Hóa đơn nhập', path: '/equipment-invoices' },
          { label: 'Hóa đơn xuất', path: '/equipment-exports' },
          { label: 'Tồn kho', path: '/equipment-stock' },
        ],
      },
      {
        label: 'Doanh thu',
        items: [
          { label: 'Khách hàng nợ', path: '/debts' },
          { label: 'Lịch sử thu', path: '/payments' },
          { label: 'Phiếu chi', path: '/expenses' },
        ],
      },
      {
        label: 'Thống kê doanh thu',
        items: [
          { label: 'Bác sĩ', path: '/reports/doctor-income' },
          { label: 'Chi nhánh', path: '/reports/branch-income' },
          { label: 'Khách hàng', path: '/reports/customer-income' },
          { label: 'Dịch vụ', path: '/reports/service-income' },
          { label: 'Chi phí của bác sĩ', path: '/reports/doctor-expense' },
          { label: 'Phiếu khám (VIP)', path: '/reports/membership' },
        ],
      },
      {
        label: 'Thống kê',
        items: [
          { label: 'Sử dụng', path: '/reports/used' },
          { label: 'Chi phí', path: '/reports/expenses' },
          { label: 'Doanh thu - Chi phí', path: '/reports/income-expense' },
          { label: 'Labo', path: '/reports/labo' },
          { label: 'Labo theo bác sĩ', path: '/reports/labo-by-doctor' },
        ],
      },
      {
        label: 'Lương',
        items: [
          { label: 'Bảng lương nhân viên', path: '/salary' },
          { label: 'Lương bác sĩ', path: '/salary/doctors' },
        ],
      },
    ],
  },
  {
    key: 'catalog',
    label: 'Danh mục',
    groups: [
      {
        label: 'Dữ liệu',
        items: [
          { label: 'Thuốc', path: '/catalog/drugs' },
          { label: 'Tài sản', path: '/catalog/equipment/fixed' },
          { label: 'Vật tư', path: '/catalog/equipment/various' },
          { label: 'Dịch vụ', path: '/catalog/services' },
          { label: 'Labo', path: '/catalog/labo' },
          { label: 'Mã VIP', path: '/catalog/memberships' },
        ],
      },
      {
        label: 'Tài khoản',
        items: [
          { label: 'Tài khoản', path: '/catalog/accounts' },
          { label: 'Nhân viên', path: '/catalog/employees' },
        ],
      },
      { label: 'Hệ thống', items: [{ label: 'Chi nhánh', path: '/catalog/branches' }] },
    ],
  },
]
