import dayjs, { type Dayjs } from 'dayjs'

/** 1234567 -> "1.234.567" (VND, no decimals) */
export function money(value?: number | null): string {
  if (value === null || value === undefined) return ''
  return new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 0 }).format(value)
}

export function fmtDate(value?: string | Date | Dayjs | null): string {
  if (!value) return ''
  return dayjs(value).format('DD/MM/YYYY')
}

export function fmtDateTime(value?: string | Date | Dayjs | null): string {
  if (!value) return ''
  return dayjs(value).format('DD/MM/YYYY HH:mm')
}

/** Serialize a Dayjs as a local ISO string without timezone (backend stores "timestamp without time zone"). */
export function toLocalIso(value?: Dayjs | null): string | undefined {
  return value ? value.format('YYYY-MM-DDTHH:mm:ss') : undefined
}

export function toLocalDate(value?: Dayjs | null): string | undefined {
  return value ? value.format('YYYY-MM-DD') : undefined
}

export const SEX_OPTIONS = [
  { value: 0, label: 'Nam' },
  { value: 1, label: 'Nữ' },
]

export function sexLabel(sex?: number | null): string {
  return sex === 1 ? 'Nữ' : sex === 0 ? 'Nam' : ''
}

/** Column helper for AntD tables showing money right-aligned. */
export const moneyColumnProps = {
  align: 'right' as const,
  render: (v: number) => money(v),
}
