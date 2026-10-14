import { useState } from 'react'
import { Button, Checkbox, Form, Input, Modal, Select, Space, Table, Tag, message } from 'antd'
import { EditOutlined, KeyOutlined, PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import { DeleteButton, PageTitle, useBranches, useLoad } from '../../components/common'
import { useAuth } from '../../auth/AuthContext'
import { errorMessage } from '../../api/client'
import { accountsApi, employeesApi, type Account, type AccountCreateInput } from './api'

/** "Tài khoản" – legacy AccountManagerControl + DeclareAccount + PasswordChangerDialog (admin rule). */
export default function AccountsPage() {
  const { user } = useAuth()
  const branches = useBranches()
  const [branchId, setBranchId] = useState<number>(0)
  const [keyword, setKeyword] = useState('')
  const [editing, setEditing] = useState<Account | null | undefined>(undefined)
  const [pwdFor, setPwdFor] = useState<Account | null>(null)
  const [form] = Form.useForm<AccountCreateInput>()
  const [pwdForm] = Form.useForm<{ newPassword: string; confirm: string }>()

  const { data, loading, reload } = useLoad(() => accountsApi.list({ branchId: branchId || undefined, keyword: keyword || undefined }), [branchId, keyword])
  const employees = useLoad(() => employeesApi.list({ type: 'all', branchId: 0, includeInvalid: true }), [])

  if (!user?.isAdministrator) {
    return (
      <div className="page-card">
        <PageTitle title="Tài khoản" />
        <p>Chỉ tài khoản quản trị mới được quản lý tài khoản. Bạn có thể đổi mật khẩu của mình ở menu người dùng (góc trên bên phải).</p>
      </div>
    )
  }

  const branchOptions = branches.filter((b) => b.id !== 0).map((b) => ({ value: b.id, label: b.name }))
  const employeeOptions = (employees.data ?? []).map((e) => ({ value: e.userName, label: `${e.userName} - ${e.fullName}` }))

  const openNew = () => {
    form.resetFields()
    form.setFieldsValue({ isAdministrator: false, branchId: user.branchId })
    setEditing(null)
  }
  const openEdit = (a: Account) => {
    form.setFieldsValue({ userName: a.userName, fullName: a.fullName ?? '', isAdministrator: a.isAdministrator, branchId: a.branchId, employeeUserName: a.employeeUserName ?? undefined })
    setEditing(a)
  }
  const save = async () => {
    const v = await form.validateFields()
    try {
      if (editing) {
        await accountsApi.update(editing.userName, { fullName: v.fullName, isAdministrator: v.isAdministrator, branchId: v.branchId, employeeUserName: v.employeeUserName })
        message.success('Đã cập nhật tài khoản')
      } else {
        await accountsApi.create(v)
        message.success('Đã tạo tài khoản')
      }
      setEditing(undefined)
      reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }
  const resetPassword = async () => {
    const v = await pwdForm.validateFields()
    if (!pwdFor) return
    try {
      await accountsApi.resetPassword(pwdFor.userName, v.newPassword)
      message.success('Mật khẩu đã được đổi.')
      setPwdFor(null)
      pwdForm.resetFields()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }
  const remove = async (a: Account) => {
    try {
      await accountsApi.remove(a.userName)
      reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  return (
    <div className="page-card">
      <PageTitle
        title="Tài khoản"
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
        <Select style={{ width: 220 }} value={branchId} onChange={setBranchId} options={[{ value: 0, label: 'Tất cả chi nhánh' }, ...branchOptions]} />
        <Input.Search allowClear placeholder="Tên đăng nhập" style={{ width: 220 }} onSearch={setKeyword} />
      </div>
      <Table<Account>
        size="small"
        rowKey="userName"
        loading={loading}
        dataSource={data ?? []}
        pagination={false}
        columns={[
          { title: 'No', width: 55, render: (_, __, i) => i + 1 },
          { title: 'Tên đăng nhập', dataIndex: 'userName' },
          { title: 'Họ tên', dataIndex: 'fullName' },
          { title: 'Là tài khoản Administrator', dataIndex: 'isAdministrator', width: 200, render: (v: boolean) => (v ? <Tag color="red">Quản trị</Tag> : <Tag>Tiếp tân</Tag>) },
          { title: 'Chi nhánh', dataIndex: 'branchName' },
          { title: 'Nhân viên liên kết', render: (_, a) => (a.employeeUserName ? `${a.employeeUserName}${a.employeeFullName ? ' - ' + a.employeeFullName : ''}` : '') },
          {
            title: '',
            width: 130,
            render: (_, a) => (
              <Space size={4}>
                <Button size="small" icon={<EditOutlined />} onClick={() => openEdit(a)} />
                <Button size="small" icon={<KeyOutlined />} title="Đổi mật khẩu" onClick={() => setPwdFor(a)} />
                <DeleteButton title="Xóa tài khoản này?" onConfirm={() => remove(a)} disabled={a.userName === user.userName} />
              </Space>
            ),
          },
        ]}
      />

      <Modal title={editing ? 'Sửa tài khoản' : 'Thêm tài khoản'} open={editing !== undefined} onOk={save} onCancel={() => setEditing(undefined)} okText={editing ? 'Lưu' : 'Tạo'} cancelText="Đóng">
        <Form form={form} layout="vertical">
          <Form.Item name="userName" label="Tài khoản" rules={[{ required: true, message: 'Vui lòng điền tên tài khoản.' }]}>
            <Input autoFocus disabled={!!editing} />
          </Form.Item>
          {!editing && (
            <Form.Item name="password" label="Mật khẩu" rules={[{ required: true, message: 'Vui lòng điền mật khẩu.' }]}>
              <Input.Password />
            </Form.Item>
          )}
          <Form.Item name="fullName" label="Họ tên">
            <Input />
          </Form.Item>
          <Form.Item name="branchId" label="Chi nhánh" rules={[{ required: true, message: 'Vui lòng chọn chi nhánh.' }]}>
            <Select options={branchOptions} />
          </Form.Item>
          <Form.Item name="employeeUserName" label="Nhân viên liên kết">
            <Select allowClear showSearch optionFilterProp="label" options={employeeOptions} loading={employees.loading} />
          </Form.Item>
          <Form.Item name="isAdministrator" valuePropName="checked">
            <Checkbox>Quản trị</Checkbox>
          </Form.Item>
        </Form>
      </Modal>

      <Modal title={`Đổi mật khẩu: ${pwdFor?.userName ?? ''}`} open={!!pwdFor} onOk={resetPassword} onCancel={() => setPwdFor(null)} okText="Thay đổi mật khẩu" cancelText="Đóng">
        <Form form={pwdForm} layout="vertical">
          <Form.Item name="newPassword" label="Mật khẩu mới" rules={[{ required: true, message: 'Nhập mật khẩu mới' }]}>
            <Input.Password autoFocus />
          </Form.Item>
          <Form.Item
            name="confirm"
            label="Nhập lại mật khẩu mới"
            dependencies={['newPassword']}
            rules={[
              { required: true, message: 'Nhập lại mật khẩu mới' },
              ({ getFieldValue }) => ({ validator: (_, v) => (v === getFieldValue('newPassword') ? Promise.resolve() : Promise.reject(new Error('Mật khẩu không khớp'))) }),
            ]}
          >
            <Input.Password />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}
