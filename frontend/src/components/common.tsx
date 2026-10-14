import { useCallback, useEffect, useState } from 'react'
import { Button, Popconfirm, Select, Typography, message } from 'antd'
import { DeleteOutlined, FileExcelOutlined } from '@ant-design/icons'
import { api, downloadFile, errorMessage } from '../api/client'
import { useAuth } from '../auth/AuthContext'

/** Page heading consistent across modules. */
export function PageTitle({ title, extra }: { title: string; extra?: React.ReactNode }) {
  return (
    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
      <Typography.Title level={4} style={{ margin: 0 }}>
        {title}
      </Typography.Title>
      {extra}
    </div>
  )
}

/** "Lưu Excel" button: downloads an .xlsx produced by the API. */
export function ExcelButton({ url, fileName, params }: { url: string; fileName: string; params?: Record<string, unknown> }) {
  const [loading, setLoading] = useState(false)
  return (
    <Button
      icon={<FileExcelOutlined />}
      loading={loading}
      onClick={async () => {
        setLoading(true)
        try {
          await downloadFile(url, fileName, params)
        } catch (e) {
          message.error(errorMessage(e))
        } finally {
          setLoading(false)
        }
      }}
    >
      Excel
    </Button>
  )
}

export function DeleteButton({ onConfirm, title = 'Xóa dòng này?', disabled }: { onConfirm: () => void | Promise<void>; title?: string; disabled?: boolean }) {
  return (
    <Popconfirm title={title} okText="Xóa" cancelText="Hủy" onConfirm={onConfirm} disabled={disabled}>
      <Button size="small" danger icon={<DeleteOutlined />} disabled={disabled} />
    </Popconfirm>
  )
}

export interface BranchOption {
  id: number
  name: string
}

/** Loads the branch list once (cached in module scope). */
let branchCache: BranchOption[] | null = null
export function useBranches(): BranchOption[] {
  const [branches, setBranches] = useState<BranchOption[]>(branchCache ?? [])
  useEffect(() => {
    if (branchCache) return
    api.get<BranchOption[]>('/branches').then((r) => {
      branchCache = r.data
      setBranches(r.data)
    })
  }, [])
  return branches
}

/**
 * Branch selector bound to the global branch filter. Receptionists see their own branch only (disabled).
 * Pass allowAll to include "Tất cả" (id 0) like the legacy BranchFilter.
 */
export function BranchSelect({ allowAll = true, style }: { allowAll?: boolean; style?: React.CSSProperties }) {
  const { user, branchId, setBranchId } = useAuth()
  const branches = useBranches()
  const options = branches.filter((b) => b.id !== 0).map((b) => ({ value: b.id, label: b.name }))
  if (allowAll) options.unshift({ value: 0, label: 'Tất cả chi nhánh' })
  return <Select style={{ width: 220, ...style }} value={branchId} onChange={setBranchId} options={options} disabled={!user?.isAdministrator} />
}

/** Generic async loader hook: const { data, loading, reload } = useLoad(() => api.get(...).then(r => r.data), [deps]) */
export function useLoad<T>(loader: () => Promise<T>, deps: unknown[]): { data: T | undefined; loading: boolean; reload: () => void; setData: (d: T) => void } {
  const [data, setData] = useState<T>()
  const [loading, setLoading] = useState(false)
  const [tick, setTick] = useState(0)
  useEffect(() => {
    let cancelled = false
    setLoading(true)
    loader()
      .then((d) => {
        if (!cancelled) setData(d)
      })
      .catch((e) => message.error(errorMessage(e)))
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...deps, tick])
  const reload = useCallback(() => setTick((t) => t + 1), [])
  return { data, loading, reload, setData }
}
