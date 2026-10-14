import { api } from '../../api/client'

// ---------- lookups ----------
export interface ServiceLookup { id: number; name: string; price: number; groupId: number; groupName?: string | null }
export interface DrugLookup { id: number; name: string; unit: string; price: number; outPrice: number }
export interface EquipmentLookup { id: number; name: string; serial: string; unit?: string | null; vendor: string; position?: string | null; usage?: string | null; note?: string | null; stockQuantity: number }
export interface IdName { id: number; name: string }
export interface EmployeeLookup { userName: string; fullName: string }
export interface Lookups {
  services: ServiceLookup[]
  drugs: DrugLookup[]
  equipment: EquipmentLookup[]
  laboTypes: IdName[]
  laboVendors: IdName[]
  doctors: EmployeeLookup[]
  assistants: EmployeeLookup[]
}

export interface InProgressItem {
  prescriptionId: number
  appointmentNo: number
  createdDate: string
  customerId: number
  customerName: string
  formattedId: string
  customerBirthYear: number
  customerSex: string
  doctorName?: string | null
  doctorFullName?: string | null
  branchId: number
}

export interface Labo {
  laboVendorId?: number | null
  laboTypeId?: number | null
  color?: string | null
  receiverDate?: string | null
  deliverDate?: string | null
  delivered: boolean
  price: number
}

export interface Consulting {
  subId: number
  serviceId: number
  originalPrice: number
  price: number
  createdDate: string
  comment?: string | null
  quantity: number
  toothPositions?: string | null
  labo?: Labo | null
}

export interface Treatment {
  subId: number
  appointmentNo: number
  serviceId: number
  originalPrice: number
  price: number
  createdDate: string
  paid: number
  doctorName?: string | null
  assistantName?: string | null
  comment?: string | null
  quantity: number
  toothPositions?: string | null
  previousDebt: number
  isCancelled: boolean
  labo?: Labo | null
  laboId?: number | null
}

export interface Receipt { drugId: number; amount: number; instruction?: string | null; perDay: number; perTimes: number }
export interface UsedEquipment { equipmentId: number; quantity: number }
export interface DiscountItem { serviceId: number; serviceName?: string | null; value: number }
export interface Payment { id: number; createdDate: string; amount: number; receptionistName: string }

export interface MembershipInfo {
  id: string
  createdDate: string
  expiredDate?: string | null
  description?: string | null
  groupId: number
  groupName: string
  isFixed: boolean
  fixedValue: number
  discounts: DiscountItem[]
}

export interface ExaminationDetail {
  customer: { id: number; name: string; formattedId: string; sex: number; birthYear: number; address?: string | null; phone?: string | null; branchId: number }
  branch: { id: number; name: string; address?: string | null; tel?: string | null }
  prescription: {
    id: number; customerId: number; hasDone: boolean; treatment?: string | null; diagnostic?: string | null; symptoms?: string | null
    createdDate: string; membershipId?: string | null; discountValue: number; discountType: 'normal' | 'free' | 'discount'; discounts: DiscountItem[]; remain: number
  }
  appointment: {
    prescriptionId: number; no: number; createdDate: string; hasDone: boolean; appointmentDate?: string | null
    appointmentReason?: string | null; advised?: string | null; doctorName?: string | null; maxNo: number
  }
  consultings: Consulting[]
  treatments: Treatment[]
  elderTreatments: Treatment[]
  previousDebt: number
  receipts: Receipt[]
  usedEquipments: UsedEquipment[]
  pictureIds: number[]
  payments: Payment[]
  membership?: MembershipInfo | null
  isSuperUser: boolean
}

export type ExamStatus = 'notyet' | 'appoint' | 'done'

export interface SaveRequest {
  diagnostic?: string | null
  symptoms?: string | null
  treatment?: string | null
  prescriptionCreatedDate?: string | null
  doctorName?: string | null
  appointmentCreatedDate?: string | null
  status: ExamStatus
  appointmentDate?: string | null
  appointmentReason?: string | null
  advised?: string | null
  consultings: Consulting[]
  treatments: Treatment[]
  receipts: Receipt[]
  usedEquipments: UsedEquipment[]
  discount?: { type: 'normal' | 'free' | 'discount'; membershipId?: string | null } | null
}

export interface PrescriptionSummary { id: number; customerId: number; customerName: string; createdDate: string; hasDone: boolean; appointmentNos: number[] }

const base = '/examination'

export const examApi = {
  lookups: (branchId?: number) => api.get<Lookups>(`${base}/lookups`, { params: { branchId } }).then((r) => r.data),
  inProgress: (branchId?: number) => api.get<InProgressItem[]>(`${base}/in-progress`, { params: { branchId } }).then((r) => r.data),
  start: (customerId: number) => api.post<{ prescriptionId: number; appointmentNo: number }>(`${base}/start`, { customerId }).then((r) => r.data),
  nextAppointment: (prescriptionId: number) =>
    api.post<{ prescriptionId: number; appointmentNo: number }>(`${base}/${prescriptionId}/appointments`).then((r) => r.data),
  remove: (prescriptionId: number, appointmentNo: number, cancelAppointmentElse: boolean) =>
    api.delete(`${base}/${prescriptionId}/${appointmentNo}`, { params: { cancelAppointmentElse } }),
  detail: (prescriptionId: number, appointmentNo: number) => api.get<ExaminationDetail>(`${base}/${prescriptionId}/${appointmentNo}`).then((r) => r.data),
  save: (prescriptionId: number, appointmentNo: number, body: SaveRequest) => api.put(`${base}/${prescriptionId}/${appointmentNo}`, body),
  cancelService: (prescriptionId: number, subId: number) => api.post(`${base}/${prescriptionId}/treatments/${subId}/cancel`),
  membership: (id: string) => api.get<MembershipInfo>(`${base}/membership/${encodeURIComponent(id)}`).then((r) => r.data),
  summary: (id: number) => api.get<PrescriptionSummary>(`${base}/prescriptions/${id}`).then((r) => r.data),
  addPicture: (prescriptionId: number, appointmentNo: number, file: File) => {
    const form = new FormData()
    form.append('file', file)
    return api.post<{ id: number }>(`${base}/${prescriptionId}/${appointmentNo}/pictures`, form).then((r) => r.data)
  },
  deletePicture: (id: number) => api.delete(`${base}/pictures/${id}`),
  pictureBlob: (id: number) => api.get(`${base}/pictures/${id}`, { responseType: 'blob' }).then((r) => r.data as Blob),
}

/** Client-side mirror of PositionsAdapter: number of teeth implied by "11,12,21-23" (0 when empty/invalid). */
export function teethCount(value?: string | null): number {
  if (!value || !value.trim()) return 0
  let total = 0
  for (const raw of value.split(',')) {
    const p = raw.trim()
    if (!p) continue
    if (p.includes('-')) {
      const [a, b] = p.split('-').map((s) => s.trim())
      if (a?.length !== 2 || b?.length !== 2) return 0
      const qa = Number(a[0]), ra = Number(a[1]), qb = Number(b[0]), rb = Number(b[1])
      if ([qa, ra, qb, rb].some((n) => Number.isNaN(n))) return 0
      total += qa === qb ? Math.abs(ra - rb) + 1 : ra + rb
    } else {
      if (p.length !== 2 || Number.isNaN(Number(p))) return 0
      total += 1
    }
  }
  return total
}

export type Ratio = (serviceId: number) => number

/** TreatmentDiscount.ValueOf: multiplier applied to the original price. */
export function makeRatio(type: 'normal' | 'free' | 'discount', membership?: MembershipInfo | null, items?: DiscountItem[]): Ratio {
  if (type === 'free') return () => 0
  if (type === 'discount' && membership) {
    if (membership.isFixed) return () => 1 - membership.fixedValue
    const list = items && items.length ? items : membership.discounts
    return (serviceId) => {
      const d = list.find((x) => x.serviceId === serviceId)
      return d ? 1 - d.value : 1
    }
  }
  if (type === 'discount' && items && items.length) {
    return (serviceId) => {
      const d = items.find((x) => x.serviceId === serviceId)
      return d ? 1 - d.value : 1
    }
  }
  return () => 1
}

export function discountedPrice(originalPrice: number, serviceId: number, ratio: Ratio): number {
  return Math.round(originalPrice * ratio(serviceId))
}
