import { useState } from 'react'
import { Button, Checkbox, Input, InputNumber, Space, Table, message } from 'antd'
import { ReloadOutlined } from '@ant-design/icons'
import { errorMessage } from '../../api/client'
import { useAuth } from '../../auth/AuthContext'
import { BranchSelect, ExcelButton, PageTitle, useLoad } from '../../components/common'
import { money } from '../../utils/format'
import { drugStockApi, type DrugStock } from './api'

/** Tồn kho thuốc theo chi nhánh (DrugInBranch). Receptionists may adjust the quantity of their branch. */
export default function DrugStockPage() {
  const { user, branchId } = useAuth()
  const [keyword, setKeyword] = useState('')
  const [onlyExisting, setOnlyExisting] = useState(false)
  const params = { branchId, keyword: keyword || undefined, onlyExisting }
  const { data, loading, reload, setData } = useLoad(() => drugStockApi.list(params), [branchId, keyword, onlyExisting])
  const canEdit = branchId !== 0

  const adjust = async (row: DrugStock, qty: number | null) => {
    if (qty === null || qty === row.quantity) return
    try {
      const updated = await drugStockApi.adjust(row.drugId, branchId, qty)
      setData((data ?? []).map((d) => (d.drugId === row.drugId ? updated : d)))
      message.success('Đã cập nhật tồn kho')
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  return (
    <div className="page-card">
      <PageTitle
        title="Tồn kho thuốc"
        extra={
          <Space>
            <ExcelButton url="/drug-stock/export" fileName="ton-kho-thuoc.xlsx" params={params} />
            <Button icon={<ReloadOutlined />} onClick={reload} />
          </Space>
        }
      />
      <div className="toolbar">
        {user?.isAdministrator && <BranchSelect />}
        <Input.Search placeholder="Tìm thuốc" allowClear style={{ width: 240 }} onSearch={setKeyword} />
        <Checkbox checked={onlyExisting} onChange={(e) => setOnlyExisting(e.target.checked)}>
          Chỉ hiện thuốc còn trong kho
        </Checkbox>
      </div>
      <Table<DrugStock>
        size="small"
        rowKey="drugId"
        loading={loading}
        dataSource={data ?? []}
        pagination={{ pageSize: 50, showSizeChanger: true }}
        columns={[
          { title: '#', width: 50, render: (_, __, i) => i + 1 },
          { title: 'Thuốc', dataIndex: 'name' },
          { title: 'Đơn vị', dataIndex: 'unit', width: 90 },
          { title: 'Giá nhập', dataIndex: 'price', width: 110, align: 'right', render: money },
          { title: 'Giá bán', dataIndex: 'outPrice', width: 110, align: 'right', render: money },
          {
            title: branchId === 0 ? 'Tồn kho (tất cả CN)' : 'Tồn kho',
            dataIndex: 'quantity',
            width: 140,
            align: 'right',
            render: (v: number, r) =>
              canEdit ? <InputNumber size="small" min={0} value={v} onBlur={(e) => adjust(r, Number(e.target.value))} onPressEnter={(e) => adjust(r, Number((e.target as HTMLInputElement).value))} style={{ width: 100 }} /> : money(v),
          },
          { title: 'Mô tả', dataIndex: 'description' },
        ]}
      />
    </div>
  )
}
