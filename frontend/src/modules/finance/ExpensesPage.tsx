import { useMemo, useState } from 'react'
import { Button, Col, DatePicker, Input, InputNumber, Modal, Row, Space, Statistic, Table, Typography, message } from 'antd'
import { DeleteOutlined, EditOutlined, PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import dayjs, { type Dayjs } from 'dayjs'
import { BranchSelect, DeleteButton, ExcelButton, PageTitle, useLoad } from '../../components/common'
import { useAuth } from '../../auth/AuthContext'
import { errorMessage } from '../../api/client'
import { fmtDate, money } from '../../utils/format'
import DateRangeFilter, { type DateRange } from './DateRangeFilter'
import { expensesApi, type Expense, type ExpenseLine } from './api'

interface EditLine extends ExpenseLine {
  key: number
}

/** Create / edit dialog mirroring DeclareExpenseDialog (lines: nội dung + số tiền). */
function ExpenseModal({ open, expenseId, onClose, onSaved }: { open: boolean; expenseId?: number; onClose: () => void; onSaved: () => void }) {
  const [date, setDate] = useState<Dayjs>(dayjs())
  const [lines, setLines] = useState<EditLine[]>([{ key: 1, description: '', amount: 0 }])
  const [saving, setSaving] = useState(false)
  const [loading, setLoading] = useState(false)

  useLoad(async () => {
    if (!open) return
    if (expenseId) {
      setLoading(true)
      try {
        const d = await expensesApi.get(expenseId)
        setDate(dayjs(d.createdDate))
        setLines(d.items.map((l, i) => ({ key: i + 1, ...l })))
      } finally {
        setLoading(false)
      }
    } else {
      setDate(dayjs())
      setLines([{ key: 1, description: '', amount: 0 }])
    }
  }, [open, expenseId])

  const update = (key: number, patch: Partial<ExpenseLine>) => setLines((ls) => ls.map((l) => (l.key === key ? { ...l, ...patch } : l)))
  const total = lines.reduce((s, l) => s + (l.amount || 0), 0)

  const save = async () => {
    const items = lines.filter((l) => l.description.trim() || l.amount)
    if (items.length === 0) { message.warning('Vui lòng nhập ít nhất một dòng chi'); return }
    for (const l of items) {
      if (!l.description.trim()) { message.warning('Vui lòng điền nội dung.'); return }
      if (!l.amount || l.amount <= 0) { message.warning('Chi phí phải lớn hơn 0.'); return }
    }
    setSaving(true)
    try {
      const payload = { createdDate: date.format('YYYY-MM-DD'), items: items.map((l) => ({ description: l.description.trim(), amount: l.amount })) }
      if (expenseId) await expensesApi.update(expenseId, payload)
      else await expensesApi.create(payload)
      message.success('Đã lưu phiếu chi')
      onSaved()
      onClose()
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal title={expenseId ? `Sửa phiếu chi #${expenseId}` : 'Tạo phiếu chi'} open={open} onCancel={onClose} onOk={save} okText="Lưu" cancelText="Hủy"
      confirmLoading={saving} width={720} destroyOnHidden>
      <Space style={{ marginBottom: 12 }}>
        <span>Ngày chi:</span>
        <DatePicker value={date} format="DD/MM/YYYY" allowClear={false} onChange={(v) => v && setDate(v)} />
        <Button size="small" onClick={() => setDate(dayjs())}>Hôm nay</Button>
      </Space>
      <Table size="small" rowKey="key" loading={loading} dataSource={lines} pagination={false}
        columns={[
          { title: 'No', width: 50, render: (_: unknown, __: EditLine, i: number) => i + 1 },
          { title: 'Nội dung', dataIndex: 'description', render: (v: string, r: EditLine) => <Input value={v} onChange={(e) => update(r.key, { description: e.target.value })} placeholder="Nội dung chi" /> },
          { title: 'Số tiền', dataIndex: 'amount', width: 170, render: (v: number, r: EditLine) => (
            <InputNumber min={0} style={{ width: '100%' }} value={v || null} onChange={(n) => update(r.key, { amount: n ?? 0 })}
              formatter={(x) => `${x ?? ''}`.replace(/\B(?=(\d{3})+(?!\d))/g, '.')} parser={(x) => Number((x ?? '').replace(/\./g, ''))} />
          ) },
          { title: '', width: 50, render: (_: unknown, r: EditLine) => <Button size="small" danger icon={<DeleteOutlined />} onClick={() => setLines((ls) => ls.filter((l) => l.key !== r.key))} /> },
        ]}
        footer={() => (
          <Space style={{ width: '100%', justifyContent: 'space-between' }}>
            <Button size="small" icon={<PlusOutlined />} onClick={() => setLines((ls) => [...ls, { key: (ls.at(-1)?.key ?? 0) + 1, description: '', amount: 0 }])}>Thêm dòng</Button>
            <Typography.Text>Số dòng: <b>{lines.length}</b> &nbsp; Tổng chi: <b>{money(total)}</b></Typography.Text>
          </Space>
        )} />
    </Modal>
  )
}

/** "Phiếu chi" / "Danh sách chi" – legacy ExpenseManagerControl. */
export default function ExpensesPage() {
  const { user, branchId } = useAuth()
  const [range, setRange] = useState<DateRange>({})
  const [limit, setLimit] = useState(500)
  const [selectedId, setSelectedId] = useState<number>()
  const [modal, setModal] = useState<{ open: boolean; id?: number }>({ open: false })

  const params = useMemo(() => ({ branchId, from: range.from, to: range.to, limit }), [branchId, range, limit])
  const { data, loading, reload } = useLoad(() => expensesApi.list(params), [params])
  const { data: detail, loading: loadingDetail, reload: reloadDetail } = useLoad(
    () => (selectedId ? expensesApi.get(selectedId) : Promise.resolve(undefined)),
    [selectedId],
  )

  const remove = async (id: number) => {
    try {
      await expensesApi.remove(id)
      message.success('Đã xóa phiếu chi')
      if (selectedId === id) setSelectedId(undefined)
      reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  const columns = [
    { title: 'No', width: 50, render: (_: unknown, __: Expense, i: number) => i + 1 },
    { title: 'Số phiếu', dataIndex: 'id', width: 80 },
    { title: 'Ngày', dataIndex: 'createdDate', width: 100, render: fmtDate },
    ...(user?.isAdministrator ? [{ title: 'Chi nhánh', dataIndex: 'branchName', width: 180 }] : []),
    { title: 'Người chi', width: 150, render: (_: unknown, r: Expense) => r.accountantFullName || r.accountantName },
    { title: 'Tổng tiền', dataIndex: 'total', width: 120, align: 'right' as const, render: money },
    {
      title: '', width: 90, render: (_: unknown, r: Expense) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={(e) => { e.stopPropagation(); setModal({ open: true, id: r.id }) }} />
          <DeleteButton title="Xóa phiếu chi này?" onConfirm={() => remove(r.id)} />
        </Space>
      ),
    },
  ]

  return (
    <div>
      <PageTitle title="Danh sách chi" extra={
        <Space>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setModal({ open: true })}>Tạo phiếu chi</Button>
          <ExcelButton url="/expenses/export" fileName="phieu-chi.xlsx" params={params} />
          <Button icon={<ReloadOutlined />} onClick={reload}>Tải lại</Button>
        </Space>
      } />
      <div className="page-card">
        <div className="toolbar">
          {user?.isAdministrator && <BranchSelect />}
          <DateRangeFilter value={range} onChange={setRange} />
          <span>Số dòng</span>
          <InputNumber min={10} max={5000} value={limit} onChange={(v) => v && setLimit(v)} style={{ width: 90 }} />
          <Space size="large" style={{ marginLeft: 'auto' }}>
            <Statistic title="Số phiếu" value={data?.count ?? 0} />
            <Statistic title="Tổng chi" value={data?.total ?? 0} formatter={(v) => money(Number(v))} />
          </Space>
        </div>
        <Row gutter={12}>
          <Col xs={24} lg={14}>
            <Table size="small" rowKey="id" loading={loading} columns={columns} dataSource={data?.items ?? []}
              pagination={{ pageSize: 25, size: 'small', showTotal: (t) => `${t} phiếu` }}
              rowClassName={(r) => (r.id === selectedId ? 'ant-table-row-selected' : '')}
              onRow={(r) => ({ onClick: () => setSelectedId(r.id), style: { cursor: 'pointer' } })} />
          </Col>
          <Col xs={24} lg={10}>
            <Table size="small" rowKey="description" loading={loadingDetail} dataSource={detail?.items ?? []} pagination={false}
              title={() => (detail ? `Chi tiết phiếu #${detail.id} – ${fmtDate(detail.createdDate)}` : 'Chi tiết phiếu chi')}
              columns={[
                { title: 'No', width: 50, render: (_: unknown, __: ExpenseLine, i: number) => i + 1 },
                { title: 'Nội dung', dataIndex: 'description' },
                { title: 'Số tiền', dataIndex: 'amount', width: 130, align: 'right' as const, render: money },
              ]}
              summary={() => detail ? (
                <Table.Summary.Row>
                  <Table.Summary.Cell index={0} colSpan={2}><b>Tổng</b></Table.Summary.Cell>
                  <Table.Summary.Cell index={1} align="right"><b>{money(detail.total)}</b></Table.Summary.Cell>
                </Table.Summary.Row>
              ) : null} />
          </Col>
        </Row>
      </div>
      <ExpenseModal open={modal.open} expenseId={modal.id} onClose={() => setModal({ open: false })} onSaved={() => { reload(); reloadDetail() }} />
    </div>
  )
}
