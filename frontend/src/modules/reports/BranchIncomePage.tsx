import { useState } from 'react'
import { Radio } from 'antd'
import PivotReportPage from './components/PivotReportPage'
import { reportsApi } from './api'

/** Doanh thu chi nhánh theo ngày (BranchIncomeControl): theo điều trị hoặc theo tiền thu. */
export default function BranchIncomePage() {
  const [mode, setMode] = useState<'treatment' | 'payment'>('treatment')
  return (
    <PivotReportPage
      title="Doanh thu chi nhánh theo ngày"
      firstColumn="Ngày"
      exportUrl="/reports/branch-income/export"
      exportFileName="doanh-thu-chi-nhanh.xlsx"
      loader={reportsApi.branchIncome}
      extra={{ mode }}
      controls={
        <Radio.Group size="small" value={mode} onChange={(e) => setMode(e.target.value)}>
          <Radio value="treatment">Theo điều trị</Radio>
          <Radio value="payment">Theo tiền thu</Radio>
        </Radio.Group>
      }
    />
  )
}
