import { useState } from 'react'
import { Alert, Button, Form, Input, Modal, Space, Table, message } from 'antd'
import { EditOutlined, PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import { DeleteButton, PageTitle, useLoad } from '../../components/common'
import { errorMessage } from '../../api/client'
import { branchesApi, type Branch, type BranchInput } from './api'

/** "Chi nhánh" – legacy DeclareBranch dialog. Branch 0 ("Tất cả") is virtual and read-only. */
export default function BranchesPage() {
  const { data, loading, reload } = useLoad(() => branchesApi.list(), [])
  const [editing, setEditing] = useState<Branch | null | undefined>(undefined)
  const [form] = Form.useForm<BranchInput>()

  const save = async () => {
    const v = await form.validateFields()
    try {
      if (editing) await branchesApi.update(editing.id, v)
      else await branchesApi.create(v)
      message.success('Đã lưu chi nhánh')
      setEditing(undefined)
      reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }
  const remove = async (b: Branch) => {
    try {
      await branchesApi.remove(b.id)
      reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  return (
    <div className="page-card">
      <PageTitle
        title="Khai báo chi nhánh"
        extra={
          <Space>
            <Button icon={<ReloadOutlined />} onClick={reload} />
            <Button type="primary" icon={<PlusOutlined />} onClick={() => { form.resetFields(); setEditing(null) }}>
              Thêm
            </Button>
          </Space>
        }
      />
      <Alert type="info" showIcon style={{ marginBottom: 12 }} message="Sau khi thay đổi chi nhánh, vui lòng đăng nhập lại để danh sách chi nhánh được cập nhật ở mọi nơi." />
      <Table<Branch>
        size="small"
        rowKey="id"
        loading={loading}
        dataSource={data ?? []}
        pagination={false}
        columns={[
          { title: 'Id', dataIndex: 'id', width: 70 },
          { title: 'Tên', dataIndex: 'name' },
          { title: 'Địa chỉ', dataIndex: 'address' },
          { title: 'Điện thoại', dataIndex: 'tel', width: 140 },
          {
            title: '',
            width: 90,
            render: (_, b) =>
              b.id === 0 ? null : (
                <Space size={4}>
                  <Button size="small" icon={<EditOutlined />} onClick={() => { form.setFieldsValue({ name: b.name, address: b.address ?? '', tel: b.tel ?? '' }); setEditing(b) }} />
                  <DeleteButton title={`Xóa chi nhánh ${b.name}?`} onConfirm={() => remove(b)} />
                </Space>
              ),
          },
        ]}
      />
      <Modal title={editing ? 'Sửa chi nhánh' : 'Thêm chi nhánh'} open={editing !== undefined} onOk={save} onCancel={() => setEditing(undefined)} okText="OK" cancelText="Đóng">
        <Form form={form} layout="vertical">
          <Form.Item name="name" label="Tên" rules={[{ required: true, message: 'Vui lòng điền tên chi nhánh.' }]}>
            <Input autoFocus />
          </Form.Item>
          <Form.Item name="address" label="Địa chỉ">
            <Input />
          </Form.Item>
          <Form.Item name="tel" label="Điện thoại">
            <Input />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}
