import { useEffect, useMemo, useState } from 'react'
import { Button, InputNumber, Select, Space, Statistic, Table } from 'antd'
import { ReloadOutlined } from '@ant-design/icons'
import { Link } from 'react-router-dom'
import { BranchSelect, ExcelButton, PageTitle, useLoad } from '../../components/common'
import { useAuth } from '../../auth/AuthContext'
import { fmtDate, money } from '../../utils/format'
import DateRangeFilter, { type DateRange } from './DateRangeFilter'
import { paymentsApi, type Payment } from './api'

/** "Lịch sử thu" – legacy PaymentHistoryControl. */
export default function PaymentsPage() {
  const { user, branchId } = useAuth()
  const [range, setRange] = useState<DateRange>({})
  const [receptionist, setReceptionist] = useState<string>()
  const [limit, setLimit] = useState(500)
  const [receptionists, setReceptionists] = useState<{ userName: string; fullName?: string }[]>([])

  const params = useMemo(() => ({ branchId, from: range.from, to: range.to, receptionist, limit }), [branchId, range, receptionist, limit])
  const { data, loading, reload } = useLoad(() => paymentsApi.list(params), [params])

  useEffect(() => {
    paymentsApi.receptionists(branchId).then(setReceptionists).catch(() => setReceptionists([]))
  }, [branchId])

  const columns = [
    { title: 'No', width: 50, render: (_: unknown, __: Payment, i: number) => i + 1 },
    { title: 'Ngày', dataIndex: 'createdDate', width: 100, render: fmtDate },
    ...(user?.isAdministrator ? [{ title: 'Chi nhánh', dataIndex: 'branchName', width: 180 }] : []),
    { title: 'Khách hàng', dataIndex: 'customerName' },
    { title: 'Mã KH', dataIndex: 'customerVisiableId', width: 80 },
    { title: 'Số tiền', dataIndex: 'amount', width: 120, align: 'right' as const, render: money },
    { title: 'Mã phiếu khám', dataIndex: 'prescriptionId', width: 110, render: (id: number) => <Link to={`/prescriptions/${id}`}>{id}</Link> },
    { title: 'Tiếp tân thu', width: 160, render: (_: unknown, r: Payment) => r.receptionistFullName || r.receptionistName },
  ]

  return (
    <div>
      <PageTitle title="Lịch sử thu tiền" extra={
        <Space>
          <ExcelButton url="/payments/export" fileName="lich-su-thu.xlsx" params={params} />
          <Button icon={<ReloadOutlined />} onClick={reload}>Tải lại</Button>
        </Space>
      } />
      <div className="page-card">
        <div className="toolbar">
          {user?.isAdministrator && <BranchSelect />}
          <DateRangeFilter value={range} onChange={setRange} modes={['all', 'today', 'day', 'month', 'range']} />
          <Select allowClear placeholder="Tiếp tân" style={{ width: 180 }} value={receptionist} onChange={setReceptionist}
            options={receptionists.map((r) => ({ value: r.userName, label: r.fullName || r.userName }))} />
          <span>Số dòng</span>
          <InputNumber min={10} max={5000} value={limit} onChange={(v) => v && setLimit(v)} style={{ width: 90 }} />
          <Space size="large" style={{ marginLeft: 'auto' }}>
            <Statistic title="Số phiếu thu" value={data?.count ?? 0} />
            <Statistic title="Tổng thu" value={data?.sum ?? 0} formatter={(v) => money(Number(v))} />
          </Space>
        </div>
        <Table size="small" rowKey="id" loading={loading} columns={columns} dataSource={data?.items ?? []}
          pagination={{ pageSize: 50, size: 'small', showSizeChanger: true, showTotal: (t) => `${t} dòng` }} scroll={{ x: 900 }} />
      </div>
    </div>
  )
}
