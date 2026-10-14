import { useCallback, useEffect, useMemo, useState } from 'react'
import { Button, DatePicker, Input, InputNumber, Radio, Space, Table, Tag, message } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { DeleteOutlined, ReloadOutlined } from '@ant-design/icons'
import dayjs, { type Dayjs } from 'dayjs'
import { useNavigate } from 'react-router-dom'
import { errorMessage } from '../../api/client'
import { useAuth } from '../../auth/AuthContext'
import { BranchSelect, ExcelButton, PageTitle } from '../../components/common'
import { fmtDate, fmtDateTime, money, toLocalDate } from '../../utils/format'
import { prescriptionsApi, type PrescriptionQuery, type PrescriptionRow } from './api'

/** Port of PrescriptionSearcherControl ("Lịch sử khám"). Row double-click opens /prescriptions/:id. */
export default function PrescriptionsPage() {
  const { user, branchId } = useAuth()
  const navigate = useNavigate()

  const [customerKeyword, setCustomerKeyword] = useState('')
  const [customerSearchBy, setCustomerSearchBy] = useState<PrescriptionQuery['customerSearchBy']>('name')
  const [diseaseKeyword, setDiseaseKeyword] = useState('')
  const [diseaseField, setDiseaseField] = useState<PrescriptionQuery['diseaseField']>('symptoms')
  const [createdMode, setCreatedMode] = useState<'all' | 'custom'>('all')
  const [createdRange, setCreatedRange] = useState<[Dayjs, Dayjs]>([dayjs().startOf('month'), dayjs()])
  const [lastMode, setLastMode] = useState<'all' | 'custom'>('all')
  const [lastRange, setLastRange] = useState<[Dayjs, Dayjs]>([dayjs().startOf('month'), dayjs()])
  const [pageSize, setPageSize] = useState(50)
  const [page, setPage] = useState(1)
  const [rows, setRows] = useState<PrescriptionRow[]>([])
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(false)
  const [selectedId, setSelectedId] = useState<number | null>(null)

  const query: PrescriptionQuery = useMemo(
    () => ({
      branchId,
      customerKeyword: customerKeyword || undefined,
      customerSearchBy,
      diseaseKeyword: diseaseKeyword || undefined,
      diseaseField,
      createdFrom: createdMode === 'custom' ? toLocalDate(createdRange[0]) : undefined,
      createdTo: createdMode === 'custom' ? toLocalDate(createdRange[1]) : undefined,
      lastFrom: lastMode === 'custom' ? toLocalDate(lastRange[0]) : undefined,
      lastTo: lastMode === 'custom' ? toLocalDate(lastRange[1]) : undefined,
      page,
      pageSize,
    }),
    [branchId, customerKeyword, customerSearchBy, diseaseKeyword, diseaseField, createdMode, createdRange, lastMode, lastRange, page, pageSize],
  )

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const res = await prescriptionsApi.list(query)
      setRows(res.items)
      setTotal(res.total)
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setLoading(false)
    }
  }, [query])

  useEffect(() => {
    load()
  }, [load])

  const remove = (row: PrescriptionRow) => {
    import('antd').then(({ Modal }) =>
      Modal.confirm({
        title: `Xóa phiếu khám #${row.id} của ${row.customerName}?`,
        content: 'Toàn bộ dữ liệu khám của phiếu sẽ bị xóa và không thể phục hồi.',
        okText: 'Xóa',
        okButtonProps: { danger: true },
        cancelText: 'Hủy',
        onOk: async () => {
          try {
            await prescriptionsApi.remove(row.id)
            message.success('Đã xóa phiếu khám')
            setSelectedId(null)
            load()
          } catch (e) {
            message.error(errorMessage(e))
          }
        },
      }),
    )
  }

  const columns: ColumnsType<PrescriptionRow> = [
    { title: 'No', width: 50, render: (_, __, i) => (page - 1) * pageSize + i + 1 },
    { title: 'Mã phiếu', dataIndex: 'id', width: 85 },
    ...(user?.isAdministrator ? [{ title: 'Chi nhánh', dataIndex: 'branchName', width: 160 } as ColumnsType<PrescriptionRow>[number]] : []),
    { title: 'Khách hàng', dataIndex: 'customerName', render: (v: string, r) => <a onClick={() => navigate(`/customers/${r.customerId}`)}>{v}</a> },
    { title: 'Mã KH', dataIndex: 'customerVisiableId', width: 80, render: (v: number) => String(v).padStart(6, '0') },
    { title: 'Ngày tạo', dataIndex: 'createdDate', width: 95, render: fmtDate },
    { title: 'Số lần hẹn', dataIndex: 'appointmentNo', width: 80, align: 'center' },
    { title: 'Ngày hẹn cuối', dataIndex: 'lastAppointmentDate', width: 105, render: fmtDate },
    { title: 'Yêu cầu điều trị', dataIndex: 'treatment', ellipsis: true },
    { title: 'Triệu chứng', dataIndex: 'diagnostic', ellipsis: true },
    { title: 'Chuẩn đoán', dataIndex: 'symptoms', ellipsis: true },
    { title: 'Lịch hẹn', dataIndex: 'lastNextAppointmentDate', width: 125, render: fmtDateTime },
    { title: 'Lí do hẹn', dataIndex: 'lastAppointmentReason', ellipsis: true },
    { title: 'Tổng tiền', dataIndex: 'total', width: 100, align: 'right', render: money },
    { title: 'Đã trả', dataIndex: 'paid', width: 100, align: 'right', render: money },
    { title: 'Còn lại', dataIndex: 'remain', width: 100, align: 'right', render: (v: number) => <span style={{ color: v > 0 ? '#cf1322' : undefined }}>{money(v)}</span> },
    { title: 'Trạng thái', dataIndex: 'hasDone', width: 95, render: (v: boolean) => (v ? <Tag color="green">Đã xong</Tag> : <Tag color="processing">Đang khám</Tag>) },
  ]

  const selected = rows.find((r) => r.id === selectedId)

  return (
    <div className="page-card">
      <PageTitle
        title="Lịch sử khám"
        extra={
          <Space>
            {user?.isAdministrator && (
              <Button danger icon={<DeleteOutlined />} disabled={!selected} onClick={() => selected && remove(selected)}>
                Xóa phiếu
              </Button>
            )}
            <ExcelButton url="/prescriptions/export" fileName="lich-su-kham.xlsx" params={query as Record<string, unknown>} />
            <Button icon={<ReloadOutlined />} onClick={load} />
          </Space>
        }
      />
      <div className="toolbar">
        <span>Khách hàng:</span>
        <Input.Search placeholder="Tìm..." allowClear style={{ width: 220 }} onSearch={(v) => { setCustomerKeyword(v.trim()); setPage(1) }} />
        <Radio.Group value={customerSearchBy} onChange={(e) => { setCustomerSearchBy(e.target.value); setPage(1) }}>
          <Radio value="name">Khách hàng</Radio>
          <Radio value="customerId">Mã khách hàng</Radio>
          <Radio value="prescriptionId">Mã phiếu</Radio>
        </Radio.Group>
        <span style={{ color: '#888' }}>|</span>
        <span>Tìm theo bệnh:</span>
        <Input.Search placeholder="Tìm..." allowClear style={{ width: 200 }} onSearch={(v) => { setDiseaseKeyword(v.trim()); setPage(1) }} />
        <Radio.Group value={diseaseField} onChange={(e) => { setDiseaseField(e.target.value); setPage(1) }}>
          <Radio value="symptoms">Chuẩn đoán</Radio>
          <Radio value="diagnostic">Triệu chứng</Radio>
          <Radio value="treatment">Yêu cầu</Radio>
        </Radio.Group>
      </div>
      <div className="toolbar">
        <span>Ngày khám đầu:</span>
        <Radio.Group value={createdMode} onChange={(e) => { setCreatedMode(e.target.value); setPage(1) }}>
          <Radio value="all">Tất cả</Radio>
          <Radio value="custom">Từ</Radio>
        </Radio.Group>
        <DatePicker.RangePicker format="DD/MM/YYYY" value={createdRange} allowClear={false} onChange={(v) => { if (v && v[0] && v[1]) { setCreatedRange([v[0], v[1]]); setCreatedMode('custom'); setPage(1) } }} />
        <span style={{ color: '#888' }}>|</span>
        <span>Ngày khám cuối:</span>
        <Radio.Group value={lastMode} onChange={(e) => { setLastMode(e.target.value); setPage(1) }}>
          <Radio value="all">Tất cả</Radio>
          <Radio value="custom">Từ</Radio>
        </Radio.Group>
        <DatePicker.RangePicker format="DD/MM/YYYY" value={lastRange} allowClear={false} onChange={(v) => { if (v && v[0] && v[1]) { setLastRange([v[0], v[1]]); setLastMode('custom'); setPage(1) } }} />
        <span>Giới hạn:</span>
        <InputNumber min={10} max={500} value={pageSize} onChange={(v) => v && setPageSize(v)} style={{ width: 80 }} />
        <span>dòng.</span>
        {user?.isAdministrator && (
          <>
            <span>Chi nhánh:</span>
            <BranchSelect />
          </>
        )}
      </div>
      <Table
        size="small"
        rowKey="id"
        loading={loading}
        columns={columns}
        dataSource={rows}
        scroll={{ x: 1700 }}
        pagination={{ current: page, pageSize, total, showSizeChanger: false, showTotal: (t) => `${t} phiếu khám`, onChange: setPage }}
        rowClassName={(r) => (r.id === selectedId ? 'ant-table-row-selected' : '')}
        onRow={(r) => ({ onClick: () => setSelectedId(r.id), onDoubleClick: () => navigate(`/prescriptions/${r.id}`), style: { cursor: 'pointer' } })}
      />
    </div>
  )
}
