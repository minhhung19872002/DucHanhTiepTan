import { useMemo, useState, type ReactNode } from 'react'
import { DatePicker, Radio, Space } from 'antd'
import dayjs, { type Dayjs } from 'dayjs'
import { BranchSelect, ExcelButton } from '../../../components/common'
import { useAuth } from '../../../auth/AuthContext'
import type { RangeParams } from '../api'

/** Legacy radio group: Tất cả / Hôm nay / Hôm qua / Ngày / Tháng / Từ ngày – đến ngày. */
export type RangeMode = 'all' | 'today' | 'yesterday' | 'day' | 'month' | 'range'

export interface RangeState {
  mode: RangeMode
  day: Dayjs
  month: Dayjs
  from: Dayjs
  to: Dayjs
}

export function defaultRange(mode: RangeMode = 'all'): RangeState {
  const today = dayjs()
  return { mode, day: today, month: today, from: today.startOf('month'), to: today }
}

/** Resolve the radio state into from/to query params (undefined = all). */
export function rangeToParams(r: RangeState): { from?: string; to?: string } {
  const f = 'YYYY-MM-DD'
  switch (r.mode) {
    case 'all':
      return {}
    case 'today':
      return { from: dayjs().format(f), to: dayjs().format(f) }
    case 'yesterday': {
      const y = dayjs().subtract(1, 'day').format(f)
      return { from: y, to: y }
    }
    case 'day':
      return { from: r.day.format(f), to: r.day.format(f) }
    case 'month':
      return { from: r.month.startOf('month').format(f), to: r.month.endOf('month').format(f) }
    default:
      return { from: r.from.format(f), to: r.to.format(f) }
  }
}

/** Hook used by every report page: range state + global branch filter → RangeParams. */
export function useReportParams(initialMode: RangeMode = 'all') {
  const [range, setRange] = useState<RangeState>(() => defaultRange(initialMode))
  const { branchId } = useAuth()
  const params: RangeParams = useMemo(() => ({ ...rangeToParams(range), branchId }), [range, branchId])
  return { range, setRange, params }
}

interface Props {
  range: RangeState
  onChange: (r: RangeState) => void
  /** Excel export endpoint (relative to /api) – renders the legacy "Lưu Excel" button. */
  exportUrl?: string
  exportFileName?: string
  exportParams?: Record<string, unknown>
  /** Extra filters rendered after the date filter (e.g. mode radios). */
  children?: ReactNode
  showBranch?: boolean
}

export default function ReportFilterBar({ range, onChange, exportUrl, exportFileName, exportParams, children, showBranch = true }: Props) {
  const set = (patch: Partial<RangeState>) => onChange({ ...range, ...patch })
  return (
    <div className="toolbar no-print">
      <Radio.Group value={range.mode} onChange={(e) => set({ mode: e.target.value })} optionType="button" buttonStyle="solid" size="small">
        <Radio.Button value="all">Tất cả</Radio.Button>
        <Radio.Button value="today">Hôm nay</Radio.Button>
        <Radio.Button value="yesterday">Hôm qua</Radio.Button>
        <Radio.Button value="day">Ngày</Radio.Button>
        <Radio.Button value="month">Tháng</Radio.Button>
        <Radio.Button value="range">Khoảng</Radio.Button>
      </Radio.Group>
      {range.mode === 'day' && <DatePicker size="small" value={range.day} format="DD/MM/YYYY" allowClear={false} onChange={(d) => d && set({ day: d })} />}
      {range.mode === 'month' && <DatePicker size="small" picker="month" value={range.month} format="MM/YYYY" allowClear={false} onChange={(d) => d && set({ month: d })} />}
      {range.mode === 'range' && (
        <Space size={4}>
          <span>Từ</span>
          <DatePicker size="small" value={range.from} format="DD/MM/YYYY" allowClear={false} onChange={(d) => d && set({ from: d })} />
          <span>đến</span>
          <DatePicker size="small" value={range.to} format="DD/MM/YYYY" allowClear={false} onChange={(d) => d && set({ to: d })} />
        </Space>
      )}
      {showBranch && <BranchSelect style={{ width: 200 }} />}
      {children}
      {exportUrl && (
        <span style={{ marginLeft: 'auto' }}>
          <ExcelButton url={exportUrl} fileName={exportFileName ?? 'bao-cao.xlsx'} params={exportParams} />
        </span>
      )}
    </div>
  )
}
