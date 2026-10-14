import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { Button, Card, Col, Empty, Input, List, Popconfirm, Row, Space, Spin, Typography, message } from 'antd'
import { DeleteOutlined, ReloadOutlined, UserAddOutlined } from '@ant-design/icons'
import { useNavigate, useParams, useSearchParams } from 'react-router-dom'
import type { InProgressItem } from './api'
import { examApi } from './api'
import ExamEditor from './ExamEditor'
import { errorMessage } from '../../api/client'
import { useAuth } from '../../auth/AuthContext'
import { fmtDateTime } from '../../utils/format'
import { VietnameseNormalize } from './text'

/**
 * "Khám bệnh": left = DoctorNavigatorUserControl ("Đang khám", grouped by doctor),
 * right = PrescriptionControl editor for the selected appointment.
 * `?customerId=` starts a new prescription for that customer (legacy "Tạo mới" in the Khám bệnh group).
 */
export default function ExaminationPage() {
  const { prescriptionId, appointmentNo } = useParams()
  const [search] = useSearchParams()
  const navigate = useNavigate()
  const { branchId } = useAuth()
  const [items, setItems] = useState<InProgressItem[]>([])
  const [loading, setLoading] = useState(false)
  const [keyword, setKeyword] = useState('')
  const startedFor = useRef<string | null>(null)

  const reload = useCallback(async () => {
    setLoading(true)
    try {
      setItems(await examApi.inProgress(branchId))
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setLoading(false)
    }
  }, [branchId])

  useEffect(() => {
    reload()
  }, [reload])

  // Start a new prescription when opened with ?customerId=
  const customerId = search.get('customerId')
  useEffect(() => {
    if (!customerId || startedFor.current === customerId) return
    startedFor.current = customerId
    examApi
      .start(Number(customerId))
      .then((r) => {
        message.success('Đã tạo phiếu khám mới')
        reload()
        navigate(`/examination/${r.prescriptionId}/${r.appointmentNo}`, { replace: true })
      })
      .catch((e) => {
        message.error(errorMessage(e))
        navigate('/examination', { replace: true })
      })
  }, [customerId, navigate, reload])

  const groups = useMemo(() => {
    const k = VietnameseNormalize(keyword)
    const filtered = items.filter((i) => !k || VietnameseNormalize(`${i.customerName} ${i.formattedId}`).includes(k))
    const map = new Map<string, InProgressItem[]>()
    for (const i of filtered) {
      const g = i.doctorFullName || i.doctorName || 'Chưa chọn bác sĩ'
      map.set(g, [...(map.get(g) ?? []), i])
    }
    return [...map.entries()].sort((a, b) => a[0].localeCompare(b[0], 'vi'))
  }, [items, keyword])

  const remove = async (i: InProgressItem) => {
    try {
      await examApi.remove(i.prescriptionId, i.appointmentNo, false)
      message.success('Đã xóa phiếu khám')
      if (Number(prescriptionId) === i.prescriptionId && Number(appointmentNo) === i.appointmentNo) navigate('/examination')
      reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  const selected = prescriptionId && appointmentNo ? { p: Number(prescriptionId), a: Number(appointmentNo) } : null

  return (
    <Row gutter={12} wrap={false}>
      <Col flex="300px" className="no-print">
        <Card
          size="small"
          title={`Đang khám (${items.length})`}
          extra={
            <Space size={4}>
              <Button size="small" icon={<UserAddOutlined />} onClick={() => navigate('/customers')} title="Tạo phiếu khám từ danh sách khách hàng" />
              <Button size="small" icon={<ReloadOutlined />} onClick={reload} />
            </Space>
          }
          styles={{ body: { padding: 8, maxHeight: 'calc(100vh - 170px)', overflow: 'auto' } }}
        >
          <Input.Search size="small" allowClear placeholder="Tìm khách hàng" value={keyword} onChange={(e) => setKeyword(e.target.value)} style={{ marginBottom: 8 }} />
          <Spin spinning={loading}>
            {groups.length === 0 && <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Không có khách đang khám" />}
            {groups.map(([doctor, list]) => (
              <div key={doctor} style={{ marginBottom: 8 }}>
                <Typography.Text strong type="secondary">{doctor} ({list.length})</Typography.Text>
                <List
                  size="small"
                  dataSource={list}
                  renderItem={(i) => {
                    const active = selected?.p === i.prescriptionId && selected?.a === i.appointmentNo
                    return (
                      <List.Item
                        style={{ cursor: 'pointer', background: active ? '#e6f4ff' : undefined, paddingInline: 6 }}
                        onClick={() => navigate(`/examination/${i.prescriptionId}/${i.appointmentNo}`)}
                        actions={[
                          <Popconfirm key="del" title="Xóa phiếu khám đang khám này?" okText="Xóa" cancelText="Hủy" onConfirm={(e) => { e?.stopPropagation(); remove(i) }} onCancel={(e) => e?.stopPropagation()}>
                            <Button size="small" type="text" danger icon={<DeleteOutlined />} onClick={(e) => e.stopPropagation()} />
                          </Popconfirm>,
                        ]}
                      >
                        <List.Item.Meta
                          title={<span>{i.customerName} <Typography.Text type="secondary" style={{ fontSize: 12 }}>({i.formattedId})</Typography.Text></span>}
                          description={<span style={{ fontSize: 12 }}>{i.customerSex} · {i.customerBirthYear} · Lần {i.appointmentNo} · {fmtDateTime(i.createdDate)}</span>}
                        />
                      </List.Item>
                    )
                  }}
                />
              </div>
            ))}
          </Spin>
        </Card>
      </Col>
      <Col flex="auto" style={{ minWidth: 0 }}>
        {customerId && !selected ? (
          <Spin style={{ display: 'block', margin: '80px auto' }} tip="Đang tạo phiếu khám..." />
        ) : selected ? (
          <ExamEditor key={`${selected.p}-${selected.a}`} prescriptionId={selected.p} appointmentNo={selected.a} onChanged={reload} />
        ) : (
          <Card>
            <Empty description="Chọn một khách hàng trong danh sách Đang khám, hoặc tạo phiếu khám mới từ màn hình Khách hàng." >
              <Button type="primary" onClick={() => navigate('/customers')}>Tìm khách hàng</Button>
            </Empty>
          </Card>
        )}
      </Col>
    </Row>
  )
}
