import { useState } from 'react'
import { Button, Checkbox, Input, InputNumber, Radio, Space, Table, message } from 'antd'
import { ReloadOutlined } from '@ant-design/icons'
import { errorMessage } from '../../api/client'
import { useAuth } from '../../auth/AuthContext'
import { BranchSelect, ExcelButton, PageTitle, useLoad } from '../../components/common'
import { money } from '../../utils/format'
import { equipmentStockApi, type EquipmentStock } from './api'

/**
 * Tồn kho tài sản / vật tư theo chi nhánh (legacy EquipmentManagerControl with ShowBranch).
 * Admins see it read-only (like AdministratorForm); receptionists can edit the quantity of their branch.
 */
export default function EquipmentStockPage() {
  const { user, branchId } = useAuth()
  const [kind, setKind] = useState<'fixed' | 'various' | 'all'>('fixed')
  const [searchBy, setSearchBy] = useState<'name' | 'serial' | 'usage'>('name')
  const [keyword, setKeyword] = useState('')
  const [onlyExisting, setOnlyExisting] = useState(true)
  const isVarious = kind === 'all' ? undefined : kind === 'various'
  const params = { branchId, isVarious, keyword: keyword || undefined, searchBy, onlyExisting }
  const { data, loading, reload, setData } = useLoad(() => equipmentStockApi.list(params), [branchId, kind, keyword, searchBy, onlyExisting])
  const canEdit = !user?.isAdministrator && branchId !== 0

  const adjust = async (row: EquipmentStock, qty: number | null) => {
    if (qty === null || qty === row.quantity) return
    try {
      const updated = await equipmentStockApi.adjust(row.id, branchId, qty)
      setData((data ?? []).map((d) => (d.id === row.id ? updated : d)))
      message.success('Đã cập nhật tồn kho')
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  return (
    <div className="page-card">
      <PageTitle
        title={kind === 'various' ? 'Tồn kho vật tư' : kind === 'fixed' ? 'Tồn kho tài sản' : 'Tồn kho thiết bị'}
        extra={
          <Space>
            <ExcelButton url="/equipment-stock/export" fileName="ton-kho-thiet-bi.xlsx" params={params} />
            <Button icon={<ReloadOutlined />} onClick={reload} />
          </Space>
        }
      />
      <div className="toolbar">
        {user?.isAdministrator && <BranchSelect />}
        <Radio.Group size="small" optionType="button" value={kind} onChange={(e) => setKind(e.target.value)}
          options={[{ value: 'fixed', label: 'Tài sản' }, { value: 'various', label: 'Vật tư' }, { value: 'all', label: 'Tất cả' }]} />
        <Input.Search placeholder="Tìm kiếm" allowClear style={{ width: 220 }} onSearch={setKeyword} />
        <Radio.Group size="small" value={searchBy} onChange={(e) => setSearchBy(e.target.value)}>
          <Radio value="name">Tên</Radio>
          <Radio value="serial">Serial</Radio>
          <Radio value="usage">Công dụng</Radio>
        </Radio.Group>
        <Checkbox checked={onlyExisting} onChange={(e) => setOnlyExisting(e.target.checked)}>
          Chỉ hiện thiết bị có trong kho
        </Checkbox>
      </div>
      <Table<EquipmentStock>
        size="small"
        rowKey="id"
        loading={loading}
        dataSource={data ?? []}
        pagination={{ pageSize: 50, showSizeChanger: true }}
        columns={[
          { title: '#', width: 50, render: (_, __, i) => i + 1 },
          { title: 'Tên', dataIndex: 'name' },
          { title: 'Serial', dataIndex: 'serial', width: 100 },
          { title: 'Loại', dataIndex: 'equipmentType', width: 90 },
          { title: 'Nhà cung cấp', dataIndex: 'vendor', width: 130 },
          { title: 'Đơn vị', dataIndex: 'unit', width: 70 },
          { title: 'Giá', dataIndex: 'price', width: 110, align: 'right', render: money },
          {
            title: branchId === 0 ? 'Tồn kho (tất cả CN)' : 'Tồn kho',
            dataIndex: 'quantity',
            width: 130,
            align: 'right',
            render: (v: number, r) =>
              canEdit ? <InputNumber size="small" min={0} value={v} style={{ width: 90 }} onBlur={(e) => adjust(r, Number(e.target.value))} onPressEnter={(e) => adjust(r, Number((e.target as HTMLInputElement).value))} /> : money(v),
          },
          { title: 'Vị trí', dataIndex: 'position', width: 130 },
          { title: 'Công dụng', dataIndex: 'usage' },
          { title: 'Ghi chú', dataIndex: 'note' },
        ]}
      />
    </div>
  )
}
