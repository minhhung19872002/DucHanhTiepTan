import { api, type PagedResult } from '../../api/client'

// ----- Customers -----------------------------------------------------------

export interface CustomerRow {
  id: number
  visiableId: number
  formattedId: string
  name: string
  sex: number
  birthYear: number
  address?: string | null
  provinceName?: string | null
  districtName?: string | null
  wardName?: string | null
  addressRepresentation: string
  createdDate: string
  phone?: string | null
  email?: string | null
  job?: string | null
  branchId: number
  branchName: string
  hasImage: boolean
}

export interface CustomerMoreInformation {
  whyYouSeeMe: number
  theReasonYouSeeMe?: string | null
  subcribeByPhone: boolean
  phoneToSubcribe?: string | null
  subcribeByEmail: boolean
  emailToSubcribe?: string | null
  subcribeByMail: boolean
  mailToSubcribe?: string | null
  yesQuestions: number
}

export interface CustomerDetail extends CustomerRow {
  age: number
  times: number
  moreInformation?: CustomerMoreInformation | null
}

export interface CustomerInput {
  visiableId?: number | null
  name: string
  sex: number
  birthYear: number
  address?: string | null
  phone?: string | null
  email?: string | null
  job?: string | null
  createdDate?: string | null
  branchId?: number | null
  provinceName?: string | null
  districtName?: string | null
  wardName?: string | null
  moreInformation?: CustomerMoreInformation | null
  imageChanged: boolean
  imageBase64?: string | null
}

export interface CustomerQuery {
  keyword?: string
  searchBy?: 'name' | 'id' | 'phone' | 'job'
  branchId?: number
  dateMode?: 'all' | 'today' | 'custom'
  from?: string
  to?: string
  province?: string
  district?: string
  ward?: string
  page?: number
  pageSize?: number
}

export interface YesNoQuestion {
  id: number
  header: string
}

export const customersApi = {
  list: (params: CustomerQuery) => api.get<PagedResult<CustomerRow>>('/customers', { params }).then((r) => r.data),
  get: (id: number) => api.get<CustomerDetail>(`/customers/${id}`).then((r) => r.data),
  create: (input: CustomerInput) => api.post<CustomerDetail>('/customers', input).then((r) => r.data),
  update: (id: number, input: CustomerInput) => api.put(`/customers/${id}`, input),
  remove: (id: number) => api.delete(`/customers/${id}`),
  nextId: (branchId?: number) => api.get<number>('/customers/next-id', { params: { branchId } }).then((r) => r.data),
  questions: () => api.get<YesNoQuestion[]>('/customers/questions').then((r) => r.data),
  jobs: () => api.get<string[]>('/customers/jobs').then((r) => r.data),
  photoUrl: (id: number) => `/api/customers/${id}/photo`,
}

// ----- Address --------------------------------------------------------------

export const addressApi = {
  provinces: () => api.get<string[]>('/address/provinces').then((r) => r.data),
  districts: (province: string) => api.get<string[]>('/address/districts', { params: { province } }).then((r) => r.data),
  wards: (province: string, district: string) => api.get<string[]>('/address/wards', { params: { province, district } }).then((r) => r.data),
}

// ----- Prescriptions (history) ---------------------------------------------

export interface PrescriptionRow {
  id: number
  branchId: number
  branchName: string
  customerId: number
  customerName: string
  customerVisiableId: number
  customerPhone?: string | null
  hasDone: boolean
  treatment?: string | null
  diagnostic?: string | null
  symptoms?: string | null
  createdDate: string
  lastAppointmentDate: string
  lastNextAppointmentDate?: string | null
  lastAppointmentReason?: string | null
  appointmentAdvised?: string | null
  appointmentNo: number
  total: number
  paid: number
  remain: number
}

export interface PrescriptionQuery {
  branchId?: number
  customerId?: number
  customerKeyword?: string
  customerSearchBy?: 'name' | 'customerId' | 'prescriptionId'
  diseaseKeyword?: string
  diseaseField?: 'symptoms' | 'treatment' | 'diagnostic'
  createdFrom?: string
  createdTo?: string
  lastFrom?: string
  lastTo?: string
  page?: number
  pageSize?: number
}

export const prescriptionsApi = {
  list: (params: PrescriptionQuery) => api.get<PagedResult<PrescriptionRow>>('/prescriptions', { params }).then((r) => r.data),
  get: (id: number) => api.get<PrescriptionRow>(`/prescriptions/${id}`).then((r) => r.data),
  remove: (id: number) => api.delete(`/prescriptions/${id}`),
}

// ----- Appointments ----------------------------------------------------------

export interface AppointmentRow {
  prescriptionId: number
  no: number
  branchId: number
  branchName: string
  createdDate: string
  hasDone: boolean
  appointmentDate?: string | null
  appointmentReason?: string | null
  advised?: string | null
  customerId: number
  customerVisiableId: number
  customerName: string
  customerPhone?: string | null
  doctorName?: string | null
  doctorFullName?: string | null
  nextAppointmentNo?: number | null
  paid: number
  isAppointed: boolean
  status: string
}

export interface AppointmentQuery {
  branchId?: number
  dateMode?: 'today' | 'date' | 'range'
  date?: string
  from?: string
  to?: string
  status?: 'all' | 'appointing' | 'appointed'
  keyword?: string
  searchBy?: 'name' | 'customerId' | 'prescriptionId'
  page?: number
  pageSize?: number
}

export interface DashboardItem {
  prescriptionId: number
  no: number
  branchName: string
  createdDate: string
  customerId: number
  visiableId: number
  customerName: string
  sex: number
  birthYear: number
  address?: string | null
  phone?: string | null
  content: string
  total: number
  totalIfNotDiscount: number
  paid: number
  remain: number
  finished: boolean
  doctorFullName?: string | null
  comment: string
  isNew: boolean
}

export interface DashboardResult {
  items: DashboardItem[]
  total: number
  totalIfNotDiscount: number
  paid: number
  remain: number
}

export interface DashboardQuery {
  branchId?: number
  dateMode?: 'all' | 'today' | 'yesterday' | 'day' | 'month' | 'range'
  date?: string
  month?: string
  from?: string
  to?: string
  limit?: number
}

export const appointmentsApi = {
  list: (params: AppointmentQuery) => api.get<PagedResult<AppointmentRow>>('/appointments', { params }).then((r) => r.data),
  get: (prescriptionId: number, no: number) => api.get<AppointmentRow>(`/appointments/${prescriptionId}/${no}`).then((r) => r.data),
  examine: (prescriptionId: number) =>
    api.post<{ prescriptionId: number; no: number; customerName: string }>(`/appointments/${prescriptionId}/examine`).then((r) => r.data),
  cancel: (prescriptionId: number, no: number) => api.post(`/appointments/${prescriptionId}/${no}/cancel`),
  changeDate: (prescriptionId: number, no: number, appointmentDate: string) =>
    api.put(`/appointments/${prescriptionId}/${no}/date`, { appointmentDate }),
  remove: (prescriptionId: number, no: number, cancelAppointmentElse: boolean) =>
    api.delete(`/appointments/${prescriptionId}/${no}`, { params: { cancelAppointmentElse } }),
  dashboard: (params: DashboardQuery) => api.get<DashboardResult>('/appointments/dashboard', { params }).then((r) => r.data),
}
