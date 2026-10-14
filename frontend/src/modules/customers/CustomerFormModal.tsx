import { useEffect, useMemo, useState } from 'react'
import { AutoComplete, Button, Checkbox, Col, DatePicker, Divider, Form, Input, InputNumber, Modal, Radio, Row, Select, Space, Switch, Typography, Upload, message } from 'antd'
import { CameraOutlined, DeleteOutlined } from '@ant-design/icons'
import dayjs, { type Dayjs } from 'dayjs'
import { errorMessage } from '../../api/client'
import { useAuth } from '../../auth/AuthContext'
import { useBranches } from '../../components/common'
import { toLocalDate } from '../../utils/format'
import AddressPicker, { type AddressValue } from './AddressPicker'
import { customersApi, type CustomerDetail, type CustomerInput, type YesNoQuestion } from './api'

interface FormValues {
  visiableId?: number
  name: string
  sex: number
  birthYear: number
  phone?: string
  email?: string
  address?: string
  job?: string
  branchId?: number
  dateMode: 'today' | 'custom'
  createdDate?: Dayjs
  whyYouSeeMe: number
  theReasonYouSeeMe?: string
  subcribeByPhone: boolean
  phoneToSubcribe?: string
  subcribeByEmail: boolean
  emailToSubcribe?: string
  subcribeByMail: boolean
  mailToSubcribe?: string
}

/**
 * Port of DeclareCustomerDialog ("Khai báo khách hàng").
 * Create mode: "Tạo" keeps the dialog open unless "Đóng form sau khi tạo" is ticked;
 * "Tạo phiếu khám luôn" sends the caller to the examination screen.
 */
export default function CustomerFormModal({
  open,
  customer,
  onClose,
  onSaved,
}: {
  open: boolean
  /** undefined = create mode */
  customer?: CustomerDetail
  onClose: () => void
  onSaved: (saved: CustomerDetail, createPrescription: boolean) => void
}) {
  const { user } = useAuth()
  const branches = useBranches()
  const [form] = Form.useForm<FormValues>()
  const [questions, setQuestions] = useState<YesNoQuestion[]>([])
  const [jobs, setJobs] = useState<string[]>([])
  const [yesMask, setYesMask] = useState(0)
  const [address, setAddress] = useState<AddressValue>({})
  const [imageData, setImageData] = useState<string | null>(null)
  const [imageChanged, setImageChanged] = useState(false)
  const [advanced, setAdvanced] = useState(false)
  const [saving, setSaving] = useState(false)
  const [closeAfterCreate, setCloseAfterCreate] = useState(true)
  const [createPrescription, setCreatePrescription] = useState(false)
  const editMode = !!customer

  useEffect(() => {
    customersApi.questions().then(setQuestions).catch(() => setQuestions([]))
    customersApi.jobs().then(setJobs).catch(() => setJobs([]))
  }, [])

  useEffect(() => {
    if (!open) return
    setAdvanced(false)
    setImageChanged(false)
    if (customer) {
      const m = customer.moreInformation
      form.setFieldsValue({
        visiableId: customer.visiableId,
        name: customer.name,
        sex: customer.sex,
        birthYear: customer.birthYear,
        phone: customer.phone ?? undefined,
        email: customer.email ?? undefined,
        address: customer.address ?? undefined,
        job: customer.job ?? undefined,
        branchId: customer.branchId,
        dateMode: 'custom',
        createdDate: dayjs(customer.createdDate),
        whyYouSeeMe: m?.whyYouSeeMe ?? 4,
        theReasonYouSeeMe: m?.theReasonYouSeeMe ?? undefined,
        subcribeByPhone: m?.subcribeByPhone ?? false,
        phoneToSubcribe: m?.phoneToSubcribe ?? undefined,
        subcribeByEmail: m?.subcribeByEmail ?? false,
        emailToSubcribe: m?.emailToSubcribe ?? undefined,
        subcribeByMail: m?.subcribeByMail ?? false,
        mailToSubcribe: m?.mailToSubcribe ?? undefined,
      })
      setYesMask(m?.yesQuestions ?? 0)
      setAddress({ provinceName: customer.provinceName, districtName: customer.districtName, wardName: customer.wardName })
      setImageData(customer.hasImage ? `${customersApi.photoUrl(customer.id)}?t=${Date.now()}` : null)
    } else {
      form.resetFields()
      form.setFieldsValue({
        sex: 0,
        birthYear: dayjs().year() - 30,
        branchId: user?.branchId,
        dateMode: 'today',
        createdDate: dayjs(),
        whyYouSeeMe: 4,
        subcribeByPhone: false,
        subcribeByEmail: false,
        subcribeByMail: false,
      })
      setYesMask(0)
      setAddress({})
      setImageData(null)
    }
  }, [open, customer, form, user?.branchId])

  const birthYear = Form.useWatch('birthYear', form)
  const age = useMemo(() => (birthYear ? dayjs().year() - birthYear : null), [birthYear])
  const whyYouSeeMe = Form.useWatch('whyYouSeeMe', form)
  const branchId = Form.useWatch('branchId', form)

  const fillNextId = async () => {
    try {
      const next = await customersApi.nextId(user?.isAdministrator ? branchId : undefined)
      form.setFieldValue('visiableId', next)
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  const onPickImage = (file: File) => {
    const reader = new FileReader()
    reader.onload = () => {
      setImageData(reader.result as string)
      setImageChanged(true)
    }
    reader.readAsDataURL(file)
    return false
  }

  const buildInput = (v: FormValues): CustomerInput => ({
    visiableId: v.visiableId ?? null,
    name: v.name,
    sex: v.sex,
    birthYear: v.birthYear,
    phone: v.phone ?? null,
    email: v.email ?? null,
    address: v.address ?? null,
    job: v.job ?? null,
    createdDate: v.dateMode === 'today' ? null : toLocalDate(v.createdDate),
    branchId: user?.isAdministrator ? v.branchId : null,
    provinceName: address.provinceName ?? null,
    districtName: address.districtName ?? null,
    wardName: address.wardName ?? null,
    moreInformation: {
      whyYouSeeMe: v.whyYouSeeMe,
      theReasonYouSeeMe: v.theReasonYouSeeMe ?? null,
      subcribeByPhone: v.subcribeByPhone,
      phoneToSubcribe: v.phoneToSubcribe ?? null,
      subcribeByEmail: v.subcribeByEmail,
      emailToSubcribe: v.emailToSubcribe ?? null,
      subcribeByMail: v.subcribeByMail,
      mailToSubcribe: v.mailToSubcribe ?? null,
      yesQuestions: yesMask,
    },
    imageChanged,
    // data: URLs are accepted by the API; an unchanged photo URL is never sent
    imageBase64: imageChanged ? imageData : null,
  })

  const submit = async () => {
    let values: FormValues
    try {
      values = await form.validateFields()
    } catch {
      setAdvanced(false)
      return
    }
    setSaving(true)
    try {
      if (editMode && customer) {
        await customersApi.update(customer.id, buildInput(values))
        const fresh = await customersApi.get(customer.id)
        message.success('Đã lưu thông tin khách hàng')
        onSaved(fresh, false)
        onClose()
      } else {
        const created = await customersApi.create(buildInput(values))
        message.success(`Đã tạo khách hàng ${created.name} (${created.formattedId})`)
        onSaved(created, closeAfterCreate && createPrescription)
        if (closeAfterCreate) {
          onClose()
        } else {
          form.setFieldsValue({ visiableId: undefined, name: '', phone: '' })
          setImageData(null)
          setImageChanged(false)
        }
      }
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setSaving(false)
    }
  }


  return (
    <Modal
      title={editMode ? 'Chỉnh sửa khách hàng' : 'Khai báo khách hàng'}
      open={open}
      onCancel={onClose}
      width={860}
      destroyOnHidden={false}
      footer={
        <Space style={{ width: '100%', justifyContent: 'space-between' }}>
          <Button onClick={() => setAdvanced((a) => !a)}>{advanced ? 'Cơ bản' : 'Nâng cao'}</Button>
          <Space>
            {!editMode && (
              <>
                <Checkbox checked={closeAfterCreate} onChange={(e) => setCloseAfterCreate(e.target.checked)}>
                  Đóng form sau khi tạo.
                </Checkbox>
                <Checkbox checked={createPrescription} disabled={!closeAfterCreate} onChange={(e) => setCreatePrescription(e.target.checked)}>
                  Tạo phiếu khám luôn
                </Checkbox>
              </>
            )}
            <Button onClick={onClose}>Đóng</Button>
            <Button type="primary" loading={saving} onClick={submit}>
              {editMode ? 'Chỉnh sửa' : 'Tạo'}
            </Button>
          </Space>
        </Space>
      }
    >
      <Form form={form} layout="vertical" size="small">
        <div style={{ display: advanced ? 'none' : 'block' }}>
          <Row gutter={16}>
            <Col span={16}>
              <Typography.Text strong>Thông tin bắt buộc</Typography.Text>
              <Divider style={{ margin: '6px 0 12px' }} />
              <Row gutter={12}>
                <Col span={10}>
                  <Form.Item
                    label={
                      <Space>
                        Mã số:
                        {!editMode && (
                          <Button type="link" size="small" onClick={fillNextId} style={{ padding: 0 }}>
                            Tiếp theo
                          </Button>
                        )}
                      </Space>
                    }
                    name="visiableId"
                    rules={[{ required: true, message: 'Vui lòng điền mã khách hàng, hoặc click "Tiếp theo".' }]}
                  >
                    <InputNumber min={0} precision={0} style={{ width: '100%' }} controls={false} />
                  </Form.Item>
                </Col>
                <Col span={14}>
                  <Form.Item label="Họ và tên:" name="name" rules={[{ required: true, message: 'Vui lòng điền tên khách hàng.' }]}>
                    <Input autoFocus />
                  </Form.Item>
                </Col>
              </Row>
              <Row gutter={12}>
                <Col span={8}>
                  <Form.Item label="Giới tính:" name="sex">
                    <Radio.Group>
                      <Radio value={0}>Nam</Radio>
                      <Radio value={1}>Nữ</Radio>
                    </Radio.Group>
                  </Form.Item>
                </Col>
                <Col span={8}>
                  <Form.Item label={<Space>Năm sinh: {age !== null && <Typography.Text type="secondary">({age} tuổi)</Typography.Text>}</Space>} name="birthYear" rules={[{ required: true }]}>
                    <InputNumber min={1900} max={dayjs().year()} style={{ width: '100%' }} />
                  </Form.Item>
                </Col>
                <Col span={8}>
                  <Form.Item label="Điện thoại:" name="phone">
                    <Input />
                  </Form.Item>
                </Col>
              </Row>
              <Form.Item label="Địa chỉ:" name="address">
                <Input placeholder="Số nhà, đường, ấp..." />
              </Form.Item>
              <Form.Item label="Tỉnh / Huyện / Xã:">
                <AddressPicker value={address} onChange={setAddress} width={200} />
              </Form.Item>

              <Typography.Text strong>Thông tin bổ sung</Typography.Text>
              <Divider style={{ margin: '6px 0 12px' }} />
              <Row gutter={12}>
                <Col span={12}>
                  <Form.Item label="Nghề nghiệp:" name="job">
                    <AutoComplete options={jobs.map((j) => ({ value: j }))} filterOption={(input, opt) => (opt?.value ?? '').toLowerCase().includes(input.toLowerCase())} allowClear />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item label="E-mail:" name="email">
                    <Input />
                  </Form.Item>
                </Col>
              </Row>
              <Row gutter={12}>
                <Col span={14}>
                  <Form.Item label="Ngày tạo:" style={{ marginBottom: 0 }}>
                    <Space>
                      <Form.Item name="dateMode" noStyle>
                        <Radio.Group disabled={editMode}>
                          <Radio value="today">Hôm nay</Radio>
                          <Radio value="custom">Vào lúc:</Radio>
                        </Radio.Group>
                      </Form.Item>
                      <Form.Item name="createdDate" noStyle>
                        <DatePicker format="DD/MM/YYYY" disabled={editMode} onChange={() => form.setFieldValue('dateMode', 'custom')} />
                      </Form.Item>
                    </Space>
                  </Form.Item>
                </Col>
                <Col span={10}>
                  {user?.isAdministrator && (
                    <Form.Item label="Chi nhánh:" name="branchId" rules={[{ required: true, message: 'Chọn chi nhánh' }]}>
                      <Select options={branches.filter((b) => b.id !== 0).map((b) => ({ value: b.id, label: b.name }))} />
                    </Form.Item>
                  )}
                </Col>
              </Row>
            </Col>
            <Col span={8}>
              <Typography.Text strong>Hình ảnh</Typography.Text>
              <Divider style={{ margin: '6px 0 12px' }} />
              <div style={{ width: '100%', aspectRatio: '3 / 4', background: '#fafafa', border: '1px dashed #d9d9d9', display: 'flex', alignItems: 'center', justifyContent: 'center', overflow: 'hidden' }}>
                {imageData ? <img src={imageData} alt="" style={{ maxWidth: '100%', maxHeight: '100%' }} /> : <Typography.Text type="secondary">Chưa có ảnh</Typography.Text>}
              </div>
              <Space style={{ marginTop: 8 }}>
                <Upload accept="image/*" showUploadList={false} beforeUpload={onPickImage}>
                  <Button icon={<CameraOutlined />}>Chọn ảnh</Button>
                </Upload>
                <Button
                  icon={<DeleteOutlined />}
                  disabled={!imageData}
                  onClick={() => {
                    setImageData(null)
                    setImageChanged(true)
                  }}
                >
                  Xóa
                </Button>
              </Space>
            </Col>
          </Row>
        </div>

        <div style={{ display: advanced ? 'block' : 'none' }}>
          <Row gutter={24}>
            <Col span={12}>
              <Typography.Text strong>Tình trạng sức khỏe</Typography.Text>
              <Divider style={{ margin: '6px 0 12px' }} />
              {questions.map((q) => (
                <div key={q.id} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '4px 0', borderBottom: '1px solid #f0f0f0' }}>
                  <span>
                    {q.id}. {q.header}
                  </span>
                  <Switch
                    size="small"
                    checkedChildren="Có"
                    unCheckedChildren="Không"
                    checked={(yesMask & (1 << q.id)) !== 0}
                    onChange={(checked) => setYesMask((m) => (checked ? m | (1 << q.id) : m & ~(1 << q.id)))}
                  />
                </div>
              ))}
            </Col>
            <Col span={12}>
              <Typography.Text strong>Khảo sát</Typography.Text>
              <Divider style={{ margin: '6px 0 12px' }} />
              <Form.Item label="Chọn PK Vạn Thành do:" name="whyYouSeeMe">
                <Radio.Group>
                  <Space direction="vertical">
                    <Radio value={1}>Người quen giới thiệu</Radio>
                    <Radio value={2}>Quảng cáo báo, đài</Radio>
                    <Radio value={3}>Tự tìm đến</Radio>
                    <Radio value={4}>Khác:</Radio>
                  </Space>
                </Radio.Group>
              </Form.Item>
              <Form.Item label="Chi tiết:" name="theReasonYouSeeMe">
                <Input disabled={whyYouSeeMe !== 4} />
              </Form.Item>
              <Typography.Text>Bạn muốn nhận thông tin từ PK bằng phương tiện nào</Typography.Text>
              <Row gutter={8} style={{ marginTop: 8 }} align="middle">
                <Col span={8}>
                  <Form.Item name="subcribeByPhone" valuePropName="checked" noStyle>
                    <Checkbox>Điện thoại</Checkbox>
                  </Form.Item>
                </Col>
                <Col span={16}>
                  <Form.Item noStyle shouldUpdate={(p, n) => p.subcribeByPhone !== n.subcribeByPhone}>
                    {({ getFieldValue }) => (
                      <Form.Item name="phoneToSubcribe" noStyle>
                        <Input disabled={!getFieldValue('subcribeByPhone')} />
                      </Form.Item>
                    )}
                  </Form.Item>
                </Col>
              </Row>
              <Row gutter={8} style={{ marginTop: 8 }} align="middle">
                <Col span={8}>
                  <Form.Item name="subcribeByEmail" valuePropName="checked" noStyle>
                    <Checkbox>Email</Checkbox>
                  </Form.Item>
                </Col>
                <Col span={16}>
                  <Form.Item noStyle shouldUpdate={(p, n) => p.subcribeByEmail !== n.subcribeByEmail}>
                    {({ getFieldValue }) => (
                      <Form.Item name="emailToSubcribe" noStyle>
                        <Input disabled={!getFieldValue('subcribeByEmail')} />
                      </Form.Item>
                    )}
                  </Form.Item>
                </Col>
              </Row>
              <Row gutter={8} style={{ marginTop: 8 }} align="middle">
                <Col span={8}>
                  <Form.Item name="subcribeByMail" valuePropName="checked" noStyle>
                    <Checkbox>Hộp thư</Checkbox>
                  </Form.Item>
                </Col>
                <Col span={16}>
                  <Form.Item noStyle shouldUpdate={(p, n) => p.subcribeByMail !== n.subcribeByMail}>
                    {({ getFieldValue }) => (
                      <Form.Item name="mailToSubcribe" noStyle>
                        <Input disabled={!getFieldValue('subcribeByMail')} />
                      </Form.Item>
                    )}
                  </Form.Item>
                </Col>
              </Row>
            </Col>
          </Row>
        </div>
      </Form>
    </Modal>
  )
}
