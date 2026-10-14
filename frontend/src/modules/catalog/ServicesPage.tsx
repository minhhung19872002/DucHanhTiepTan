import { useEffect, useState } from 'react'
import { Button, Col, Form, Input, Modal, Radio, Row, Space, Table, Tag, message } from 'antd'
import { EditOutlined, PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import { DeleteButton, PageTitle, useLoad } from '../../components/common'
import { errorMessage } from '../../api/client'
import { money } from '../../utils/format'
import { MoneyInput } from './components'
import { serviceGroupsApi, servicesApi, type Service, type ServiceGroup, type ServiceInput } from './api'

/** "Dịch vụ" – legacy ServiceManagerControl + DeclareServiceDialog (groups on the left, services on the right). */
export default function ServicesPage() {
  const [groupId, setGroupId] = useState<number | undefined>(undefined)
  const [keyword, setKeyword] = useState('')
  const [priceMode, setPriceMode] = useState<'all' | 'custom'>('all')
  const [fromPrice, setFromPrice] = useState<number | null>(0)
  const [toPrice, setToPrice] = useState<number | null>(10000000)
  const [newGroup, setNewGroup] = useState('')
  const [editing, setEditing] = useState<Service | null | undefined>(undefined)
  const [form] = Form.useForm<ServiceInput>()

  const groups = useLoad(() => serviceGroupsApi.list(), [])
  const services = useLoad(
    () =>
      servicesApi.list({
        keyword: keyword || undefined,
        groupId,
        fromPrice: priceMode === 'custom' ? (fromPrice ?? 0) : undefined,
        toPrice: priceMode === 'custom' ? (toPrice ?? undefined) : undefined,
      }),
    [keyword, groupId, priceMode, fromPrice, toPrice],
  )
  useEffect(() => {
    if (groupId === undefined && groups.data?.length) setGroupId(groups.data[0].id)
  }, [groups.data, groupId])

  const addGroup = async () => {
    if (!newGroup.trim()) return
    try {
      await serviceGroupsApi.create(newGroup.trim())
      setNewGroup('')
      groups.reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }
  const renameGroup = (g: ServiceGroup) => {
    let name = g.name ?? ''
    Modal.confirm({
      title: 'Đổi tên nhóm dịch vụ',
      content: <Input defaultValue={name} onChange={(e) => (name = e.target.value)} />,
      okText: 'Lưu',
      cancelText: 'Hủy',
      onOk: async () => {
        try {
          await serviceGroupsApi.update(g.id, name)
          groups.reload()
          services.reload()
        } catch (e) {
          message.error(errorMessage(e))
          throw e
        }
      },
    })
  }
  const deleteGroup = async (g: ServiceGroup) => {
    try {
      await serviceGroupsApi.remove(g.id)
      if (groupId === g.id) setGroupId(undefined)
      groups.reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  const openNew = () => {
    if (!groupId) {
      message.warning('Vui lòng chọn nhóm dịch vụ.')
      return
    }
    form.resetFields()
    form.setFieldsValue({ groupId, price: 0 })
    setEditing(null)
  }
  const openEdit = (s: Service) => {
    form.setFieldsValue({ name: s.name, price: s.price, description: s.description ?? '', groupId: s.groupId })
    setEditing(s)
  }
  const save = async () => {
    const v = await form.validateFields()
    try {
      if (editing) {
        await servicesApi.update(editing.id, v)
        message.success('Đã cập nhật dịch vụ')
      } else {
        await servicesApi.create(v)
        message.success('Đã thêm dịch vụ')
      }
      setEditing(undefined)
      services.reload()
      groups.reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }
  const remove = async (s: Service) => {
    try {
      await servicesApi.remove(s.id)
      services.reload()
      groups.reload()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  const groupOptions = (groups.data ?? []).map((g) => ({ value: g.id, label: g.name ?? `Nhóm ${g.id}` }))

  return (
    <div className="page-card">
      <PageTitle title="Dịch vụ" />
      <Row gutter={16}>
        <Col xs={24} md={8} lg={7}>
          <div className="toolbar">
            <strong>Nhóm dịch vụ:</strong>
          </div>
          <Space.Compact style={{ width: '100%', marginBottom: 8 }}>
            <Input placeholder="Tên nhóm mới" value={newGroup} onChange={(e) => setNewGroup(e.target.value)} onPressEnter={addGroup} />
            <Button type="primary" icon={<PlusOutlined />} onClick={addGroup}>
              Thêm
            </Button>
          </Space.Compact>
          <Table<ServiceGroup>
            size="small"
            rowKey="id"
            loading={groups.loading}
            dataSource={groups.data ?? []}
            pagination={false}
            rowSelection={{ type: 'radio', selectedRowKeys: groupId ? [groupId] : [], onChange: (keys) => setGroupId(Number(keys[0])) }}
            onRow={(g) => ({ onClick: () => setGroupId(g.id), style: { cursor: 'pointer' } })}
            columns={[
              { title: 'No', width: 50, render: (_, __, i) => i + 1 },
              { title: 'Tên', dataIndex: 'name', render: (v, g) => <>{v} {g.isBuiltIn && <Tag>dựng sẵn</Tag>}</> },
              { title: 'SL', dataIndex: 'serviceCount', width: 50, align: 'right' },
              {
                title: '',
                width: 80,
                render: (_, g) => (
                  <Space size={4} onClick={(e) => e.stopPropagation()}>
                    <Button size="small" icon={<EditOutlined />} onClick={() => renameGroup(g)} />
                    <DeleteButton title="Xóa nhóm dịch vụ này?" onConfirm={() => deleteGroup(g)} disabled={g.isBuiltIn} />
                  </Space>
                ),
              },
            ]}
          />
        </Col>
        <Col xs={24} md={16} lg={17}>
          <div className="toolbar">
            <Input.Search allowClear placeholder="Tìm theo tên dịch vụ" style={{ width: 240 }} onSearch={setKeyword} />
            <span>Theo giá:</span>
            <Radio.Group value={priceMode} onChange={(e) => setPriceMode(e.target.value)}>
              <Radio value="all">Tất cả</Radio>
              <Radio value="custom">Từ</Radio>
            </Radio.Group>
            <MoneyInput value={fromPrice ?? undefined} onChange={(v) => setFromPrice(v ?? null)} disabled={priceMode !== 'custom'} style={{ width: 130 }} />
            <span>đến</span>
            <MoneyInput value={toPrice ?? undefined} onChange={(v) => setToPrice(v ?? null)} disabled={priceMode !== 'custom'} style={{ width: 130 }} />
            <span style={{ flex: 1 }} />
            <Button icon={<ReloadOutlined />} onClick={services.reload} />
            <Button type="primary" icon={<PlusOutlined />} onClick={openNew}>
              Thêm
            </Button>
          </div>
          <Table<Service>
            size="small"
            rowKey="id"
            loading={services.loading}
            dataSource={services.data ?? []}
            pagination={{ pageSize: 50, showSizeChanger: true, showTotal: (t) => `${t} dòng dữ liệu` }}
            columns={[
              { title: 'No', width: 55, render: (_, __, i) => i + 1 },
              { title: 'Id', dataIndex: 'id', width: 60 },
              { title: 'Tên', dataIndex: 'name', sorter: (a, b) => a.name.localeCompare(b.name) },
              { title: 'Giá', dataIndex: 'price', align: 'right', width: 120, render: money, sorter: (a, b) => a.price - b.price },
              { title: 'Mô tả', dataIndex: 'description' },
              {
                title: '',
                width: 90,
                render: (_, s) => (
                  <Space size={4}>
                    <Button size="small" icon={<EditOutlined />} onClick={() => openEdit(s)} />
                    <DeleteButton title="Xóa dịch vụ này?" onConfirm={() => remove(s)} />
                  </Space>
                ),
              },
            ]}
          />
        </Col>
      </Row>

      <Modal title={editing ? 'Sửa dịch vụ' : 'Thêm dịch vụ'} open={editing !== undefined} onOk={save} onCancel={() => setEditing(undefined)} okText={editing ? 'Lưu' : 'Tạo'} cancelText="Đóng">
        <Form form={form} layout="vertical">
          <Form.Item name="groupId" label="Nhóm dịch vụ" rules={[{ required: true, message: 'Vui lòng chọn nhóm dịch vụ.' }]}>
            <Radio.Group options={groupOptions} optionType="button" buttonStyle="solid" size="small" />
          </Form.Item>
          <Form.Item name="name" label="Tên dịch vụ" rules={[{ required: true, message: 'Vui lòng điền tên dịch vụ.' }]}>
            <Input autoFocus />
          </Form.Item>
          <Form.Item name="price" label="Giá" rules={[{ required: true }]}>
            <MoneyInput />
          </Form.Item>
          <Form.Item name="description" label="Mô tả">
            <Input />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}
