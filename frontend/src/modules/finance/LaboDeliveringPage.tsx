import { useEffect, useMemo, useState } from 'react'
import { Button, Checkbox, DatePicker, InputNumber, Modal, Popconfirm, Radio, Select, Space, Statistic, Table, message } from 'antd'
import { EditOutlined, ReloadOutlined } from '@ant-design/icons'
import dayjs, { type Dayjs } from 'dayjs'
import { Link } from 'react-router-dom'
import { BranchSelect, ExcelButton, PageTitle, useLoad } from '../../components/common'
import { useAuth } from '../../auth/AuthContext'
import { errorMessage } from '../../api/client'
import { fmtDate, money } from '../../utils/format'
import DateRangeFilter, { type DateRange } from './DateRangeFilter'
import { laboApi, type LaboItem } from './api'

/** Edit dates / price / color of one labo row (receptionist only). */
function LaboEditModal({ item, onClose, onSaved }: { item?: LaboItem; onClose: () => void; onSaved: () => void }) {
  const [receiver, setReceiver] = useState<Dayjs>(dayjs())
  const [deliver, setDeliver] = useState<Dayjs>(dayjs())
  const [price, setPrice] = useState<number>(0)
  const [saving, setSaving] = useState(false)
  useEffect(() => {
    if (item) {
      setReceiver(dayjs(item.receiverDate))
      setDeliver(dayjs(item.deliverDate))
      setPrice(item.price)
    }
  }, [item])
  const save = async () => {
    if (!item) return
    setSaving(true)
    try {
      await laboApi.update(item, { receiverDate: receiver.format('YYYY-MM-DDTHH:mm:ss'), deliverDate: deliver.format('YYYY-MM-DDTHH:mm:ss'), price })
      message.success('Đã lưu')
      onSaved()
      onClose()
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setSaving(false)
    }
  }
  return (
    <Modal title={item ? `Labo – ${item.customerName} / ${item.serviceName}` : ''} open={!!item} onCancel={onClose} onOk={save} okText="Lưu" cancelText="Hủy" confirmLoading={saving} destroyOnHidden>
      <Space direction="vertical" style={{ width: '100%' }}>
        <Space><span style={{ width: 90, display: 'inline-block' }}>Ngày nhận</span><DatePicker value={receiver} format="DD/MM/YYYY" allowClear={false} onChange={(v) => v && setReceiver(v)} /></Space>
        <Space><span style={{ width: 90, display: 'inline-block' }}>Ngày giao</span><DatePicker value={deliver} format="DD/MM/YYYY" allowClear={false} onChange={(v) => v && setDeliver(v)} /></Space>
        <Space><span style={{ width: 90, display: 'inline-block' }}>Đơn giá</span>
          <InputNumber min={0} style={{ width: 180 }} value={price} onChange={(v) => setPrice(v ?? 0)}
            formatter={(x) => `${x ?? ''}`.replace(/\B(?=(\d{3})+(?!\d))/g, '.')} parser={(x) => Number((x ?? '').replace(/\./g, ''))} />
          <span>x {item?.quantity} = <b>{money((item?.quantity ?? 0) * price)}</b></span>
        </Space>
      </Space>
    </Modal>
  )
}

/** "Labo chờ" / "Labo hẹn" – legacy LaboDeliveringControl. */
export default function LaboDeliveringPage() {
  const { user, branchId } = useAuth()
  const readOnly = !!user?.isAdministrator
  const [receiver, setReceiver] = useState<DateRange>({})
  const [deliver, setDeliver] = useState<DateRange>({})
  const [vendorId, setVendorId] = useState<number>(0)
  const [delivered, setDelivered] = useState<boolean | undefined>(false)
  const [limit, setLimit] = useState(500)
  const [vendors, setVendors] = useState<{ id: number; name: string }[]>([])
  const [editing, setEditing] = useState<LaboItem>()

  const params = useMemo(() => ({
    branchId, receiverFrom: receiver.from, receiverTo: receiver.to, deliverFrom: deliver.from, deliverTo: deliver.to,
    vendorId: vendorId || undefined, delivered, limit,
  }), [branchId, receiver, deliver, vendorId, delivered, limit])
  const { data, loading, reload } = useLoad(() => laboApi.list(params), [params])

  useEffect(() => {
    laboApi.vendors().then(setVendors).catch(() => setVendors([]))
  }, [])

  const toggle = async (item: LaboItem, value: boolean) => {
    try {
      await laboApi.update(item, { delivered: value })
      reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  const columns = [
    { title: 'No', width: 45, render: (_: unknown, __: LaboItem, i: number) => i + 1 },
    { title: 'Nha Khoa', dataIndex: 'laboVendor', width: 160 },
    { title: 'Loại', dataIndex: 'laboType', width: 150 },
    { title: 'Màu sắc', dataIndex: 'color', width: 80 },
    { title: 'Ngày nhận', dataIndex: 'receiverDate', width: 100, render: fmtDate },
    { title: 'Ngày giao', dataIndex: 'deliverDate', width: 100, render: fmtDate },
    {
      title: 'Đã giao', dataIndex: 'delivered', width: 80, align: 'center' as const,
      render: (v: boolean, r: LaboItem) =>
        readOnly ? <Checkbox checked={v} disabled /> : v ? (
          <Popconfirm title="Labo này đã được giao, xác nhận lại là chưa giao ?" okText="Chưa giao" cancelText="Hủy" onConfirm={() => toggle(r, false)}>
            <Checkbox checked />
          </Popconfirm>
        ) : (
          <Checkbox checked={false} onChange={() => toggle(r, true)} />
        ),
    },
    { title: 'Mã bác sĩ', dataIndex: 'doctorName', width: 90 },
    { title: 'Bác sĩ', dataIndex: 'doctorFullName', width: 150 },
    { title: 'Dịch vụ', dataIndex: 'serviceName', width: 180 },
    { title: 'Khách hàng', dataIndex: 'customerName', width: 180, render: (v: string, r: LaboItem) => <Link to={`/prescriptions/${r.prescriptionId}`}>{v}</Link> },
    { title: 'Điện thoại KH', dataIndex: 'customerPhone', width: 110 },
    { title: 'Số lượng', dataIndex: 'quantity', width: 80, align: 'right' as const },
    { title: 'Đơn giá', dataIndex: 'price', width: 100, align: 'right' as const, render: money },
    { title: 'Thành tiền', dataIndex: 'total', width: 110, align: 'right' as const, render: money },
    ...(user?.isAdministrator ? [{ title: 'Chi nhánh', dataIndex: 'branchName', width: 170 }] : []),
    ...(readOnly ? [] : [{ title: '', width: 50, render: (_: unknown, r: LaboItem) => <Button size="small" icon={<EditOutlined />} onClick={() => setEditing(r)} /> }]),
  ]

  return (
    <div>
      <PageTitle title="Giao Labo" extra={
        <Space>
          <ExcelButton url="/labo-delivering/export" fileName="labo.xlsx" params={params} />
          <Button icon={<ReloadOutlined />} onClick={reload}>Tải lại</Button>
        </Space>
      } />
      <div className="page-card">
        <div className="toolbar">
          {user?.isAdministrator && <BranchSelect />}
          <Select style={{ width: 200 }} value={vendorId} onChange={setVendorId}
            options={[{ value: 0, label: 'Tất cả nha khoa (labo)' }, ...vendors.map((v) => ({ value: v.id, label: v.name }))]} />
          <Radio.Group size="small" value={delivered === undefined ? 'all' : delivered ? 'yes' : 'no'}
            onChange={(e) => setDelivered(e.target.value === 'all' ? undefined : e.target.value === 'yes')}
            options={[{ value: 'no', label: 'Chưa giao' }, { value: 'yes', label: 'Đã giao' }, { value: 'all', label: 'Tất cả' }]} optionType="button" buttonStyle="solid" />
          <span>Số dòng</span>
          <InputNumber min={10} max={5000} value={limit} onChange={(v) => v && setLimit(v)} style={{ width: 90 }} />
          <Space size="large" style={{ marginLeft: 'auto' }}>
            <Statistic title="Số labo" value={data?.count ?? 0} />
            <Statistic title="Tổng tiền" value={data?.total ?? 0} formatter={(v) => money(Number(v))} />
          </Space>
        </div>
        <div className="toolbar">
          <span style={{ width: 80 }}>Ngày nhận:</span>
          <DateRangeFilter value={receiver} onChange={setReceiver} modes={['all', 'today', 'thisMonth', 'range']} />
        </div>
        <div className="toolbar">
          <span style={{ width: 80 }}>Ngày giao:</span>
          <DateRangeFilter value={deliver} onChange={setDeliver} modes={['all', 'today', 'thisMonth', 'range']} />
        </div>
        <Table size="small" rowKey={(r) => `${r.prescriptionId}-${r.appointmentNo}-${r.subId}`} loading={loading} columns={columns}
          dataSource={data?.items ?? []} pagination={{ pageSize: 50, size: 'small', showTotal: (t) => `${t} dòng` }} scroll={{ x: 1800 }} />
      </div>
      <LaboEditModal item={editing} onClose={() => setEditing(undefined)} onSaved={reload} />
    </div>
  )
}
