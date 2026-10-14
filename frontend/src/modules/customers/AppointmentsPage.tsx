import { useCallback, useEffect, useMemo, useState } from 'react'
import { Button, DatePicker, Input, Modal, Radio, Space, Table, Tag, message } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { CalendarOutlined, CloseCircleOutlined, MedicineBoxOutlined, ReloadOutlined } from '@ant-design/icons'
import dayjs, { type Dayjs } from 'dayjs'
import { useNavigate } from 'react-router-dom'
import { errorMessage } from '../../api/client'
import { useAuth } from '../../auth/AuthContext'
import { BranchSelect, ExcelButton, PageTitle } from '../../components/common'
import { fmtDateTime, toLocalDate, toLocalIso } from '../../utils/format'
import { appointmentsApi, type AppointmentQuery, type AppointmentRow } from './api'

/** Port of AppointingControl ("Danh sách hẹn") + ribbon buttons Khám / Hủy / Đổi. */
export default function AppointmentsPage() {
  const { user, branchId } = useAuth()
  const navigate = useNavigate()

  const [dateMode, setDateMode] = useState<AppointmentQuery['dateMode']>('today')
  const [date, setDate] = useState<Dayjs>(dayjs())
  const [range, setRange] = useState<[Dayjs, Dayjs]>([dayjs(), dayjs().add(7, 'day')])
  const [status, setStatus] = useState<AppointmentQuery['status']>('all')
  const [keyword, setKeyword] = useState('')
  const [searchBy, setSearchBy] = useState<AppointmentQuery['searchBy']>('name')
  const [rows, setRows] = useState<AppointmentRow[]>([])
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(false)
  const [selectedKey, setSelectedKey] = useState<string | null>(null)
  const [changeOpen, setChangeOpen] = useState(false)
  const [newDate, setNewDate] = useState<Dayjs | null>(null)

  const query: AppointmentQuery = useMemo(
    () => ({
      branchId,
      dateMode,
      date: dateMode === 'date' ? toLocalDate(date) : undefined,
      from: dateMode === 'range' ? toLocalDate(range[0]) : undefined,
      to: dateMode === 'range' ? toLocalDate(range[1]) : undefined,
      status,
      keyword: keyword || undefined,
      searchBy,
      page,
      pageSize: 100,
    }),
    [branchId, dateMode, date, range, status, keyword, searchBy, page],
  )

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const res = await appointmentsApi.list(query)
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

  const keyOf = (r: AppointmentRow) => `${r.prescriptionId}-${r.no}`
  const selected = rows.find((r) => keyOf(r) === selectedKey) ?? null

  const requireSelected = (): AppointmentRow | null => {
    if (!selected) {
      message.warning('Vui lòng chọn khách hàng từ danh sách lịch hẹn')
      return null
    }
    if (!selected.appointmentDate || selected.isAppointed) {
      message.warning('Phiếu khám này đã hẹn rồi.')
      return null
    }
    return selected
  }

  const examine = async () => {
    const row = requireSelected()
    if (!row) return
    try {
      const res = await appointmentsApi.examine(row.prescriptionId)
      navigate(`/examination/${res.prescriptionId}/${res.no}`)
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  const cancel = () => {
    const row = requireSelected()
    if (!row) return
    Modal.confirm({
      title: 'Thông báo',
      content: 'Bạn có muốn hủy hẹn ?',
      okText: 'Có',
      cancelText: 'Không',
      onOk: async () => {
        try {
          await appointmentsApi.cancel(row.prescriptionId, row.no)
          message.success('Lưu dữ liệu thành công.')
          setSelectedKey(null)
          load()
        } catch (e) {
          message.error(errorMessage(e))
        }
      },
    })
  }

  const openChange = () => {
    const row = requireSelected()
    if (!row) return
    setNewDate(dayjs(row.appointmentDate))
    setChangeOpen(true)
  }

  const saveChange = async () => {
    if (!selected || !newDate) return
    try {
      await appointmentsApi.changeDate(selected.prescriptionId, selected.no, toLocalIso(newDate)!)
      message.success('Lưu dữ liệu thành công.')
      setChangeOpen(false)
      setSelectedKey(null)
      load()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  const columns: ColumnsType<AppointmentRow> = [
    { title: 'No', width: 50, render: (_, __, i) => (page - 1) * 100 + i + 1 },
    { title: 'Ngày', dataIndex: 'appointmentDate', width: 130, render: fmtDateTime, sorter: (a, b) => (a.appointmentDate ?? '').localeCompare(b.appointmentDate ?? '') },
    { title: 'Bác sĩ hẹn', dataIndex: 'doctorFullName', width: 160 },
    { title: 'Khách hàng', dataIndex: 'customerName', render: (v: string, r) => <a onClick={(e) => { e.stopPropagation(); navigate(`/customers/${r.customerId}`) }}>{v}</a> },
    { title: 'Mã KH', dataIndex: 'customerVisiableId', width: 80, render: (v: number) => String(v).padStart(6, '0') },
    { title: 'Điện thoại', dataIndex: 'customerPhone', width: 110 },
    { title: 'Lí do hẹn', dataIndex: 'appointmentReason', ellipsis: true },
    { title: 'Phiếu', dataIndex: 'prescriptionId', width: 80, render: (v: number) => <a onClick={(e) => { e.stopPropagation(); navigate(`/prescriptions/${v}`) }}>{v}</a> },
    ...(user?.isAdministrator ? [{ title: 'Chi nhánh', dataIndex: 'branchName', width: 160 } as ColumnsType<AppointmentRow>[number]] : []),
    { title: 'Trạng thái', dataIndex: 'status', width: 95, render: (v: string, r) => <Tag color={r.isAppointed ? 'green' : 'orange'}>{v}</Tag> },
  ]

  return (
    <div className="page-card">
      <PageTitle
        title="Danh sách hẹn"
        extra={
          <Space>
            <Button type="primary" icon={<MedicineBoxOutlined />} onClick={examine} disabled={!selected}>
              Khám
            </Button>
            <Button icon={<CloseCircleOutlined />} onClick={cancel} disabled={!selected}>
              Hủy
            </Button>
            <Button icon={<CalendarOutlined />} onClick={openChange} disabled={!selected}>
              Đổi
            </Button>
            <ExcelButton url="/appointments/export" fileName="danh-sach-hen.xlsx" params={query as Record<string, unknown>} />
            <Button icon={<ReloadOutlined />} onClick={load} />
          </Space>
        }
      />
      <div className="toolbar">
        <span>Hẹn ngày:</span>
        <Radio.Group value={dateMode} onChange={(e) => { setDateMode(e.target.value); setPage(1) }}>
          <Radio value="today">Hôm nay</Radio>
          <Radio value="date">Ngày</Radio>
          <Radio value="range">Từ</Radio>
        </Radio.Group>
        <DatePicker format="DD/MM/YYYY" value={date} allowClear={false} onChange={(v) => { if (v) { setDate(v); setDateMode('date'); setPage(1) } }} />
        <DatePicker.RangePicker format="DD/MM/YYYY" value={range} allowClear={false} onChange={(v) => { if (v && v[0] && v[1]) { setRange([v[0], v[1]]); setDateMode('range'); setPage(1) } }} />
        <span style={{ color: '#888' }}>|</span>
        <span>Trạng thái:</span>
        <Radio.Group value={status} onChange={(e) => { setStatus(e.target.value); setPage(1) }}>
          <Radio value="all">Tất cả</Radio>
          <Radio value="appointing">Đang hẹn</Radio>
          <Radio value="appointed">Đã hẹn</Radio>
        </Radio.Group>
      </div>
      <div className="toolbar">
        <Input.Search placeholder="Tìm..." allowClear style={{ width: 220 }} onSearch={(v) => { setKeyword(v.trim()); setPage(1) }} />
        <Radio.Group value={searchBy} onChange={(e) => { setSearchBy(e.target.value); setPage(1) }}>
          <Radio value="name">Khách hàng</Radio>
          <Radio value="customerId">Mã khách hàng</Radio>
          <Radio value="prescriptionId">Mã phiếu</Radio>
        </Radio.Group>
        {user?.isAdministrator && (
          <>
            <span>Chi nhánh:</span>
            <BranchSelect />
          </>
        )}
      </div>
      <Table
        size="small"
        rowKey={keyOf}
        loading={loading}
        columns={columns}
        dataSource={rows}
        scroll={{ x: 1100 }}
        pagination={{ current: page, pageSize: 100, total, showSizeChanger: false, showTotal: (t) => `${t} lịch hẹn`, onChange: setPage }}
        rowClassName={(r) => (keyOf(r) === selectedKey ? 'ant-table-row-selected' : '')}
        onRow={(r) => ({ onClick: () => setSelectedKey(keyOf(r)), onDoubleClick: () => { setSelectedKey(keyOf(r)) }, style: { cursor: 'pointer' } })}
      />

      <Modal title="Chọn ngày" open={changeOpen} onOk={saveChange} onCancel={() => setChangeOpen(false)} okText="OK" cancelText="Cancel" width={380}>
        <DatePicker showTime format="DD/MM/YYYY HH:mm" value={newDate} onChange={setNewDate} style={{ width: '100%' }} />
      </Modal>
    </div>
  )
}
