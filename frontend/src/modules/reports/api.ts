import { api } from '../../api/client'

// ---------------------------------------------------------------- shared

/** Date filter of every statistics screen. Both undefined = "Tất cả". */
export type RangeParams = {
  from?: string
  to?: string
  branchId: number
}

export interface PivotColumn {
  key: string
  label: string
}
export interface PivotRow {
  key: string
  label: string
  values: Record<string, number>
  total: number
}
export interface PivotResult {
  columns: PivotColumn[]
  rows: PivotRow[]
  columnTotals: Record<string, number>
  grandTotal: number
}

const get = <T,>(url: string, params?: Record<string, unknown>) => api.get<T>(url, { params }).then((r) => r.data)

// ---------------------------------------------------------------- reports

export const reportsApi = {
  doctorIncome: (p: RangeParams) => get<PivotResult>('/reports/doctor-income', p),
  branchIncome: (p: RangeParams & { mode: 'treatment' | 'payment' }) => get<PivotResult>('/reports/branch-income', p),
  serviceIncome: (p: RangeParams) => get<PivotResult>('/reports/service-income', p),
  laboByDoctor: (p: RangeParams) => get<PivotResult>('/reports/labo-by-doctor', p),
  customerIncome: (p: RangeParams & { province?: string; district?: string; ward?: string }) => get<CustomerIncomeResult>('/reports/customer-income', p),
  doctorExpense: (p: RangeParams) => get<DoctorExpenseResult>('/reports/doctor-expense', p),
  membership: (p: RangeParams & { discount: DiscountKind | 'all'; memberCode?: string }) => get<MembershipResult>('/reports/membership', p),
  used: (p: RangeParams) => get<UsedResult>('/reports/used', p),
  expenses: (p: RangeParams & { mode: ExpenseMode }) => get<ExpenseStatResult>('/reports/expenses', p),
  incomeExpense: (p: RangeParams) => get<IncomeExpenseResult>('/reports/income-expense', p),
  labo: (p: RangeParams) => get<LaboStatResult>('/reports/labo', p),
  provinces: () => get<string[]>('/reports/lookups/provinces'),
  districts: (province: string) => get<string[]>('/reports/lookups/districts', { province }),
  wards: (province: string, district: string) => get<string[]>('/reports/lookups/wards', { province, district }),
}

export interface CustomerIncomeRow {
  branchName: string
  customerId: number
  customerVisiableId: number
  customerName: string
  createdDate: string
  total: number
  paid: number
  remain: number
  prescriptionNo: number
  province?: string | null
  district?: string | null
  ward?: string | null
}
export interface CustomerIncomeResult {
  items: CustomerIncomeRow[]
  total: number
  paid: number
  remain: number
}

export interface DoctorExpenseRow {
  branchName?: string | null
  doctorName: string
  doctorFullName: string
  income: number
  expense: number
  incomeWoExpense: number
}
export interface DoctorExpenseResult {
  items: DoctorExpenseRow[]
  totalIncome: number
  totalExpense: number
  totalIncomeWoExpense: number
}

export type DiscountKind = 'normal' | 'free' | 'discount'
export const DISCOUNT_LABELS: Record<DiscountKind | 'all', string> = { all: 'Tất cả', normal: 'Bình thường', free: 'Miễn phí', discount: 'Khuyến mãi' }

export interface MembershipRow {
  id: number
  branchName: string
  visiableId: number
  customerId: number
  createdDate: string
  customerName: string
  memberCode?: string | null
  discount: DiscountKind
  totalIfNotDiscount: number
  total: number
  paid: number
  remain: number
}
export interface MembershipResult {
  items: MembershipRow[]
  total: number
  totalIfNotDiscount: number
  paid: number
  remain: number
}

export interface UsedDrugRow {
  id: number
  name: string
  unit: string
  sum: number
}
export interface UsedImportRow {
  id: number
  name: string
  serial: string
  unit?: string | null
  quantity: number
  total: number
  average: number
}
export interface UsedExportRow {
  id: number
  name: string
  serial: string
  unit?: string | null
  quantity: number
}
export interface UsedResult {
  drugs: UsedDrugRow[]
  imports: UsedImportRow[]
  exports: UsedExportRow[]
  importTotal: number
}

export type ExpenseMode = 'detail' | 'content' | 'date'
export interface ExpenseStatRow {
  createdDate?: string | null
  branchName?: string | null
  accountantFullName?: string | null
  description?: string | null
  amount: number
}
export interface ExpenseStatResult {
  items: ExpenseStatRow[]
  total: number
  mode: ExpenseMode
}

export interface IncomeExpenseRow {
  createdDate: string
  income: number
  expense: number
  subtract: number
  description?: string | null
}
export interface IncomeExpenseResult {
  items: IncomeExpenseRow[]
  income: number
  expense: number
  incomeWoExpense: number
}

export interface LaboStatRow {
  id: number
  laboName: string
  laboType: string
  quantity: number
  total: number
  averagePrice: number
}
export interface LaboStatResult {
  items: LaboStatRow[]
  totalFixed: number
  quantityFixed: number
  totalDisassembly: number
  quantityDisassembly: number
  totalPrice: number
  totalQuantity: number
}

// ---------------------------------------------------------------- salary (Lương nhân viên)

export interface MonthYearDto {
  year: number
  month: number
  branchId: number
  baseSalary: number
  workDaysInMonth: number
}

/** Editable inputs of one salary line. */
export interface SalaryLineInput {
  userName: string
  rank?: string | null
  ratio?: number | null
  workDays: number
  overtimeInMinute: number
  towHoursOvertimeDays: number
  degreeAllowance: number
  heathAllowance: number
  dayOffAllowance: number
  riskAllowance: number
  retireAllowance: number
  absent: number
  fee: number
  advance: number
  baseLunchSubsidy: number
  basePhoneSubsidy: number
  baseToxicSubsidy: number
}

export interface SalaryLine extends SalaryLineInput {
  fullName: string
  branchName?: string | null
  employeeType?: string | null
  baseSalary: number
  baseSalaryDependOnWorkDays: number
  overtimeRatio: number
  overtimeBonus: number
  lunchSubsidy: number
  phoneSubsidy: number
  toxicSubsidy: number
  income: number
  total: number
}

export interface SalarySheet {
  declared: boolean
  monthYear: MonthYearDto
  items: SalaryLine[]
  totalIncome: number
  totalAdvance: number
  totalReal: number
}

export interface SalarySheetInput {
  year: number
  month: number
  branchId: number
  baseSalary: number
  workDaysInMonth: number
  items: SalaryLineInput[]
}

/** Client-side port of SalaryItemAdapter so edits recompute instantly (server is the source of truth on save). */
export function computeSalaryLine(line: SalaryLine, monthBaseSalary: number, workDaysInMonth: number): SalaryLine {
  const ratio = line.ratio ?? 0
  const ww = workDaysInMonth === 0 ? 0 : line.workDays / workDaysInMonth
  const baseSalary = ratio * monthBaseSalary
  const baseSalaryDependOnWorkDays = baseSalary * ww
  const hourRate = workDaysInMonth === 0 ? 0 : (ratio * monthBaseSalary) / workDaysInMonth / 8
  const overtimeRatio = hourRate * 2 * line.towHoursOvertimeDays
  const overtimeBonus = line.workDays === 0 ? 0 : Math.trunc(line.overtimeInMinute / 60) * hourRate * 2
  const lunchSubsidy = line.baseLunchSubsidy * ww
  const phoneSubsidy = line.basePhoneSubsidy * ww
  const toxicSubsidy = line.baseToxicSubsidy * ww
  const income =
    baseSalaryDependOnWorkDays + overtimeBonus + overtimeRatio + lunchSubsidy + phoneSubsidy + toxicSubsidy +
    line.degreeAllowance + line.heathAllowance + line.dayOffAllowance + line.riskAllowance + line.retireAllowance - line.absent - line.fee
  return { ...line, baseSalary, baseSalaryDependOnWorkDays, overtimeRatio, overtimeBonus, lunchSubsidy, phoneSubsidy, toxicSubsidy, income, total: income - line.advance }
}

export function toSalaryLineInput(l: SalaryLine): SalaryLineInput {
  return {
    userName: l.userName, rank: l.rank ?? null, ratio: l.ratio ?? null, workDays: l.workDays, overtimeInMinute: l.overtimeInMinute,
    towHoursOvertimeDays: l.towHoursOvertimeDays, degreeAllowance: l.degreeAllowance, heathAllowance: l.heathAllowance, dayOffAllowance: l.dayOffAllowance,
    riskAllowance: l.riskAllowance, retireAllowance: l.retireAllowance, absent: l.absent, fee: l.fee, advance: l.advance,
    baseLunchSubsidy: l.baseLunchSubsidy, basePhoneSubsidy: l.basePhoneSubsidy, baseToxicSubsidy: l.baseToxicSubsidy,
  }
}

export const salaryApi = {
  get: (year: number, month: number, branchId: number) => get<SalarySheet>('/salary', { year, month, branchId }),
  declaration: (year: number, month: number, branchId: number) => get<SalarySheet>('/salary/declaration', { year, month, branchId }),
  declare: (input: SalarySheetInput) => api.post<SalarySheet>('/salary', input).then((r) => r.data),
  update: (input: SalarySheetInput) => api.put<SalarySheet>(`/salary/${input.year}/${input.month}/${input.branchId}`, input).then((r) => r.data),
  remove: (year: number, month: number, branchId: number) => api.delete(`/salary/${year}/${month}/${branchId}`),
}

// ---------------------------------------------------------------- doctor salary (Lương bác sĩ)

export interface DoctorDashboard {
  income: number
  expense: number
  equipment: number
  fixedLabo: number
  dynamicLabo: number
  totalExpense: number
  profit: number
  numberOfDoctors: number
  percentProfit: number
  rankProfit: number
}
export interface DoctorLineInput {
  userName: string
  baseSalary: number
  lunchSubsidy: number
  respondSubsidy: number
  advance: number
}
export interface DoctorLine extends DoctorLineInput {
  fullName: string
  income: number
  salary: number
  total: number
}
export interface DoctorSheet {
  declared: boolean
  year: number
  month: number
  branchId: number
  dashboard: DoctorDashboard
  items: DoctorLine[]
  totalSalary: number
  totalAdvance: number
  totalReal: number
}
export interface DoctorSheetInput {
  year: number
  month: number
  branchId: number
  items: DoctorLineInput[]
}

export function computeDoctorLine(l: DoctorLine): DoctorLine {
  const salary = l.income + l.baseSalary + l.respondSubsidy + l.lunchSubsidy
  return { ...l, salary, total: salary - l.advance }
}

export const doctorSalaryApi = {
  get: (year: number, month: number, branchId: number) => get<DoctorSheet>('/doctor-salary', { year, month, branchId }),
  declaration: (year: number, month: number, branchId: number) => get<DoctorSheet>('/doctor-salary/declaration', { year, month, branchId }),
  declare: (input: DoctorSheetInput) => api.post<DoctorSheet>('/doctor-salary', input).then((r) => r.data),
  update: (input: DoctorSheetInput) => api.put<DoctorSheet>(`/doctor-salary/${input.year}/${input.month}/${input.branchId}`, input).then((r) => r.data),
  remove: (year: number, month: number, branchId: number) => api.delete(`/doctor-salary/${year}/${month}/${branchId}`),
}

// ---------------------------------------------------------------- configuration

export interface ConfigurationItem {
  name: string
  value?: string | null
}
export const configurationApi = {
  list: () => get<ConfigurationItem[]>('/configuration'),
  set: (name: string, value: string) => api.put<ConfigurationItem>(`/configuration/${encodeURIComponent(name)}`, { name, value }).then((r) => r.data),
}
