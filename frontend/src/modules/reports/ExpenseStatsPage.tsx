import { useState } from 'react'
import { Radio, Table, type TableColumnsType } from 'antd'
import { PageTitle, useLoad } from '../../components/common'
import { fmtDate, fmtDateTime, money } from '../../utils/format'
import ReportFilterBar, { useReportParams } from './components/ReportFilterBar'
import StatBar from './components/StatBar'
import { reportsApi, type ExpenseMode, type ExpenseStatRow } from './api'

/** Thống kê chi phí: chi tiết / theo nội dung / theo ngày (ExpenseStatisticsControl). */
export default function ExpenseStatsPage() {
  const { range, setRange, params } = useReportParams()
  const [mode, setMode] = useState<ExpenseMode>('detail')
  const full = { ...params, mode }
  const { data, loading } = useLoad(() => reportsApi.expenses(full), [JSON.stringify(full)])

  const no = { title: 'No', key: 'no', width: 50, render: (_: unknown, __: unknown, i: number) => i + 1 }
  const amount = { title: 'Số tiền', dataIndex: 'amount', width: 140, align: 'right' as const, render: money }
  const columns: TableColumnsType<ExpenseStatRow & { _k: number }> =
    mode === 'detail'
      ? [
          no,
          { title: 'Ngày', dataIndex: 'createdDate', width: 140, render: fmtDateTime },
          { title: 'Chi nhánh', dataIndex: 'branchName', width: 160 },
          { title: 'Người chi', dataIndex: 'accountantFullName', width: 160 },
          { title: 'Nội dung', dataIndex: 'description' },
          amount,
        ]
      : mode === 'content'
        ? [no, { title: 'Nội dung', dataIndex: 'description' }, amount]
        : [no, { title: 'Ngày', dataIndex: 'createdDate', width: 140, render: fmtDate }, amount]

  return (
    <div className="page-card">
      <PageTitle title="Thống kê chi phí" />
      <ReportFilterBar range={range} onChange={setRange} exportUrl="/reports/expenses/export" exportFileName="thong-ke-chi-phi.xlsx" exportParams={full}>
        <Radio.Group size="small" value={mode} onChange={(e) => setMode(e.target.value)}>
          <Radio value="detail">Chi tiết</Radio>
          <Radio value="content">Theo nội dung</Radio>
          <Radio value="date">Theo ngày</Radio>
        </Radio.Group>
      </ReportFilterBar>
      <StatBar items={[{ label: 'Tổng chi', value: data?.total ?? 0, color: '#cf1322' }]} />
      <Table
        size="small"
        rowKey="_k"
        loading={loading}
        columns={columns}
        dataSource={(data?.items ?? []).map((r, i) => ({ ...r, _k: i }))}
        pagination={{ pageSize: 50, showSizeChanger: true }}
        scroll={{ x: 'max-content' }}
      />
    </div>
  )
}
