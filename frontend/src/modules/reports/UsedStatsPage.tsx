import { useState } from 'react'
import { Table, Tabs } from 'antd'
import { PageTitle, useLoad } from '../../components/common'
import { money } from '../../utils/format'
import ReportFilterBar, { useReportParams } from './components/ReportFilterBar'
import StatBar from './components/StatBar'
import { reportsApi, type UsedDrugRow, type UsedExportRow, type UsedImportRow } from './api'

type Tab = 'drugs' | 'imports' | 'exports'
const FILE_NAMES: Record<Tab, string> = { drugs: 'thuoc-da-dung.xlsx', imports: 'thiet-bi-da-nhap.xlsx', exports: 'thiet-bi-da-xuat.xlsx' }

/** Thống kê sử dụng: thuốc đã dùng, thiết bị nhập / xuất (UsedStatisticsControl). */
export default function UsedStatsPage() {
  const { range, setRange, params } = useReportParams()
  const [tab, setTab] = useState<Tab>('drugs')
  const { data, loading } = useLoad(() => reportsApi.used(params), [JSON.stringify(params)])
  const no = { title: 'No', key: 'no', width: 50, render: (_: unknown, __: unknown, i: number) => i + 1 }

  return (
    <div className="page-card">
      <PageTitle title="Thống kê sử dụng" />
      <ReportFilterBar range={range} onChange={setRange} exportUrl="/reports/used/export" exportFileName={FILE_NAMES[tab]} exportParams={{ ...params, tab }} />
      <Tabs
        activeKey={tab}
        onChange={(k) => setTab(k as Tab)}
        items={[
          {
            key: 'drugs',
            label: 'Thuốc',
            children: (
              <Table<UsedDrugRow>
                size="small"
                rowKey="id"
                loading={loading}
                dataSource={data?.drugs ?? []}
                pagination={{ pageSize: 50 }}
                columns={[no, { title: 'Tên thuốc', dataIndex: 'name' }, { title: 'Đơn vị', dataIndex: 'unit', width: 100 }, { title: 'Số lượng', dataIndex: 'sum', width: 120, align: 'right' }]}
              />
            ),
          },
          {
            key: 'imports',
            label: 'Thiết bị nhập',
            children: (
              <>
                <StatBar items={[{ label: 'Tổng tiền nhập', value: data?.importTotal ?? 0 }]} />
                <Table<UsedImportRow>
                  size="small"
                  rowKey="id"
                  loading={loading}
                  dataSource={data?.imports ?? []}
                  pagination={{ pageSize: 50 }}
                  columns={[
                    no,
                    { title: 'Tên', dataIndex: 'name' },
                    { title: 'Serial', dataIndex: 'serial', width: 140 },
                    { title: 'Đơn vị', dataIndex: 'unit', width: 90 },
                    { title: 'Số lượng', dataIndex: 'quantity', width: 100, align: 'right' },
                    { title: 'Đơn giá TB', dataIndex: 'average', width: 130, align: 'right', render: money },
                    { title: 'Thành tiền', dataIndex: 'total', width: 140, align: 'right', render: money },
                  ]}
                />
              </>
            ),
          },
          {
            key: 'exports',
            label: 'Thiết bị xuất',
            children: (
              <Table<UsedExportRow>
                size="small"
                rowKey="id"
                loading={loading}
                dataSource={data?.exports ?? []}
                pagination={{ pageSize: 50 }}
                columns={[
                  no,
                  { title: 'Tên', dataIndex: 'name' },
                  { title: 'Serial', dataIndex: 'serial', width: 140 },
                  { title: 'Đơn vị', dataIndex: 'unit', width: 90 },
                  { title: 'Số lượng', dataIndex: 'quantity', width: 100, align: 'right' },
                ]}
              />
            ),
          },
        ]}
      />
    </div>
  )
}
