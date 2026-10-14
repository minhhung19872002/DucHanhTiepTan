import { Space, Statistic } from 'antd'
import { money } from '../../../utils/format'

export interface StatItem {
  label: string
  value: number
  /** Format as plain number (quantities) instead of money. */
  plain?: boolean
  color?: string
}

/** Row of totals shown under the legacy grids (labelTotal / labelPaid / labelRemain ...). */
export default function StatBar({ items }: { items: StatItem[] }) {
  return (
    <Space size={32} wrap style={{ marginBottom: 12 }}>
      {items.map((s) => (
        <Statistic key={s.label} title={s.label} value={s.plain ? s.value : money(s.value)} valueStyle={{ fontSize: 18, color: s.color }} />
      ))}
    </Space>
  )
}
