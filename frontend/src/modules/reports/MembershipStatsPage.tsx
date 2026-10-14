import { useState } from 'react'
import { Input, Select, Table, Tag, type TableColumnsType } from 'antd'
import { Link } from 'react-router-dom'
import { PageTitle, useLoad } from '../../components/common'
import { fmtDate, money } from '../../utils/format'
import ReportFilterBar, { useReportParams } from './components/ReportFilterBar'
import StatBar from './components/StatBar'
import { DISCOUNT_LABELS, reportsApi, type DiscountKind, type MembershipRow } from './api'

const DISCOUNT_COLORS: Record<DiscountKind, string> = { normal: 'default', free: 'green', discount: 'gold' }

/** Thống kê phiếu khám theo loại giảm giá / mã VIP (MembershipStatisticsControl). */
export default function MembershipStatsPage() {
  const { range, setRange, params } = useReportParams()
  const [discount, setDiscount] = useState<DiscountKind | 'all'>('all')
  const [memberCode, setMemberCode] = useState<string>()
  const full = { ...params, discount, memberCode: memberCode?.trim() || undefined }
  const { data, loading } = useLoad(() => reportsApi.membership(full), [JSON.stringify(full)])

  const columns: TableColumnsType<MembershipRow> = [
    { title: 'No', key: 'no', width: 50, render: (_, __, i) => i + 1 },
    { title: 'Chi nhánh', dataIndex: 'branchName', width: 160 },
    { title: 'Mã KH', dataIndex: 'visiableId', width: 80, align: 'right' },
    { title: 'Khách hàng', dataIndex: 'customerName', render: (v, r) => <Link to={`/customers/${r.customerId}`}>{v}</Link> },
    { title: 'Phiếu khám', dataIndex: 'id', width: 90, render: (v) => <Link to={`/prescriptions/${v}`}>{v}</Link> },
    { title: 'Ngày tạo', dataIndex: 'createdDate', width: 100, render: fmtDate },
    { title: 'Loại', dataIndex: 'discount', width: 110, render: (v: DiscountKind) => <Tag color={DISCOUNT_COLORS[v]}>{DISCOUNT_LABELS[v]}</Tag> },
    { title: 'Mã VIP', dataIndex: 'memberCode', width: 110 },
    { title: 'Tổng nếu không giảm', dataIndex: 'totalIfNotDiscount', width: 140, align: 'right', render: money },
    { title: 'Tổng tiền', dataIndex: 'total', width: 120, align: 'right', render: money },
    { title: 'Đã trả', dataIndex: 'paid', width: 120, align: 'right', render: money },
    { title: 'Còn nợ', dataIndex: 'remain', width: 120, align: 'right', render: money },
  ]

  return (
    <div className="page-card">
      <PageTitle title="Thống kê phiếu khám" />
      <ReportFilterBar range={range} onChange={setRange} exportUrl="/reports/membership/export" exportFileName="thong-ke-phieu-kham.xlsx" exportParams={full}>
        <Select
          size="small"
          style={{ width: 140 }}
          value={discount}
          onChange={setDiscount}
          options={(Object.keys(DISCOUNT_LABELS) as (DiscountKind | 'all')[]).map((k) => ({ value: k, label: DISCOUNT_LABELS[k] }))}
        />
        <Input.Search size="small" style={{ width: 180 }} placeholder="Mã VIP" allowClear onSearch={(v) => setMemberCode(v)} />
      </ReportFilterBar>
      <StatBar
        items={[
          { label: 'Tổng nếu không giảm', value: data?.totalIfNotDiscount ?? 0 },
          { label: 'Tổng tiền', value: data?.total ?? 0 },
          { label: 'Đã trả', value: data?.paid ?? 0, color: '#389e0d' },
          { label: 'Còn nợ', value: data?.remain ?? 0, color: '#cf1322' },
        ]}
      />
      <Table size="small" rowKey="id" loading={loading} columns={columns} dataSource={data?.items ?? []} pagination={{ pageSize: 50, showSizeChanger: true }} scroll={{ x: 'max-content' }} />
    </div>
  )
}
