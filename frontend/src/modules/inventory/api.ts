import { api } from '../../api/client'

// ---- shared types -------------------------------------------------------
export interface ListResult<T> {
  items: T[]
  total: number
  count: number
}

export interface DrugLookup {
  id: number
  name: string
  unit: string
  price: number
  outPrice: number
  description?: string | null
}

export interface EquipmentLookup {
  id: number
  name: string
  serial: string
  unit?: string | null
  price: number
  isVarious: boolean
  vendor: string
  position?: string | null
  usage?: string | null
  note?: string | null
  quantity: number
}

export interface InvoiceLineInput {
  itemId: number
  quantity: number
  price: number
}

export interface DateFilter {
  from?: string
  to?: string
}

export interface ListParams extends DateFilter {
  branchId?: number
  limit?: number
  isVarious?: boolean
  keyword?: string
}

// ---- drug invoices ------------------------------------------------------
export interface DrugInvoice {
  id: number
  createdDate: string
  userName: string
  fullName?: string | null
  branchId: number
  branchName: string
  quantity: number
  total: number
}
export interface DrugInvoiceLine {
  drugId: number
  drugName: string
  unit: string
  price: number
  quantity: number
  total: number
}
export interface DrugInvoiceDetail {
  invoice: DrugInvoice
  lines: DrugInvoiceLine[]
}

export const drugInvoicesApi = {
  list: (params: ListParams) => api.get<ListResult<DrugInvoice>>('/drug-invoices', { params }).then((r) => r.data),
  get: (id: number) => api.get<DrugInvoiceDetail>(`/drug-invoices/${id}`).then((r) => r.data),
  create: (body: { createdDate?: string; branchId?: number; lines: InvoiceLineInput[] }) =>
    api.post<DrugInvoiceDetail>('/drug-invoices', body).then((r) => r.data),
  remove: (id: number) => api.delete(`/drug-invoices/${id}`),
  lookupDrugs: (keyword?: string) => api.get<DrugLookup[]>('/drug-invoices/lookup/drugs', { params: { keyword } }).then((r) => r.data),
}

// ---- drug receipts (xuất thuốc khi khám) ---------------------------------
export interface DrugReceipt {
  prescriptionId: number
  appointmentNo: number
  createdDate: string
  customerId: number
  customerName: string
  customerVisiableId: number
  branchId: number
  branchName: string
  quantity: number
  total: number
}
export interface DrugReceiptLine {
  drugId: number
  drugName: string
  unit: string
  price: number
  quantity: number
  total: number
}
export const drugReceiptsApi = {
  list: (params: ListParams) => api.get<ListResult<DrugReceipt>>('/drug-receipts', { params }).then((r) => r.data),
  get: (prescriptionId: number, appointmentNo: number) =>
    api.get<{ receipt: DrugReceipt; lines: DrugReceiptLine[] }>(`/drug-receipts/${prescriptionId}/${appointmentNo}`).then((r) => r.data),
}

// ---- drug stock ---------------------------------------------------------
export interface DrugStock {
  drugId: number
  name: string
  unit: string
  price: number
  outPrice: number
  description?: string | null
  branchId: number
  quantity: number
}
export const drugStockApi = {
  list: (params: { branchId?: number; keyword?: string; onlyExisting?: boolean; limit?: number }) =>
    api.get<DrugStock[]>('/drug-stock', { params }).then((r) => r.data),
  adjust: (drugId: number, branchId: number, quantity: number) =>
    api.put<DrugStock>(`/drug-stock/${drugId}`, { branchId, quantity }).then((r) => r.data),
}

// ---- equipment invoices -------------------------------------------------
export interface EquipmentInvoice {
  id: number
  createdDate: string
  branchId: number
  branchName: string
  quantity: number
  total: number
}
export interface EquipmentInvoiceLine {
  equipmentId: number
  equipmentName: string
  serial: string
  equipmentType: string
  vendor: string
  unit?: string | null
  price: number
  quantity: number
  total: number
}
export const equipmentInvoicesApi = {
  list: (params: ListParams) => api.get<ListResult<EquipmentInvoice>>('/equipment-invoices', { params }).then((r) => r.data),
  get: (id: number) => api.get<{ invoice: EquipmentInvoice; lines: EquipmentInvoiceLine[] }>(`/equipment-invoices/${id}`).then((r) => r.data),
  create: (body: { createdDate?: string; branchId?: number; lines: InvoiceLineInput[] }) =>
    api.post<{ invoice: EquipmentInvoice; lines: EquipmentInvoiceLine[] }>('/equipment-invoices', body).then((r) => r.data),
  remove: (id: number) => api.delete(`/equipment-invoices/${id}`),
  lookupEquipment: (params: { isVarious?: boolean; keyword?: string; branchId?: number }) =>
    api.get<EquipmentLookup[]>('/equipment-invoices/lookup/equipment', { params }).then((r) => r.data),
}

// ---- equipment exports --------------------------------------------------
export interface ExportInvoice {
  id: number
  createdDate: string
  branchId: number
  branchName: string
  quantity: number
}
export interface ExportInvoiceLine {
  equipmentId: number
  equipmentName: string
  serial: string
  equipmentType: string
  vendor: string
  unit?: string | null
  quantity: number
}
export const equipmentExportsApi = {
  list: (params: ListParams) => api.get<ListResult<ExportInvoice>>('/equipment-exports', { params }).then((r) => r.data),
  get: (id: number) => api.get<{ invoice: ExportInvoice; lines: ExportInvoiceLine[] }>(`/equipment-exports/${id}`).then((r) => r.data),
  create: (body: { createdDate?: string; branchId?: number; lines: { equipmentId: number; quantity: number }[] }) =>
    api.post<{ invoice: ExportInvoice; lines: ExportInvoiceLine[] }>('/equipment-exports', body).then((r) => r.data),
  remove: (id: number) => api.delete(`/equipment-exports/${id}`),
  lookupExisting: (params: { isVarious?: boolean; keyword?: string; searchBy?: string; branchId?: number }) =>
    api.get<EquipmentLookup[]>('/equipment-exports/lookup/equipment', { params }).then((r) => r.data),
}

// ---- equipment stock ----------------------------------------------------
export interface EquipmentStock {
  id: number
  name: string
  serial: string
  unit?: string | null
  price: number
  isVarious: boolean
  equipmentType: string
  vendor: string
  position?: string | null
  usage?: string | null
  note?: string | null
  branchId: number
  quantity: number
}
export const equipmentStockApi = {
  list: (params: { branchId?: number; isVarious?: boolean; keyword?: string; searchBy?: string; onlyExisting?: boolean; limit?: number }) =>
    api.get<EquipmentStock[]>('/equipment-stock', { params }).then((r) => r.data),
  adjust: (equipmentId: number, branchId: number, quantity: number) =>
    api.put<EquipmentStock>(`/equipment-stock/${equipmentId}`, { branchId, quantity }).then((r) => r.data),
}
