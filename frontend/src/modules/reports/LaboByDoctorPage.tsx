import PivotReportPage from './components/PivotReportPage'
import { reportsApi } from './api'

/** Labo của bác sĩ theo ngày (LaboOfDoctorControl). */
export default function LaboByDoctorPage() {
  return (
    <PivotReportPage
      title="Labo theo bác sĩ"
      firstColumn="Ngày"
      exportUrl="/reports/labo-by-doctor/export"
      exportFileName="labo-theo-bac-si.xlsx"
      loader={reportsApi.laboByDoctor}
    />
  )
}
