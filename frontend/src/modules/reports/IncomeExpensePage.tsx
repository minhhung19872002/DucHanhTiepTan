import { Table, type TableColumnsType } from 'antd'
import { PageTitle, useLoad } from '../../components/common'
import { fmtDate, money } from '../../utils/format'
import ReportFilterBar, { useReportParams } from './components/ReportFilterBar'
import StatBar from './components/StatBar'
import { reportsApi, type IncomeExpenseRow } from './api'

/** Doanh thu - Chi phí theo ngày (IncomeExpenseStatisticsControl). */
export default function IncomeExpensePage() {
  const { range, setRange, params } = useReportParams('month')
  const { data, loading } = useLoad(() => reportsApi.incomeExpense(params), [JSON.stringify(params)])

  const columns: TableColumnsType<IncomeExpenseRow> = [
    { title: 'No', key: 'no', width: 50, render: (_, __, i) => i + 1 },
    { title: 'Ngày', dataIndex: 'createdDate', width: 110, render: fmtDate },
    { title: 'Doanh thu', dataIndex: 'income', width: 140, align: 'right', render: money },
    { title: 'Chi phí', dataIndex: 'expense', width: 140, align: 'right', render: money },
    {
      title: 'Doanh thu - Chi phí',
      dataIndex: 'subtract',
      width: 160,
      align: 'right',
      render: (v: number) => <span style={{ color: v < 0 ? '#cf1322' : undefined }}>{money(v)}</span>,
    },
    { title: 'Ghi chú', dataIndex: 'description' },
  ]

  return (
    <div className="page-card">
      <PageTitle title="Doanh thu - Chi phí" />
      <ReportFilterBar range={range} onChange={setRange} exportUrl="/reports/income-expense/export" exportFileName="doanh-thu-chi-phi.xlsx" exportParams={params} />
      <StatBar
        items={[
          { label: 'Doanh thu', value: data?.income ?? 0, color: '#389e0d' },
          { label: 'Chi phí', value: data?.expense ?? 0, color: '#cf1322' },
          { label: 'Doanh thu - Chi phí', value: data?.incomeWoExpense ?? 0 },
        ]}
      />
      <Table size="small" rowKey="createdDate" loading={loading} columns={columns} dataSource={data?.items ?? []} pagination={{ pageSize: 62 }} scroll={{ x: 'max-content' }} />
    </div>
  )
}
