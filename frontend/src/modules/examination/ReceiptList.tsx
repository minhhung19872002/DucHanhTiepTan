import { useState } from 'react'
import { AutoComplete, Button, InputNumber, Select, Space, Typography } from 'antd'
import { DeleteOutlined, PlusOutlined } from '@ant-design/icons'
import type { DrugLookup, Receipt } from './api'
import { money } from '../../utils/format'

interface Props {
  rows: Receipt[]
  onChange: (rows: Receipt[]) => void
  drugs: DrugLookup[]
  readOnly?: boolean
}

const INSTRUCTIONS = ['Sau ăn', 'Trước ăn', 'Sáng', 'Tối', 'Sáng - Tối', 'Sáng - Trưa - Tối', 'Khi đau']

/** Port of KeThuocPanel + DrugUsingPanel: one block per prescribed drug. */
export default function ReceiptList({ rows, onChange, drugs, readOnly }: Props) {
  const [days, setDays] = useState<Record<number, number>>({})
  const update = (i: number, patch: Partial<Receipt>) => onChange(rows.map((r, idx) => (idx === i ? { ...r, ...patch } : r)))
  const remove = (i: number) => onChange(rows.filter((_, idx) => idx !== i))
  const add = () => onChange([...rows, { drugId: 0, amount: 0, instruction: '', perDay: 2, perTimes: 1 }])
  const total = rows.reduce((s, r) => s + (drugs.find((d) => d.id === r.drugId)?.outPrice ?? 0) * r.amount, 0)

  return (
    <div>
      {rows.map((r, i) => {
        const drug = drugs.find((d) => d.id === r.drugId)
        const unit = drug?.unit ?? 'viên'
        return (
          <div key={i} style={{ border: '1px solid #e5e5e5', borderRadius: 4, padding: 8, marginBottom: 8, display: 'flex', flexWrap: 'wrap', gap: 8, alignItems: 'center' }}>
            <b style={{ width: 24 }}>{i + 1}.</b>
            <Select showSearch optionFilterProp="label" placeholder="Chọn thuốc" style={{ width: 240 }} value={r.drugId || undefined}
              options={drugs.map((d) => ({ value: d.id, label: `${d.name} (${d.unit})` }))} onChange={(v) => update(i, { drugId: v })} disabled={readOnly} />
            <span>Số lượng</span>
            <InputNumber min={0} value={r.amount} onChange={(v) => update(i, { amount: v ?? 0 })} disabled={readOnly} style={{ width: 90 }} />
            <span>{unit}</span>
            <span>Ngày uống</span>
            <InputNumber min={0} step={1} value={r.perDay} onChange={(v) => update(i, { perDay: v ?? 0 })} disabled={readOnly} style={{ width: 70 }} />
            <span>lần, mỗi lần</span>
            <InputNumber min={0} step={0.5} value={r.perTimes} onChange={(v) => update(i, { perTimes: v ?? 0 })} disabled={readOnly} style={{ width: 70 }} />
            <span>{unit}</span>
            {!readOnly && (
              <>
                <span>Số ngày</span>
                <InputNumber min={0} value={days[i] ?? 0} style={{ width: 70 }}
                  onChange={(v) => {
                    const d = v ?? 0
                    setDays({ ...days, [i]: d })
                    if (d > 0) update(i, { amount: Math.ceil(d * r.perDay * r.perTimes) })
                  }} />
              </>
            )}
            <AutoComplete style={{ width: 200 }} placeholder="Ghi chú / cách dùng" value={r.instruction ?? ''} options={INSTRUCTIONS.map((x) => ({ value: x }))}
              onChange={(v) => update(i, { instruction: v })} disabled={readOnly} />
            {!readOnly && <Button danger type="text" icon={<DeleteOutlined />} onClick={() => remove(i)}>Xóa</Button>}
          </div>
        )
      })}
      <Space style={{ justifyContent: 'space-between', width: '100%' }}>
        {!readOnly ? <Button size="small" icon={<PlusOutlined />} onClick={add}>Thêm</Button> : <span />}
        <Typography.Text type="secondary">Tiền thuốc (giá bán): <b>{money(total)}</b></Typography.Text>
      </Space>
    </div>
  )
}
