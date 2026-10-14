import { useEffect, useState } from 'react'
import { Col, Descriptions, Modal, Row, Spin, Table, Tabs, Tag, message } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { useNavigate } from 'react-router-dom'
import { errorMessage } from '../../api/client'
import { fmtDate, fmtDateTime, money, sexLabel } from '../../utils/format'
import { customersApi, prescriptionsApi, type CustomerDetail, type PrescriptionRow, type YesNoQuestion } from './api'

/** Port of ViewCustomerDialog ("Xem thông tin khách hàng"). */
export default function ViewCustomerModal({ customerId, open, onClose }: { customerId: number | null; open: boolean; onClose: () => void }) {
  const navigate = useNavigate()
  const [customer, setCustomer] = useState<CustomerDetail | null>(null)
  const [rows, setRows] = useState<PrescriptionRow[]>([])
  const [questions, setQuestions] = useState<YesNoQuestion[]>([])
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    if (!open || !customerId) return
    setLoading(true)
    Promise.all([customersApi.get(customerId), prescriptionsApi.list({ customerId, pageSize: 500 }), customersApi.questions()])
      .then(([c, p, q]) => {
        setCustomer(c)
        setRows(p.items)
        setQuestions(q)
      })
      .catch((e) => message.error(errorMessage(e)))
      .finally(() => setLoading(false))
  }, [open, customerId])

  const columns: ColumnsType<PrescriptionRow> = [
    { title: 'No', render: (_, __, i) => i + 1, width: 50 },
    { title: 'Mã phiếu', dataIndex: 'id', width: 90 },
    { title: 'Ngày tạo', dataIndex: 'createdDate', render: fmtDate, width: 100 },
    { title: 'Số lần hẹn', dataIndex: 'appointmentNo', width: 90, align: 'center' },
    { title: 'Ngày hẹn cuối', dataIndex: 'lastAppointmentDate', render: fmtDate, width: 110 },
    { title: 'Yêu cầu điều trị', dataIndex: 'treatment' },
    { title: 'Triệu chứng', dataIndex: 'diagnostic' },
    { title: 'Chuẩn đoán', dataIndex: 'symptoms' },
    { title: 'Lịch hẹn', dataIndex: 'lastNextAppointmentDate', render: fmtDateTime, width: 130 },
    { title: 'Lí do hẹn', dataIndex: 'lastAppointmentReason' },
    { title: 'Tổng tiền', dataIndex: 'total', align: 'right', render: money, width: 110 },
    { title: 'Đã trả', dataIndex: 'paid', align: 'right', render: money, width: 110 },
    { title: 'Còn lại', dataIndex: 'remain', align: 'right', render: (v: number) => <span style={{ color: v > 0 ? '#cf1322' : undefined }}>{money(v)}</span>, width: 110 },
  ]

  return (
    <Modal title="Xem thông tin khách hàng" open={open} onCancel={onClose} footer={null} width={1100}>
      <Spin spinning={loading}>
        {customer && (
          <>
            <Row gutter={16}>
              <Col span={5}>
                <div style={{ width: '100%', aspectRatio: '3 / 4', background: '#fafafa', border: '1px solid #f0f0f0', display: 'flex', alignItems: 'center', justifyContent: 'center', overflow: 'hidden' }}>
                  {customer.hasImage ? <img src={customersApi.photoUrl(customer.id)} alt="" style={{ maxWidth: '100%', maxHeight: '100%' }} /> : <span style={{ color: '#999' }}>Không có ảnh</span>}
                </div>
              </Col>
              <Col span={19}>
                <Descriptions title="Thông tin bệnh nhân" size="small" column={3} bordered>
                  <Descriptions.Item label="Họ tên">{customer.name}</Descriptions.Item>
                  <Descriptions.Item label="Mã số">{customer.formattedId}</Descriptions.Item>
                  <Descriptions.Item label="Chi nhánh">{customer.branchName}</Descriptions.Item>
                  <Descriptions.Item label="Giới tính">{sexLabel(customer.sex)}</Descriptions.Item>
                  <Descriptions.Item label="Tuổi">
                    {customer.age} ({customer.birthYear})
                  </Descriptions.Item>
                  <Descriptions.Item label="Số lần đã khám">{customer.times} lần</Descriptions.Item>
                  <Descriptions.Item label="Điện thoại">{customer.phone}</Descriptions.Item>
                  <Descriptions.Item label="E-mail">{customer.email}</Descriptions.Item>
                  <Descriptions.Item label="Ngày tạo">{fmtDateTime(customer.createdDate)}</Descriptions.Item>
                  <Descriptions.Item label="Địa chỉ" span={2}>
                    {customer.addressRepresentation}
                  </Descriptions.Item>
                  <Descriptions.Item label="Nghề nghiệp">{customer.job}</Descriptions.Item>
                </Descriptions>
              </Col>
            </Row>
            <Tabs
              style={{ marginTop: 12 }}
              items={[
                {
                  key: 'history',
                  label: 'Lịch sử khám',
                  children: (
                    <Table
                      size="small"
                      rowKey="id"
                      columns={columns}
                      dataSource={rows}
                      pagination={false}
                      scroll={{ x: 1400, y: 320 }}
                      onRow={(r) => ({ onDoubleClick: () => navigate(`/prescriptions/${r.id}`), style: { cursor: 'pointer' } })}
                    />
                  ),
                },
                {
                  key: 'health',
                  label: 'Tình trạng sức khỏe',
                  children: (
                    <div style={{ columns: 2 }}>
                      {questions.map((q) => {
                        const yes = ((customer.moreInformation?.yesQuestions ?? 0) & (1 << q.id)) !== 0
                        return (
                          <div key={q.id} style={{ padding: '4px 0', breakInside: 'avoid' }}>
                            <Tag color={yes ? 'red' : 'default'}>{yes ? 'Có' : 'Không'}</Tag> {q.id}. {q.header}
                          </div>
                        )
                      })}
                      {customer.moreInformation && (
                        <div style={{ marginTop: 12, color: '#666' }}>
                          Chọn PK do:{' '}
                          {['', 'Người quen giới thiệu', 'Quảng cáo báo, đài', 'Tự tìm đến', `Khác: ${customer.moreInformation.theReasonYouSeeMe ?? ''}`][customer.moreInformation.whyYouSeeMe] ?? ''}
                        </div>
                      )}
                    </div>
                  ),
                },
              ]}
            />
          </>
        )}
      </Spin>
    </Modal>
  )
}
