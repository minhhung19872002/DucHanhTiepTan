import { useEffect, useState } from 'react'
import { Alert, Button, DatePicker, Input, InputNumber, Popconfirm, Space, Table, Tag, message, type TableColumnsType } from 'antd'
import { DeleteOutlined, FormOutlined, SaveOutlined } from '@ant-design/icons'
import dayjs, { type Dayjs } from 'dayjs'
import { BranchSelect, ExcelButton, PageTitle } from '../../components/common'
import { useAuth } from '../../auth/AuthContext'
import { errorMessage } from '../../api/client'
import { money } from '../../utils/format'
import StatBar from './components/StatBar'
import { computeSalaryLine, salaryApi, toSalaryLineInput, type SalaryLine, type SalaryLineInput, type SalarySheet } from './api'

type NumKey =
  | 'workDays' | 'overtimeInMinute' | 'towHoursOvertimeDays' | 'degreeAllowance' | 'heathAllowance' | 'dayOffAllowance' | 'riskAllowance'
  | 'retireAllowance' | 'absent' | 'fee' | 'advance' | 'baseLunchSubsidy' | 'basePhoneSubsidy' | 'baseToxicSubsidy'

/**
 * Bảng lương nhân viên (SalaryControl + DeclareSalaryDialog).
 * Chưa khai báo → "Khai báo" nạp bảng mặc định từ nhân viên của chi nhánh; đã khai báo → sửa / xóa / xuất Excel.
 */
export default function SalaryPage() {
  const { branchId } = useAuth()
  const [month, setMonth] = useState<Dayjs>(() => dayjs().subtract(1, 'month'))
  const [sheet, setSheet] = useState<SalarySheet>()
  const [lines, setLines] = useState<SalaryLine[]>([])
  const [baseSalary, setBaseSalary] = useState(0)
  const [workDays, setWorkDays] = useState(26)
  const [editing, setEditing] = useState(false) // true while declaring a new month
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [dirty, setDirty] = useState(false)

  const year = month.year()
  const mon = month.month() + 1

  const apply = (s: SalarySheet, isDeclaring: boolean) => {
    setSheet(s)
    setLines(s.items)
    setBaseSalary(s.monthYear.baseSalary)
    setWorkDays(s.monthYear.workDaysInMonth)
    setEditing(isDeclaring)
    setDirty(false)
  }

  const load = async () => {
    setLoading(true)
    try {
      apply(await salaryApi.get(year, mon, branchId), false)
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [year, mon, branchId])

  const startDeclare = async () => {
    setLoading(true)
    try {
      apply(await salaryApi.declaration(year, mon, branchId), true)
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setLoading(false)
    }
  }

  const recomputeAll = (bs: number, wd: number, src = lines) => setLines(src.map((l) => computeSalaryLine(l, bs, wd)))

  const setField = <K extends keyof SalaryLineInput>(userName: string, key: K, value: SalaryLineInput[K]) => {
    setLines((prev) => prev.map((l) => (l.userName === userName ? computeSalaryLine({ ...l, [key]: value }, baseSalary, workDays) : l)))
    setDirty(true)
  }

  const save = async () => {
    const input = { year, month: mon, branchId, baseSalary, workDaysInMonth: workDays, items: lines.map(toSalaryLineInput) }
    setSaving(true)
    try {
      const s = editing ? await salaryApi.declare(input) : await salaryApi.update(input)
      apply(s, false)
      message.success(editing ? 'Đã khai báo lương' : 'Đã lưu bảng lương')
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setSaving(false)
    }
  }

  const remove = async () => {
    try {
      await salaryApi.remove(year, mon, branchId)
      message.success('Đã xóa bảng lương tháng')
      load()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  const canEdit = branchId !== 0 && (editing || !!sheet?.declared)
  const num = (key: NumKey, title: string, width = 100) => ({
    title,
    key,
    width,
    align: 'right' as const,
    render: (_: unknown, r: SalaryLine) =>
      canEdit ? (
        <InputNumber
          size="small"
          style={{ width: width - 12 }}
          min={0}
          value={r[key] as number}
          formatter={(v) => (v === undefined || v === null ? '' : new Intl.NumberFormat('vi-VN').format(Number(v)))}
          parser={(v) => Number((v ?? '').replace(/\./g, '').replace(/,/g, '')) || 0}
          onChange={(v) => setField(r.userName, key, (v ?? 0) as never)}
        />
      ) : (
        money(r[key] as number)
      ),
  })
  const computed = (key: keyof SalaryLine, title: string, width = 120, bold = false) => ({
    title,
    key,
    width,
    align: 'right' as const,
    render: (_: unknown, r: SalaryLine) => (bold ? <b>{money(Math.round(r[key] as number))}</b> : money(Math.round(r[key] as number))),
  })

  const columns: TableColumnsType<SalaryLine> = [
    { title: 'No', key: 'no', width: 45, fixed: 'left', render: (_, __, i) => i + 1 },
    { title: 'Nhân viên', dataIndex: 'fullName', width: 170, fixed: 'left' },
    { title: 'Chức vụ', dataIndex: 'employeeType', width: 90 },
    ...(branchId === 0 ? [{ title: 'Chi nhánh', dataIndex: 'branchName', width: 150 }] : []),
    {
      title: 'Cấp bậc',
      key: 'rank',
      width: 90,
      render: (_, r) => (canEdit ? <Input size="small" value={r.rank ?? ''} onChange={(e) => setField(r.userName, 'rank', e.target.value)} /> : r.rank),
    },
    {
      title: 'Hệ số',
      key: 'ratio',
      width: 80,
      align: 'right',
      render: (_, r) =>
        canEdit ? (
          <InputNumber size="small" style={{ width: 68 }} min={0} step={0.1} value={r.ratio ?? 0} onChange={(v) => setField(r.userName, 'ratio', v ?? 0)} />
        ) : (
          r.ratio
        ),
    },
    num('workDays', 'Ngày công', 85),
    num('overtimeInMinute', 'Tăng ca (phút)', 100),
    num('towHoursOvertimeDays', 'Ngày tăng ca 2h', 105),
    computed('baseSalary', 'Lương cơ bản'),
    computed('baseSalaryDependOnWorkDays', 'Lương theo ngày công', 140),
    computed('overtimeBonus', 'Tiền tăng ca'),
    computed('overtimeRatio', 'Tăng ca 2h'),
    num('baseLunchSubsidy', 'PC ăn trưa (cơ bản)', 130),
    computed('lunchSubsidy', 'PC ăn trưa'),
    num('basePhoneSubsidy', 'PC điện thoại (cơ bản)', 140),
    computed('phoneSubsidy', 'PC điện thoại'),
    num('baseToxicSubsidy', 'PC độc hại (cơ bản)', 130),
    computed('toxicSubsidy', 'PC độc hại'),
    num('degreeAllowance', 'PC bằng cấp', 110),
    num('heathAllowance', 'PC sức khỏe', 110),
    num('dayOffAllowance', 'PC ngày nghỉ', 110),
    num('riskAllowance', 'PC rủi ro', 110),
    num('retireAllowance', 'PC hưu trí', 110),
    num('absent', 'Trừ vắng', 110),
    num('fee', 'Trừ phí', 110),
    computed('income', 'Thu nhập', 130, true),
    num('advance', 'Tạm ứng', 110),
    { ...computed('total', 'Thực lãnh', 130, true), fixed: 'right' as const },
  ]

  const totals = lines.reduce((a, l) => ({ income: a.income + l.income, advance: a.advance + l.advance, total: a.total + l.total }), { income: 0, advance: 0, total: 0 })

  return (
    <div className="page-card">
      <PageTitle title="Bảng lương nhân viên" />
      <div className="toolbar no-print">
        <span>Tháng</span>
        <DatePicker picker="month" format="MM/YYYY" value={month} allowClear={false} onChange={(d) => d && setMonth(d)} />
        <BranchSelect />
        {sheet && (sheet.declared ? <Tag color="green">Đã khai báo</Tag> : editing ? <Tag color="orange">Đang khai báo</Tag> : <Tag>Chưa khai báo</Tag>)}
        <Space style={{ marginLeft: 'auto' }}>
          {!sheet?.declared && !editing && branchId !== 0 && (
            <Button type="primary" icon={<FormOutlined />} onClick={startDeclare} loading={loading}>
              Khai báo lương
            </Button>
          )}
          {editing && <Button onClick={load}>Hủy</Button>}
          {canEdit && (
            <Button type="primary" icon={<SaveOutlined />} onClick={save} loading={saving} disabled={!editing && !dirty}>
              {editing ? 'Khai báo' : 'Lưu'}
            </Button>
          )}
          {sheet?.declared && branchId !== 0 && (
            <Popconfirm title={`Xóa bảng lương tháng ${mon}/${year}?`} okText="Xóa" cancelText="Hủy" onConfirm={remove}>
              <Button danger icon={<DeleteOutlined />}>
                Xóa
              </Button>
            </Popconfirm>
          )}
          {sheet?.declared && <ExcelButton url="/salary/export" fileName={`bang-luong-${mon}-${year}.xlsx`} params={{ year, month: mon, branchId }} />}
        </Space>
      </div>
      {branchId === 0 && <Alert type="info" showIcon style={{ marginBottom: 12 }} message="Đang xem tất cả chi nhánh (chỉ xem). Chọn một chi nhánh để khai báo hoặc sửa bảng lương." />}
      {canEdit && (
        <Space style={{ marginBottom: 12 }} wrap>
          <span>Lương cơ bản</span>
          <InputNumber
            style={{ width: 160 }}
            min={0}
            value={baseSalary}
            formatter={(v) => new Intl.NumberFormat('vi-VN').format(Number(v ?? 0))}
            parser={(v) => Number((v ?? '').replace(/\./g, '').replace(/,/g, '')) || 0}
            onChange={(v) => {
              const bs = v ?? 0
              setBaseSalary(bs)
              recomputeAll(bs, workDays)
              setDirty(true)
            }}
          />
          <span>Số ngày công trong tháng</span>
          <InputNumber
            style={{ width: 90 }}
            min={1}
            max={31}
            value={workDays}
            onChange={(v) => {
              const wd = v ?? 0
              setWorkDays(wd)
              recomputeAll(baseSalary, wd)
              setDirty(true)
            }}
          />
        </Space>
      )}
      <StatBar
        items={[
          { label: 'Tổng thu nhập', value: Math.round(totals.income) },
          { label: 'Tổng tạm ứng', value: totals.advance, color: '#d46b08' },
          { label: 'Tổng thực lãnh', value: Math.round(totals.total), color: '#1f6fb2' },
        ]}
      />
      <Table<SalaryLine>
        size="small"
        rowKey="userName"
        loading={loading}
        columns={columns}
        dataSource={lines}
        pagination={false}
        scroll={{ x: 'max-content', y: 'calc(100vh - 380px)' }}
        locale={{ emptyText: sheet?.declared || editing ? 'Không có nhân viên' : 'Tháng này chưa được khai báo lương' }}
      />
    </div>
  )
}
