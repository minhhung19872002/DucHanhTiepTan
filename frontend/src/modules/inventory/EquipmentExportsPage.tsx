import { useEffect, useState } from 'react'
import { Button, Card, Col, Radio, Row, Space, Table, Typography, message } from 'antd'
import { PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import { errorMessage } from '../../api/client'
import { useAuth } from '../../auth/AuthContext'
import { BranchSelect, DeleteButton, ExcelButton, PageTitle, useLoad } from '../../components/common'
import { fmtDate } from '../../utils/format'
import { equipmentExportsApi, type ExportInvoice, type ExportInvoiceLine } from './api'
import DateRangeFilter, { toDateParams, useRangeState } from './DateRangeFilter'
import ExportInvoiceModal from './ExportInvoiceModal'

/** Hóa đơn xuất tài sản / vật tư (legacy ExportInvoiceManagerControl + CreateExportInvoiceDialog). */
export default function EquipmentExportsPage() {
  const { user, branchId } = useAuth()
  const [range, setRange] = useRangeState()
  const [kind, setKind] = useState<'all' | 'fixed' | 'various'>('all')
  const isVarious = kind === 'all' ? undefined : kind === 'various'
  const params = { branchId, limit: range.limit, isVarious, ...toDateParams(range) }
  const { data, loading, reload } = useLoad(() => equipmentExportsApi.list(params), [branchId, range, kind])
  const [selected, setSelected] = useState<ExportInvoice | null>(null)
  const [lines, setLines] = useState<ExportInvoiceLine[]>([])
  const [createKind, setCreateKind] = useState<'fixed' | 'various' | null>(null)

  useEffect(() => {
    if (!selected) return setLines([])
    equipmentExportsApi.get(selected.id).then((d) => setLines(d.lines)).catch((e) => message.error(errorMessage(e)))
  }, [selected])

  return (
    <div className="page-card">
      <PageTitle
        title="Hóa đơn xuất thiết bị"
        extra={
          <Space>
            <Button type="primary" icon={<PlusOutlined />} onClick={() => setCreateKind('fixed')}>
              Tạo hóa đơn xuất tài sản
            </Button>
            <Button type="primary" ghost icon={<PlusOutlined />} onClick={() => setCreateKind('various')}>
              Tạo hóa đơn xuất vật tư
            </Button>
            <ExcelButton url="/equipment-exports/export" fileName="hoa-don-xuat-thiet-bi.xlsx" params={params} />
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
          <Card size="small" title={<span>Hóa đơn &nbsp; <Typography.Text type="secondary">({data?.count ?? 0} hóa đơn)</Typography.Text></span>}>
            <Table<ExportInvoice>
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
                ...(user?.isAdministrator
                  ? [{ title: '', width: 50, render: (_: unknown, r: ExportInvoice) => (
                      <DeleteButton title="Xóa hóa đơn và trả lại tồn kho?" onConfirm={async () => { try { await equipmentExportsApi.remove(r.id); message.success('Đã xóa'); setSelected(null); reload() } catch (e) { message.error(errorMessage(e)) } }} />
                    ) }]
                  : []),
              ]}
            />
          </Card>
        </Col>
        <Col span={12}>
          <Card size="small" title={selected ? `Chi tiết hóa đơn #${selected.id} – ${fmtDate(selected.createdDate)}` : 'Chi tiết hóa đơn'}>
            <Table<ExportInvoiceLine>
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
                { title: 'SL xuất', dataIndex: 'quantity', width: 70, align: 'right' },
              ]}
            />
          </Card>
        </Col>
      </Row>
      <ExportInvoiceModal open={createKind !== null} kind={createKind ?? 'fixed'} onClose={() => setCreateKind(null)} onCreated={reload} />
    </div>
  )
}
