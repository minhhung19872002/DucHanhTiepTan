import { useCallback, useEffect, useMemo, useState } from 'react'
import { Alert, Button, Card, Col, DatePicker, Descriptions, Divider, Form, Input, Popconfirm, Radio, Row, Select, Space, Spin, Table, Tabs, Tag, Typography, message } from 'antd'
import { DeleteOutlined, PrinterOutlined, SaveOutlined } from '@ant-design/icons'
import dayjs from 'dayjs'
import { useNavigate } from 'react-router-dom'
import type { Consulting, ExamStatus, ExaminationDetail, Lookups, MembershipInfo, Receipt, Treatment, UsedEquipment } from './api'
import { discountedPrice, examApi, makeRatio, type Ratio } from './api'
import ConsultingTable from './ConsultingTable'
import TreatmentTable, { computeChain } from './TreatmentTable'
import ElderTreatmentTable from './ElderTreatmentTable'
import ReceiptList from './ReceiptList'
import UsedEquipmentTable from './UsedEquipmentTable'
import PicturesPanel from './PicturesPanel'
import DiscountPanel, { type DiscountType } from './DiscountPanel'
import PrintStyles from './PrintStyles'
import { errorMessage } from '../../api/client'
import { fmtDate, fmtDateTime, money, sexLabel, toLocalIso } from '../../utils/format'

interface Props {
  prescriptionId: number
  appointmentNo: number
  /** Called after save/delete so the "Đang khám" list can refresh. */
  onChanged: () => void
}

/** Port of PrescriptionControl: edits one appointment (đợt khám) of a prescription (phiếu khám). */
export default function ExamEditor({ prescriptionId, appointmentNo, onChanged }: Props) {
  const navigate = useNavigate()
  const [detail, setDetail] = useState<ExaminationDetail>()
  const [lookups, setLookups] = useState<Lookups>()
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)

  // editable state
  const [symptoms, setSymptoms] = useState('')
  const [diagnostic, setDiagnostic] = useState('')
  const [treatmentPlan, setTreatmentPlan] = useState('')
  const [doctorName, setDoctorName] = useState<string | null>(null)
  const [consultings, setConsultings] = useState<Consulting[]>([])
  const [treatments, setTreatments] = useState<Treatment[]>([])
  const [receipts, setReceipts] = useState<Receipt[]>([])
  const [usedEquipments, setUsedEquipments] = useState<UsedEquipment[]>([])
  const [status, setStatus] = useState<ExamStatus>('notyet')
  const [appointmentDate, setAppointmentDate] = useState<dayjs.Dayjs | null>(null)
  const [appointmentReason, setAppointmentReason] = useState('')
  const [advised, setAdvised] = useState('')
  const [discountType, setDiscountType] = useState<DiscountType>('normal')
  const [membership, setMembership] = useState<MembershipInfo | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const d = await examApi.detail(prescriptionId, appointmentNo)
      const l = await examApi.lookups(d.customer.branchId)
      setDetail(d)
      setLookups(l)
      setSymptoms(d.prescription.symptoms ?? '')
      setDiagnostic(d.prescription.diagnostic ?? '')
      setTreatmentPlan(d.prescription.treatment ?? '')
      setDoctorName(d.appointment.doctorName ?? null)
      setConsultings(d.consultings)
      setTreatments(d.treatments)
      setReceipts(d.receipts)
      setUsedEquipments(d.usedEquipments)
      setStatus(!d.appointment.hasDone ? 'notyet' : d.appointment.appointmentDate ? 'appoint' : 'done')
      setAppointmentDate(d.appointment.appointmentDate ? dayjs(d.appointment.appointmentDate) : null)
      setAppointmentReason(d.appointment.appointmentReason ?? '')
      setAdvised(d.appointment.advised ?? '')
      setDiscountType(d.prescription.discountType)
      setMembership(d.membership ?? null)
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setLoading(false)
    }
  }, [prescriptionId, appointmentNo])

  useEffect(() => {
    load()
  }, [load])

  // Discount ratio (TreatmentDiscount). A fixed VIP discount is stored as DiscountValue without MembershipId.
  const ratio: Ratio = useMemo(() => {
    if (!detail) return () => 1
    const p = detail.prescription
    if (discountType === 'normal' && p.discountType === 'normal' && p.discountValue > 0 && p.discountValue < 1) return () => 1 - p.discountValue
    return makeRatio(discountType, membership, discountType === 'discount' && membership?.id === p.membershipId ? p.discounts : undefined)
  }, [detail, discountType, membership])

  /** Re-price all lines after the discount changed (legacy behaviour when choosing a VIP code). */
  const applyDiscount = (type: DiscountType, m: MembershipInfo | null) => {
    setDiscountType(type)
    setMembership(m)
    const r = makeRatio(type, m)
    setTreatments((rows) => rows.map((t) => ({ ...t, price: discountedPrice(t.originalPrice, t.serviceId, r) })))
    setConsultings((rows) => rows.map((c) => ({ ...c, price: discountedPrice(c.originalPrice, c.serviceId, r) })))
  }

  if (loading && !detail) return <Spin style={{ display: 'block', margin: '80px auto' }} />
  if (!detail || !lookups) return null

  const { customer, branch, prescription, appointment } = detail
  const readOnly = appointment.hasDone && !detail.isSuperUser
  const isFirst = appointment.no === 1
  const chain = computeChain(treatments, detail.previousDebt)
  const remain = chain.length ? chain[chain.length - 1].remain : detail.previousDebt
  const paidNow = treatments.reduce((s, t) => s + t.paid, 0)
  const doctorOptions = lookups.doctors.map((d) => ({ value: d.userName, label: d.fullName }))
  const serviceName = (id: number) => lookups.services.find((s) => s.id === id)?.name ?? `#${id}`
  const drug = (id: number) => lookups.drugs.find((d) => d.id === id)
  const empName = (u?: string | null) => (u ? lookups.doctors.find((d) => d.userName === u)?.fullName ?? lookups.assistants.find((d) => d.userName === u)?.fullName ?? u : '')

  const save = async () => {
    if (status === 'appoint' && !appointmentDate) {
      message.warning('Vui lòng chọn ngày hẹn khám lại')
      return
    }
    setSaving(true)
    try {
      await examApi.save(prescriptionId, appointmentNo, {
        diagnostic, symptoms, treatment: treatmentPlan,
        doctorName,
        status,
        appointmentDate: status === 'appoint' ? toLocalIso(appointmentDate) : null,
        appointmentReason, advised,
        consultings: isFirst ? consultings : detail.consultings,
        treatments, receipts, usedEquipments,
        discount: isFirst ? { type: discountType, membershipId: discountType === 'discount' ? membership?.id ?? null : null } : null,
      })
      message.success('Đã lưu')
      onChanged()
      await load()
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setSaving(false)
    }
  }

  const remove = async () => {
    try {
      await examApi.remove(prescriptionId, appointmentNo, false)
      message.success('Đã hủy lần khám')
      onChanged()
      navigate('/examination')
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  const cancelService = async (subId: number) => {
    try {
      await examApi.cancelService(prescriptionId, subId)
      message.success('Đã hủy dịch vụ')
      await load()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  return (
    <div>
      <PrintStyles />
      <div className="no-print">
        <Card size="small" style={{ marginBottom: 12 }}>
          <Row gutter={16} align="middle">
            <Col flex="auto">
              <Space wrap size="large">
                <Typography.Title level={5} style={{ margin: 0 }}>
                  {customer.name} <Typography.Text type="secondary">({customer.formattedId})</Typography.Text>
                </Typography.Title>
                <span>{sexLabel(customer.sex)} · {customer.birthYear}</span>
                {customer.phone && <span>ĐT: {customer.phone}</span>}
                <span>{branch.name}</span>
                <Tag color="blue">Phiếu #{prescription.id}</Tag>
                <Tag color={appointment.hasDone ? 'green' : 'orange'}>
                  Lần khám {appointment.no}/{appointment.maxNo} {appointment.hasDone ? '· Đã xong' : '· Đang khám'}
                </Tag>
                <span>Ngày: {fmtDateTime(appointment.createdDate)}</span>
              </Space>
            </Col>
            <Col>
              <Space>
                <Button icon={<PrinterOutlined />} onClick={() => window.print()}>In</Button>
                <Button onClick={() => navigate(`/prescriptions/${prescriptionId}`)}>Xem phiếu khám</Button>
                {!appointment.hasDone && (
                  <Popconfirm title="Hủy lần khám này? Dữ liệu chưa lưu sẽ mất." okText="Hủy lần khám" cancelText="Không" onConfirm={remove}>
                    <Button danger icon={<DeleteOutlined />}>Hủy lần khám</Button>
                  </Popconfirm>
                )}
                <Button type="primary" icon={<SaveOutlined />} loading={saving} onClick={save} disabled={readOnly}>Lưu lại</Button>
              </Space>
            </Col>
          </Row>
          {customer.address && <Typography.Text type="secondary">Địa chỉ: {customer.address}</Typography.Text>}
        </Card>

        {readOnly && <Alert type="info" showIcon style={{ marginBottom: 12 }} message="Đợt khám đã xong, chỉ quản trị viên mới được chỉnh sửa." />}

        <Card size="small" style={{ marginBottom: 12 }}>
          <Form layout="vertical" size="small">
            <Row gutter={12}>
              <Col xs={24} md={8}>
                <Form.Item label="Triệu chứng"><Input.TextArea rows={2} value={symptoms} onChange={(e) => setSymptoms(e.target.value)} disabled={readOnly} /></Form.Item>
              </Col>
              <Col xs={24} md={8}>
                <Form.Item label="Chẩn đoán"><Input.TextArea rows={2} value={diagnostic} onChange={(e) => setDiagnostic(e.target.value)} disabled={readOnly} /></Form.Item>
              </Col>
              <Col xs={24} md={8}>
                <Form.Item label="Hướng điều trị"><Input.TextArea rows={2} value={treatmentPlan} onChange={(e) => setTreatmentPlan(e.target.value)} disabled={readOnly} /></Form.Item>
              </Col>
            </Row>
            <Row gutter={12}>
              <Col xs={24} md={8}>
                <Form.Item label="Bác sĩ khám">
                  <Select allowClear showSearch optionFilterProp="label" value={doctorName ?? undefined} onChange={(v) => setDoctorName(v ?? null)} options={doctorOptions} disabled={readOnly} />
                </Form.Item>
              </Col>
            </Row>
          </Form>
        </Card>

        <Card size="small" style={{ marginBottom: 12 }}>
          <Tabs
            defaultActiveKey="treatment"
            items={[
              {
                key: 'consulting',
                label: 'Tư vấn',
                children: (
                  <>
                    {!isFirst && <Alert type="info" showIcon style={{ marginBottom: 8 }} message="Tư vấn chỉ chỉnh sửa được ở lần khám đầu tiên." />}
                    <ConsultingTable rows={consultings} onChange={setConsultings} lookups={lookups} ratio={ratio} readOnly={readOnly || !isFirst} />
                  </>
                ),
              },
              {
                key: 'treatment',
                label: 'Điều trị',
                children: (
                  <>
                    {detail.elderTreatments.length > 0 && (
                      <>
                        <Typography.Text strong>Các lần khám trước</Typography.Text>
                        <ElderTreatmentTable rows={detail.elderTreatments} lookups={lookups} onCancelService={readOnly ? undefined : (subId) => cancelService(subId)} />
                        <Divider style={{ margin: '12px 0' }} />
                      </>
                    )}
                    <Typography.Text strong>Lần khám này</Typography.Text> <Typography.Text type="secondary">(nợ trước: {money(detail.previousDebt)})</Typography.Text>
                    <TreatmentTable rows={treatments} onChange={setTreatments} lookups={lookups} ratio={ratio} previousDebt={detail.previousDebt} appointmentNo={appointmentNo} readOnly={readOnly} />
                  </>
                ),
              },
              { key: 'receipt', label: 'Kê thuốc', children: <ReceiptList rows={receipts} onChange={setReceipts} drugs={lookups.drugs} readOnly={readOnly} /> },
              { key: 'equipment', label: 'Vật tư sử dụng', children: <UsedEquipmentTable rows={usedEquipments} onChange={setUsedEquipments} equipment={lookups.equipment} readOnly={readOnly} /> },
              {
                key: 'pictures',
                label: `Hình ảnh (${detail.pictureIds.length})`,
                children: <PicturesPanel prescriptionId={prescriptionId} appointmentNo={appointmentNo} pictureIds={detail.pictureIds} onChanged={load} readOnly={readOnly} />,
              },
              {
                key: 'discount',
                label: 'Giảm giá',
                children: <DiscountPanel type={discountType} membership={membership} onChange={applyDiscount} readOnly={readOnly} firstAppointment={isFirst} />,
              },
              {
                key: 'payments',
                label: 'Lịch sử thu',
                children: (
                  <Table
                    size="small"
                    rowKey="id"
                    pagination={false}
                    dataSource={detail.payments}
                    columns={[
                      { title: 'Ngày', dataIndex: 'createdDate', render: (v: string) => fmtDateTime(v) },
                      { title: 'Số tiền', dataIndex: 'amount', align: 'right' as const, render: (v: number) => money(v) },
                      { title: 'Người thu', dataIndex: 'receptionistName' },
                    ]}
                  />
                ),
              },
            ]}
          />
        </Card>

        <Card size="small" title="Kết thúc lần khám">
          <Row gutter={16}>
            <Col xs={24} md={10}>
              <Radio.Group value={status} onChange={(e) => setStatus(e.target.value)} disabled={readOnly}>
                <Space direction="vertical">
                  <Radio value="notyet">Chưa xong (tiếp tục khám)</Radio>
                  <Radio value="appoint">Xong lần này – hẹn tái khám</Radio>
                  <Radio value="done">Hoàn tất điều trị (đóng phiếu khám)</Radio>
                </Space>
              </Radio.Group>
            </Col>
            <Col xs={24} md={14}>
              {status === 'appoint' && (
                <Form layout="vertical" size="small">
                  <Form.Item label="Ngày hẹn" required>
                    <DatePicker showTime={{ format: 'HH:mm' }} format="DD/MM/YYYY HH:mm" value={appointmentDate} onChange={setAppointmentDate} disabled={readOnly} />
                  </Form.Item>
                  <Form.Item label="Lý do hẹn"><Input value={appointmentReason} onChange={(e) => setAppointmentReason(e.target.value)} disabled={readOnly} /></Form.Item>
                </Form>
              )}
              {status !== 'notyet' && (
                <Form layout="vertical" size="small">
                  <Form.Item label="Lời dặn"><Input.TextArea rows={2} value={advised} onChange={(e) => setAdvised(e.target.value)} disabled={readOnly} /></Form.Item>
                </Form>
              )}
            </Col>
          </Row>
          <Divider style={{ margin: '12px 0' }} />
          <Space size="large" wrap>
            <span>Thanh toán lần này: <b>{money(paidNow)}</b></span>
            <span>Còn nợ sau lần này: <b style={{ color: remain > 0 ? '#c41d7f' : undefined }}>{money(remain)}</b></span>
            <span>Còn nợ phiếu khám (đã lưu): <b>{money(prescription.remain)}</b></span>
          </Space>
        </Card>
      </div>

      {/* ---------- printable treatment report (replacement of TreatmentReportPrinter / PrescriptionReportPrinter) ---------- */}
      <div className="print-only">
        <div style={{ textAlign: 'center' }}>
          <h2 style={{ margin: 0 }}>{branch.name}</h2>
          <div>{branch.address} {branch.tel ? `· ĐT: ${branch.tel}` : ''}</div>
          <h3>PHIẾU ĐIỀU TRỊ</h3>
        </div>
        <Descriptions size="small" column={2} bordered>
          <Descriptions.Item label="Khách hàng">{customer.name} ({customer.formattedId})</Descriptions.Item>
          <Descriptions.Item label="Giới tính / Năm sinh">{sexLabel(customer.sex)} / {customer.birthYear}</Descriptions.Item>
          <Descriptions.Item label="Địa chỉ">{customer.address}</Descriptions.Item>
          <Descriptions.Item label="Điện thoại">{customer.phone}</Descriptions.Item>
          <Descriptions.Item label="Ngày khám">{fmtDate(appointment.createdDate)}</Descriptions.Item>
          <Descriptions.Item label="Bác sĩ">{empName(doctorName)}</Descriptions.Item>
          <Descriptions.Item label="Triệu chứng" span={2}>{symptoms}</Descriptions.Item>
          <Descriptions.Item label="Chẩn đoán" span={2}>{diagnostic}</Descriptions.Item>
        </Descriptions>
        <h4>Điều trị</h4>
        <table className="print-table">
          <thead>
            <tr><th>#</th><th>Dịch vụ</th><th>Răng</th><th>SL</th><th>Giá</th><th>Thành tiền</th><th>Thanh toán</th><th>Còn lại</th></tr>
          </thead>
          <tbody>
            {chain.map((t, i) => (
              <tr key={i}>
                <td>{i + 1}</td><td>{serviceName(t.serviceId)}</td><td>{t.toothPositions}</td><td>{t.qty}</td>
                <td style={{ textAlign: 'right' }}>{money(t.price)}</td><td style={{ textAlign: 'right' }}>{money(t.total)}</td>
                <td style={{ textAlign: 'right' }}>{money(t.paid)}</td><td style={{ textAlign: 'right' }}>{money(t.remain)}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {receipts.length > 0 && (
          <>
            <h4>Đơn thuốc</h4>
            <table className="print-table">
              <thead><tr><th>#</th><th>Thuốc</th><th>Số lượng</th><th>Cách dùng</th></tr></thead>
              <tbody>
                {receipts.map((r, i) => (
                  <tr key={i}>
                    <td>{i + 1}</td><td>{drug(r.drugId)?.name}</td><td>{r.amount} {drug(r.drugId)?.unit}</td>
                    <td>Ngày {r.perDay} lần, mỗi lần {r.perTimes} {drug(r.drugId)?.unit}. {r.instruction}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </>
        )}
        <p>Thanh toán: <b>{money(paidNow)}</b> · Còn nợ: <b>{money(remain)}</b></p>
        {status === 'appoint' && appointmentDate && <p>Hẹn tái khám: <b>{appointmentDate.format('DD/MM/YYYY HH:mm')}</b> {appointmentReason}</p>}
        {advised && <p>Lời dặn: {advised}</p>}
        <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: 32 }}>
          <div style={{ textAlign: 'center', width: 200 }}>Khách hàng<br /><br /><br /></div>
          <div style={{ textAlign: 'center', width: 200 }}>Ngày {dayjs().format('DD/MM/YYYY')}<br />Bác sĩ<br /><br /><br />{empName(doctorName)}</div>
        </div>
      </div>
    </div>
  )
}
