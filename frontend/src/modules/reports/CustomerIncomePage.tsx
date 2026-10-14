import { useEffect, useState } from 'react'
import { Checkbox, Select, Space, Table, type TableColumnsType } from 'antd'
import { Link } from 'react-router-dom'
import { PageTitle, useLoad } from '../../components/common'
import { fmtDate, money } from '../../utils/format'
import ReportFilterBar, { useReportParams } from './components/ReportFilterBar'
import StatBar from './components/StatBar'
import { reportsApi, type CustomerIncomeRow } from './api'

/** Doanh thu theo khách hàng (CustomerIncomeControl) with optional Tỉnh / Quận / Phường filters. */
export default function CustomerIncomePage() {
  const { range, setRange, params } = useReportParams()
  const [useProvince, setUseProvince] = useState(false)
  const [province, setProvince] = useState<string>()
  const [district, setDistrict] = useState<string>()
  const [ward, setWard] = useState<string>()
  const [provinces, setProvinces] = useState<string[]>([])
  const [districts, setDistricts] = useState<string[]>([])
  const [wards, setWards] = useState<string[]>([])

  useEffect(() => {
    if (useProvince && provinces.length === 0) reportsApi.provinces().then(setProvinces)
  }, [useProvince, provinces.length])
  useEffect(() => {
    setDistrict(undefined)
    setDistricts([])
    if (province) reportsApi.districts(province).then(setDistricts)
  }, [province])
  useEffect(() => {
    setWard(undefined)
    setWards([])
    if (province && district) reportsApi.wards(province, district).then(setWards)
  }, [province, district])

  const full = { ...params, province: useProvince ? province : undefined, district: useProvince ? district : undefined, ward: useProvince ? ward : undefined }
  const { data, loading } = useLoad(() => reportsApi.customerIncome(full), [JSON.stringify(full)])

  const columns: TableColumnsType<CustomerIncomeRow> = [
    { title: 'No', key: 'no', width: 50, render: (_, __, i) => i + 1 },
    { title: 'Chi nhánh', dataIndex: 'branchName', width: 160 },
    { title: 'Mã KH', dataIndex: 'customerVisiableId', width: 80, align: 'right' },
    { title: 'Khách hàng', dataIndex: 'customerName', render: (v, r) => <Link to={`/customers/${r.customerId}`}>{v}</Link>, sorter: (a, b) => a.customerName.localeCompare(b.customerName) },
    { title: 'Ngày tạo', dataIndex: 'createdDate', width: 100, render: fmtDate },
    { title: 'Số phiếu', dataIndex: 'prescriptionNo', width: 80, align: 'right' },
    { title: 'Tổng tiền', dataIndex: 'total', width: 120, align: 'right', render: money, sorter: (a, b) => a.total - b.total },
    { title: 'Đã trả', dataIndex: 'paid', width: 120, align: 'right', render: money },
    { title: 'Còn nợ', dataIndex: 'remain', width: 120, align: 'right', render: money, sorter: (a, b) => a.remain - b.remain },
    { title: 'Tỉnh/TP', dataIndex: 'province', width: 140 },
    { title: 'Quận/Huyện', dataIndex: 'district', width: 140 },
    { title: 'Phường/Xã', dataIndex: 'ward', width: 140 },
  ]

  return (
    <div className="page-card">
      <PageTitle title="Doanh thu theo khách hàng" />
      <ReportFilterBar range={range} onChange={setRange} exportUrl="/reports/customer-income/export" exportFileName="doanh-thu-khach-hang.xlsx" exportParams={full}>
        <Space size={4}>
          <Checkbox checked={useProvince} onChange={(e) => setUseProvince(e.target.checked)}>
            Tỉnh/TP
          </Checkbox>
          <Select size="small" style={{ width: 160 }} allowClear disabled={!useProvince} value={province} onChange={setProvince} options={provinces.map((p) => ({ value: p, label: p }))} placeholder="Tỉnh/TP" showSearch />
          <Select size="small" style={{ width: 150 }} allowClear disabled={!province} value={district} onChange={setDistrict} options={districts.map((p) => ({ value: p, label: p }))} placeholder="Quận/Huyện" showSearch />
          <Select size="small" style={{ width: 150 }} allowClear disabled={!district} value={ward} onChange={setWard} options={wards.map((p) => ({ value: p, label: p }))} placeholder="Phường/Xã" showSearch />
        </Space>
      </ReportFilterBar>
      <StatBar items={[{ label: 'Tổng tiền', value: data?.total ?? 0 }, { label: 'Đã trả', value: data?.paid ?? 0, color: '#389e0d' }, { label: 'Còn nợ', value: data?.remain ?? 0, color: '#cf1322' }]} />
      <Table size="small" rowKey="customerId" loading={loading} columns={columns} dataSource={data?.items ?? []} pagination={{ pageSize: 50, showSizeChanger: true }} scroll={{ x: 'max-content' }} />
    </div>
  )
}
