import type { ReactNode } from 'react'
import { PageTitle, useLoad } from '../../../components/common'
import ReportFilterBar, { useReportParams } from './ReportFilterBar'
import PivotTable from './PivotTable'
import type { PivotResult, RangeParams } from '../api'

interface Props<E extends Record<string, unknown>> {
  title: string
  firstColumn: string
  exportUrl: string
  exportFileName: string
  loader: (p: RangeParams & E) => Promise<PivotResult>
  /** Extra query params (e.g. { mode }) merged into loader/export params. */
  extra?: E
  /** Extra filter controls rendered in the toolbar. */
  controls?: ReactNode
}

/** Shared page for the date × (doctor | branch) pivot reports. */
export default function PivotReportPage<E extends Record<string, unknown>>({ title, firstColumn, exportUrl, exportFileName, loader, extra, controls }: Props<E>) {
  const { range, setRange, params } = useReportParams()
  const full = { ...params, ...(extra ?? ({} as E)) } as RangeParams & E
  const { data, loading } = useLoad(() => loader(full), [JSON.stringify(full)])
  return (
    <div className="page-card">
      <PageTitle title={title} />
      <ReportFilterBar range={range} onChange={setRange} exportUrl={exportUrl} exportFileName={exportFileName} exportParams={full}>
        {controls}
      </ReportFilterBar>
      <PivotTable data={data} loading={loading} firstColumn={firstColumn} />
    </div>
  )
}
