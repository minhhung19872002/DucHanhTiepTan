import { useState } from 'react'
import { Button, Checkbox, Form, Input, Modal, Radio, Space, Table, Tag, message } from 'antd'
import { EditOutlined, PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import { useParams } from 'react-router-dom'
import { BranchSelect, DeleteButton, PageTitle, useLoad } from '../../components/common'
import { useAuth } from '../../auth/AuthContext'
import { errorMessage } from '../../api/client'
import { money } from '../../utils/format'
import { MoneyInput } from './components'
import { equipmentApi, type Equipment, type EquipmentInput } from './api'

/**
 * Equipment catalog – legacy EquipmentManagerControl + DeclareEquipmentDialog.
 * Route param kind: "fixed" = Tài sản (IsVarious=false), "various" = Vật tư (IsVarious=true).
 */
export default function EquipmentCatalogPage() {
  const { kind } = useParams<{ kind: string }>()
  const isVarious = kind === 'various'
  const title = isVarious ? 'Vật tư' : 'Tài sản'
  const { branchId } = useAuth()
  const [keyword, setKeyword] = useState('')
  const [searchBy, setSearchBy] = useState<'name' | 'serial' | 'usage'>('name')
  const [showEmpty, setShowEmpty] = useState(true)
  const [editing, setEditing] = useState<Equipment | null | undefined>(undefined)
  const [form] = Form.useForm<EquipmentInput>()

  const { data, loading, reload } = useLoad(
    () => equipmentApi.list({ isVarious, keyword: keyword || undefined, searchBy, branchId, showEmpty }),
    [isVarious, keyword, searchBy, branchId, showEmpty],
  )

  const openNew = () => {
    form.resetFields()
    form.setFieldsValue({ isVarious, price: 0, unit: isVarious ? 'Cái' : 'Bộ' })
    setEditing(null)
  }
  const openEdit = (e: Equipment) => {
    form.setFieldsValue({ name: e.name, serial: e.serial, unit: e.unit ?? '', price: e.price, isVarious: e.isVarious, vendor: e.vendor, position: e.position ?? '', usage: e.usage ?? '', note: e.note ?? '' })
    setEditing(e)
  }
  const save = async () => {
    const v = await form.validateFields()
    try {
      if (editing) {
        await equipmentApi.update(editing.id, v)
        message.success('Đã cập nhật thiết bị')
      } else {
        await equipmentApi.create(v)
        message.success('Đã thêm thiết bị')
      }
      setEditing(undefined)
      reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }
  const remove = async (e: Equipment) => {
    try {
      await equipmentApi.remove(e.id)
      reload()
    } catch (err) {
      message.error(errorMessage(err))
    }
  }

  return (
    <div className="page-card">
      <PageTitle
        title={`Danh mục ${title.toLowerCase()}`}
        extra={
          <Space>
            <Button icon={<ReloadOutlined />} onClick={reload} />
            <Button type="primary" icon={<PlusOutlined />} onClick={openNew}>
              Thêm
            </Button>
          </Space>
        }
      />
      <div className="toolbar">
        <span>Chi nhánh:</span>
        <BranchSelect />
        <span>Tìm kiếm:</span>
        <Input.Search allowClear style={{ width: 220 }} onSearch={setKeyword} />
        <Radio.Group value={searchBy} onChange={(e) => setSearchBy(e.target.value)}>
          <Radio value="name">Tên</Radio>
          <Radio value="serial">Serial</Radio>
          <Radio value="usage">Chức năng</Radio>
        </Radio.Group>
        <Checkbox checked={showEmpty} onChange={(e) => setShowEmpty(e.target.checked)}>
          Hiển thị cả số lượng rỗng
        </Checkbox>
      </div>
      <Table<Equipment>
        size="small"
        rowKey="id"
        loading={loading}
        dataSource={data ?? []}
        pagination={{ pageSize: 50, showSizeChanger: true, showTotal: (t) => `${t} dòng dữ liệu` }}
        scroll={{ x: 1100 }}
        columns={[
          { title: 'No', width: 55, render: (_, __, i) => i + 1 },
          { title: 'Id', dataIndex: 'id', width: 70 },
          { title: 'Tên', dataIndex: 'name', sorter: (a, b) => a.name.localeCompare(b.name) },
          { title: 'Đơn vị', dataIndex: 'unit', width: 80 },
          { title: 'Loại', dataIndex: 'isVarious', width: 90, render: (v: boolean) => (v ? <Tag color="orange">Tiêu hao</Tag> : <Tag color="blue">Cố định</Tag>) },
          { title: 'Giá nhập', dataIndex: 'price', align: 'right', width: 110, render: money },
          { title: 'Serial', dataIndex: 'serial', width: 110 },
          { title: 'Vị trí', dataIndex: 'position', width: 130 },
          { title: 'Chức năng', dataIndex: 'usage' },
          { title: 'Ghi chú', dataIndex: 'note' },
          { title: 'Nhà sản xuất', dataIndex: 'vendor', width: 130 },
          { title: 'Số lượng', dataIndex: 'quantity', align: 'right', width: 90 },
          {
            title: '',
            width: 90,
            fixed: 'right',
            render: (_, e) => (
              <Space size={4}>
                <Button size="small" icon={<EditOutlined />} onClick={() => openEdit(e)} />
                <DeleteButton title="Xóa thiết bị này?" onConfirm={() => remove(e)} />
              </Space>
            ),
          },
        ]}
      />

      <Modal title={editing ? `Sửa ${title.toLowerCase()}` : 'Thêm thiết bị'} open={editing !== undefined} onOk={save} onCancel={() => setEditing(undefined)} okText={editing ? 'Lưu' : 'Tạo'} cancelText="Đóng">
        <Form form={form} layout="vertical">
          <Form.Item name="name" label="Tên" rules={[{ required: true, message: 'Vui lòng điền tên.' }]}>
            <Input autoFocus />
          </Form.Item>
          <Form.Item name="serial" label="Số serial" rules={[{ required: true, message: 'Vui lòng điền số Serial.' }]}>
            <Input />
          </Form.Item>
          <Form.Item name="vendor" label="Nhà sản xuất" rules={[{ required: true, message: 'Vui lòng điền nhà sản xuất.' }]}>
            <Input />
          </Form.Item>
          <Form.Item name="isVarious" label="Loại">
            <Radio.Group>
              <Radio value={false}>Cố định (Tài sản)</Radio>
              <Radio value={true}>Tiêu hao (Vật tư)</Radio>
            </Radio.Group>
          </Form.Item>
          <Form.Item name="unit" label="Đơn vị">
            <Input />
          </Form.Item>
          <Form.Item name="price" label="Giá nhập">
            <MoneyInput />
          </Form.Item>
          <Form.Item name="position" label="Vị trí">
            <Input />
          </Form.Item>
          <Form.Item name="usage" label="Chức năng">
            <Input />
          </Form.Item>
          <Form.Item name="note" label="Ghi chú">
            <Input />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}
