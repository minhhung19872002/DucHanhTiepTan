import { Table, type TableColumnsType } from 'antd'
import { PageTitle, useLoad } from '../../components/common'
import { money } from '../../utils/format'
import ReportFilterBar, { useReportParams } from './components/ReportFilterBar'
import StatBar from './components/StatBar'
import { reportsApi, type LaboStatRow } from './api'

/** Thống kê Labo theo nơi làm / loại (LaboStatisticsControl). */
export default function LaboStatsPage() {
  const { range, setRange, params } = useReportParams()
  const { data, loading } = useLoad(() => reportsApi.labo(params), [JSON.stringify(params)])

  const columns: TableColumnsType<LaboStatRow> = [
    { title: 'No', key: 'no', width: 50, render: (_, __, i) => i + 1 },
    { title: 'Labo', dataIndex: 'laboName' },
    { title: 'Loại', dataIndex: 'laboType' },
    { title: 'Số lượng', dataIndex: 'quantity', width: 100, align: 'right' },
    { title: 'Giá trung bình', dataIndex: 'averagePrice', width: 140, align: 'right', render: money },
    { title: 'Thành tiền', dataIndex: 'total', width: 140, align: 'right', render: money },
  ]

  return (
    <div className="page-card">
      <PageTitle title="Thống kê Labo" />
      <ReportFilterBar range={range} onChange={setRange} exportUrl="/reports/labo/export" exportFileName="thong-ke-labo.xlsx" exportParams={params} />
      <StatBar
        items={[
          { label: 'SL cố định', value: data?.quantityFixed ?? 0, plain: true },
          { label: 'Tiền cố định', value: data?.totalFixed ?? 0 },
          { label: 'SL tháo lắp', value: data?.quantityDisassembly ?? 0, plain: true },
          { label: 'Tiền tháo lắp', value: data?.totalDisassembly ?? 0 },
          { label: 'Tổng số lượng', value: data?.totalQuantity ?? 0, plain: true },
          { label: 'Tổng tiền', value: data?.totalPrice ?? 0, color: '#1f6fb2' },
        ]}
      />
      <Table size="small" rowKey={(r) => `${r.id}-${r.laboType}`} loading={loading} columns={columns} dataSource={data?.items ?? []} pagination={{ pageSize: 50 }} scroll={{ x: 'max-content' }} />
    </div>
  )
}
