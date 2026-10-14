import { useState } from 'react'
import { DatePicker, InputNumber, Radio, Space } from 'antd'
import dayjs, { type Dayjs } from 'dayjs'
import type { DateFilter } from './api'

export type RangeMode = 'all' | 'today' | 'yesterday' | 'day' | 'month' | 'range'

export interface RangeState {
  mode: RangeMode
  day: Dayjs
  month: Dayjs
  from: Dayjs
  to: Dayjs
  limit: number
}

export const defaultRange = (): RangeState => ({ mode: 'all', day: dayjs(), month: dayjs(), from: dayjs().startOf('month'), to: dayjs(), limit: 200 })

/** Convert the legacy radio filter (Tất cả / Hôm nay / Hôm qua / Ngày / Tháng / Từ-đến) into API params. */
export function toDateParams(s: RangeState): DateFilter {
  const fmt = (d: Dayjs) => d.format('YYYY-MM-DD')
  switch (s.mode) {
    case 'today':
      return { from: fmt(dayjs()), to: fmt(dayjs()) }
    case 'yesterday':
      return { from: fmt(dayjs().subtract(1, 'day')), to: fmt(dayjs().subtract(1, 'day')) }
    case 'day':
      return { from: fmt(s.day), to: fmt(s.day) }
    case 'month':
      return { from: fmt(s.month.startOf('month')), to: fmt(s.month.endOf('month')) }
    case 'range':
      return { from: fmt(s.from), to: fmt(s.to) }
    default:
      return {}
  }
}

export function useRangeState(initial?: Partial<RangeState>) {
  return useState<RangeState>({ ...defaultRange(), ...initial })
}

export default function DateRangeFilter({ value, onChange, showLimit = true }: { value: RangeState; onChange: (v: RangeState) => void; showLimit?: boolean }) {
  const set = (patch: Partial<RangeState>) => onChange({ ...value, ...patch })
  return (
    <Space wrap size="middle">
      <Radio.Group value={value.mode} onChange={(e) => set({ mode: e.target.value })} optionType="button" buttonStyle="solid" size="small"
        options={[
          { value: 'all', label: 'Tất cả' },
          { value: 'today', label: 'Hôm nay' },
          { value: 'yesterday', label: 'Hôm qua' },
          { value: 'day', label: 'Ngày' },
          { value: 'month', label: 'Tháng' },
          { value: 'range', label: 'Từ ngày' },
        ]}
      />
      {value.mode === 'day' && <DatePicker size="small" value={value.day} format="DD/MM/YYYY" allowClear={false} onChange={(d) => d && set({ day: d })} />}
      {value.mode === 'month' && <DatePicker size="small" picker="month" value={value.month} format="MM/YYYY" allowClear={false} onChange={(d) => d && set({ month: d })} />}
      {value.mode === 'range' && (
        <DatePicker.RangePicker
          size="small"
          value={[value.from, value.to]}
          format="DD/MM/YYYY"
          allowClear={false}
          onChange={(v) => v && v[0] && v[1] && set({ from: v[0], to: v[1] })}
        />
      )}
      {showLimit && (
        <span>
          Giới hạn: <InputNumber size="small" min={1} max={5000} value={value.limit} onChange={(v) => set({ limit: v ?? 200 })} style={{ width: 90 }} />
        </span>
      )}
    </Space>
  )
}
