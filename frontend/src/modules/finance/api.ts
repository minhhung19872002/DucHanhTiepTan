import { api } from '../../api/client'

// ---------- Debts ("Khách hàng đang nợ") ----------
export interface DebtPrescription {
  id: number
  createdDate: string
  lastAppointmentDate: string
  total: number
  paid: number
  remain: number
  branchName: string
}
export interface DebtCustomer {
  customerId: number
  visiableId: number
  name: string
  phone?: string | null
  totalCost: number
  paid: number
  remain: number
  prescriptions: DebtPrescription[]
}
export interface DebtList {
  customers: DebtCustomer[]
  totalCost: number
  totalPaid: number
  totalRemain: number
}
export interface DebtTreatment {
  prescriptionId: number
  appointmentNo: number
  subId: number
  createdDate: string
  serviceName: string
  quantity: number
  price: number
  positions?: string | null
  doctorName?: string | null
  assistantName?: string | null
  labo?: string | null
  total: number
  paid: number
  remain: number
  isCancelled: boolean
}
export interface DebtTreatmentList {
  items: DebtTreatment[]
  total: number
  paid: number
  remain: number
}

export const debtsApi = {
  list: (params: { branchId?: number; keyword?: string; byId?: boolean }) => api.get<DebtList>('/debts', { params }).then((r) => r.data),
  treatments: (prescriptionId: number) => api.get<DebtTreatmentList>(`/debts/prescriptions/${prescriptionId}/treatments`).then((r) => r.data),
  collect: (prescriptionId: number, amount: number, date?: string) =>
    api.post<{ paymentId: number; paid: number; remain: number }>(`/debts/prescriptions/${prescriptionId}/payments`, { amount, date }).then((r) => r.data),
}

// ---------- Payments ("Lịch sử thu") ----------
export interface Payment {
  id: number
  createdDate: string
  branchName: string
  customerName: string
  customerId: number
  customerVisiableId: number
  amount: number
  prescriptionId: number
  receptionistName: string
  receptionistFullName?: string | null
}
export interface PaymentFilter {
  branchId?: number
  from?: string
  to?: string
  receptionist?: string
  limit?: number
}
export const paymentsApi = {
  list: (params: PaymentFilter) => api.get<{ items: Payment[]; sum: number; count: number }>('/payments', { params }).then((r) => r.data),
  receptionists: (branchId?: number) => api.get<{ userName: string; fullName?: string }[]>('/payments/receptionists', { params: { branchId } }).then((r) => r.data),
}

// ---------- Expenses ("Phiếu chi") ----------
export interface ExpenseLine {
  description: string
  amount: number
}
export interface Expense {
  id: number
  createdDate: string
  accountantName: string
  accountantFullName?: string | null
  branchName: string
  branchId: number
  total: number
}
export interface ExpenseDetail extends Expense {
  items: ExpenseLine[]
}
export interface ExpenseFilter {
  branchId?: number
  from?: string
  to?: string
  limit?: number
}
export const expensesApi = {
  list: (params: ExpenseFilter) => api.get<{ items: Expense[]; total: number; count: number }>('/expenses', { params }).then((r) => r.data),
  get: (id: number) => api.get<ExpenseDetail>(`/expenses/${id}`).then((r) => r.data),
  create: (input: { createdDate?: string; items: ExpenseLine[] }) => api.post<ExpenseDetail>('/expenses', input).then((r) => r.data),
  update: (id: number, input: { createdDate?: string; items: ExpenseLine[] }) => api.put(`/expenses/${id}`, input),
  remove: (id: number) => api.delete(`/expenses/${id}`),
}

// ---------- Labo delivering ("Labo chờ") ----------
export interface LaboItem {
  prescriptionId: number
  appointmentNo: number
  subId: number
  laboVendorId: number
  laboVendor: string
  laboTypeId: number
  laboType: string
  color?: string | null
  receiverDate: string
  deliverDate: string
  delivered: boolean
  doctorName?: string | null
  doctorFullName?: string | null
  serviceName: string
  customerName: string
  customerPhone?: string | null
  customerId: number
  quantity: number
  price: number
  total: number
  branchName: string
}
export interface LaboFilter {
  branchId?: number
  receiverFrom?: string
  receiverTo?: string
  deliverFrom?: string
  deliverTo?: string
  vendorId?: number
  delivered?: boolean
  limit?: number
}
export const laboApi = {
  list: (params: LaboFilter) => api.get<{ items: LaboItem[]; total: number; count: number }>('/labo-delivering', { params }).then((r) => r.data),
  vendors: () => api.get<{ id: number; name: string }[]>('/labo-delivering/vendors').then((r) => r.data),
  update: (item: Pick<LaboItem, 'prescriptionId' | 'appointmentNo' | 'subId'>, input: { delivered?: boolean; receiverDate?: string; deliverDate?: string; price?: number; color?: string }) =>
    api.put(`/labo-delivering/${item.prescriptionId}/${item.appointmentNo}/${item.subId}`, input),
}

// ---------- Memberships ("Mã VIP") ----------
export interface MembershipGroup {
  id: number
  name: string
  branchId: number
  branchName: string
  isFixed: boolean
  percent?: number | null
  description?: string | null
  services: string
  memberCount: number
}
export interface ServiceDiscount {
  serviceId: number
  serviceName: string
  percent: number
}
export interface MembershipGroupDetail {
  id: number
  name: string
  branchId: number
  branchName: string
  isFixed: boolean
  percent?: number | null
  description?: string | null
  discounts: ServiceDiscount[]
}
export interface MembershipGroupInput {
  name: string
  branchId: number
  isFixed: boolean
  percent: number
  description?: string | null
  discounts?: { serviceId: number; percent: number }[]
}
export interface Membership {
  id: string
  createdDate: string
  expiredDate?: string | null
  description?: string | null
  groupId: number
  groupName: string
  isFixed: boolean
  percent?: number | null
  usedCount: number
}
export interface ServiceLookup {
  id: number
  name: string
  price: number
  groupName?: string | null
}
export const membershipGroupsApi = {
  list: () => api.get<MembershipGroup[]>('/membership-groups').then((r) => r.data),
  get: (id: number) => api.get<MembershipGroupDetail>(`/membership-groups/${id}`).then((r) => r.data),
  services: () => api.get<ServiceLookup[]>('/membership-groups/lookups/services').then((r) => r.data),
  create: (input: MembershipGroupInput) => api.post<MembershipGroupDetail>('/membership-groups', input).then((r) => r.data),
  update: (id: number, input: MembershipGroupInput) => api.put(`/membership-groups/${id}`, input),
  remove: (id: number) => api.delete(`/membership-groups/${id}`),
}
export const membershipsApi = {
  list: (params: { groupId?: number; keyword?: string }) => api.get<Membership[]>('/memberships', { params }).then((r) => r.data),
  create: (input: { id: string; createdDate: string; expiredDate?: string; description?: string; groupId: number }) =>
    api.post<Membership>('/memberships', input).then((r) => r.data),
  update: (id: string, input: { createdDate: string; expiredDate?: string; description?: string }) => api.put(`/memberships/${encodeURIComponent(id)}`, input),
  remove: (id: string) => api.delete(`/memberships/${encodeURIComponent(id)}`),
}
