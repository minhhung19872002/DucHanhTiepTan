import { useMemo } from 'react'
import { Button, DatePicker, Input, InputNumber, Select, Space, Table, Tooltip, Typography } from 'antd'
import { DeleteOutlined, PlusOutlined } from '@ant-design/icons'
import dayjs from 'dayjs'
import type { Lookups, Ratio, Treatment } from './api'
import { discountedPrice, teethCount } from './api'
import LaboPopover from './LaboPopover'
import { money, toLocalIso } from '../../utils/format'

interface Props {
  rows: Treatment[]
  onChange: (rows: Treatment[]) => void
  lookups: Lookups
  ratio: Ratio
  previousDebt: number
  appointmentNo: number
  readOnly?: boolean
}

export interface ComputedTreatment extends Treatment {
  key: number
  realPreviousDebt: number
  qty: number
  total: number
  remain: number
}

/** Port of TreatmentItemAdapter chain: each line carries the remain of the previous one. */
export function computeChain(rows: Treatment[], previousDebt: number): ComputedTreatment[] {
  let prev = previousDebt
  return rows.map((r, i) => {
    const t = teethCount(r.toothPositions)
    const qty = t > 0 ? t : r.quantity
    const total = (r.isCancelled ? 0 : r.price * qty) + prev
    const remain = total - r.paid
    const c: ComputedTreatment = { ...r, key: i, realPreviousDebt: prev, qty, total, remain }
    prev = remain
    return c
  })
}

export default function TreatmentTable({ rows, onChange, lookups, ratio, previousDebt, appointmentNo, readOnly }: Props) {
  const data = useMemo(() => computeChain(rows, previousDebt), [rows, previousDebt])
  const update = (i: number, patch: Partial<Treatment>) => onChange(rows.map((r, idx) => (idx === i ? { ...r, ...patch } : r)))
  const remove = (i: number) => onChange(rows.filter((_, idx) => idx !== i))
  const add = () =>
    onChange([
      ...rows,
      {
        subId: 0, appointmentNo, serviceId: 0, originalPrice: 0, price: 0, createdDate: dayjs().startOf('day').format('YYYY-MM-DDTHH:mm:ss'),
        paid: 0, doctorName: null, assistantName: null, comment: '', quantity: 1, toothPositions: '', previousDebt: 0, isCancelled: false, labo: null, laboId: null,
      },
    ])

  const paidSum = data.reduce((s, r) => s + r.paid, 0)
  const lastRemain = data.length ? Math.max(0, data[data.length - 1].remain) : 0

  const columns = [
    { title: 'No', width: 40, render: (_: unknown, __: ComputedTreatment, i: number) => i + 1 },
    {
      title: 'Ngày', width: 120,
      render: (_: unknown, r: ComputedTreatment, i: number) => (
        <DatePicker size="small" format="DD/MM/YYYY" allowClear={false} value={dayjs(r.createdDate)} onChange={(d) => update(i, { createdDate: (d && toLocalIso(d.startOf("day"))) || r.createdDate })} disabled={readOnly} />
      ),
    },
    {
      title: 'Dịch vụ', width: 220,
      render: (_: unknown, r: ComputedTreatment, i: number) => (
        <Select
          size="small" showSearch optionFilterProp="label" style={{ width: '100%' }} placeholder="Chọn dịch vụ"
          value={r.serviceId || undefined}
          options={lookups.services.map((s) => ({ value: s.id, label: s.name }))}
          onChange={(id) => {
            const s = lookups.services.find((x) => x.id === id)!
            update(i, { serviceId: id, originalPrice: s.price, price: discountedPrice(s.price, id, ratio) })
          }}
          disabled={readOnly}
        />
      ),
    },
    {
      title: 'Số răng', width: 110,
      render: (_: unknown, r: ComputedTreatment, i: number) => (
        <Tooltip title="VD: 11,12 hoặc 21-23 (cầu răng)">
          <Input size="small" value={r.toothPositions ?? ''} onChange={(e) => update(i, { toothPositions: e.target.value })} disabled={readOnly} />
        </Tooltip>
      ),
    },
    {
      title: 'Số lượng', width: 80,
      render: (_: unknown, r: ComputedTreatment, i: number) => (
        <InputNumber size="small" min={1} value={r.qty} onChange={(v) => update(i, { quantity: v ?? 1 })} disabled={readOnly || teethCount(r.toothPositions) > 0} style={{ width: '100%' }} />
      ),
    },
    { title: 'Giá gốc', width: 100, align: 'right' as const, render: (_: unknown, r: ComputedTreatment) => money(r.originalPrice) },
    {
      title: 'Giá', width: 110,
      render: (_: unknown, r: ComputedTreatment, i: number) => (
        <InputNumber size="small" min={0} step={10000} value={r.price} onChange={(v) => update(i, { price: v ?? 0 })} disabled={readOnly} style={{ width: '100%' }} formatter={(v) => money(Number(v))} parser={(v) => Number((v ?? '').replace(/\D/g, ''))} />
      ),
    },
    {
      title: 'Bác sĩ', width: 150,
      render: (_: unknown, r: ComputedTreatment, i: number) => (
        <Select size="small" allowClear showSearch optionFilterProp="label" style={{ width: '100%' }} value={r.doctorName ?? undefined}
          options={lookups.doctors.map((d) => ({ value: d.userName, label: d.fullName }))} onChange={(v) => update(i, { doctorName: v ?? null })} disabled={readOnly} />
      ),
    },
    {
      title: 'Phụ tá', width: 150,
      render: (_: unknown, r: ComputedTreatment, i: number) => (
        <Select size="small" allowClear showSearch optionFilterProp="label" style={{ width: '100%' }} value={r.assistantName ?? undefined}
          options={lookups.assistants.map((d) => ({ value: d.userName, label: d.fullName }))} onChange={(v) => update(i, { assistantName: v ?? null })} disabled={readOnly} />
      ),
    },
    {
      title: 'Labo', width: 150,
      render: (_: unknown, r: ComputedTreatment, i: number) => (
        <LaboPopover value={r.labo} onChange={(l) => update(i, { labo: l })} laboTypes={lookups.laboTypes} laboVendors={lookups.laboVendors} disabled={readOnly} />
      ),
    },
    {
      title: 'Ghi chú', width: 160,
      render: (_: unknown, r: ComputedTreatment, i: number) => <Input size="small" value={r.comment ?? ''} onChange={(e) => update(i, { comment: e.target.value })} disabled={readOnly} />,
    },
    { title: 'Tổng tiền', width: 110, align: 'right' as const, render: (_: unknown, r: ComputedTreatment) => <b>{money(r.total)}</b> },
    {
      title: 'Thanh toán', width: 120,
      render: (_: unknown, r: ComputedTreatment, i: number) => (
        <Tooltip title="Nhấp đúp để thanh toán hết">
          <InputNumber size="small" min={0} step={10000} value={r.paid} onChange={(v) => update(i, { paid: v ?? 0 })} disabled={readOnly} style={{ width: '100%' }}
            onDoubleClick={() => !readOnly && update(i, { paid: r.total })} formatter={(v) => money(Number(v))} parser={(v) => Number((v ?? '').replace(/\D/g, ''))} />
        </Tooltip>
      ),
    },
    {
      title: 'Còn lại', width: 110, align: 'right' as const,
      render: (_: unknown, r: ComputedTreatment, i: number) => (
        <Tooltip title="Nhấp đúp để hủy thanh toán">
          <span style={{ color: r.remain > 0 ? '#c41d7f' : undefined, cursor: 'pointer' }} onDoubleClick={() => !readOnly && update(i, { paid: 0 })}>{money(r.remain)}</span>
        </Tooltip>
      ),
    },
    ...(readOnly ? [] : [{ title: '', width: 40, render: (_: unknown, __: ComputedTreatment, i: number) => <Button size="small" danger type="text" icon={<DeleteOutlined />} onClick={() => remove(i)} /> }]),
  ]

  return (
    <div>
      <Table<ComputedTreatment> size="small" rowKey="key" columns={columns} dataSource={data} pagination={false} scroll={{ x: 1800 }} />
      <Space style={{ marginTop: 8, justifyContent: 'space-between', width: '100%' }}>
        {!readOnly ? <Button size="small" icon={<PlusOutlined />} onClick={add}>Thêm dịch vụ</Button> : <span />}
        <Space size="large">
          <Typography.Text>Tổng tiền: <b>{money(paidSum + lastRemain)}</b></Typography.Text>
          <Typography.Text>Thanh toán: <b>{money(paidSum)}</b></Typography.Text>
          <Typography.Text>Còn lại: <b style={{ color: lastRemain > 0 ? '#c41d7f' : undefined }}>{money(lastRemain)}</b></Typography.Text>
        </Space>
      </Space>
    </div>
  )
}
