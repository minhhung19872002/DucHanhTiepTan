import { useCallback, useEffect, useMemo, useState } from 'react'
import { Button, DatePicker, Dropdown, Input, InputNumber, Radio, Space, Table, Tag, Typography, message, type MenuProps } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { DeleteOutlined, EditOutlined, EyeOutlined, FileAddOutlined, PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import dayjs, { type Dayjs } from 'dayjs'
import { useNavigate, useParams } from 'react-router-dom'
import { errorMessage } from '../../api/client'
import { useAuth } from '../../auth/AuthContext'
import { BranchSelect, ExcelButton, PageTitle } from '../../components/common'
import { fmtDate, sexLabel, toLocalDate } from '../../utils/format'
import AddressPicker, { type AddressValue } from './AddressPicker'
import { customersApi, type CustomerDetail, type CustomerQuery, type CustomerRow } from './api'
import CustomerFormModal from './CustomerFormModal'
import ViewCustomerModal from './ViewCustomerModal'

/** Port of CustomerUserControl ("Tìm kiếm khách hàng") + ribbon buttons Tạo mới / Chỉnh sửa / Xóa. */
export default function CustomersPage() {
  const { user, branchId } = useAuth()
  const navigate = useNavigate()
  const params = useParams<{ id?: string }>()

  const [keyword, setKeyword] = useState('')
  const [searchBy, setSearchBy] = useState<CustomerQuery['searchBy']>('name')
  const [dateMode, setDateMode] = useState<CustomerQuery['dateMode']>('all')
  const [range, setRange] = useState<[Dayjs, Dayjs]>([dayjs().startOf('month'), dayjs()])
  const [address, setAddress] = useState<AddressValue>({})
  const [pageSize, setPageSize] = useState(50)
  const [page, setPage] = useState(1)
  const [rows, setRows] = useState<CustomerRow[]>([])
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(false)
  const [selectedId, setSelectedId] = useState<number | null>(null)

  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<CustomerDetail | undefined>()
  const [viewId, setViewId] = useState<number | null>(params.id ? Number(params.id) : null)

  const query: CustomerQuery = useMemo(
    () => ({
      keyword: keyword || undefined,
      searchBy,
      branchId,
      dateMode,
      from: dateMode === 'custom' ? toLocalDate(range[0]) : undefined,
      to: dateMode === 'custom' ? toLocalDate(range[1]) : undefined,
      province: address.provinceName ?? undefined,
      district: address.districtName ?? undefined,
      ward: address.wardName ?? undefined,
      page,
      pageSize,
    }),
    [keyword, searchBy, branchId, dateMode, range, address, page, pageSize],
  )

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const res = await customersApi.list(query)
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

  useEffect(() => {
    if (params.id) setViewId(Number(params.id))
  }, [params.id])

  const selected = rows.find((r) => r.id === selectedId) ?? null

  const openEdit = async (id: number) => {
    try {
      const c = await customersApi.get(id)
      setEditing(c)
      setFormOpen(true)
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  const remove = async (row: CustomerRow) => {
    try {
      await customersApi.remove(row.id)
      message.success('Đã xóa khách hàng')
      setSelectedId(null)
      load()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  const confirmRemove = (row: CustomerRow) => {
    import('antd').then(({ Modal }) =>
      Modal.confirm({
        title: `Xóa khách hàng ${row.name}?`,
        content: 'Chỉ có thể xóa khách hàng không chứa dữ liệu khám.',
        okText: 'Xóa',
        okButtonProps: { danger: true },
        cancelText: 'Hủy',
        onOk: () => remove(row),
      }),
    )
  }

  const newPrescription = (row: CustomerRow) => navigate(`/examination?customerId=${row.id}`)

  const contextItems = (row: CustomerRow): MenuProps['items'] => [
    { key: 'view', icon: <EyeOutlined />, label: 'Xem', onClick: () => setViewId(row.id) },
    { key: 'edit', icon: <EditOutlined />, label: 'Sửa', onClick: () => openEdit(row.id) },
    { key: 'delete', icon: <DeleteOutlined />, label: 'Xóa', danger: true, onClick: () => confirmRemove(row) },
    { type: 'divider' },
    { key: 'new', icon: <FileAddOutlined />, label: 'Tạo phiếu khám mới', onClick: () => newPrescription(row) },
  ]

  const columns: ColumnsType<CustomerRow> = [
    { title: 'No', width: 50, render: (_, __, i) => (page - 1) * pageSize + i + 1 },
    ...(user?.isAdministrator ? [{ title: 'Chi nhánh', dataIndex: 'branchName', width: 170 } as ColumnsType<CustomerRow>[number]] : []),
    { title: 'Khách hàng', dataIndex: 'name', sorter: (a, b) => a.name.localeCompare(b.name), render: (v: string) => <Typography.Text strong>{v}</Typography.Text> },
    { title: 'Mã KH', dataIndex: 'formattedId', width: 90 },
    { title: 'Ngày tạo', dataIndex: 'createdDate', width: 100, render: fmtDate, sorter: (a, b) => a.createdDate.localeCompare(b.createdDate) },
    { title: 'Giới tính', dataIndex: 'sex', width: 80, render: sexLabel },
    { title: 'Năm sinh', dataIndex: 'birthYear', width: 85 },
    { title: 'Địa chỉ', dataIndex: 'addressRepresentation', ellipsis: true },
    { title: 'Điện thoại', dataIndex: 'phone', width: 110 },
    { title: 'Email', dataIndex: 'email', width: 140, ellipsis: true },
    { title: 'Nghề nghiệp', dataIndex: 'job', width: 110 },
  ]

  return (
    <div className="page-card">
      <PageTitle
        title={user?.isAdministrator ? 'Danh sách khách hàng' : `Danh sách khách hàng chi nhánh ${user?.branchName ?? ''}`}
        extra={
          <Space>
            <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditing(undefined); setFormOpen(true) }}>
              Tạo mới
            </Button>
            <Button icon={<EditOutlined />} disabled={!selected} onClick={() => selected && openEdit(selected.id)}>
              Chỉnh sửa
            </Button>
            <Button icon={<DeleteOutlined />} danger disabled={!selected} onClick={() => selected && confirmRemove(selected)}>
              Xóa
            </Button>
            <Button icon={<FileAddOutlined />} disabled={!selected} onClick={() => selected && newPrescription(selected)}>
              Tạo phiếu khám
            </Button>
            <ExcelButton url="/customers/export" fileName="khach-hang.xlsx" params={query as Record<string, unknown>} />
          </Space>
        }
      />
      <div className="toolbar">
        <Input.Search
          placeholder="Tìm kiếm..."
          allowClear
          style={{ width: 260 }}
          onSearch={(v) => { setKeyword(v.trim()); setPage(1) }}
        />
        <Radio.Group value={searchBy} onChange={(e) => { setSearchBy(e.target.value); setPage(1) }}>
          <Radio value="name">Tên</Radio>
          <Radio value="id">Mã KH</Radio>
          <Radio value="phone">Điện thoại</Radio>
          <Radio value="job">Nghề nghiệp</Radio>
        </Radio.Group>
        <span style={{ color: '#888' }}>|</span>
        <span>Ngày tạo:</span>
        <Radio.Group value={dateMode} onChange={(e) => { setDateMode(e.target.value); setPage(1) }}>
          <Radio value="all">Tất cả</Radio>
          <Radio value="today">Hôm nay</Radio>
          <Radio value="custom">Từ</Radio>
        </Radio.Group>
        <DatePicker.RangePicker
          format="DD/MM/YYYY"
          value={range}
          allowClear={false}
          onChange={(v) => {
            if (v && v[0] && v[1]) {
              setRange([v[0], v[1]])
              setDateMode('custom')
              setPage(1)
            }
          }}
        />
        <span>Giới hạn:</span>
        <InputNumber min={10} max={500} value={pageSize} onChange={(v) => v && setPageSize(v)} style={{ width: 80 }} />
        <span>dòng</span>
        <Button icon={<ReloadOutlined />} onClick={load} />
      </div>
      {user?.isAdministrator && (
        <div className="toolbar">
          <span>Chi nhánh:</span>
          <BranchSelect />
          <span>Địa chỉ:</span>
          <AddressPicker value={address} onChange={(v) => { setAddress(v); setPage(1) }} width={170} />
        </div>
      )}
      <Table
        size="small"
        rowKey="id"
        loading={loading}
        columns={columns}
        dataSource={rows}
        scroll={{ x: 1200 }}
        pagination={{ current: page, pageSize, total, showSizeChanger: false, showTotal: (t) => `${t} khách hàng`, onChange: setPage }}
        rowClassName={(r) => (r.id === selectedId ? 'ant-table-row-selected' : '')}
        onRow={(r) => ({
          onClick: () => setSelectedId(r.id),
          onDoubleClick: () => setViewId(r.id),
          onContextMenu: (e) => { e.preventDefault(); setSelectedId(r.id) },
          style: { cursor: 'pointer' },
        })}
        components={{
          body: {
            row: (props: React.HTMLAttributes<HTMLTableRowElement> & { 'data-row-key'?: number }) => {
              const row = rows.find((r) => r.id === props['data-row-key'])
              if (!row) return <tr {...props} />
              return (
                <Dropdown menu={{ items: contextItems(row) }} trigger={['contextMenu']}>
                  <tr {...props} />
                </Dropdown>
              )
            },
          },
        }}
      />
      {selected && (
        <div style={{ marginTop: 8 }}>
          <Tag color="blue">Đang chọn: {selected.name} ({selected.formattedId})</Tag>
        </div>
      )}

      <CustomerFormModal
        open={formOpen}
        customer={editing}
        onClose={() => setFormOpen(false)}
        onSaved={(saved, createPrescription) => {
          load()
          if (createPrescription) navigate(`/examination?customerId=${saved.id}`)
        }}
      />
      <ViewCustomerModal
        customerId={viewId}
        open={viewId !== null}
        onClose={() => {
          setViewId(null)
          if (params.id) navigate('/customers', { replace: true })
        }}
      />
    </div>
  )
}
