import { useEffect, useState } from 'react'
import { Button, Card, Descriptions, Empty, Space, Spin, Table, Tag, Typography, message } from 'antd'
import { ArrowLeftOutlined, EditOutlined, PrinterOutlined } from '@ant-design/icons'
import dayjs from 'dayjs'
import { useNavigate, useParams } from 'react-router-dom'
import type { ExaminationDetail, Lookups, PrescriptionSummary, Receipt } from './api'
import { examApi, teethCount } from './api'
import ElderTreatmentTable from './ElderTreatmentTable'
import { laboLabel } from './LaboPopover'
import PrintStyles from './PrintStyles'
import { errorMessage } from '../../api/client'
import { fmtDate, fmtDateTime, money, sexLabel } from '../../utils/format'

/** Port of ViewPrescriptionDialog: read-only history of a prescription (all appointments), printable. */
export default function PrescriptionDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [summary, setSummary] = useState<PrescriptionSummary>()
  const [details, setDetails] = useState<ExaminationDetail[]>([])
  const [lookups, setLookups] = useState<Lookups>()
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    if (!id) return
    let cancelled = false
    setLoading(true)
    ;(async () => {
      try {
        const s = await examApi.summary(Number(id))
        const nos = [...s.appointmentNos].sort((a, b) => a - b)
        const ds = await Promise.all(nos.map((no) => examApi.detail(s.id, no)))
        const l = ds.length ? await examApi.lookups(ds[0].customer.branchId) : undefined
        if (cancelled) return
        setSummary(s)
        setDetails(ds)
        setLookups(l)
      } catch (e) {
        message.error(errorMessage(e))
      } finally {
        if (!cancelled) setLoading(false)
      }
    })()
    return () => {
      cancelled = true
    }
  }, [id])

  if (loading) return <Spin style={{ display: 'block', margin: '80px auto' }} />
  if (!summary) return <Empty description="Không tìm thấy phiếu khám" />
  if (!details.length || !lookups) return <Empty description="Phiếu khám chưa có lần khám nào" />

  const first = details[0]
  const last = details[details.length - 1]
  const { customer, branch, prescription } = last
  const serviceName = (sid: number) => lookups.services.find((s) => s.id === sid)?.name ?? `#${sid}`
  const drug = (did: number) => lookups.drugs.find((d) => d.id === did)
  const empName = (u?: string | null) => (u ? lookups.doctors.find((d) => d.userName === u)?.fullName ?? lookups.assistants.find((d) => d.userName === u)?.fullName ?? u : '')
  const allTreatments = details.flatMap((d) => d.treatments)
  const totalPaid = last.payments.reduce((s, p) => s + p.amount, 0)
  const discountText =
    prescription.discountType === 'free'
      ? 'Miễn phí'
      : prescription.membershipId
        ? `Mã VIP ${prescription.membershipId}`
        : prescription.discountValue > 0
          ? `Giảm ${Math.round(prescription.discountValue * 100)}%`
          : 'Không'

  const receiptColumns = [
    { title: 'Thuốc', render: (_: unknown, r: Receipt) => drug(r.drugId)?.name ?? `#${r.drugId}` },
    { title: 'Số lượng', width: 110, render: (_: unknown, r: Receipt) => `${r.amount} ${drug(r.drugId)?.unit ?? ''}` },
    { title: 'Cách dùng', render: (_: unknown, r: Receipt) => `Ngày ${r.perDay} lần, mỗi lần ${r.perTimes} ${drug(r.drugId)?.unit ?? ''}` },
    { title: 'Ghi chú', dataIndex: 'instruction' },
  ]

  return (
    <div>
      <PrintStyles />
      <Space className="no-print" style={{ marginBottom: 12 }} wrap>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(-1)}>Quay lại</Button>
        <Button icon={<PrinterOutlined />} onClick={() => window.print()}>In phiếu khám</Button>
        <Button icon={<EditOutlined />} onClick={() => navigate(`/examination/${summary.id}/${last.appointment.no}`)}>
          Mở lần khám {last.appointment.no}
        </Button>
        <Button onClick={() => navigate(`/customers/${customer.id}`)}>Khách hàng</Button>
      </Space>

      <div className="print-only" style={{ textAlign: 'center', marginBottom: 8 }}>
        <h2 style={{ margin: 0 }}>{branch.name}</h2>
        <div>{branch.address} {branch.tel ? `· ĐT: ${branch.tel}` : ''}</div>
        <h3>PHIẾU KHÁM BỆNH #{summary.id}</h3>
      </div>

      <Card size="small" style={{ marginBottom: 12 }} title={<span className="no-print">Phiếu khám #{summary.id} <Tag color={summary.hasDone ? 'green' : 'orange'}>{summary.hasDone ? 'Đã hoàn tất' : 'Đang điều trị'}</Tag></span>}>
        <Descriptions size="small" bordered column={{ xs: 1, md: 2, lg: 3 }}>
          <Descriptions.Item label="Khách hàng">{customer.name} ({customer.formattedId})</Descriptions.Item>
          <Descriptions.Item label="Giới tính / Năm sinh">{sexLabel(customer.sex)} / {customer.birthYear}</Descriptions.Item>
          <Descriptions.Item label="Điện thoại">{customer.phone}</Descriptions.Item>
          <Descriptions.Item label="Địa chỉ" span={2}>{customer.address}</Descriptions.Item>
          <Descriptions.Item label="Chi nhánh">{branch.name}</Descriptions.Item>
          <Descriptions.Item label="Ngày tạo">{fmtDateTime(summary.createdDate)}</Descriptions.Item>
          <Descriptions.Item label="Số lần khám">{details.length}</Descriptions.Item>
          <Descriptions.Item label="Giảm giá">{discountText}</Descriptions.Item>
          <Descriptions.Item label="Triệu chứng" span={3}>{prescription.symptoms}</Descriptions.Item>
          <Descriptions.Item label="Chẩn đoán" span={3}>{prescription.diagnostic}</Descriptions.Item>
          <Descriptions.Item label="Hướng điều trị" span={3}>{prescription.treatment}</Descriptions.Item>
          <Descriptions.Item label="Đã thanh toán"><b>{money(totalPaid)}</b></Descriptions.Item>
          <Descriptions.Item label="Còn nợ"><b style={{ color: prescription.remain > 0 ? '#c41d7f' : undefined }}>{money(prescription.remain)}</b></Descriptions.Item>
        </Descriptions>
      </Card>

      {first.consultings.length > 0 && (
        <Card size="small" title="Tư vấn" style={{ marginBottom: 12 }}>
          <Table
            size="small"
            rowKey="subId"
            pagination={false}
            dataSource={first.consultings}
            columns={[
              { title: 'Ngày', width: 100, render: (_: unknown, r) => fmtDate(r.createdDate) },
              { title: 'Dịch vụ', render: (_: unknown, r) => serviceName(r.serviceId) },
              { title: 'Số răng', dataIndex: 'toothPositions', width: 100 },
              { title: 'SL', width: 60, align: 'right' as const, render: (_: unknown, r) => (teethCount(r.toothPositions) || r.quantity) },
              { title: 'Giá', width: 110, align: 'right' as const, render: (_: unknown, r) => money(r.price) },
              { title: 'Thành tiền', width: 120, align: 'right' as const, render: (_: unknown, r) => money(r.price * (teethCount(r.toothPositions) || r.quantity)) },
              { title: 'Labo', width: 160, render: (_: unknown, r) => laboLabel(r.labo, lookups.laboTypes, lookups.laboVendors) },
              { title: 'Ghi chú', dataIndex: 'comment' },
            ]}
          />
        </Card>
      )}

      {details.map((d) => (
        <Card
          key={d.appointment.no}
          size="small"
          style={{ marginBottom: 12, breakInside: 'avoid' }}
          title={
            <Space wrap>
              <span>Lần khám {d.appointment.no}</span>
              <Typography.Text type="secondary">{fmtDateTime(d.appointment.createdDate)}</Typography.Text>
              {d.appointment.doctorName && <span>BS: {empName(d.appointment.doctorName)}</span>}
              <Tag className="no-print" color={d.appointment.hasDone ? 'green' : 'orange'}>{d.appointment.hasDone ? 'Đã xong' : 'Đang khám'}</Tag>
            </Space>
          }
          extra={
            <Button className="no-print" size="small" onClick={() => navigate(`/examination/${summary.id}/${d.appointment.no}`)}>
              Mở
            </Button>
          }
        >
          {d.treatments.length > 0 ? (
            <ElderTreatmentTable rows={d.treatments} lookups={lookups} />
          ) : (
            <Typography.Text type="secondary">Không có điều trị</Typography.Text>
          )}
          {d.receipts.length > 0 && (
            <>
              <Typography.Text strong style={{ display: 'block', marginTop: 8 }}>Đơn thuốc</Typography.Text>
              <Table size="small" rowKey="drugId" pagination={false} dataSource={d.receipts} columns={receiptColumns} />
            </>
          )}
          {d.usedEquipments.length > 0 && (
            <div style={{ marginTop: 8 }}>
              <Typography.Text strong>Vật tư sử dụng: </Typography.Text>
              {d.usedEquipments.map((u) => `${lookups.equipment.find((e) => e.id === u.equipmentId)?.name ?? '#' + u.equipmentId} × ${u.quantity}`).join('; ')}
            </div>
          )}
          {(d.appointment.appointmentDate || d.appointment.advised) && (
            <div style={{ marginTop: 8 }}>
              {d.appointment.appointmentDate && (
                <div>Hẹn tái khám: <b>{dayjs(d.appointment.appointmentDate).format('DD/MM/YYYY HH:mm')}</b> {d.appointment.appointmentReason}</div>
              )}
              {d.appointment.advised && <div>Lời dặn: {d.appointment.advised}</div>}
            </div>
          )}
        </Card>
      ))}

      <Card size="small" title="Lịch sử thanh toán" style={{ marginBottom: 12 }}>
        <Table
          size="small"
          rowKey="id"
          pagination={false}
          dataSource={last.payments}
          columns={[
            { title: 'Ngày', dataIndex: 'createdDate', render: (v: string) => fmtDateTime(v) },
            { title: 'Số tiền', dataIndex: 'amount', align: 'right' as const, render: (v: number) => money(v) },
            { title: 'Người thu', dataIndex: 'receptionistName' },
          ]}
          summary={() => (
            <Table.Summary.Row>
              <Table.Summary.Cell index={0}><b>Tổng</b></Table.Summary.Cell>
              <Table.Summary.Cell index={1} align="right"><b>{money(totalPaid)}</b></Table.Summary.Cell>
              <Table.Summary.Cell index={2} />
            </Table.Summary.Row>
          )}
        />
        <div style={{ marginTop: 8 }}>
          Tổng tiền điều trị: <b>{money(allTreatments.filter((t) => !t.isCancelled).reduce((s, t) => s + t.price * t.quantity, 0))}</b> · Còn nợ:{' '}
          <b>{money(prescription.remain)}</b>
        </div>
      </Card>

      <div className="print-only" style={{ marginTop: 24 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between' }}>
          <div style={{ textAlign: 'center', width: 200 }}>Khách hàng<br /><br /><br /></div>
          <div style={{ textAlign: 'center', width: 200 }}>Ngày {dayjs().format('DD/MM/YYYY')}<br />Người lập phiếu<br /><br /><br /></div>
        </div>
      </div>
    </div>
  )
}
