import { useEffect, useMemo, useState } from 'react'
import { Button, DatePicker, InputNumber, Modal, Radio, Select, Space, Table, Typography, message } from 'antd'
import { DeleteOutlined, PlusOutlined } from '@ant-design/icons'
import dayjs, { type Dayjs } from 'dayjs'
import { errorMessage } from '../../api/client'
import { useAuth } from '../../auth/AuthContext'
import { useBranches } from '../../components/common'
import { money } from '../../utils/format'
import type { InvoiceLineInput } from './api'

export interface ImportItem {
  id: number
  name: string
  unit?: string | null
  price: number
  extra?: string
}

interface Line {
  key: number
  itemId?: number
  unit?: string | null
  price: number
  quantity: number
}

/**
 * Generic "Khai báo nhập" dialog (legacy DeclareInvoiceDialog / DeclareEquipmentInvoiceDialog):
 * date = today or a chosen day, a grid of item / unit / price / quantity / total.
 */
export default function ImportInvoiceModal({
  open,
  title,
  itemLabel,
  loadItems,
  onSubmit,
  onClose,
}: {
  open: boolean
  title: string
  itemLabel: string
  loadItems: () => Promise<ImportItem[]>
  onSubmit: (body: { createdDate?: string; branchId?: number; lines: InvoiceLineInput[] }) => Promise<void>
  onClose: () => void
}) {
  const { user, branchId } = useAuth()
  const branches = useBranches()
  const [items, setItems] = useState<ImportItem[]>([])
  const [lines, setLines] = useState<Line[]>([])
  const [dateMode, setDateMode] = useState<'today' | 'at'>('today')
  const [date, setDate] = useState<Dayjs>(dayjs())
  const [branch, setBranch] = useState<number>(branchId || user?.branchId || 0)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    if (!open) return
    setLines([{ key: 1, price: 0, quantity: 1 }])
    setDateMode('today')
    setDate(dayjs())
    setBranch(branchId || user?.branchId || 0)
    loadItems().then(setItems).catch((e) => message.error(errorMessage(e)))
  }, [open, loadItems, branchId, user?.branchId])

  const itemMap = useMemo(() => new Map(items.map((i) => [i.id, i])), [items])
  const total = lines.reduce((s, l) => s + l.price * l.quantity, 0)

  const update = (key: number, patch: Partial<Line>) => setLines((ls) => ls.map((l) => (l.key === key ? { ...l, ...patch } : l)))

  const submit = async () => {
    const valid = lines.filter((l) => l.itemId)
    if (valid.length === 0) return message.warning(`Vui lòng chọn ${itemLabel.toLowerCase()}.`)
    if (valid.some((l) => l.quantity <= 0 || l.price <= 0)) return message.warning('Số lượng và giá phải lớn hơn 0.')
    const dup = valid.find((l, i) => valid.findIndex((x) => x.itemId === l.itemId) !== i)
    if (dup) return message.warning(`"${itemMap.get(dup.itemId!)?.name}" bị trùng`)
    setSaving(true)
    try {
      await onSubmit({
        createdDate: dateMode === 'today' ? undefined : date.format('YYYY-MM-DDT00:00:00'),
        branchId: branch || undefined,
        lines: valid.map((l) => ({ itemId: l.itemId!, quantity: l.quantity, price: l.price })),
      })
      onClose()
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal title={title} open={open} onCancel={onClose} onOk={submit} okText="Lưu" cancelText="Hủy" confirmLoading={saving} width={900} destroyOnHidden>
      <Space wrap style={{ marginBottom: 12 }}>
        <Radio.Group value={dateMode} onChange={(e) => setDateMode(e.target.value)}>
          <Radio value="today">Hôm nay</Radio>
          <Radio value="at">Ngày</Radio>
        </Radio.Group>
        <DatePicker value={date} format="DD/MM/YYYY" allowClear={false} onChange={(d) => { if (d) { setDate(d); setDateMode('at') } }} />
        {user?.isAdministrator && (
          <Select
            style={{ width: 220 }}
            value={branch || undefined}
            placeholder="Chọn chi nhánh"
            onChange={setBranch}
            options={branches.filter((b) => b.id !== 0).map((b) => ({ value: b.id, label: b.name }))}
          />
        )}
      </Space>
      <Table<Line>
        size="small"
        rowKey="key"
        dataSource={lines}
        pagination={false}
        columns={[
          { title: '#', width: 40, render: (_, __, i) => i + 1 },
          {
            title: itemLabel,
            dataIndex: 'itemId',
            render: (v: number | undefined, l) => (
              <Select
                showSearch
                style={{ width: '100%' }}
                value={v}
                placeholder={`Chọn ${itemLabel.toLowerCase()}`}
                optionFilterProp="label"
                options={items.map((i) => ({ value: i.id, label: i.extra ? `${i.name} (${i.extra})` : i.name }))}
                onChange={(id: number) => {
                  const it = itemMap.get(id)
                  update(l.key, { itemId: id, unit: it?.unit, price: it?.price ?? 0 })
                }}
              />
            ),
          },
          { title: 'Đơn vị', dataIndex: 'unit', width: 90 },
          { title: 'Giá', dataIndex: 'price', width: 140, render: (v: number, l) => <InputNumber min={0} value={v} style={{ width: '100%' }} formatter={(x) => money(Number(x))} parser={(x) => Number((x ?? '').replace(/\D/g, ''))} onChange={(x) => update(l.key, { price: x ?? 0 })} /> },
          { title: 'Số lượng', dataIndex: 'quantity', width: 110, render: (v: number, l) => <InputNumber min={1} value={v} style={{ width: '100%' }} onChange={(x) => update(l.key, { quantity: x ?? 1 })} /> },
          { title: 'Thành tiền', width: 130, align: 'right', render: (_, l) => money(l.price * l.quantity) },
          { title: '', width: 40, render: (_, l) => <Button size="small" type="text" danger icon={<DeleteOutlined />} onClick={() => setLines((ls) => ls.filter((x) => x.key !== l.key))} /> },
        ]}
        footer={() => (
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <Button size="small" icon={<PlusOutlined />} onClick={() => setLines((ls) => [...ls, { key: (ls.at(-1)?.key ?? 0) + 1, price: 0, quantity: 1 }])}>
              Thêm dòng
            </Button>
            <Typography.Text strong>
              Số mặt hàng: {lines.filter((l) => l.itemId).length} &nbsp;|&nbsp; Tổng tiền: {money(total)}
            </Typography.Text>
          </div>
        )}
      />
    </Modal>
  )
}
