import { Button, Popconfirm, Table } from 'antd'
import type { Lookups, Treatment } from './api'
import { laboLabel } from './LaboPopover'
import { fmtDate, money } from '../../utils/format'

interface Props {
  rows: Treatment[]
  lookups: Lookups
  onCancelService?: (subId: number, serviceName: string) => void
}

/** "Các lần hẹn trước": read-only treatments of previous appointments with the "Hủy dịch vụ" action. */
export default function ElderTreatmentTable({ rows, lookups, onCancelService }: Props) {
  const name = (id: number) => lookups.services.find((s) => s.id === id)?.name ?? `#${id}`
  const emp = (u?: string | null) => (u ? lookups.doctors.find((d) => d.userName === u)?.fullName ?? lookups.assistants.find((d) => d.userName === u)?.fullName ?? u : '')
  const remain = (r: Treatment) => (r.isCancelled ? r.previousDebt - r.paid : r.previousDebt + r.quantity * r.price - r.paid)
  const columns = [
    { title: 'Đợt', dataIndex: 'appointmentNo', width: 50 },
    { title: 'Ngày', width: 100, render: (_: unknown, r: Treatment) => fmtDate(r.createdDate) },
    { title: 'Dịch vụ', width: 220, render: (_: unknown, r: Treatment) => <span className={r.isCancelled ? 'row-cancelled' : ''}>{name(r.serviceId)}</span> },
    { title: 'Số răng', dataIndex: 'toothPositions', width: 100 },
    { title: 'Số lượng', dataIndex: 'quantity', width: 70, align: 'right' as const },
    { title: 'Giá gốc', width: 100, align: 'right' as const, render: (_: unknown, r: Treatment) => money(r.originalPrice) },
    { title: 'Giá', width: 100, align: 'right' as const, render: (_: unknown, r: Treatment) => money(r.price) },
    { title: 'Bác sĩ', width: 140, render: (_: unknown, r: Treatment) => emp(r.doctorName) },
    { title: 'Phụ tá', width: 140, render: (_: unknown, r: Treatment) => emp(r.assistantName) },
    { title: 'Labo', width: 150, render: (_: unknown, r: Treatment) => laboLabel(r.labo, lookups.laboTypes, lookups.laboVendors) },
    { title: 'Ghi chú', dataIndex: 'comment', width: 160 },
    { title: 'Tổng tiền', width: 110, align: 'right' as const, render: (_: unknown, r: Treatment) => money((r.isCancelled ? 0 : r.price * r.quantity) + r.previousDebt) },
    { title: 'Thanh toán', width: 110, align: 'right' as const, render: (_: unknown, r: Treatment) => money(r.paid) },
    { title: 'Còn lại', width: 110, align: 'right' as const, render: (_: unknown, r: Treatment) => money(remain(r)) },
    ...(onCancelService
      ? [{
          title: '', width: 110,
          render: (_: unknown, r: Treatment) =>
            r.isCancelled ? null : (
              <Popconfirm title={`Bạn có muốn hủy dịch vụ ${name(r.serviceId)}? Thao tác này không thể phục hồi`} okText="Hủy dịch vụ" cancelText="Không" onConfirm={() => onCancelService(r.subId, name(r.serviceId))}>
                <Button size="small" danger>Hủy dịch vụ</Button>
              </Popconfirm>
            ),
        }]
      : []),
  ]
  return <Table size="small" rowKey="subId" columns={columns} dataSource={rows} pagination={false} scroll={{ x: 1700 }} />
}
