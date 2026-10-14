import { useState } from 'react'
import { DatePicker, Radio, Space } from 'antd'
import dayjs, { type Dayjs } from 'dayjs'

export type RangeMode = 'all' | 'today' | 'yesterday' | 'day' | 'month' | 'range' | 'thisMonth'

export interface DateRange {
  from?: string
  to?: string
}

/** Compute from/to (yyyy-MM-dd) the way the legacy radio-button filters did. */
export function computeRange(mode: RangeMode, day: Dayjs, month: Dayjs, range: [Dayjs, Dayjs] | null): DateRange {
  const d = (x: Dayjs) => x.format('YYYY-MM-DD')
  switch (mode) {
    case 'all':
      return {}
    case 'today':
      return { from: d(dayjs()), to: d(dayjs()) }
    case 'yesterday': {
      const y = dayjs().subtract(1, 'day')
      return { from: d(y), to: d(y) }
    }
    case 'day':
      return { from: d(day), to: d(day) }
    case 'month':
      return { from: d(month.startOf('month')), to: d(month.endOf('month')) }
    case 'thisMonth':
      return { from: d(dayjs().startOf('month')), to: d(dayjs().endOf('month')) }
    case 'range':
      return range ? { from: d(range[0]), to: d(range[1]) } : {}
  }
}

interface Props {
  value: DateRange
  onChange: (range: DateRange) => void
  /** Which modes to show (default mirrors ExpenseManagerControl). */
  modes?: RangeMode[]
  defaultMode?: RangeMode
}

const LABELS: Record<RangeMode, string> = {
  all: 'Tất cả',
  today: 'Hôm nay',
  yesterday: 'Hôm qua',
  day: 'Ngày',
  month: 'Tháng',
  thisMonth: 'Tháng này',
  range: 'Từ ngày',
}

/** Date range filter: radio modes + pickers; emits from/to strings. */
export default function DateRangeFilter({ onChange, modes = ['all', 'today', 'yesterday', 'day', 'month', 'range'], defaultMode = 'all' }: Props) {
  const [mode, setMode] = useState<RangeMode>(defaultMode)
  const [day, setDay] = useState<Dayjs>(dayjs())
  const [month, setMonth] = useState<Dayjs>(dayjs())
  const [range, setRange] = useState<[Dayjs, Dayjs] | null>([dayjs().startOf('month'), dayjs()])

  const emit = (m: RangeMode, d = day, mo = month, r = range) => {
    setMode(m)
    onChange(computeRange(m, d, mo, r))
  }

  return (
    <Space wrap>
      <Radio.Group value={mode} onChange={(e) => emit(e.target.value as RangeMode)} optionType="button" buttonStyle="solid" size="small"
        options={modes.map((m) => ({ value: m, label: LABELS[m] }))} />
      {modes.includes('day') && (
        <DatePicker size="small" value={day} format="DD/MM/YYYY" allowClear={false}
          onChange={(v) => { if (v) { setDay(v); emit('day', v) } }} />
      )}
      {modes.includes('month') && (
        <DatePicker size="small" picker="month" value={month} format="MM/YYYY" allowClear={false}
          onChange={(v) => { if (v) { setMonth(v); emit('month', day, v) } }} />
      )}
      {modes.includes('range') && (
        <DatePicker.RangePicker size="small" value={range} format="DD/MM/YYYY" allowClear={false}
          onChange={(v) => { if (v && v[0] && v[1]) { const r: [Dayjs, Dayjs] = [v[0], v[1]]; setRange(r); emit('range', day, month, r) } }} />
      )}
    </Space>
  )
}
