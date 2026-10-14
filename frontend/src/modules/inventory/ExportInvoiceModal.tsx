import { useEffect, useState } from 'react'
import { Button, Col, DatePicker, Input, InputNumber, Modal, Radio, Row, Select, Space, Table, Typography, message } from 'antd'
import { DeleteOutlined } from '@ant-design/icons'
import dayjs, { type Dayjs } from 'dayjs'
import { errorMessage } from '../../api/client'
import { useAuth } from '../../auth/AuthContext'
import { useBranches } from '../../components/common'
import { equipmentExportsApi, type EquipmentLookup } from './api'

interface Line {
  equipment: EquipmentLookup
  amount: number
}

/**
 * "Khai báo xuất tài sản / vật tư" (legacy CreateExportInvoiceDialog):
 * left = equipment existing in the branch (search by name / serial / usage, double-click to add),
 * right = selected lines with amount and remaining quantity; amount must not exceed stock.
 */
export default function ExportInvoiceModal({ open, kind, onClose, onCreated }: { open: boolean; kind: 'fixed' | 'various'; onClose: () => void; onCreated: () => void }) {
  const { user, branchId } = useAuth()
  const branches = useBranches()
  const [isVarious, setIsVarious] = useState(kind === 'various')
  const [searchBy, setSearchBy] = useState<'name' | 'serial' | 'usage'>('name')
  const [keyword, setKeyword] = useState('')
  const [branch, setBranch] = useState<number>(branchId || user?.branchId || 0)
  const [left, setLeft] = useState<EquipmentLookup[]>([])
  const [lines, setLines] = useState<Line[]>([])
  const [dateMode, setDateMode] = useState<'today' | 'at'>('today')
  const [date, setDate] = useState<Dayjs>(dayjs())
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    if (!open) return
    setIsVarious(kind === 'various')
    setLines([])
    setKeyword('')
    setDateMode('today')
    setDate(dayjs())
    setBranch(branchId || user?.branchId || 0)
  }, [open, kind, branchId, user?.branchId])

  useEffect(() => {
    if (!open) return
    equipmentExportsApi
      .lookupExisting({ isVarious, keyword: keyword || undefined, searchBy, branchId: branch || undefined })
      .then(setLeft)
      .catch((e) => message.error(errorMessage(e)))
  }, [open, isVarious, keyword, searchBy, branch])

  const add = (e: EquipmentLookup) => setLines((ls) => (ls.some((l) => l.equipment.id === e.id) ? ls : [...ls, { equipment: e, amount: 1 }]))

  const submit = async () => {
    if (lines.length === 0) return message.warning('Vui lòng chọn thiết bị.')
    if (lines.some((l) => l.amount <= 0 || l.equipment.quantity - l.amount < 0)) return message.warning('Số lượng xuất phải lớn hơn 0 và nhỏ hơn tồn kho.')
    setSaving(true)
    try {
      await equipmentExportsApi.create({
        createdDate: dateMode === 'today' ? undefined : date.format('YYYY-MM-DDT00:00:00'),
        branchId: branch || undefined,
        lines: lines.map((l) => ({ equipmentId: l.equipment.id, quantity: l.amount })),
      })
      message.success('Đã tạo hóa đơn xuất')
      onCreated()
      onClose()
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal title={isVarious ? 'Khai báo xuất vật tư' : 'Khai báo xuất tài sản'} open={open} onCancel={onClose} onOk={submit} okText="Lưu" cancelText="Hủy" confirmLoading={saving} width={1100} destroyOnHidden>
      <Space wrap style={{ marginBottom: 12 }}>
        <Radio.Group value={isVarious} onChange={(e) => { setIsVarious(e.target.value); setLines([]) }}>
          <Radio value={false}>Tài sản</Radio>
          <Radio value={true}>Vật tư</Radio>
        </Radio.Group>
        <Radio.Group value={dateMode} onChange={(e) => setDateMode(e.target.value)}>
          <Radio value="today">Hôm nay</Radio>
          <Radio value="at">Ngày</Radio>
        </Radio.Group>
        <DatePicker value={date} format="DD/MM/YYYY" allowClear={false} onChange={(d) => { if (d) { setDate(d); setDateMode('at') } }} />
        {user?.isAdministrator && (
          <Select style={{ width: 220 }} value={branch || undefined} placeholder="Chọn chi nhánh" onChange={(v) => { setBranch(v); setLines([]) }}
            options={branches.filter((b) => b.id !== 0).map((b) => ({ value: b.id, label: b.name }))} />
        )}
      </Space>
      <Row gutter={12}>
        <Col span={12}>
          <Space style={{ marginBottom: 8 }}>
            <Input.Search size="small" placeholder="Tìm kiếm" allowClear onSearch={setKeyword} style={{ width: 200 }} />
            <Radio.Group size="small" value={searchBy} onChange={(e) => setSearchBy(e.target.value)}>
              <Radio value="name">Tên</Radio>
              <Radio value="serial">Serial</Radio>
              <Radio value="usage">Công dụng</Radio>
            </Radio.Group>
          </Space>
          <Typography.Text type="secondary" style={{ display: 'block', marginBottom: 4 }}>Nhấp đúp để chọn thiết bị trong kho</Typography.Text>
          <Table<EquipmentLookup>
            size="small"
            rowKey="id"
            dataSource={left}
            pagination={{ pageSize: 10, showSizeChanger: false }}
            onRow={(r) => ({ onDoubleClick: () => add(r), style: { cursor: 'pointer' } })}
            columns={[
              { title: 'Tên', dataIndex: 'name' },
              { title: 'Serial', dataIndex: 'serial', width: 90 },
              { title: 'NCC', dataIndex: 'vendor', width: 100 },
              { title: 'Tồn', dataIndex: 'quantity', width: 60, align: 'right' },
              { title: '', width: 60, render: (_, r) => <Button size="small" onClick={() => add(r)}>Chọn</Button> },
            ]}
          />
        </Col>
        <Col span={12}>
          <Typography.Text strong style={{ display: 'block', marginBottom: 8 }}>Danh sách xuất ({lines.length})</Typography.Text>
          <Table<Line>
            size="small"
            rowKey={(l) => l.equipment.id}
            dataSource={lines}
            pagination={false}
            columns={[
              { title: 'Tên', dataIndex: ['equipment', 'name'] },
              { title: 'Serial', dataIndex: ['equipment', 'serial'], width: 90 },
              { title: 'Tồn', dataIndex: ['equipment', 'quantity'], width: 60, align: 'right' },
              { title: 'Xuất', dataIndex: 'amount', width: 90, render: (v: number, l) => (
                <InputNumber size="small" min={1} max={l.equipment.quantity} value={v} style={{ width: 80 }}
                  onChange={(x) => setLines((ls) => ls.map((y) => (y.equipment.id === l.equipment.id ? { ...y, amount: x ?? 1 } : y)))} />
              ) },
              { title: 'Còn lại', width: 70, align: 'right', render: (_, l) => <span style={{ color: l.equipment.quantity - l.amount < 0 ? 'red' : undefined }}>{l.equipment.quantity - l.amount}</span> },
              { title: '', width: 40, render: (_, l) => <Button size="small" type="text" danger icon={<DeleteOutlined />} onClick={() => setLines((ls) => ls.filter((y) => y.equipment.id !== l.equipment.id))} /> },
            ]}
          />
        </Col>
      </Row>
    </Modal>
  )
}
