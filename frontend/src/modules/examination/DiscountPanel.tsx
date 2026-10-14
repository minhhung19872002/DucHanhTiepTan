import { useState } from 'react'
import { Alert, Button, Descriptions, Input, Radio, Space, Table, message } from 'antd'
import type { MembershipInfo } from './api'
import { examApi } from './api'
import { errorMessage } from '../../api/client'
import { fmtDate } from '../../utils/format'

export type DiscountType = 'normal' | 'free' | 'discount'

interface Props {
  type: DiscountType
  membership: MembershipInfo | null
  onChange: (type: DiscountType, membership: MembershipInfo | null) => void
  readOnly?: boolean
  /** Discounts can only be chosen on the first appointment (legacy DiscountControl rule). */
  firstAppointment: boolean
}

/** Port of DiscountControl + MembershipViewerPopup ("Giảm giá"). */
export default function DiscountPanel({ type, membership, onChange, readOnly, firstAppointment }: Props) {
  const [code, setCode] = useState(membership?.id ?? '')
  const [checking, setChecking] = useState(false)
  const disabled = readOnly || !firstAppointment

  const check = async () => {
    if (!code.trim()) return
    setChecking(true)
    try {
      const m = await examApi.membership(code.trim())
      onChange('discount', m)
      message.success(`Mã VIP hợp lệ: ${m.groupName}`)
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setChecking(false)
    }
  }

  return (
    <div>
      {!firstAppointment && <Alert type="info" showIcon style={{ marginBottom: 12 }} message="Giảm giá chỉ được chọn ở lần khám đầu tiên của phiếu khám." />}
      <Radio.Group value={type} disabled={disabled} onChange={(e) => onChange(e.target.value, e.target.value === 'discount' ? membership : null)}>
        <Radio value="normal">Bình thường</Radio>
        <Radio value="free">Miễn phí</Radio>
        <Radio value="discount">Mã VIP</Radio>
      </Radio.Group>
      {type === 'discount' && (
        <div style={{ marginTop: 12 }}>
          <Space>
            <Input placeholder="Nhập mã VIP" value={code} onChange={(e) => setCode(e.target.value)} onPressEnter={check} disabled={disabled} style={{ width: 200 }} />
            <Button onClick={check} loading={checking} disabled={disabled}>
              Kiểm tra
            </Button>
          </Space>
          {membership && (
            <div style={{ marginTop: 12 }}>
              <Descriptions size="small" bordered column={2}>
                <Descriptions.Item label="Mã">{membership.id}</Descriptions.Item>
                <Descriptions.Item label="Nhóm">{membership.groupName}</Descriptions.Item>
                <Descriptions.Item label="Ngày tạo">{fmtDate(membership.createdDate)}</Descriptions.Item>
                <Descriptions.Item label="Hết hạn">{fmtDate(membership.expiredDate)}</Descriptions.Item>
                <Descriptions.Item label="Kiểu giảm" span={2}>
                  {membership.isFixed ? `Cố định ${Math.round(membership.fixedValue * 100)}% cho mọi dịch vụ` : 'Theo từng dịch vụ'}
                </Descriptions.Item>
                {membership.description && <Descriptions.Item label="Ghi chú" span={2}>{membership.description}</Descriptions.Item>}
              </Descriptions>
              {!membership.isFixed && (
                <Table
                  style={{ marginTop: 8 }}
                  size="small"
                  rowKey="serviceId"
                  pagination={false}
                  dataSource={membership.discounts}
                  columns={[
                    { title: 'Dịch vụ', dataIndex: 'serviceName' },
                    { title: 'Giảm', dataIndex: 'value', width: 100, align: 'right' as const, render: (v: number) => `${Math.round(v * 100)}%` },
                  ]}
                />
              )}
            </div>
          )}
        </div>
      )}
    </div>
  )
}
