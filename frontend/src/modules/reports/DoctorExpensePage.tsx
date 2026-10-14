import { Table, type TableColumnsType } from 'antd'
import { PageTitle, useLoad } from '../../components/common'
import { money } from '../../utils/format'
import ReportFilterBar, { useReportParams } from './components/ReportFilterBar'
import StatBar from './components/StatBar'
import { reportsApi, type DoctorExpenseRow } from './api'

/** Doanh thu và chi phí Labo của bác sĩ (DoctorExpenseControl). */
export default function DoctorExpensePage() {
  const { range, setRange, params } = useReportParams()
  const { data, loading } = useLoad(() => reportsApi.doctorExpense(params), [JSON.stringify(params)])
  const columns: TableColumnsType<DoctorExpenseRow> = [
    { title: 'No', key: 'no', width: 50, render: (_, __, i) => i + 1 },
    { title: 'Chi nhánh', dataIndex: 'branchName', width: 180 },
    { title: 'Bác sĩ', dataIndex: 'doctorFullName', sorter: (a, b) => a.doctorFullName.localeCompare(b.doctorFullName) },
    { title: 'Doanh thu', dataIndex: 'income', width: 140, align: 'right', render: money, sorter: (a, b) => a.income - b.income },
    { title: 'Chi phí Labo', dataIndex: 'expense', width: 140, align: 'right', render: money, sorter: (a, b) => a.expense - b.expense },
    { title: 'Doanh thu - Chi phí', dataIndex: 'incomeWoExpense', width: 160, align: 'right', render: money, sorter: (a, b) => a.incomeWoExpense - b.incomeWoExpense },
  ]
  return (
    <div className="page-card">
      <PageTitle title="Chi phí của bác sĩ" />
      <ReportFilterBar range={range} onChange={setRange} exportUrl="/reports/doctor-expense/export" exportFileName="chi-phi-bac-si.xlsx" exportParams={params} />
      <StatBar items={[{ label: 'Tổng doanh thu', value: data?.totalIncome ?? 0 }, { label: 'Tổng chi phí Labo', value: data?.totalExpense ?? 0, color: '#cf1322' }, { label: 'Doanh thu - Chi phí', value: data?.totalIncomeWoExpense ?? 0, color: '#389e0d' }]} />
      <Table size="small" rowKey="doctorName" loading={loading} columns={columns} dataSource={data?.items ?? []} pagination={false} scroll={{ x: 'max-content' }} />
    </div>
  )
}
