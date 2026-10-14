import { Button, InputNumber, Select, Table } from 'antd'
import { DeleteOutlined, PlusOutlined } from '@ant-design/icons'
import type { EquipmentLookup, UsedEquipment } from './api'

interface Props {
  rows: UsedEquipment[]
  onChange: (rows: UsedEquipment[]) => void
  equipment: EquipmentLookup[]
  readOnly?: boolean
}

/** Port of UsedEquipmentControl ("Vật tư sử dụng"): consumables used during the appointment (reduce branch stock on save). */
export default function UsedEquipmentTable({ rows, onChange, equipment, readOnly }: Props) {
  const update = (i: number, patch: Partial<UsedEquipment>) => onChange(rows.map((r, idx) => (idx === i ? { ...r, ...patch } : r)))
  const columns = [
    { title: 'No', width: 40, render: (_: unknown, __: UsedEquipment, i: number) => i + 1 },
    {
      title: 'Vật tư', width: 320,
      render: (_: unknown, r: UsedEquipment, i: number) => (
        <Select size="small" showSearch optionFilterProp="label" style={{ width: '100%' }} placeholder="Chọn vật tư" value={r.equipmentId || undefined}
          options={equipment.map((e) => ({ value: e.id, label: `${e.name} - ${e.serial} (tồn: ${e.stockQuantity})` }))}
          onChange={(v) => update(i, { equipmentId: v })} disabled={readOnly} />
      ),
    },
    { title: 'Đơn vị', width: 80, render: (_: unknown, r: UsedEquipment) => equipment.find((e) => e.id === r.equipmentId)?.unit ?? '' },
    {
      title: 'Số lượng', width: 100,
      render: (_: unknown, r: UsedEquipment, i: number) => <InputNumber size="small" min={1} value={r.quantity} onChange={(v) => update(i, { quantity: v ?? 1 })} disabled={readOnly} />,
    },
    ...(readOnly ? [] : [{ title: '', width: 40, render: (_: unknown, __: UsedEquipment, i: number) => <Button size="small" type="text" danger icon={<DeleteOutlined />} onClick={() => onChange(rows.filter((_, idx) => idx !== i))} /> }]),
  ]
  return (
    <div>
      <Table size="small" rowKey={(_, i) => String(i)} columns={columns} dataSource={rows} pagination={false} />
      {!readOnly && (
        <Button size="small" style={{ marginTop: 8 }} icon={<PlusOutlined />} onClick={() => onChange([...rows, { equipmentId: 0, quantity: 1 }])}>
          Thêm vật tư
        </Button>
      )}
    </div>
  )
}
