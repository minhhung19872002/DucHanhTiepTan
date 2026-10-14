import { useCallback, useEffect, useState } from 'react'
import { Button, Card, Col, Radio, Row, Space, Table, Typography, message } from 'antd'
import { PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import { errorMessage } from '../../api/client'
import { useAuth } from '../../auth/AuthContext'
import { BranchSelect, DeleteButton, ExcelButton, PageTitle, useLoad } from '../../components/common'
import { fmtDate, money } from '../../utils/format'
import { equipmentInvoicesApi, type EquipmentInvoice, type EquipmentInvoiceLine } from './api'
import DateRangeFilter, { toDateParams, useRangeState } from './DateRangeFilter'
import ImportInvoiceModal from './ImportInvoiceModal'

/** Hóa đơn nhập tài sản / vật tư (legacy EquipmentInvoiceManagerControl + DeclareEquipmentInvoiceDialog). */
export default function EquipmentInvoicesPage() {
  const { user, branchId } = useAuth()
  const [range, setRange] = useRangeState()
  const [kind, setKind] = useState<'all' | 'fixed' | 'various'>('all')
  const isVarious = kind === 'all' ? undefined : kind === 'various'
  const params = { branchId, limit: range.limit, isVarious, ...toDateParams(range) }
  const { data, loading, reload } = useLoad(() => equipmentInvoicesApi.list(params), [branchId, range, kind])
  const [selected, setSelected] = useState<EquipmentInvoice | null>(null)
  const [lines, setLines] = useState<EquipmentInvoiceLine[]>([])
  const [createKind, setCreateKind] = useState<'fixed' | 'various' | null>(null)

  useEffect(() => {
    if (!selected) return setLines([])
    equipmentInvoicesApi.get(selected.id).then((d) => setLines(d.lines)).catch((e) => message.error(errorMessage(e)))
  }, [selected])

  const loadItems = useCallback(
    () =>
      equipmentInvoicesApi
        .lookupEquipment({ isVarious: createKind === 'various' })
        .then((es) => es.map((e) => ({ id: e.id, name: e.name, unit: e.unit, price: e.price, extra: e.serial }))),
    [createKind],
  )

  return (
    <div className="page-card">
      <PageTitle
        title="Hóa đơn nhập thiết bị"
        extra={
          <Space>
            <Button type="primary" icon={<PlusOutlined />} onClick={() => setCreateKind('fixed')}>
              Tạo hóa đơn nhập tài sản
            </Button>
            <Button type="primary" ghost icon={<PlusOutlined />} onClick={() => setCreateKind('various')}>
              Tạo hóa đơn nhập vật tư
            </Button>
            <ExcelButton url="/equipment-invoices/export" fileName="hoa-don-nhap-thiet-bi.xlsx" params={params} />
            <Button icon={<ReloadOutlined />} onClick={reload} />
          </Space>
        }
      />
      <div className="toolbar">
        {user?.isAdministrator && <BranchSelect />}
        <Radio.Group size="small" optionType="button" value={kind} onChange={(e) => setKind(e.target.value)}
          options={[{ value: 'all', label: 'Tất cả' }, { value: 'fixed', label: 'Tài sản' }, { value: 'various', label: 'Vật tư' }]} />
        <DateRangeFilter value={range} onChange={setRange} />
      </div>
      <Row gutter={12}>
        <Col span={12}>
          <Card size="small" title={<span>Hóa đơn &nbsp; <Typography.Text type="secondary">({data?.count ?? 0} hóa đơn, tổng: <b>{money(data?.total)}</b>)</Typography.Text></span>}>
            <Table<EquipmentInvoice>
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
                { title: 'Số MH', dataIndex: 'quantity', width: 70, align: 'right' },
                { title: 'Tổng tiền', dataIndex: 'total', width: 130, align: 'right', render: money },
                ...(user?.isAdministrator
                  ? [{ title: '', width: 50, render: (_: unknown, r: EquipmentInvoice) => (
                      <DeleteButton title="Xóa hóa đơn và trừ lại tồn kho?" onConfirm={async () => { try { await equipmentInvoicesApi.remove(r.id); message.success('Đã xóa'); setSelected(null); reload() } catch (e) { message.error(errorMessage(e)) } }} />
                    ) }]
                  : []),
              ]}
            />
          </Card>
        </Col>
        <Col span={12}>
          <Card size="small" title={selected ? `Chi tiết hóa đơn #${selected.id} – ${fmtDate(selected.createdDate)}` : 'Chi tiết hóa đơn'}>
            <Table<EquipmentInvoiceLine>
              size="small"
              rowKey="equipmentId"
              dataSource={lines}
              pagination={false}
              columns={[
                { title: 'Thiết bị', dataIndex: 'equipmentName' },
                { title: 'Serial', dataIndex: 'serial', width: 90 },
                { title: 'Loại', dataIndex: 'equipmentType', width: 80 },
                { title: 'NCC', dataIndex: 'vendor', width: 100 },
                { title: 'ĐV', dataIndex: 'unit', width: 60 },
                { title: 'Giá', dataIndex: 'price', width: 100, align: 'right', render: money },
                { title: 'SL', dataIndex: 'quantity', width: 50, align: 'right' },
                { title: 'Thành tiền', dataIndex: 'total', width: 110, align: 'right', render: money },
              ]}
            />
          </Card>
        </Col>
      </Row>
      <ImportInvoiceModal
        open={createKind !== null}
        title={createKind === 'various' ? 'Khai báo nhập vật tư' : 'Khai báo nhập tài sản'}
        itemLabel="Thiết bị"
        loadItems={loadItems}
        onSubmit={async (body) => {
          await equipmentInvoicesApi.create(body)
          message.success('Đã tạo hóa đơn nhập')
          reload()
        }}
        onClose={() => setCreateKind(null)}
      />
    </div>
  )
}
