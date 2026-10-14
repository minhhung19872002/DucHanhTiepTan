import { useState } from 'react'
import { Button, Checkbox, Col, DatePicker, Form, Input, InputNumber, Modal, Radio, Row, Select, Space, Table, Tag, message } from 'antd'
import { EditOutlined, LockOutlined, PlusOutlined, ReloadOutlined, UnlockOutlined } from '@ant-design/icons'
import dayjs from 'dayjs'
import { DeleteButton, PageTitle, useBranches, useLoad } from '../../components/common'
import { useAuth } from '../../auth/AuthContext'
import { errorMessage } from '../../api/client'
import { fmtDate, money, sexLabel, toLocalDate } from '../../utils/format'
import { MoneyInput } from './components'
import { EMPLOYEE_TYPE, employeesApi, type Employee, type EmployeeInput } from './api'

type FormValues = Omit<EmployeeInput, 'joinDate'> & { joinDate: dayjs.Dayjs }

/** "Nhân viên" – legacy EmployeeManagerControl (doctors, receptionists, assistants, others). */
export default function EmployeesPage() {
  const { user } = useAuth()
  const branches = useBranches()
  const [branchId, setBranchId] = useState<number>(0)
  const [typeFilter, setTypeFilter] = useState<'all' | 'doctor' | 'receptionist' | 'assistant' | 'other'>('all')
  const [validOnly, setValidOnly] = useState(true)
  const [keyword, setKeyword] = useState('')
  const [editing, setEditing] = useState<Employee | null | undefined>(undefined)
  const [form] = Form.useForm<FormValues>()
  const typeId = Form.useWatch('employeeTypeId', form)

  const types = useLoad(() => employeesApi.types(), [])
  const { data, loading, reload } = useLoad(
    () => employeesApi.list({ type: typeFilter, branchId, includeInvalid: !validOnly, keyword: keyword || undefined }),
    [typeFilter, branchId, validOnly, keyword],
  )

  const branchOptions = branches.filter((b) => b.id !== 0).map((b) => ({ value: b.id, label: b.name }))
  const typeOptions = (types.data ?? []).map((t) => ({ value: t.id, label: t.name }))

  const openNew = () => {
    form.resetFields()
    form.setFieldsValue({
      employeeTypeId: EMPLOYEE_TYPE.DOCTOR, branchId: user?.branchId || undefined, joinDate: dayjs(), sex: 0,
      defaultSalary: 0, defaultRatio: 1, baseLunchSubsidy: 0, basePhoneSubsidy: 0, baseToxicSubsidy: 0, isValid: true,
    })
    setEditing(null)
  }
  const openEdit = (e: Employee) => {
    form.setFieldsValue({
      userName: e.userName, fullName: e.fullName, employeeTypeId: e.employeeTypeId, branchId: e.branchId, joinDate: dayjs(e.joinDate), sex: e.sex,
      phone: e.phone ?? '', email: e.email ?? '', address: e.address ?? '', rank: e.rank ?? '', otherEmployeeTypeName: e.otherEmployeeTypeName ?? '',
      defaultSalary: e.defaultSalary, defaultRatio: e.defaultRatio ?? undefined, baseLunchSubsidy: e.baseLunchSubsidy, basePhoneSubsidy: e.basePhoneSubsidy,
      baseToxicSubsidy: e.baseToxicSubsidy, isValid: e.isValid,
    })
    setEditing(e)
  }
  const save = async () => {
    const v = await form.validateFields()
    const input: EmployeeInput = { ...v, joinDate: toLocalDate(v.joinDate)! }
    try {
      if (editing) {
        await employeesApi.update(editing.userName, input)
        message.success('Đã cập nhật nhân viên')
      } else {
        await employeesApi.create(input)
        message.success('Đã thêm nhân viên')
      }
      setEditing(undefined)
      reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }
  const toggleValid = (e: Employee) => {
    Modal.confirm({
      title: 'Xác nhận',
      content: e.isValid ? 'Bạn có muốn khóa (sa thải) nhân viên này ?' : 'Nhân viên này đã bị khóa (sa thải), bạn có muốn kích hoạt lại ?',
      okText: 'Có',
      cancelText: 'Không',
      onOk: async () => {
        try {
          await employeesApi.setValid(e.userName, !e.isValid)
          reload()
        } catch (err) {
          message.error(errorMessage(err))
        }
      },
    })
  }
  const remove = async (e: Employee) => {
    try {
      await employeesApi.remove(e.userName)
      reload()
    } catch (err) {
      message.error(errorMessage(err))
    }
  }

  const isDoctor = typeId === EMPLOYEE_TYPE.DOCTOR
  const isOther = typeId === EMPLOYEE_TYPE.OTHER

  return (
    <div className="page-card">
      <PageTitle
        title="Nhân viên"
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
        <Select style={{ width: 200 }} value={branchId} onChange={setBranchId} options={[{ value: 0, label: 'Tất cả chi nhánh' }, ...branchOptions]} />
        <span>Chức vụ:</span>
        <Select
          style={{ width: 140 }}
          value={typeFilter}
          onChange={setTypeFilter}
          options={[
            { value: 'all', label: 'Tất cả' },
            { value: 'doctor', label: 'Bác sĩ' },
            { value: 'receptionist', label: 'Tiếp tân' },
            { value: 'assistant', label: 'Phụ tá' },
            { value: 'other', label: 'Khác' },
          ]}
        />
        <Input.Search allowClear placeholder="Tìm theo tên" style={{ width: 200 }} onSearch={setKeyword} />
        <Checkbox checked={validOnly} onChange={(e) => setValidOnly(e.target.checked)}>
          Chỉ hiển thị nhân viên đang làm việc
        </Checkbox>
      </div>
      <Table<Employee>
        size="small"
        rowKey="userName"
        loading={loading}
        dataSource={data ?? []}
        pagination={{ pageSize: 50, showSizeChanger: true }}
        scroll={{ x: 1500 }}
        rowClassName={(e) => (e.isValid ? '' : 'row-cancelled')}
        columns={[
          { title: 'No', width: 50, render: (_, __, i) => i + 1 },
          { title: 'Mã NV', dataIndex: 'userName', width: 90, fixed: 'left' },
          { title: 'Tên', dataIndex: 'fullName', fixed: 'left', sorter: (a, b) => a.fullName.localeCompare(b.fullName) },
          { title: 'Chức danh', dataIndex: 'position', width: 110 },
          { title: 'Chi nhánh', dataIndex: 'branchName', width: 160 },
          { title: 'Ngày vào làm', dataIndex: 'joinDate', width: 110, render: fmtDate },
          { title: 'Giới tính', dataIndex: 'sex', width: 80, render: sexLabel },
          { title: 'Đang làm', dataIndex: 'isValid', width: 90, render: (v: boolean) => (v ? <Tag color="green">Đang làm</Tag> : <Tag>Đã nghỉ</Tag>) },
          { title: 'Cấp bậc', dataIndex: 'rank', width: 90 },
          { title: 'Lương cơ bản', dataIndex: 'defaultSalary', align: 'right', width: 120, render: money },
          { title: 'Hệ số lương', dataIndex: 'defaultRatio', align: 'right', width: 100 },
          { title: 'Phụ cấp ăn', dataIndex: 'baseLunchSubsidy', align: 'right', width: 110, render: money },
          { title: 'Phụ cấp điện thoại', dataIndex: 'basePhoneSubsidy', align: 'right', width: 130, render: money },
          { title: 'Phụ cấp độc hại', dataIndex: 'baseToxicSubsidy', align: 'right', width: 120, render: money },
          { title: 'Điện thoại', dataIndex: 'phone', width: 110 },
          { title: 'Email', dataIndex: 'email', width: 150 },
          { title: 'Địa chỉ', dataIndex: 'address', width: 200 },
          {
            title: '',
            width: 120,
            fixed: 'right',
            render: (_, e) => (
              <Space size={4}>
                <Button size="small" icon={<EditOutlined />} onClick={() => openEdit(e)} disabled={!user?.isAdministrator} />
                <Button size="small" icon={e.isValid ? <LockOutlined /> : <UnlockOutlined />} title={e.isValid ? 'Khóa (sa thải)' : 'Kích hoạt lại'} onClick={() => toggleValid(e)} disabled={!user?.isAdministrator} />
                <DeleteButton title="Xóa nhân viên này?" onConfirm={() => remove(e)} disabled={!user?.isAdministrator} />
              </Space>
            ),
          },
        ]}
      />

      <Modal title={editing ? 'Sửa nhân viên' : 'Thêm nhân viên'} open={editing !== undefined} onOk={save} onCancel={() => setEditing(undefined)} okText={editing ? 'Lưu' : 'Thêm'} cancelText="Đóng" width={760}>
        <Form form={form} layout="vertical">
          <Row gutter={12}>
            <Col span={8}>
              <Form.Item name="userName" label="Mã NV" rules={[{ required: true, message: 'Vui lòng điền mã số nhân viên.' }]}>
                <Input autoFocus disabled={!!editing} />
              </Form.Item>
            </Col>
            <Col span={16}>
              <Form.Item name="fullName" label="Tên" rules={[{ required: true, message: 'Vui lòng điền tên của nhân viên.' }]}>
                <Input />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="employeeTypeId" label="Chức vụ" rules={[{ required: true }]}>
                <Select options={typeOptions} loading={types.loading} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="otherEmployeeTypeName" label="Tên chức vụ khác">
                <Input disabled={!isOther} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="rank" label="Cấp bậc">
                <Input />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="branchId" label="Chi nhánh" rules={[{ required: true, message: 'Vui lòng chọn chi nhánh.' }]}>
                <Select options={branchOptions} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="joinDate" label="Ngày vào làm" rules={[{ required: true }]}>
                <DatePicker format="DD/MM/YYYY" style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="sex" label="Giới tính">
                <Radio.Group>
                  <Radio value={0}>Nam</Radio>
                  <Radio value={1}>Nữ</Radio>
                </Radio.Group>
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="defaultSalary" label="Lương cơ bản" tooltip="Chỉ có tài khoản bác sĩ mới có mức lương cơ bản">
                <MoneyInput disabled={!isDoctor} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="defaultRatio" label="Hệ số lương" tooltip="Tài khoản bác sĩ không có hệ số lương">
                <InputNumber min={0} step={0.1} style={{ width: '100%' }} disabled={isDoctor} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="baseLunchSubsidy" label="Phụ cấp ăn">
                <MoneyInput />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="basePhoneSubsidy" label="Phụ cấp điện thoại">
                <MoneyInput />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="baseToxicSubsidy" label="Phụ cấp độc hại">
                <MoneyInput />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="phone" label="Điện thoại">
                <Input />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="email" label="Email">
                <Input />
              </Form.Item>
            </Col>
            <Col span={16}>
              <Form.Item name="address" label="Địa chỉ">
                <Input />
              </Form.Item>
            </Col>
            {editing && (
              <Col span={8}>
                <Form.Item name="isValid" valuePropName="checked" label=" " colon={false}>
                  <Checkbox>Đang làm việc</Checkbox>
                </Form.Item>
              </Col>
            )}
          </Row>
        </Form>
      </Modal>
    </div>
  )
}
