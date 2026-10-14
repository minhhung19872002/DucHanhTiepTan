import { useEffect, useState } from 'react'
import { Select, Space } from 'antd'
import { addressApi } from './api'

export interface AddressValue {
  provinceName?: string | null
  districtName?: string | null
  wardName?: string | null
}

let provinceCache: string[] | null = null

/**
 * Cascading Tỉnh / Huyện / Xã selects (port of the ProvinceCollector combos).
 * Values not present in the lookup tables (legacy data) are still shown as options.
 */
export default function AddressPicker({
  value,
  onChange,
  disabled,
  allowClear = true,
  width = 180,
}: {
  value: AddressValue
  onChange: (v: AddressValue) => void
  disabled?: boolean
  allowClear?: boolean
  width?: number
}) {
  const [provinces, setProvinces] = useState<string[]>(provinceCache ?? [])
  const [districts, setDistricts] = useState<string[]>([])
  const [wards, setWards] = useState<string[]>([])

  useEffect(() => {
    if (provinceCache) return
    addressApi.provinces().then((p) => {
      provinceCache = p
      setProvinces(p)
    })
  }, [])

  useEffect(() => {
    if (!value.provinceName) {
      setDistricts([])
      return
    }
    addressApi.districts(value.provinceName).then(setDistricts).catch(() => setDistricts([]))
  }, [value.provinceName])

  useEffect(() => {
    if (!value.provinceName || !value.districtName) {
      setWards([])
      return
    }
    addressApi.wards(value.provinceName, value.districtName).then(setWards).catch(() => setWards([]))
  }, [value.provinceName, value.districtName])

  const withCurrent = (list: string[], current?: string | null) =>
    (current && !list.includes(current) ? [current, ...list] : list).map((n) => ({ value: n, label: n }))

  return (
    <Space wrap>
      <Select
        showSearch
        allowClear={allowClear}
        placeholder="Tỉnh"
        style={{ width }}
        disabled={disabled}
        value={value.provinceName ?? undefined}
        options={withCurrent(provinces, value.provinceName)}
        onChange={(v) => onChange({ provinceName: v ?? null, districtName: null, wardName: null })}
      />
      <Select
        showSearch
        allowClear={allowClear}
        placeholder="Huyện"
        style={{ width }}
        disabled={disabled || !value.provinceName}
        value={value.districtName ?? undefined}
        options={withCurrent(districts, value.districtName)}
        onChange={(v) => onChange({ ...value, districtName: v ?? null, wardName: null })}
      />
      <Select
        showSearch
        allowClear={allowClear}
        placeholder="Xã"
        style={{ width }}
        disabled={disabled || !value.districtName}
        value={value.wardName ?? undefined}
        options={withCurrent(wards, value.wardName)}
        onChange={(v) => onChange({ ...value, wardName: v ?? null })}
      />
    </Space>
  )
}
