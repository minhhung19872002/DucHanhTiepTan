import { useState } from 'react'
import { Button, Form, Input, InputNumber, Modal, Radio, Space, Table, message } from 'antd'
import { EditOutlined, PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import { DeleteButton, PageTitle, useLoad } from '../../components/common'
import { errorMessage } from '../../api/client'
import { money } from '../../utils/format'
import { drugsApi, type Drug, type DrugInput } from './api'

/** "Dữ liệu thuốc" – legacy DrugManagerControl + DeclareDrug. */
export default function DrugsPage() {
  const [keyword, setKeyword] = useState('')
  const [priceMode, setPriceMode] = useState<'all' | 'custom'>('all')
  const [fromPrice, setFromPrice] = useState<number | null>(0)
  const [toPrice, setToPrice] = useState<number | null>(1000000)
  const [editing, setEditing] = useState<Drug | null | undefined>(undefined) // undefined = closed, null = new
  const [form] = Form.useForm<DrugInput & { outMode: 'fixed' | 'custom' }>()

  const { data, loading, reload } = useLoad(
    () => drugsApi.list({ keyword: keyword || undefined, fromPrice: priceMode === 'custom' ? (fromPrice ?? 0) : undefined, toPrice: priceMode === 'custom' ? (toPrice ?? undefined) : undefined }),
    [keyword, priceMode, fromPrice, toPrice],
  )

  const openNew = () => {
    form.resetFields()
    form.setFieldsValue({ unit: 'Viên', price: 0, outMode: 'fixed', outPrice: 0 })
    setEditing(null)
  }
  const openEdit = (d: Drug) => {
    form.setFieldsValue({ name: d.name, unit: d.unit, price: d.price, outPrice: d.outPrice, description: d.description ?? '', outMode: 'custom' })
    setEditing(d)
  }
  const save = async () => {
    const v = await form.validateFields()
    const input: DrugInput = { name: v.name, unit: v.unit, price: v.price, description: v.description, outPrice: v.outPrice, fixedOutPrice: v.outMode === 'fixed' }
    try {
      if (editing) {
        await drugsApi.update(editing.id, input)
        message.success('Đã cập nhật thuốc')
      } else {
        await drugsApi.create(input)
        message.success('Đã thêm thuốc')
      }
      setEditing(undefined)
      reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }
  const remove = async (d: Drug) => {
    try {
      await drugsApi.remove(d.id)
      message.success('Đã xóa thuốc')
      reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  return (
    <div className="page-card">
      <PageTitle
        title="Dữ liệu thuốc"
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
        <span>Tên thuốc:</span>
        <Input.Search allowClear placeholder="Tìm theo tên" style={{ width: 260 }} onSearch={setKeyword} />
        <span>Theo giá:</span>
        <Radio.Group value={priceMode} onChange={(e) => setPriceMode(e.target.value)}>
          <Radio value="all">Tất cả</Radio>
          <Radio value="custom">Từ</Radio>
        </Radio.Group>
        <InputNumber<number> min={0} value={fromPrice} onChange={setFromPrice} disabled={priceMode !== 'custom'} style={{ width: 130 }} formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, '.')} />
        <span>đến</span>
        <InputNumber<number> min={0} value={toPrice} onChange={setToPrice} disabled={priceMode !== 'custom'} style={{ width: 130 }} formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, '.')} />
      </div>
      <Table<Drug>
        size="small"
        rowKey="id"
        loading={loading}
        dataSource={data ?? []}
        pagination={{ pageSize: 50, showSizeChanger: true, showTotal: (t) => `${t} dòng dữ liệu` }}
        columns={[
          { title: 'No', width: 60, render: (_, __, i) => i + 1 },
          { title: 'Id', dataIndex: 'id', width: 70 },
          { title: 'Tên thuốc', dataIndex: 'name', sorter: (a, b) => a.name.localeCompare(b.name) },
          { title: 'Đơn vị tính', dataIndex: 'unit', width: 110 },
          { title: 'Giá nhập', dataIndex: 'price', align: 'right', width: 120, render: money, sorter: (a, b) => a.price - b.price },
          { title: 'Giá xuất', dataIndex: 'outPrice', align: 'right', width: 120, render: money, sorter: (a, b) => a.outPrice - b.outPrice },
          { title: 'Hoạt chất', dataIndex: 'description' },
          {
            title: '',
            width: 90,
            render: (_, d) => (
              <Space size={4}>
                <Button size="small" icon={<EditOutlined />} onClick={() => openEdit(d)} />
                <DeleteButton title="Xóa thuốc này?" onConfirm={() => remove(d)} />
              </Space>
            ),
          },
        ]}
      />

      <Modal title={editing ? 'Sửa thuốc' : 'Thêm thuốc'} open={editing !== undefined} onOk={save} onCancel={() => setEditing(undefined)} okText={editing ? 'Lưu' : 'Tạo'} cancelText="Đóng" destroyOnHidden={false}>
        <Form form={form} layout="vertical">
          <Form.Item name="name" label="Tên thuốc" rules={[{ required: true, message: 'Vui lòng điền tên thuốc.' }]}>
            <Input autoFocus />
          </Form.Item>
          <Form.Item name="unit" label="Đơn vị tính" rules={[{ required: true, message: 'Vui lòng điền đơn vị tính của thuốc.' }]}>
            <Input />
          </Form.Item>
          <Form.Item name="price" label="Giá nhập" rules={[{ required: true }]}>
            <InputNumber<number> min={0} style={{ width: '100%' }} formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, '.')} parser={(v) => Number((v ?? '').replace(/\./g, ''))} />
          </Form.Item>
          <Form.Item name="outMode" label="Giá xuất">
            <Radio.Group>
              <Radio value="fixed">Bằng 120% của giá nhập</Radio>
              <Radio value="custom">Chọn:</Radio>
            </Radio.Group>
          </Form.Item>
          <Form.Item noStyle shouldUpdate={(p, c) => p.outMode !== c.outMode}>
            {({ getFieldValue }) => (
              <Form.Item name="outPrice" label=" " colon={false}>
                <InputNumber<number> min={0} style={{ width: '100%' }} disabled={getFieldValue('outMode') === 'fixed'} formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, '.')} parser={(v) => Number((v ?? '').replace(/\./g, ''))} />
              </Form.Item>
            )}
          </Form.Item>
          <Form.Item name="description" label="Hoạt chất">
            <Input />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}
