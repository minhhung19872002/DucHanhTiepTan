import { useEffect, useMemo, useState } from 'react'
import { Button, Card, Col, DatePicker, Input, InputNumber, Radio, Row, Space, Statistic, Table, Typography, message } from 'antd'
import { ReloadOutlined, SearchOutlined } from '@ant-design/icons'
import dayjs, { type Dayjs } from 'dayjs'
import { Link } from 'react-router-dom'
import { BranchSelect, ExcelButton, PageTitle, useLoad } from '../../components/common'
import { useAuth } from '../../auth/AuthContext'
import { errorMessage } from '../../api/client'
import { fmtDate, fmtDateTime, money } from '../../utils/format'
import { debtsApi, type DebtCustomer, type DebtPrescription, type DebtTreatment } from './api'

/** "Khách hàng đang nợ" / "Thu nợ khách hàng" – legacy CustomerDebtViewerControl. */
export default function DebtsPage() {
  const { user, branchId } = useAuth()
  const readOnly = !!user?.isAdministrator // legacy: admin form opens the screen read-only
  const [keyword, setKeyword] = useState('')
  const [search, setSearch] = useState('')
  const [byId, setById] = useState(false)
  const [customerId, setCustomerId] = useState<number>()
  const [prescriptionId, setPrescriptionId] = useState<number>()
  const [amount, setAmount] = useState<number | null>(null)
  const [payDate, setPayDate] = useState<Dayjs>(dayjs())
  const [saving, setSaving] = useState(false)

  const params = useMemo(() => ({ branchId, keyword: search || undefined, byId }), [branchId, search, byId])
  const { data, loading, reload } = useLoad(() => debtsApi.list(params), [params])
  const customer = data?.customers.find((c) => c.customerId === customerId)
  const prescription = customer?.prescriptions.find((p) => p.id === prescriptionId)
  const { data: treat, loading: loadingTreat, reload: reloadTreat } = useLoad(
    () => (prescriptionId ? debtsApi.treatments(prescriptionId) : Promise.resolve(undefined)),
    [prescriptionId],
  )

  useEffect(() => {
    if (!data) return
    if (!data.customers.some((c) => c.customerId === customerId)) {
      setCustomerId(data.customers[0]?.customerId)
    }
  }, [data, customerId])
  useEffect(() => {
    if (!customer) { setPrescriptionId(undefined); return }
    if (!customer.prescriptions.some((p) => p.id === prescriptionId)) setPrescriptionId(customer.prescriptions[0]?.id)
  }, [customer, prescriptionId])

  const collect = async () => {
    if (!prescription || !amount) return
    setSaving(true)
    try {
      await debtsApi.collect(prescription.id, amount, payDate.format('YYYY-MM-DD'))
      message.success('Lưu dữ liệu thành công')
      setAmount(null)
      reload()
      reloadTreat()
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setSaving(false)
    }
  }

  const customerColumns = [
    { title: 'No', width: 50, render: (_: unknown, __: DebtCustomer, i: number) => i + 1 },
    { title: 'Khách hàng', dataIndex: 'name' },
    { title: 'Mã KH', dataIndex: 'visiableId', width: 80 },
    { title: 'Tổng phí', dataIndex: 'totalCost', align: 'right' as const, width: 110, render: money },
    { title: 'Đã trả', dataIndex: 'paid', align: 'right' as const, width: 110, render: money },
    { title: 'Còn nợ', dataIndex: 'remain', align: 'right' as const, width: 110, render: (v: number) => <b style={{ color: '#c41d7f' }}>{money(v)}</b> },
  ]
  const prescriptionColumns = [
    { title: 'No', width: 50, render: (_: unknown, __: DebtPrescription, i: number) => i + 1 },
    { title: 'Mã khám', dataIndex: 'id', width: 90, render: (id: number) => <Link to={`/prescriptions/${id}`}>{id}</Link> },
    { title: 'Ngày tạo', dataIndex: 'createdDate', width: 100, render: fmtDate },
    { title: 'Ngày khám cuối', dataIndex: 'lastAppointmentDate', width: 110, render: fmtDate },
    { title: 'Tổng phí', dataIndex: 'total', align: 'right' as const, width: 110, render: money },
    { title: 'Đã trả', dataIndex: 'paid', align: 'right' as const, width: 110, render: money },
    { title: 'Còn nợ', dataIndex: 'remain', align: 'right' as const, width: 110, render: money },
  ]
  const treatmentColumns = [
    { title: 'No', width: 45, render: (_: unknown, __: DebtTreatment, i: number) => i + 1 },
    { title: 'Ngày', dataIndex: 'createdDate', width: 130, render: fmtDateTime },
    { title: 'Nội dung', dataIndex: 'serviceName' },
    { title: 'Vị trí răng', dataIndex: 'positions', width: 90 },
    { title: 'Số lượng', dataIndex: 'quantity', width: 70, align: 'right' as const },
    { title: 'Giá', dataIndex: 'price', width: 100, align: 'right' as const, render: money },
    { title: 'Tổng tiền', dataIndex: 'total', width: 110, align: 'right' as const, render: money },
    { title: 'Thanh toán', dataIndex: 'paid', width: 110, align: 'right' as const, render: money },
    { title: 'Còn lại', dataIndex: 'remain', width: 110, align: 'right' as const, render: money },
    { title: 'Labo', dataIndex: 'labo', width: 140 },
    { title: 'Bác sĩ', dataIndex: 'doctorName', width: 130 },
    { title: 'Phụ tá', dataIndex: 'assistantName', width: 130 },
  ]

  return (
    <div>
      <PageTitle
        title={readOnly ? 'Khách hàng nợ' : 'Thu nợ khách hàng'}
        extra={
          <Space>
            <ExcelButton url="/debts/export" fileName="khach-hang-no.xlsx" params={params} />
            <Button icon={<ReloadOutlined />} onClick={reload}>Tải lại</Button>
          </Space>
        }
      />
      <div className="page-card">
        <div className="toolbar">
          <BranchSelect />
          <Radio.Group value={byId} onChange={(e) => setById(e.target.value)} size="small"
            options={[{ value: false, label: 'Theo tên' }, { value: true, label: 'Theo mã KH' }]} />
          <Input.Search placeholder="Tìm khách hàng" allowClear style={{ width: 260 }} value={keyword}
            onChange={(e) => setKeyword(e.target.value)} onSearch={(v) => setSearch(v.trim())} enterButton={<SearchOutlined />} />
          <Space size="large" style={{ marginLeft: 'auto' }}>
            <Statistic title="Tổng phí" value={data?.totalCost ?? 0} formatter={(v) => money(Number(v))} />
            <Statistic title="Đã trả" value={data?.totalPaid ?? 0} formatter={(v) => money(Number(v))} />
            <Statistic title="Còn nợ" value={data?.totalRemain ?? 0} formatter={(v) => money(Number(v))} styles={{ content: { color: '#c41d7f' } }} />
          </Space>
        </div>
        <Row gutter={12}>
          <Col xs={24} lg={11}>
            <Table size="small" rowKey="customerId" loading={loading} columns={customerColumns} dataSource={data?.customers ?? []}
              pagination={{ pageSize: 20, size: 'small', showTotal: (t) => `${t} khách hàng` }} scroll={{ x: 'max-content', y: 320 }}
              rowClassName={(r) => (r.customerId === customerId ? 'ant-table-row-selected' : '')}
              onRow={(r) => ({ onClick: () => setCustomerId(r.customerId), style: { cursor: 'pointer' } })} />
          </Col>
          <Col xs={24} lg={13}>
            <Table size="small" rowKey="id" columns={prescriptionColumns} dataSource={customer?.prescriptions ?? []} pagination={false} scroll={{ x: 'max-content', y: 320 }}
              rowClassName={(r) => (r.id === prescriptionId ? 'ant-table-row-selected' : '')}
              onRow={(r) => ({ onClick: () => setPrescriptionId(r.id), style: { cursor: 'pointer' } })} />
          </Col>
        </Row>
        <Card size="small" style={{ marginTop: 12 }} title={prescription ? `Chi tiết phiếu khám ${prescription.id} – ${customer?.name}` : 'Chi tiết phiếu khám'}
          extra={
            <Space>
              <Typography.Text>Tổng: <b>{money(treat?.total ?? 0)}</b></Typography.Text>
              <Typography.Text>Còn lại: <b style={{ color: '#c41d7f' }}>{money(treat?.remain ?? 0)}</b></Typography.Text>
            </Space>
          }>
          <Table size="small" rowKey={(r) => `${r.appointmentNo}-${r.subId}`} loading={loadingTreat} columns={treatmentColumns}
            dataSource={treat?.items ?? []} pagination={false} scroll={{ x: 1300, y: 240 }}
            rowClassName={(r) => (r.isCancelled ? 'row-cancelled' : '')} />
          {!readOnly && (
            <Space style={{ marginTop: 12 }} wrap>
              <Typography.Text strong>Thanh toán:</Typography.Text>
              <InputNumber min={0} style={{ width: 160 }} value={amount} onChange={(v) => setAmount(v)} formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, '.')}
                parser={(v) => Number((v ?? '').replace(/\./g, ''))} placeholder="Số tiền" disabled={!prescription} />
              <Button type="link" size="small" disabled={!prescription} onClick={() => setAmount(prescription?.remain ?? 0)}>Trả hết</Button>
              <DatePicker value={payDate} format="DD/MM/YYYY" allowClear={false} onChange={(v) => v && setPayDate(v)} />
              <Button type="primary" loading={saving} disabled={!prescription || !amount} onClick={collect}>Xác nhận thu</Button>
            </Space>
          )}
        </Card>
      </div>
    </div>
  )
}
