import { useCallback, useEffect, useMemo, useState } from 'react'
import { Button, DatePicker, InputNumber, Radio, Space, Statistic, Table, Tag, message } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { ReloadOutlined } from '@ant-design/icons'
import dayjs, { type Dayjs } from 'dayjs'
import { useNavigate } from 'react-router-dom'
import { errorMessage } from '../../api/client'
import { useAuth } from '../../auth/AuthContext'
import { BranchSelect, ExcelButton, PageTitle } from '../../components/common'
import { fmtDateTime, money, sexLabel, toLocalDate } from '../../utils/format'
import { appointmentsApi, type DashboardItem, type DashboardQuery, type DashboardResult } from './api'

/** Port of AppointmentDashboardControl ("Công việc trong ngày", admin). New customers are highlighted. */
export default function AppointmentDashboardPage() {
  const { branchId } = useAuth()
  const navigate = useNavigate()
  const [dateMode, setDateMode] = useState<DashboardQuery['dateMode']>('today')
  const [day, setDay] = useState<Dayjs>(dayjs())
  const [month, setMonth] = useState<Dayjs>(dayjs())
  const [range, setRange] = useState<[Dayjs, Dayjs]>([dayjs().startOf('month'), dayjs()])
  const [limit, setLimit] = useState(200)
  const [data, setData] = useState<DashboardResult | null>(null)
  const [loading, setLoading] = useState(false)

  const query: DashboardQuery = useMemo(
    () => ({
      branchId,
      dateMode,
      date: dateMode === 'day' ? toLocalDate(day) : undefined,
      month: dateMode === 'month' ? toLocalDate(month.startOf('month')) : undefined,
      from: dateMode === 'range' ? toLocalDate(range[0]) : undefined,
      to: dateMode === 'range' ? toLocalDate(range[1]) : undefined,
      limit,
    }),
    [branchId, dateMode, day, month, range, limit],
  )

  const load = useCallback(async () => {
    setLoading(true)
    try {
      setData(await appointmentsApi.dashboard(query))
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setLoading(false)
    }
  }, [query])

  useEffect(() => {
    load()
  }, [load])

  const columns: ColumnsType<DashboardItem> = [
    { title: 'No', width: 50, render: (_, __, i) => i + 1 },
    { title: 'Chi nhánh', dataIndex: 'branchName', width: 150 },
    { title: 'Ngày tạo', dataIndex: 'createdDate', width: 125, render: fmtDateTime },
    { title: 'Mã KH', dataIndex: 'visiableId', width: 80, render: (v: number) => String(v).padStart(6, '0') },
    { title: 'Khách hàng', dataIndex: 'customerName', width: 180, render: (v: string, r) => <a onClick={() => navigate(`/customers/${r.customerId}`)}>{v}</a> },
    { title: 'Giới tính', dataIndex: 'sex', width: 75, render: sexLabel },
    { title: 'Năm sinh', dataIndex: 'birthYear', width: 80 },
    { title: 'Điện thoại', dataIndex: 'phone', width: 110 },
    { title: 'Địa chỉ', dataIndex: 'address', width: 220, ellipsis: true },
    { title: 'Nội dung làm việc trong ngày', dataIndex: 'content', width: 260, ellipsis: true },
    { title: 'Tổng (nếu không giảm)', dataIndex: 'totalIfNotDiscount', width: 130, align: 'right', render: money },
    { title: 'Tổng', dataIndex: 'total', width: 110, align: 'right', render: money },
    { title: 'Đã trả', dataIndex: 'paid', width: 110, align: 'right', render: money },
    { title: 'Còn lại', dataIndex: 'remain', width: 110, align: 'right', render: (v: number) => <span style={{ color: v > 0 ? '#cf1322' : undefined }}>{money(v)}</span> },
    { title: 'Kết thúc', dataIndex: 'finished', width: 80, align: 'center', render: (v: boolean) => (v ? <Tag>R</Tag> : null) },
    { title: 'Bác sĩ', dataIndex: 'doctorFullName', width: 150 },
    { title: 'Ghi chú', dataIndex: 'comment', ellipsis: true },
    { title: 'Phiếu', dataIndex: 'prescriptionId', width: 80, render: (v: number, r) => <a onClick={() => navigate(`/prescriptions/${v}`)}>{v}/{r.no}</a> },
  ]

  return (
    <div className="page-card">
      <PageTitle
        title="Công việc trong ngày"
        extra={
          <Space>
            <ExcelButton url="/appointments/dashboard/export" fileName="cong-viec-trong-ngay.xlsx" params={query as Record<string, unknown>} />
            <Button icon={<ReloadOutlined />} onClick={load} />
          </Space>
        }
      />
      <div className="toolbar">
        <span>Ngày:</span>
        <Radio.Group value={dateMode} onChange={(e) => setDateMode(e.target.value)}>
          <Radio value="all">Tất cả</Radio>
          <Radio value="today">Hôm nay</Radio>
          <Radio value="yesterday">Hôm qua</Radio>
          <Radio value="day">Ngày</Radio>
          <Radio value="month">Tháng</Radio>
          <Radio value="range">Giới hạn từ</Radio>
        </Radio.Group>
        <DatePicker format="DD/MM/YYYY" value={day} allowClear={false} onChange={(v) => { if (v) { setDay(v); setDateMode('day') } }} />
        <DatePicker picker="month" format="MM/YYYY" value={month} allowClear={false} onChange={(v) => { if (v) { setMonth(v); setDateMode('month') } }} />
        <DatePicker.RangePicker format="DD/MM/YYYY" value={range} allowClear={false} onChange={(v) => { if (v && v[0] && v[1]) { setRange([v[0], v[1]]); setDateMode('range') } }} />
        <span>Giới hạn:</span>
        <InputNumber min={10} max={2000} value={limit} onChange={(v) => v && setLimit(v)} style={{ width: 90 }} />
        <span>dòng dữ liệu.</span>
        <span>Chi nhánh:</span>
        <BranchSelect />
      </div>
      <Space size="large" style={{ marginBottom: 12 }}>
        <Statistic title="Tổng tiền" value={data?.total ?? 0} formatter={(v) => money(Number(v))} />
        <Statistic title="Tổng nếu không giảm" value={data?.totalIfNotDiscount ?? 0} formatter={(v) => money(Number(v))} />
        <Statistic title="Đã trả" value={data?.paid ?? 0} formatter={(v) => money(Number(v))} />
        <Statistic title="Còn lại" value={data?.remain ?? 0} formatter={(v) => money(Number(v))} styles={{ content: { color: '#cf1322' } }} />
        <Tag color="cyan">Khách hàng mới trong ngày</Tag>
      </Space>
      <Table
        size="small"
        rowKey={(r) => `${r.prescriptionId}-${r.no}`}
        loading={loading}
        columns={columns}
        dataSource={data?.items ?? []}
        scroll={{ x: 2300 }}
        pagination={{ pageSize: 50, showSizeChanger: false, showTotal: (t) => `${t} lượt khám` }}
        onRow={(r) => ({ style: r.isNew ? { background: '#c5f5f5' } : undefined })}
      />
    </div>
  )
}
