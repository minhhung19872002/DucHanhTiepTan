import { Table, type TableColumnsType } from 'antd'
import { money } from '../../../utils/format'
import type { PivotResult, PivotRow } from '../api'

interface Props {
  data?: PivotResult
  loading?: boolean
  /** Header of the first column ("Ngày", "Dịch vụ", ...). */
  firstColumn: string
}

/** Dynamic-column grid with a bold totals row, like the legacy DataTable pivots (doctor/branch/service income). */
export default function PivotTable({ data, loading, firstColumn }: Props) {
  const columns: TableColumnsType<PivotRow> = [
    { title: 'No', key: 'no', width: 50, render: (_, __, i) => (i < (data?.rows.length ?? 0) ? i + 1 : ''), fixed: 'left' },
    { title: firstColumn, dataIndex: 'label', key: 'label', width: 140, fixed: 'left', render: (v, r) => (r.key === '__total' ? <b>{v}</b> : v) },
    ...(data?.columns ?? []).map((c) => ({
      title: c.label,
      key: c.key,
      align: 'right' as const,
      width: 130,
      render: (_: unknown, r: PivotRow) => {
        const v = r.values[c.key]
        return r.key === '__total' ? <b>{money(v ?? 0)}</b> : v === undefined ? '' : money(v)
      },
    })),
    { title: 'Tổng', dataIndex: 'total', key: 'total', align: 'right', width: 130, fixed: 'right', render: (v, r) => (r.key === '__total' ? <b>{money(v)}</b> : money(v)) },
  ]
  const rows: PivotRow[] = data ? [...data.rows, { key: '__total', label: 'Tổng cộng', values: data.columnTotals, total: data.grandTotal }] : []
  return (
    <Table<PivotRow>
      size="small"
      rowKey="key"
      loading={loading}
      columns={columns}
      dataSource={rows}
      pagination={false}
      scroll={{ x: 'max-content', y: 'calc(100vh - 260px)' }}
      rowClassName={(r) => (r.key === '__total' ? 'pivot-total-row' : '')}
    />
  )
}
