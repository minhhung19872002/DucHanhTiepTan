import { Button, Checkbox, DatePicker, Form, Input, InputNumber, Popover, Select, Space } from 'antd'
import dayjs from 'dayjs'
import type { IdName, Labo } from './api'
import { toLocalIso } from '../../utils/format'

interface Props {
  value?: Labo | null
  onChange: (labo: Labo | null) => void
  laboTypes: IdName[]
  laboVendors: IdName[]
  disabled?: boolean
}

export const emptyLabo = (): Labo => ({ laboVendorId: null, laboTypeId: null, color: '', receiverDate: null, deliverDate: null, delivered: false, price: 0 })

export function laboLabel(labo: Labo | null | undefined, laboTypes: IdName[], laboVendors: IdName[]): string {
  if (!labo || !labo.laboVendorId || !labo.laboTypeId) return ''
  const v = laboVendors.find((x) => x.id === labo.laboVendorId)?.name ?? ''
  const t = laboTypes.find((x) => x.id === labo.laboTypeId)?.name ?? ''
  return `${v}, ${t}`
}

/** Port of LaboEditorPopup: edits the labo attached to a treatment/consulting line. */
export default function LaboPopover({ value, onChange, laboTypes, laboVendors, disabled }: Props) {
  const labo = value ?? emptyLabo()
  const set = (patch: Partial<Labo>) => onChange({ ...labo, ...patch })
  const label = laboLabel(labo, laboTypes, laboVendors)

  const content = (
    <Form layout="horizontal" size="small" labelCol={{ span: 8 }} style={{ width: 340 }}>
      <Form.Item label="Nha khoa">
        <Select
          allowClear
          value={labo.laboVendorId ?? undefined}
          onChange={(v) => set({ laboVendorId: v ?? null })}
          options={laboVendors.map((x) => ({ value: x.id, label: x.name }))}
          disabled={disabled}
        />
      </Form.Item>
      <Form.Item label="Chủng loại">
        <Select
          allowClear
          value={labo.laboTypeId ?? undefined}
          onChange={(v) => set({ laboTypeId: v ?? null })}
          options={laboTypes.map((x) => ({ value: x.id, label: x.name }))}
          disabled={disabled}
        />
      </Form.Item>
      <Form.Item label="Màu sắc">
        <Input value={labo.color ?? ''} onChange={(e) => set({ color: e.target.value })} disabled={disabled} />
      </Form.Item>
      <Form.Item label="Ngày nhận">
        <DatePicker format="DD/MM/YYYY" value={labo.receiverDate ? dayjs(labo.receiverDate) : dayjs()} onChange={(d) => set({ receiverDate: toLocalIso(d) })} disabled={disabled} />
      </Form.Item>
      <Form.Item label="Ngày giao">
        <DatePicker format="DD/MM/YYYY" value={labo.deliverDate ? dayjs(labo.deliverDate) : dayjs()} onChange={(d) => set({ deliverDate: toLocalIso(d) })} disabled={disabled} />
      </Form.Item>
      <Form.Item label="Giá tiền">
        <InputNumber min={0} step={10000} value={labo.price} onChange={(v) => set({ price: v ?? 0 })} style={{ width: '100%' }} disabled={disabled} />
      </Form.Item>
      <Form.Item label=" " colon={false}>
        <Space>
          <Checkbox checked={labo.delivered} onChange={(e) => set({ delivered: e.target.checked })} disabled={disabled}>
            Xác nhận đã giao
          </Checkbox>
          <Button size="small" onClick={() => onChange(null)} disabled={disabled}>
            Xóa
          </Button>
        </Space>
      </Form.Item>
    </Form>
  )

  return (
    <Popover content={content} trigger="click" title="Labo">
      <Button size="small" type={label ? 'primary' : 'default'} ghost={!!label}>
        {label || 'Labo'}
      </Button>
    </Popover>
  )
}
