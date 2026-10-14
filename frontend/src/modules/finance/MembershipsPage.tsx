import { useEffect, useMemo, useState } from 'react'
import { Button, Col, DatePicker, Form, Input, InputNumber, Modal, Radio, Row, Select, Space, Table, Tag, Typography, message } from 'antd'
import { EditOutlined, PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import dayjs, { type Dayjs } from 'dayjs'
import { DeleteButton, PageTitle, useBranches, useLoad } from '../../components/common'
import { errorMessage } from '../../api/client'
import { fmtDate } from '../../utils/format'
import { membershipGroupsApi, membershipsApi, type Membership, type MembershipGroup, type ServiceLookup } from './api'

interface DiscountRow {
  serviceId: number
  serviceName: string
  percent: number
}

/** Create / edit a membership group – legacy CreateMememberShipGroupDialog. */
function GroupModal({ open, groupId, onClose, onSaved }: { open: boolean; groupId?: number; onClose: () => void; onSaved: () => void }) {
  const [form] = Form.useForm<{ name: string; branchId: number; isFixed: boolean; percent: number; description?: string }>()
  const branches = useBranches()
  const [services, setServices] = useState<ServiceLookup[]>([])
  const [rows, setRows] = useState<DiscountRow[]>([])
  const [saving, setSaving] = useState(false)
  const isFixed = Form.useWatch('isFixed', form)

  useEffect(() => {
    if (!open) return
    membershipGroupsApi.services().then(setServices).catch(() => setServices([]))
    if (groupId) {
      membershipGroupsApi.get(groupId).then((g) => {
        form.setFieldsValue({ name: g.name, branchId: g.branchId, isFixed: g.isFixed, percent: g.percent ?? 0, description: g.description ?? '' })
        setRows(g.discounts.map((d) => ({ serviceId: d.serviceId, serviceName: d.serviceName, percent: d.percent })))
      }).catch((e) => message.error(errorMessage(e)))
    } else {
      form.resetFields()
      form.setFieldsValue({ isFixed: true, percent: 0, branchId: branches.find((b) => b.id !== 0)?.id })
      setRows([])
    }
  }, [open, groupId, form, branches])

  const addService = (id: number) => {
    const s = services.find((x) => x.id === id)
    if (!s) return
    if (rows.some((r) => r.serviceId === id)) { message.warning(`Dịch vụ ${s.name} bị trùng`); return }
    setRows((rs) => [...rs, { serviceId: s.id, serviceName: s.name, percent: 0 }])
  }
  const loadAll = () => setRows(services.map((s) => ({ serviceId: s.id, serviceName: s.name, percent: 0 })))

  const save = async () => {
    const v = await form.validateFields()
    setSaving(true)
    try {
      const payload = { ...v, discounts: v.isFixed ? [] : rows.map((r) => ({ serviceId: r.serviceId, percent: r.percent })) }
      if (groupId) await membershipGroupsApi.update(groupId, payload)
      else await membershipGroupsApi.create(payload)
      message.success('Đã lưu nhóm thẻ')
      onSaved()
      onClose()
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal title={groupId ? 'Sửa nhóm thẻ VIP' : 'Tạo nhóm thẻ VIP'} open={open} onCancel={onClose} onOk={save} okText="Lưu" cancelText="Hủy" confirmLoading={saving} width={760} destroyOnHidden>
      <Form form={form} layout="vertical">
        <Row gutter={12}>
          <Col span={12}><Form.Item name="name" label="Tên nhóm" rules={[{ required: true, message: 'Vui lòng điền tên' }]}><Input /></Form.Item></Col>
          <Col span={12}><Form.Item name="branchId" label="Chi nhánh" rules={[{ required: true, message: 'Chọn chi nhánh' }]}>
            <Select options={branches.filter((b) => b.id !== 0).map((b) => ({ value: b.id, label: b.name }))} /></Form.Item></Col>
        </Row>
        <Form.Item name="description" label="Mô tả"><Input /></Form.Item>
        <Form.Item name="isFixed" label="Áp dụng">
          <Radio.Group options={[{ value: true, label: 'Giảm giá tất cả dịch vụ' }, { value: false, label: 'Giảm giá theo dịch vụ chỉ định' }]} />
        </Form.Item>
        {isFixed ? (
          <Form.Item name="percent" label="Giảm giá (%)" rules={[{ required: true, message: 'Nhập giá trị' }]}><InputNumber min={0} max={100} /></Form.Item>
        ) : (
          <>
            <Space style={{ marginBottom: 8 }}>
              <Select showSearch placeholder="Thêm dịch vụ" style={{ width: 320 }} value={null} optionFilterProp="label" onChange={(id) => id && addService(id as number)}
                options={services.map((s) => ({ value: s.id, label: s.name }))} />
              <Button size="small" type="link" onClick={loadAll}>Tải tất cả dịch vụ</Button>
            </Space>
            <Table size="small" rowKey="serviceId" dataSource={rows} pagination={false} scroll={{ y: 260 }}
              columns={[
                { title: 'Dịch vụ', dataIndex: 'serviceName' },
                { title: 'Giảm giá (%)', dataIndex: 'percent', width: 130, render: (v: number, r: DiscountRow) => (
                  <InputNumber min={0} max={100} value={v} onChange={(n) => setRows((rs) => rs.map((x) => (x.serviceId === r.serviceId ? { ...x, percent: n ?? 0 } : x)))} />
                ) },
                { title: '', width: 50, render: (_: unknown, r: DiscountRow) => <DeleteButton onConfirm={() => setRows((rs) => rs.filter((x) => x.serviceId !== r.serviceId))} /> },
              ]} />
          </>
        )}
      </Form>
    </Modal>
  )
}

/** Create / edit a membership card – legacy CreateMememberShipDialog. */
function CardModal({ open, group, card, onClose, onSaved }: { open: boolean; group?: MembershipGroup; card?: Membership; onClose: () => void; onSaved: () => void }) {
  const [form] = Form.useForm<{ id: string; createdDate: Dayjs; expiredDate?: Dayjs; description?: string }>()
  const [saving, setSaving] = useState(false)
  useEffect(() => {
    if (!open) return
    if (card) form.setFieldsValue({ id: card.id, createdDate: dayjs(card.createdDate), expiredDate: card.expiredDate ? dayjs(card.expiredDate) : undefined, description: card.description ?? '' })
    else { form.resetFields(); form.setFieldsValue({ createdDate: dayjs(), expiredDate: dayjs().add(1, 'year') }) }
  }, [open, card, form])

  const save = async (keepOpen: boolean) => {
    const v = await form.validateFields()
    if (v.expiredDate && !v.expiredDate.isAfter(v.createdDate, 'day')) { message.warning('Hạn dùng phải sau ngày tạo.'); return }
    setSaving(true)
    try {
      const payload = { createdDate: v.createdDate.format('YYYY-MM-DD'), expiredDate: v.expiredDate?.format('YYYY-MM-DD'), description: v.description }
      if (card) await membershipsApi.update(card.id, payload)
      else await membershipsApi.create({ ...payload, id: v.id, groupId: group!.id })
      message.success('Đã lưu mã thẻ')
      onSaved()
      if (keepOpen && !card) form.setFieldsValue({ id: '' })
      else onClose()
    } catch (e) {
      message.error(errorMessage(e))
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal title={card ? `Sửa thẻ ${card.id}` : `Tạo thẻ – nhóm ${group?.name ?? ''}`} open={open} onCancel={onClose} destroyOnHidden
      footer={[
        <Button key="c" onClick={onClose}>Đóng</Button>,
        !card && <Button key="m" loading={saving} onClick={() => save(true)}>Lưu & tạo tiếp</Button>,
        <Button key="s" type="primary" loading={saving} onClick={() => save(false)}>Lưu</Button>,
      ]}>
      <Form form={form} layout="vertical">
        <Form.Item name="id" label="Mã thẻ" rules={[{ required: true, message: 'Vui lòng điền mã thẻ' }, { max: 30 }]}>
          <Input disabled={!!card} autoFocus style={{ textTransform: 'uppercase' }} />
        </Form.Item>
        <Row gutter={12}>
          <Col span={12}><Form.Item name="createdDate" label="Ngày tạo" rules={[{ required: true }]}><DatePicker format="DD/MM/YYYY" style={{ width: '100%' }} /></Form.Item></Col>
          <Col span={12}><Form.Item name="expiredDate" label="Hạn dùng"><DatePicker format="DD/MM/YYYY" style={{ width: '100%' }} /></Form.Item></Col>
        </Row>
        <Form.Item name="description" label="Mô tả"><Input /></Form.Item>
      </Form>
    </Modal>
  )
}

/** "Mã VIP" – legacy MemberShipManagerControl (admin only). */
export default function MembershipsPage() {
  const [groupId, setGroupId] = useState<number>()
  const [keyword, setKeyword] = useState('')
  const [groupModal, setGroupModal] = useState<{ open: boolean; id?: number }>({ open: false })
  const [cardModal, setCardModal] = useState<{ open: boolean; card?: Membership }>({ open: false })

  const { data: groups, loading, reload } = useLoad(() => membershipGroupsApi.list(), [])
  const group = useMemo(() => groups?.find((g) => g.id === groupId), [groups, groupId])
  const { data: cards, loading: loadingCards, reload: reloadCards } = useLoad(
    () => (groupId ? membershipsApi.list({ groupId, keyword: keyword || undefined }) : Promise.resolve([] as Membership[])),
    [groupId, keyword],
  )
  useEffect(() => {
    if (groups && !groups.some((g) => g.id === groupId)) setGroupId(groups[0]?.id)
  }, [groups, groupId])

  const removeGroup = async (id: number) => {
    try { await membershipGroupsApi.remove(id); message.success('Đã xóa nhóm thẻ'); reload() } catch (e) { message.error(errorMessage(e)) }
  }
  const removeCard = async (id: string) => {
    try { await membershipsApi.remove(id); message.success('Đã xóa thẻ'); reloadCards(); reload() } catch (e) { message.error(errorMessage(e)) }
  }

  return (
    <div>
      <PageTitle title="Quản lí thẻ VIP" extra={<Button icon={<ReloadOutlined />} onClick={() => { reload(); reloadCards() }}>Tải lại</Button>} />
      <Row gutter={12}>
        <Col xs={24} lg={12}>
          <div className="page-card">
            <div className="toolbar">
              <Typography.Text strong>Nhóm thẻ</Typography.Text>
              <Button type="primary" size="small" icon={<PlusOutlined />} onClick={() => setGroupModal({ open: true })} style={{ marginLeft: 'auto' }}>Tạo nhóm</Button>
            </div>
            <Table size="small" rowKey="id" loading={loading} dataSource={groups ?? []} pagination={false} scroll={{ x: 'max-content' }}
              rowClassName={(r) => (r.id === groupId ? 'ant-table-row-selected' : '')}
              onRow={(r) => ({ onClick: () => setGroupId(r.id), style: { cursor: 'pointer' } })}
              columns={[
                { title: 'No', width: 45, render: (_: unknown, __: MembershipGroup, i: number) => i + 1 },
                { title: 'Tên', dataIndex: 'name', width: 140 },
                { title: 'Chi nhánh', dataIndex: 'branchName', width: 150 },
                { title: 'Loại', dataIndex: 'isFixed', width: 110, render: (v: boolean) => (v ? <Tag color="green">Tất cả DV</Tag> : <Tag color="blue">DV chỉ định</Tag>) },
                { title: 'Giảm giá (%)', dataIndex: 'percent', width: 100, align: 'right' as const },
                { title: 'Số thẻ', dataIndex: 'memberCount', width: 70, align: 'right' as const },
                { title: 'Mô tả', dataIndex: 'description' },
                { title: '', width: 80, render: (_: unknown, r: MembershipGroup) => (
                  <Space>
                    <Button size="small" icon={<EditOutlined />} onClick={(e) => { e.stopPropagation(); setGroupModal({ open: true, id: r.id }) }} />
                    <DeleteButton title="Xóa nhóm thẻ này?" onConfirm={() => removeGroup(r.id)} />
                  </Space>
                ) },
              ]} />
            {group && !group.isFixed && (
              <Typography.Paragraph type="secondary" style={{ marginTop: 8 }}>Dịch vụ áp dụng: {group.services || '(chưa có)'}</Typography.Paragraph>
            )}
          </div>
        </Col>
        <Col xs={24} lg={12}>
          <div className="page-card">
            <div className="toolbar">
              <Typography.Text strong>Mã thẻ{group ? ` – ${group.name}` : ''}</Typography.Text>
              <Input.Search allowClear placeholder="Tìm mã thẻ" size="small" style={{ width: 200 }} onSearch={(v) => setKeyword(v.trim())} />
              <Button type="primary" size="small" icon={<PlusOutlined />} disabled={!group} onClick={() => setCardModal({ open: true })} style={{ marginLeft: 'auto' }}>Tạo thẻ</Button>
            </div>
            <Table size="small" rowKey="id" loading={loadingCards} dataSource={cards ?? []} pagination={{ pageSize: 25, size: 'small' }} scroll={{ x: 'max-content' }}
              columns={[
                { title: 'No', width: 45, render: (_: unknown, __: Membership, i: number) => i + 1 },
                { title: 'Mã thẻ', dataIndex: 'id', width: 120 },
                { title: 'Ngày tạo', dataIndex: 'createdDate', width: 100, render: fmtDate },
                { title: 'Hạn dùng', dataIndex: 'expiredDate', width: 100, render: (v: string | null, r: Membership) => (
                  <span style={{ color: v && dayjs(v).isBefore(dayjs(), 'day') ? '#cf1322' : undefined }}>{fmtDate(v)}{r.usedCount ? '' : ''}</span>
                ) },
                { title: 'Đã dùng', dataIndex: 'usedCount', width: 80, align: 'right' as const },
                { title: 'Mô tả', dataIndex: 'description' },
                { title: '', width: 80, render: (_: unknown, r: Membership) => (
                  <Space>
                    <Button size="small" icon={<EditOutlined />} onClick={() => setCardModal({ open: true, card: r })} />
                    <DeleteButton title="Xóa thẻ này?" onConfirm={() => removeCard(r.id)} />
                  </Space>
                ) },
              ]} />
          </div>
        </Col>
      </Row>
      <GroupModal open={groupModal.open} groupId={groupModal.id} onClose={() => setGroupModal({ open: false })} onSaved={reload} />
      <CardModal open={cardModal.open} group={group} card={cardModal.card} onClose={() => setCardModal({ open: false })} onSaved={() => { reloadCards(); reload() }} />
    </div>
  )
}
