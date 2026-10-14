import PivotReportPage from './components/PivotReportPage'
import { reportsApi } from './api'

/** Doanh thu dịch vụ theo chi nhánh (ServiceInBranchControl): nhóm dịch vụ × chi nhánh. */
export default function ServiceIncomePage() {
  return (
    <PivotReportPage
      title="Doanh thu theo dịch vụ"
      firstColumn="Dịch vụ"
      exportUrl="/reports/service-income/export"
      exportFileName="doanh-thu-dich-vu.xlsx"
      loader={reportsApi.serviceIncome}
    />
  )
}
