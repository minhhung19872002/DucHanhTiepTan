import { Button, DatePicker, Input, InputNumber, Select, Space, Table, Typography } from 'antd'
import { DeleteOutlined, PlusOutlined } from '@ant-design/icons'
import dayjs from 'dayjs'
import type { Consulting, Lookups, Ratio } from './api'
import { discountedPrice, teethCount } from './api'
import LaboPopover from './LaboPopover'
import { money, toLocalIso } from '../../utils/format'

interface Props {
  rows: Consulting[]
  onChange: (rows: Consulting[]) => void
  lookups: Lookups
  ratio: Ratio
  readOnly?: boolean
}

/** Port of ConsultingUserControl ("Tư vấn"): planned services, only editable on the first appointment. */
export default function ConsultingTable({ rows, onChange, lookups, ratio, readOnly }: Props) {
  const update = (i: number, patch: Partial<Consulting>) => onChange(rows.map((r, idx) => (idx === i ? { ...r, ...patch } : r)))
  const remove = (i: number) => onChange(rows.filter((_, idx) => idx !== i))
  const add = () =>
    onChange([...rows, { subId: 0, serviceId: 0, originalPrice: 0, price: 0, createdDate: dayjs().startOf('day').format('YYYY-MM-DDTHH:mm:ss'), comment: '', quantity: 1, toothPositions: '', labo: null }])
  const qtyOf = (r: Consulting) => (teethCount(r.toothPositions) > 0 ? teethCount(r.toothPositions) : r.quantity)
  const total = rows.reduce((s, r) => s + r.price * qtyOf(r), 0)

  const columns = [
    { title: 'No', width: 40, render: (_: unknown, __: Consulting, i: number) => i + 1 },
    {
      title: 'Ngày', width: 120,
      render: (_: unknown, r: Consulting, i: number) => (
        <DatePicker size="small" format="DD/MM/YYYY" allowClear={false} value={dayjs(r.createdDate)} onChange={(d) => update(i, { createdDate: (d && toLocalIso(d.startOf("day"))) || r.createdDate })} disabled={readOnly} />
      ),
    },
    {
      title: 'Dịch vụ', width: 240,
      render: (_: unknown, r: Consulting, i: number) => (
        <Select size="small" showSearch optionFilterProp="label" style={{ width: '100%' }} placeholder="Chọn dịch vụ" value={r.serviceId || undefined}
          options={lookups.services.map((s) => ({ value: s.id, label: s.name }))}
          onChange={(id) => { const s = lookups.services.find((x) => x.id === id)!; update(i, { serviceId: id, originalPrice: s.price, price: discountedPrice(s.price, id, ratio) }) }}
          disabled={readOnly} />
      ),
    },
    { title: 'Số răng', width: 110, render: (_: unknown, r: Consulting, i: number) => <Input size="small" value={r.toothPositions ?? ''} onChange={(e) => update(i, { toothPositions: e.target.value })} disabled={readOnly} /> },
    {
      title: 'Số lượng', width: 80,
      render: (_: unknown, r: Consulting, i: number) => <InputNumber size="small" min={1} value={qtyOf(r)} onChange={(v) => update(i, { quantity: v ?? 1 })} disabled={readOnly || teethCount(r.toothPositions) > 0} style={{ width: '100%' }} />,
    },
    { title: 'Giá gốc', width: 100, align: 'right' as const, render: (_: unknown, r: Consulting) => money(r.originalPrice) },
    {
      title: 'Giá', width: 110,
      render: (_: unknown, r: Consulting, i: number) => (
        <InputNumber size="small" min={0} step={10000} value={r.price} onChange={(v) => update(i, { price: v ?? 0 })} disabled={readOnly} style={{ width: '100%' }} formatter={(v) => money(Number(v))} parser={(v) => Number((v ?? '').replace(/\D/g, ''))} />
      ),
    },
    { title: 'Labo', width: 150, render: (_: unknown, r: Consulting, i: number) => <LaboPopover value={r.labo} onChange={(l) => update(i, { labo: l })} laboTypes={lookups.laboTypes} laboVendors={lookups.laboVendors} disabled={readOnly} /> },
    { title: 'Ghi chú', width: 200, render: (_: unknown, r: Consulting, i: number) => <Input size="small" value={r.comment ?? ''} onChange={(e) => update(i, { comment: e.target.value })} disabled={readOnly} /> },
    { title: 'Tổng tiền', width: 110, align: 'right' as const, render: (_: unknown, r: Consulting) => <b>{money(r.price * qtyOf(r))}</b> },
    ...(readOnly ? [] : [{ title: '', width: 40, render: (_: unknown, __: Consulting, i: number) => <Button size="small" danger type="text" icon={<DeleteOutlined />} onClick={() => remove(i)} /> }]),
  ]

  return (
    <div>
      <Table size="small" rowKey={(_, i) => String(i)} columns={columns} dataSource={rows} pagination={false} scroll={{ x: 1300 }} />
      <Space style={{ marginTop: 8, justifyContent: 'space-between', width: '100%' }}>
        {!readOnly ? <Button size="small" icon={<PlusOutlined />} onClick={add}>Thêm dịch vụ tư vấn</Button> : <span />}
        <Typography.Text>Tổng tiền: <b>{money(total)}</b></Typography.Text>
      </Space>
    </div>
  )
}
