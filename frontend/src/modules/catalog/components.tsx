import { InputNumber, type InputNumberProps } from 'antd'

/** VND input with thousand separators ("1.234.567"). */
export function MoneyInput(props: InputNumberProps<number>) {
  return (
    <InputNumber<number>
      min={0}
      style={{ width: '100%' }}
      formatter={(v) => (v === undefined || v === null || `${v}` === '' ? '' : `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, '.'))}
      parser={(v) => Number((v ?? '').toString().replace(/\./g, ''))}
      {...props}
    />
  )
}
