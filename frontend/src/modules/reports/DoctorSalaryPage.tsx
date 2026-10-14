import { useEffect, useState } from 'react'
import { Alert, Button, Card, Col, DatePicker, InputNumber, Popconfirm, Row, Space, Statistic, Table, Tag, message, type TableColumnsType } from 'antd'
import { DeleteOutlined, FormOutlined, SaveOutlined } from '@ant-design/icons'
import dayjs, { type Dayjs } from 'dayjs'
import { BranchSelect, ExcelButton, PageTitle } from '../../components/common'
import { useAuth } from '../../auth/AuthContext'
import { errorMessage } from '../../api/client'
import { money } from '../../utils/format'
import StatBar from './components/StatBar'
import { computeDoctorLine, doctorSalaryApi, type DoctorLine, type DoctorLineInput, type DoctorSheet } from './api'

type NumKey = 'baseSalary' | 'lunchSubsidy' | 'respondSubsidy' | 'advance'

/** Lương bác sĩ (DoctorSalaryControl + DeclareDoctorSalary) with the profit dashboard. */
export default function DoctorSalaryPage() {
  const { branchId } = useAuth()
  const [month, setMonth] = useState<Dayjs>(() => dayjs().subtract(1, 'month'))
  const [sheet, setSheet] = useState<DoctorSheet>()
  const [lines, setLines] = useState<DoctorLine[]>([])
  const [editing, setEditing] = useState(false)
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [dirty, setDirty] = useState(false)
  const year = month.year()
  const mon = month.month() + 1

  const apply = (s: DoctorSheet, isDeclaring: boolean) => {
    setSheet(s)
    setLines(s.items)
    setEditing(isDeclaring)
    setDirty(false)
  }

  const run = async (fn: () => Promise<DoctorSheet>, declaring: boolean) => {
    setLoading(true)
    try {
      apply(await fn(), declaring)
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setLoading(false)
    }
  }
  const load = () => run(() => doctorSalaryApi.get(year, mon, branchId), false)

  useEffect(() => {
    load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [year, mon, branchId])

  const setField = (userName: string, key: NumKey, value: number) => {
    setLines((prev) => prev.map((l) => (l.userName === userName ? computeDoctorLine({ ...l, [key]: value }) : l)))
    setDirty(true)
  }

  const save = async () => {
    const items: DoctorLineInput[] = lines.map((l) => ({ userName: l.userName, baseSalary: l.baseSalary, lunchSubsidy: l.lunchSubsidy, respondSubsidy: l.respondSubsidy, advance: l.advance }))
    const input = { year, month: mon, branchId, items }
    setSaving(true)
    try {
      apply(editing ? await doctorSalaryApi.declare(input) : await doctorSalaryApi.update(input), false)
      message.success(editing ? 'Đã khai báo lương bác sĩ' : 'Đã lưu')
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setSaving(false)
    }
  }

  const remove = async () => {
    try {
      await doctorSalaryApi.remove(year, mon, branchId)
      message.success('Đã xóa')
      load()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  const canEdit = branchId !== 0 && (editing || !!sheet?.declared)
  const num = (key: NumKey, title: string) => ({
    title,
    key,
    width: 140,
    align: 'right' as const,
    render: (_: unknown, r: DoctorLine) =>
      canEdit ? (
        <InputNumber
          size="small"
          style={{ width: 128 }}
          min={0}
          value={r[key]}
          formatter={(v) => new Intl.NumberFormat('vi-VN').format(Number(v ?? 0))}
          parser={(v) => Number((v ?? '').replace(/\./g, '').replace(/,/g, '')) || 0}
          onChange={(v) => setField(r.userName, key, v ?? 0)}
        />
      ) : (
        money(r[key])
      ),
  })

  const columns: TableColumnsType<DoctorLine> = [
    { title: 'No', key: 'no', width: 45, render: (_, __, i) => i + 1 },
    { title: 'Bác sĩ', dataIndex: 'fullName', width: 180 },
    { title: 'Mã', dataIndex: 'userName', width: 90 },
    { title: 'Thu nhập theo doanh thu', dataIndex: 'income', width: 160, align: 'right', render: money },
    num('baseSalary', 'Lương cơ bản'),
    num('lunchSubsidy', 'PC ăn trưa'),
    num('respondSubsidy', 'PC trách nhiệm'),
    { title: 'Tổng lương', dataIndex: 'salary', width: 130, align: 'right', render: (v) => <b>{money(v)}</b> },
    num('advance', 'Tạm ứng'),
    { title: 'Thực lãnh', dataIndex: 'total', width: 130, align: 'right', render: (v) => <b>{money(v)}</b> },
  ]

  const d = sheet?.dashboard
  const totals = lines.reduce((a, l) => ({ salary: a.salary + l.salary, advance: a.advance + l.advance, total: a.total + l.total }), { salary: 0, advance: 0, total: 0 })

  return (
    <div className="page-card">
      <PageTitle title="Lương bác sĩ" />
      <div className="toolbar no-print">
        <span>Tháng</span>
        <DatePicker picker="month" format="MM/YYYY" value={month} allowClear={false} onChange={(v) => v && setMonth(v)} />
        <BranchSelect />
        {sheet && (sheet.declared ? <Tag color="green">Đã khai báo</Tag> : editing ? <Tag color="orange">Đang khai báo</Tag> : <Tag>Chưa khai báo</Tag>)}
        <Space style={{ marginLeft: 'auto' }}>
          {!sheet?.declared && !editing && branchId !== 0 && (
            <Button type="primary" icon={<FormOutlined />} loading={loading} onClick={() => run(() => doctorSalaryApi.declaration(year, mon, branchId), true)}>
              Khai báo lương
            </Button>
          )}
          {editing && <Button onClick={load}>Hủy</Button>}
          {canEdit && (
            <Button type="primary" icon={<SaveOutlined />} onClick={save} loading={saving} disabled={!editing && !dirty}>
              {editing ? 'Khai báo' : 'Lưu'}
            </Button>
          )}
          {sheet?.declared && (
            <Popconfirm title={`Xóa lương bác sĩ tháng ${mon}/${year}?`} okText="Xóa" cancelText="Hủy" onConfirm={remove}>
              <Button danger icon={<DeleteOutlined />}>
                Xóa
              </Button>
            </Popconfirm>
          )}
          {sheet?.declared && <ExcelButton url="/doctor-salary/export" fileName={`luong-bac-si-${mon}-${year}.xlsx`} params={{ year, month: mon, branchId }} />}
        </Space>
      </div>
      {branchId === 0 ? (
        <Alert type="info" showIcon message="Chọn một chi nhánh để xem / khai báo lương bác sĩ." />
      ) : (
        <>
          {d && (
            <Card size="small" title={`Tổng hợp tháng ${mon}/${year}`} style={{ marginBottom: 12 }}>
              <Row gutter={[16, 8]}>
                <Col xs={12} md={6}><Statistic title="Doanh thu" value={money(d.income)} /></Col>
                <Col xs={12} md={6}><Statistic title="Chi phí" value={money(d.expense)} /></Col>
                <Col xs={12} md={6}><Statistic title="Thiết bị / vật tư" value={money(d.equipment)} /></Col>
                <Col xs={12} md={6}><Statistic title="Labo cố định" value={money(d.fixedLabo)} /></Col>
                <Col xs={12} md={6}><Statistic title="Labo tháo lắp" value={money(d.dynamicLabo)} /></Col>
                <Col xs={12} md={6}><Statistic title="Tổng chi phí" value={money(d.totalExpense)} valueStyle={{ color: '#cf1322' }} /></Col>
                <Col xs={12} md={6}><Statistic title="Lợi nhuận" value={money(d.profit)} valueStyle={{ color: '#389e0d' }} /></Col>
                <Col xs={12} md={6}>
                  <Statistic title={`Lợi nhuận / bác sĩ (${d.numberOfDoctors} BS, ${Math.round(d.percentProfit * 100)}%)`} value={money(Math.round(d.rankProfit))} />
                </Col>
              </Row>
            </Card>
          )}
          <StatBar
            items={[
              { label: 'Tổng lương', value: totals.salary },
              { label: 'Tổng tạm ứng', value: totals.advance, color: '#d46b08' },
              { label: 'Tổng thực lãnh', value: totals.total, color: '#1f6fb2' },
            ]}
          />
          <Table<DoctorLine>
            size="small"
            rowKey="userName"
            loading={loading}
            columns={columns}
            dataSource={lines}
            pagination={false}
            scroll={{ x: 'max-content' }}
            locale={{ emptyText: sheet?.declared || editing ? 'Không có bác sĩ' : 'Tháng này chưa được khai báo lương' }}
          />
        </>
      )}
    </div>
  )
}
