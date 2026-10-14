import { lazy, Suspense, type ReactNode } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { Spin } from 'antd'
import AppLayout from './layout/AppLayout'
import LoginPage from './pages/LoginPage'
import Placeholder from './pages/Placeholder'
import { useAuth } from './auth/AuthContext'

/**
 * Route registry. Each module registers its pages here; unported screens fall back to <Placeholder/>.
 * Paths must match frontend/src/layout/menu.ts.
 */
const page = (loader: () => Promise<{ default: React.ComponentType }>) => {
  const C = lazy(loader)
  return (
    <Suspense fallback={<Spin style={{ display: 'block', margin: '80px auto' }} />}>
      <C />
    </Suspense>
  )
}

// Module: customers / appointments / prescription history
const CustomersPage = () => page(() => import('./modules/customers/CustomersPage'))
const PrescriptionsPage = () => page(() => import('./modules/customers/PrescriptionsPage'))
const AppointmentsPage = () => page(() => import('./modules/customers/AppointmentsPage'))
const AppointmentDashboardPage = () => page(() => import('./modules/customers/AppointmentDashboardPage'))
// Module: examination
const ExaminationPage = () => page(() => import('./modules/examination/ExaminationPage'))
const PrescriptionDetailPage = () => page(() => import('./modules/examination/PrescriptionDetailPage'))
// Module: finance
const DebtsPage = () => page(() => import('./modules/finance/DebtsPage'))
const PaymentsPage = () => page(() => import('./modules/finance/PaymentsPage'))
const ExpensesPage = () => page(() => import('./modules/finance/ExpensesPage'))
const LaboDeliveringPage = () => page(() => import('./modules/finance/LaboDeliveringPage'))
const MembershipsPage = () => page(() => import('./modules/finance/MembershipsPage'))
// Module: catalog
const DrugsPage = () => page(() => import('./modules/catalog/DrugsPage'))
const LaboCatalogPage = () => page(() => import('./modules/catalog/LaboCatalogPage'))
const ServicesPage = () => page(() => import('./modules/catalog/ServicesPage'))
const AccountsPage = () => page(() => import('./modules/catalog/AccountsPage'))
const EmployeesPage = () => page(() => import('./modules/catalog/EmployeesPage'))
const BranchesPage = () => page(() => import('./modules/catalog/BranchesPage'))
const EquipmentCatalogPage = () => page(() => import('./modules/catalog/EquipmentCatalogPage'))
// Module: inventory
const DrugInvoicesPage = () => page(() => import('./modules/inventory/DrugInvoicesPage'))
const DrugReceiptsPage = () => page(() => import('./modules/inventory/DrugReceiptsPage'))
const DrugStockPage = () => page(() => import('./modules/inventory/DrugStockPage'))
const EquipmentInvoicesPage = () => page(() => import('./modules/inventory/EquipmentInvoicesPage'))
const EquipmentExportsPage = () => page(() => import('./modules/inventory/EquipmentExportsPage'))
const EquipmentStockPage = () => page(() => import('./modules/inventory/EquipmentStockPage'))
// Module: reports & salary
const DoctorIncomePage = () => page(() => import('./modules/reports/DoctorIncomePage'))
const BranchIncomePage = () => page(() => import('./modules/reports/BranchIncomePage'))
const CustomerIncomePage = () => page(() => import('./modules/reports/CustomerIncomePage'))
const ServiceIncomePage = () => page(() => import('./modules/reports/ServiceIncomePage'))
const DoctorExpensePage = () => page(() => import('./modules/reports/DoctorExpensePage'))
const MembershipStatsPage = () => page(() => import('./modules/reports/MembershipStatsPage'))
const UsedStatsPage = () => page(() => import('./modules/reports/UsedStatsPage'))
const ExpenseStatsPage = () => page(() => import('./modules/reports/ExpenseStatsPage'))
const IncomeExpensePage = () => page(() => import('./modules/reports/IncomeExpensePage'))
const LaboStatsPage = () => page(() => import('./modules/reports/LaboStatsPage'))
const LaboByDoctorPage = () => page(() => import('./modules/reports/LaboByDoctorPage'))
const SalaryPage = () => page(() => import('./modules/reports/SalaryPage'))
const DoctorSalaryPage = () => page(() => import('./modules/reports/DoctorSalaryPage'))

function RequireAuth({ children }: { children: ReactNode }) {
  const { user } = useAuth()
  if (!user) return <Navigate to="/login" replace />
  return <>{children}</>
}

function RequireAdmin({ children }: { children: ReactNode }) {
  const { user } = useAuth()
  if (!user?.isAdministrator) return <Navigate to="/" replace />
  return <>{children}</>
}

export default function AppRoutes() {
  const { user } = useAuth()
  const home = user?.isAdministrator ? '/customers' : '/examination'
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/"
        element={
          <RequireAuth>
            <AppLayout />
          </RequireAuth>
        }
      >
        <Route index element={<Navigate to={home} replace />} />

        <Route path="customers" element={<CustomersPage />} />
        <Route path="customers/:id" element={<CustomersPage />} />
        <Route path="prescriptions" element={<PrescriptionsPage />} />
        <Route path="prescriptions/:id" element={<PrescriptionDetailPage />} />
        <Route path="appointments" element={<AppointmentsPage />} />
        <Route path="appointments/dashboard" element={<RequireAdmin><AppointmentDashboardPage /></RequireAdmin>} />

        <Route path="examination" element={<ExaminationPage />} />
        <Route path="examination/:prescriptionId/:appointmentNo" element={<ExaminationPage />} />

        <Route path="debts" element={<DebtsPage />} />
        <Route path="payments" element={<PaymentsPage />} />
        <Route path="expenses" element={<ExpensesPage />} />
        <Route path="labo-delivering" element={<LaboDeliveringPage />} />

        <Route path="catalog/drugs" element={<DrugsPage />} />
        <Route path="catalog/labo" element={<LaboCatalogPage />} />
        <Route path="catalog/services" element={<ServicesPage />} />
        <Route path="catalog/accounts" element={<AccountsPage />} />
        <Route path="catalog/employees" element={<RequireAdmin><EmployeesPage /></RequireAdmin>} />
        <Route path="catalog/branches" element={<RequireAdmin><BranchesPage /></RequireAdmin>} />
        <Route path="catalog/memberships" element={<RequireAdmin><MembershipsPage /></RequireAdmin>} />
        <Route path="catalog/equipment/:kind" element={<EquipmentCatalogPage />} />

        <Route path="drug-invoices" element={<DrugInvoicesPage />} />
        <Route path="drug-receipts" element={<DrugReceiptsPage />} />
        <Route path="drug-stock" element={<DrugStockPage />} />
        <Route path="equipment-invoices" element={<EquipmentInvoicesPage />} />
        <Route path="equipment-exports" element={<EquipmentExportsPage />} />
        <Route path="equipment-stock" element={<EquipmentStockPage />} />

        <Route path="reports/doctor-income" element={<RequireAdmin><DoctorIncomePage /></RequireAdmin>} />
        <Route path="reports/branch-income" element={<RequireAdmin><BranchIncomePage /></RequireAdmin>} />
        <Route path="reports/customer-income" element={<RequireAdmin><CustomerIncomePage /></RequireAdmin>} />
        <Route path="reports/service-income" element={<RequireAdmin><ServiceIncomePage /></RequireAdmin>} />
        <Route path="reports/doctor-expense" element={<RequireAdmin><DoctorExpensePage /></RequireAdmin>} />
        <Route path="reports/membership" element={<RequireAdmin><MembershipStatsPage /></RequireAdmin>} />
        <Route path="reports/used" element={<RequireAdmin><UsedStatsPage /></RequireAdmin>} />
        <Route path="reports/expenses" element={<RequireAdmin><ExpenseStatsPage /></RequireAdmin>} />
        <Route path="reports/income-expense" element={<RequireAdmin><IncomeExpensePage /></RequireAdmin>} />
        <Route path="reports/labo" element={<RequireAdmin><LaboStatsPage /></RequireAdmin>} />
        <Route path="reports/labo-by-doctor" element={<RequireAdmin><LaboByDoctorPage /></RequireAdmin>} />
        <Route path="salary" element={<RequireAdmin><SalaryPage /></RequireAdmin>} />
        <Route path="salary/doctors" element={<RequireAdmin><DoctorSalaryPage /></RequireAdmin>} />

        <Route path="*" element={<Placeholder title="Không tìm thấy trang" />} />
      </Route>
    </Routes>
  )
}
