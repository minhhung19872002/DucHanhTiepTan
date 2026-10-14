import PivotReportPage from './components/PivotReportPage'
import { reportsApi } from './api'

/** Doanh thu bác sĩ theo ngày (DoctorIncomeControl). */
export default function DoctorIncomePage() {
  return (
    <PivotReportPage
      title="Doanh thu bác sĩ theo ngày"
      firstColumn="Ngày"
      exportUrl="/reports/doctor-income/export"
      exportFileName="doanh-thu-bac-si.xlsx"
      loader={reportsApi.doctorIncome}
    />
  )
}
