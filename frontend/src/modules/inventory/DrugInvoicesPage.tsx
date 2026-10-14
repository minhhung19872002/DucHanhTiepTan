import { useCallback, useEffect, useState } from 'react'
import { Button, Card, Col, Row, Space, Table, Typography, message } from 'antd'
import { PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import { errorMessage } from '../../api/client'
import { useAuth } from '../../auth/AuthContext'
import { BranchSelect, DeleteButton, ExcelButton, PageTitle, useLoad } from '../../components/common'
import { fmtDate, money } from '../../utils/format'
import { drugInvoicesApi, type DrugInvoice, type DrugInvoiceLine } from './api'
import DateRangeFilter, { toDateParams, useRangeState } from './DateRangeFilter'
import ImportInvoiceModal from './ImportInvoiceModal'

/** Hóa đơn nhập thuốc (legacy InvoiceManagerControl + DeclareInvoiceDialog). */
export default function DrugInvoicesPage() {
  const { user, branchId } = useAuth()
  const [range, setRange] = useRangeState()
  const params = { branchId, limit: range.limit, ...toDateParams(range) }
  const { data, loading, reload } = useLoad(() => drugInvoicesApi.list(params), [branchId, range])
  const [selected, setSelected] = useState<DrugInvoice | null>(null)
  const [lines, setLines] = useState<DrugInvoiceLine[]>([])
  const [open, setOpen] = useState(false)

  useEffect(() => {
    if (!selected) return setLines([])
    drugInvoicesApi.get(selected.id).then((d) => setLines(d.lines)).catch((e) => message.error(errorMessage(e)))
  }, [selected])

  const loadDrugs = useCallback(() => drugInvoicesApi.lookupDrugs().then((ds) => ds.map((d) => ({ id: d.id, name: d.name, unit: d.unit, price: d.price }))), [])

  return (
    <div className="page-card">
      <PageTitle
        title="Hóa đơn nhập thuốc"
        extra={
          <Space>
            <Button type="primary" icon={<PlusOutlined />} onClick={() => setOpen(true)}>
              Tạo hóa đơn nhập
            </Button>
            <ExcelButton url="/drug-invoices/export" fileName="hoa-don-nhap-thuoc.xlsx" params={params} />
            <Button icon={<ReloadOutlined />} onClick={reload} />
          </Space>
        }
      />
      <div className="toolbar">
        {user?.isAdministrator && <BranchSelect />}
        <DateRangeFilter value={range} onChange={setRange} />
      </div>
      <Row gutter={12}>
        <Col span={14}>
          <Card size="small" title={<span>Hóa đơn &nbsp; <Typography.Text type="secondary">({data?.count ?? 0} hóa đơn, tổng: <b>{money(data?.total)}</b>)</Typography.Text></span>}>
            <Table<DrugInvoice>
              size="small"
              rowKey="id"
              loading={loading}
              dataSource={data?.items ?? []}
              pagination={{ pageSize: 20, showSizeChanger: false }}
              onRow={(r) => ({ onClick: () => setSelected(r), style: { cursor: 'pointer' } })}
              rowClassName={(r) => (r.id === selected?.id ? 'ant-table-row-selected' : '')}
              columns={[
                { title: '#', width: 50, render: (_, __, i) => i + 1 },
                { title: 'Số HĐ', dataIndex: 'id', width: 80 },
                { title: 'Ngày', dataIndex: 'createdDate', width: 110, render: fmtDate },
                { title: 'Chi nhánh', dataIndex: 'branchName' },
                { title: 'Người lập', render: (_, r) => r.fullName || r.userName },
                { title: 'Số MH', dataIndex: 'quantity', width: 70, align: 'right' },
                { title: 'Tổng tiền', dataIndex: 'total', width: 120, align: 'right', render: money },
                ...(user?.isAdministrator
                  ? [{ title: '', width: 50, render: (_: unknown, r: DrugInvoice) => (
                      <DeleteButton title="Xóa hóa đơn và trừ lại tồn kho?" onConfirm={async () => { try { await drugInvoicesApi.remove(r.id); message.success('Đã xóa'); setSelected(null); reload() } catch (e) { message.error(errorMessage(e)) } }} />
                    ) }]
                  : []),
              ]}
            />
          </Card>
        </Col>
        <Col span={10}>
          <Card size="small" title={selected ? `Chi tiết hóa đơn #${selected.id} – ${fmtDate(selected.createdDate)}` : 'Chi tiết hóa đơn'}>
            <Table<DrugInvoiceLine>
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
              summary={() => selected ? (
                <Table.Summary.Row>
                  <Table.Summary.Cell index={0} colSpan={3}><b>Tổng</b></Table.Summary.Cell>
                  <Table.Summary.Cell index={1} align="right"><b>{lines.reduce((s, l) => s + l.quantity, 0)}</b></Table.Summary.Cell>
                  <Table.Summary.Cell index={2} align="right"><b>{money(lines.reduce((s, l) => s + l.total, 0))}</b></Table.Summary.Cell>
                </Table.Summary.Row>
              ) : null}
            />
          </Card>
        </Col>
      </Row>
      <ImportInvoiceModal
        open={open}
        title="Khai báo nhập thuốc"
        itemLabel="Thuốc"
        loadItems={loadDrugs}
        onSubmit={async (body) => {
          await drugInvoicesApi.create(body)
          message.success('Đã tạo hóa đơn nhập thuốc')
          reload()
        }}
        onClose={() => setOpen(false)}
      />
    </div>
  )
}
