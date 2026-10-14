import { useState } from 'react'
import { Button, Col, Form, Input, Modal, Radio, Row, Space, Table, Tag, message } from 'antd'
import { EditOutlined, PlusOutlined } from '@ant-design/icons'
import { DeleteButton, PageTitle, useLoad } from '../../components/common'
import { errorMessage } from '../../api/client'
import { laboTypesApi, laboVendorsApi, type LaboType, type LaboVendor } from './api'

/** "Labo" catalog – legacy LaboManagerControl: labo types (chủng loại) + labo vendors (nhà cung cấp). */
export default function LaboCatalogPage() {
  return (
    <div className="page-card">
      <PageTitle title="Danh mục Labo" />
      <Row gutter={16}>
        <Col xs={24} lg={12}>
          <LaboTypesPanel />
        </Col>
        <Col xs={24} lg={12}>
          <LaboVendorsPanel />
        </Col>
      </Row>
    </div>
  )
}

function LaboTypesPanel() {
  const { data, loading, reload } = useLoad(() => laboTypesApi.list(), [])
  const [editing, setEditing] = useState<LaboType | null | undefined>(undefined)
  const [form] = Form.useForm<Omit<LaboType, 'id'>>()

  const openNew = () => {
    form.resetFields()
    form.setFieldsValue({ disassembly: false })
    setEditing(null)
  }
  const save = async () => {
    const v = await form.validateFields()
    try {
      if (editing) await laboTypesApi.update(editing.id, v)
      else await laboTypesApi.create(v)
      setEditing(undefined)
      reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }
  const remove = async (t: LaboType) => {
    try {
      await laboTypesApi.remove(t.id)
      reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }
  return (
    <>
      <div className="toolbar">
        <strong>Chủng loại Labo</strong>
        <span style={{ flex: 1 }} />
        <Button type="primary" size="small" icon={<PlusOutlined />} onClick={openNew}>
          Thêm
        </Button>
      </div>
      <Table<LaboType>
        size="small"
        rowKey="id"
        loading={loading}
        dataSource={data ?? []}
        pagination={false}
        columns={[
          { title: 'No', width: 50, render: (_, __, i) => i + 1 },
          { title: 'Tên', dataIndex: 'name' },
          { title: 'Loại', dataIndex: 'disassembly', width: 90, render: (v: boolean) => (v ? <Tag color="orange">Tháo lắp</Tag> : <Tag color="blue">Cố định</Tag>) },
          { title: 'Mô tả', dataIndex: 'description' },
          {
            title: '',
            width: 80,
            render: (_, t) => (
              <Space size={4}>
                <Button size="small" icon={<EditOutlined />} onClick={() => { form.setFieldsValue({ name: t.name, description: t.description ?? '', disassembly: t.disassembly }); setEditing(t) }} />
                <DeleteButton title="Xóa chủng loại này?" onConfirm={() => remove(t)} />
              </Space>
            ),
          },
        ]}
      />
      <Modal title={editing ? 'Sửa chủng loại Labo' : 'Thêm chủng loại Labo'} open={editing !== undefined} onOk={save} onCancel={() => setEditing(undefined)} okText={editing ? 'Lưu' : 'Thêm'} cancelText="Đóng">
        <Form form={form} layout="vertical">
          <Form.Item name="name" label="Tên" rules={[{ required: true, message: 'Vui lòng điền tên.' }]}>
            <Input autoFocus />
          </Form.Item>
          <Form.Item name="disassembly" label="Loại">
            <Radio.Group>
              <Radio value={true}>Tháo lắp</Radio>
              <Radio value={false}>Cố định</Radio>
            </Radio.Group>
          </Form.Item>
          <Form.Item name="description" label="Mô tả">
            <Input />
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}

function LaboVendorsPanel() {
  const { data, loading, reload } = useLoad(() => laboVendorsApi.list(), [])
  const [editing, setEditing] = useState<LaboVendor | null | undefined>(undefined)
  const [form] = Form.useForm<Omit<LaboVendor, 'id'>>()

  const save = async () => {
    const v = await form.validateFields()
    try {
      if (editing) await laboVendorsApi.update(editing.id, v)
      else await laboVendorsApi.create(v)
      setEditing(undefined)
      reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }
  const remove = async (v: LaboVendor) => {
    try {
      await laboVendorsApi.remove(v.id)
      reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }
  return (
    <>
      <div className="toolbar">
        <strong>Nhà cung cấp Labo</strong>
        <span style={{ flex: 1 }} />
        <Button type="primary" size="small" icon={<PlusOutlined />} onClick={() => { form.resetFields(); setEditing(null) }}>
          Thêm
        </Button>
      </div>
      <Table<LaboVendor>
        size="small"
        rowKey="id"
        loading={loading}
        dataSource={data ?? []}
        pagination={false}
        columns={[
          { title: 'No', width: 50, render: (_, __, i) => i + 1 },
          { title: 'Tên', dataIndex: 'name' },
          { title: 'Điện thoại', dataIndex: 'tel', width: 110 },
          { title: 'Địa chỉ', dataIndex: 'address' },
          { title: 'Mô tả', dataIndex: 'description' },
          {
            title: '',
            width: 80,
            render: (_, v) => (
              <Space size={4}>
                <Button size="small" icon={<EditOutlined />} onClick={() => { form.setFieldsValue({ name: v.name, address: v.address ?? '', tel: v.tel ?? '', description: v.description ?? '' }); setEditing(v) }} />
                <DeleteButton title="Xóa nhà cung cấp này?" onConfirm={() => remove(v)} />
              </Space>
            ),
          },
        ]}
      />
      <Modal title={editing ? 'Sửa nhà cung cấp Labo' : 'Thêm nhà cung cấp Labo'} open={editing !== undefined} onOk={save} onCancel={() => setEditing(undefined)} okText={editing ? 'Lưu' : 'Thêm'} cancelText="Đóng">
        <Form form={form} layout="vertical">
          <Form.Item name="name" label="Tên" rules={[{ required: true, message: 'Vui lòng điền tên.' }]}>
            <Input autoFocus />
          </Form.Item>
          <Form.Item name="tel" label="Tel">
            <Input />
          </Form.Item>
          <Form.Item name="address" label="Địa chỉ">
            <Input />
          </Form.Item>
          <Form.Item name="description" label="Mô tả">
            <Input />
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}
