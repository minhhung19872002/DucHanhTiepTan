import { useEffect, useState } from 'react'
import { Button, Card, Col, Input, Row, Space, Table, Typography, message } from 'antd'
import { ReloadOutlined } from '@ant-design/icons'
import { Link } from 'react-router-dom'
import { errorMessage } from '../../api/client'
import { useAuth } from '../../auth/AuthContext'
import { BranchSelect, ExcelButton, PageTitle, useLoad } from '../../components/common'
import { fmtDate, money } from '../../utils/format'
import { drugReceiptsApi, type DrugReceipt, type DrugReceiptLine } from './api'
import DateRangeFilter, { toDateParams, useRangeState } from './DateRangeFilter'

/** Các hóa đơn xuất thuốc: thuốc bán cho khách trong lần khám (UsedReceipt). */
export default function DrugReceiptsPage() {
  const { user, branchId } = useAuth()
  const [range, setRange] = useRangeState()
  const [keyword, setKeyword] = useState('')
  const params = { branchId, limit: range.limit, keyword: keyword || undefined, ...toDateParams(range) }
  const { data, loading, reload } = useLoad(() => drugReceiptsApi.list(params), [branchId, range, keyword])
  const [selected, setSelected] = useState<DrugReceipt | null>(null)
  const [lines, setLines] = useState<DrugReceiptLine[]>([])

  useEffect(() => {
    if (!selected) return setLines([])
    drugReceiptsApi.get(selected.prescriptionId, selected.appointmentNo).then((d) => setLines(d.lines)).catch((e) => message.error(errorMessage(e)))
  }, [selected])

  const key = (r: DrugReceipt) => `${r.prescriptionId}-${r.appointmentNo}`

  return (
    <div className="page-card">
      <PageTitle
        title="Hóa đơn xuất thuốc"
        extra={
          <Space>
            <ExcelButton url="/drug-receipts/export" fileName="hoa-don-xuat-thuoc.xlsx" params={params} />
            <Button icon={<ReloadOutlined />} onClick={reload} />
          </Space>
        }
      />
      <div className="toolbar">
        {user?.isAdministrator && <BranchSelect />}
        <Input.Search placeholder="Tên / điện thoại khách hàng" allowClear style={{ width: 260 }} onSearch={setKeyword} />
        <DateRangeFilter value={range} onChange={setRange} />
      </div>
      <Row gutter={12}>
        <Col span={14}>
          <Card size="small" title={<span>Hóa đơn &nbsp; <Typography.Text type="secondary">({data?.count ?? 0} hóa đơn, tổng: <b>{money(data?.total)}</b>)</Typography.Text></span>}>
            <Table<DrugReceipt>
              size="small"
              rowKey={key}
              loading={loading}
              dataSource={data?.items ?? []}
              pagination={{ pageSize: 20, showSizeChanger: false }}
              onRow={(r) => ({ onClick: () => setSelected(r), style: { cursor: 'pointer' } })}
              rowClassName={(r) => (selected && key(r) === key(selected) ? 'ant-table-row-selected' : '')}
              columns={[
                { title: '#', width: 50, render: (_, __, i) => i + 1 },
                { title: 'Ngày', dataIndex: 'createdDate', width: 110, render: fmtDate },
                { title: 'Mã KH', dataIndex: 'customerVisiableId', width: 80 },
                { title: 'Khách hàng', dataIndex: 'customerName', render: (v, r) => <Link to={`/prescriptions/${r.prescriptionId}`}>{v}</Link> },
                { title: 'Lần khám', dataIndex: 'appointmentNo', width: 80, align: 'center' },
                { title: 'Chi nhánh', dataIndex: 'branchName' },
                { title: 'Số MH', dataIndex: 'quantity', width: 70, align: 'right' },
                { title: 'Tổng tiền', dataIndex: 'total', width: 120, align: 'right', render: money },
              ]}
            />
          </Card>
        </Col>
        <Col span={10}>
          <Card size="small" title={selected ? `Chi tiết – ${selected.customerName} (${fmtDate(selected.createdDate)})` : 'Chi tiết hóa đơn'}>
            <Table<DrugReceiptLine>
              size="small"
              rowKey="drugId"
              dataSource={lines}
              pagination={false}
              columns={[
                { title: 'Thuốc', dataIndex: 'drugName' },
                { title: 'Đơn vị', dataIndex: 'unit', width: 80 },
                { title: 'Giá', dataIndex: 'price', width: 100, align: 'right', render: money },
                { title: 'SL', dataIndex: 'quantity', width: 60, align: 'right' },
                { title: 'Thành tiền', dataIndex: 'total', width: 110, align: 'right', render: money },
              ]}
            />
          </Card>
        </Col>
      </Row>
    </div>
  )
}
