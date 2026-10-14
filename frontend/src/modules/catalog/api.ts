import { api } from '../../api/client'

// ---------- Drugs ----------
export interface Drug {
  id: number
  name: string
  unit: string
  price: number
  outPrice: number
  description?: string | null
}
export interface DrugInput {
  name: string
  unit: string
  price: number
  outPrice?: number | null
  description?: string | null
  fixedOutPrice?: boolean
}
export const drugsApi = {
  list: (params?: { keyword?: string; fromPrice?: number; toPrice?: number; limit?: number }) => api.get<Drug[]>('/drugs', { params }).then((r) => r.data),
  create: (input: DrugInput) => api.post<Drug>('/drugs', input).then((r) => r.data),
  update: (id: number, input: DrugInput) => api.put<Drug>(`/drugs/${id}`, input).then((r) => r.data),
  remove: (id: number) => api.delete(`/drugs/${id}`),
}

// ---------- Services ----------
export interface ServiceGroup {
  id: number
  name?: string | null
  serviceCount: number
  isBuiltIn: boolean
}
export interface Service {
  id: number
  name: string
  price: number
  description?: string | null
  groupId: number
  groupName?: string | null
}
export interface ServiceInput {
  name: string
  price: number
  description?: string | null
  groupId: number
}
export const serviceGroupsApi = {
  list: () => api.get<ServiceGroup[]>('/service-groups').then((r) => r.data),
  create: (name: string) => api.post<ServiceGroup>('/service-groups', { name }).then((r) => r.data),
  update: (id: number, name: string) => api.put(`/service-groups/${id}`, { name }),
  remove: (id: number) => api.delete(`/service-groups/${id}`),
}
export const servicesApi = {
  list: (params?: { keyword?: string; groupId?: number; fromPrice?: number; toPrice?: number; limit?: number }) =>
    api.get<Service[]>('/services', { params }).then((r) => r.data),
  create: (input: ServiceInput) => api.post<Service>('/services', input).then((r) => r.data),
  update: (id: number, input: ServiceInput) => api.put<Service>(`/services/${id}`, input).then((r) => r.data),
  remove: (id: number) => api.delete(`/services/${id}`),
}

// ---------- Labo ----------
export interface LaboType {
  id: number
  name: string
  description?: string | null
  disassembly: boolean
}
export interface LaboVendor {
  id: number
  name: string
  address?: string | null
  tel?: string | null
  description?: string | null
}
export const laboTypesApi = {
  list: (keyword?: string) => api.get<LaboType[]>('/labo-types', { params: { keyword } }).then((r) => r.data),
  create: (input: Omit<LaboType, 'id'>) => api.post<LaboType>('/labo-types', input).then((r) => r.data),
  update: (id: number, input: Omit<LaboType, 'id'>) => api.put(`/labo-types/${id}`, input),
  remove: (id: number) => api.delete(`/labo-types/${id}`),
}
export const laboVendorsApi = {
  list: (keyword?: string) => api.get<LaboVendor[]>('/labo-vendors', { params: { keyword } }).then((r) => r.data),
  create: (input: Omit<LaboVendor, 'id'>) => api.post<LaboVendor>('/labo-vendors', input).then((r) => r.data),
  update: (id: number, input: Omit<LaboVendor, 'id'>) => api.put(`/labo-vendors/${id}`, input),
  remove: (id: number) => api.delete(`/labo-vendors/${id}`),
}

// ---------- Equipment ----------
export interface Equipment {
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
export type EquipmentInput = Omit<Equipment, 'id' | 'quantity'>
export const equipmentApi = {
  list: (params: { isVarious?: boolean; keyword?: string; searchBy?: 'name' | 'serial' | 'usage'; branchId?: number; showEmpty?: boolean; limit?: number }) =>
    api.get<Equipment[]>('/equipment', { params }).then((r) => r.data),
  create: (input: EquipmentInput) => api.post<Equipment>('/equipment', input).then((r) => r.data),
  update: (id: number, input: EquipmentInput) => api.put<Equipment>(`/equipment/${id}`, input).then((r) => r.data),
  remove: (id: number) => api.delete(`/equipment/${id}`),
}

// ---------- Employees ----------
export interface EmployeeType {
  id: number
  name: string
}
export interface Employee {
  userName: string
  fullName: string
  employeeTypeId: number
  employeeTypeName: string
  branchId: number
  branchName: string
  isValid: boolean
  joinDate: string
  sex: number
  phone?: string | null
  email?: string | null
  address?: string | null
  rank?: string | null
  otherEmployeeTypeName?: string | null
  defaultSalary: number
  defaultRatio?: number | null
  baseLunchSubsidy: number
  basePhoneSubsidy: number
  baseToxicSubsidy: number
  position: string
}
export interface EmployeeInput {
  userName: string
  fullName: string
  employeeTypeId: number
  branchId: number
  joinDate: string
  sex: number
  phone?: string | null
  email?: string | null
  address?: string | null
  rank?: string | null
  otherEmployeeTypeName?: string | null
  defaultSalary: number
  defaultRatio?: number | null
  baseLunchSubsidy: number
  basePhoneSubsidy: number
  baseToxicSubsidy: number
  isValid?: boolean
}
export const EMPLOYEE_TYPE = { DOCTOR: 1, RECEPTIONIST: 2, ASSISTANT: 3, OTHER: 4 } as const
export const employeesApi = {
  list: (params?: { type?: 'doctor' | 'assistant' | 'receptionist' | 'other' | 'all'; branchId?: number; includeInvalid?: boolean; keyword?: string }) =>
    api.get<Employee[]>('/employees', { params }).then((r) => r.data),
  types: () => api.get<EmployeeType[]>('/employees/types').then((r) => r.data),
  create: (input: EmployeeInput) => api.post<Employee>('/employees', input).then((r) => r.data),
  update: (userName: string, input: EmployeeInput) => api.put<Employee>(`/employees/${encodeURIComponent(userName)}`, input).then((r) => r.data),
  setValid: (userName: string, isValid: boolean) => api.post(`/employees/${encodeURIComponent(userName)}/valid`, isValid),
  remove: (userName: string) => api.delete(`/employees/${encodeURIComponent(userName)}`),
}

// ---------- Accounts ----------
export interface Account {
  userName: string
  fullName?: string | null
  isAdministrator: boolean
  branchId: number
  branchName: string
  employeeUserName?: string | null
  employeeFullName?: string | null
}
export interface AccountCreateInput {
  userName: string
  password: string
  fullName?: string | null
  isAdministrator: boolean
  branchId: number
  employeeUserName?: string | null
}
export type AccountUpdateInput = Omit<AccountCreateInput, 'userName' | 'password'>
export const accountsApi = {
  list: (params?: { branchId?: number; keyword?: string }) => api.get<Account[]>('/accounts', { params }).then((r) => r.data),
  create: (input: AccountCreateInput) => api.post<Account>('/accounts', input).then((r) => r.data),
  update: (userName: string, input: AccountUpdateInput) => api.put<Account>(`/accounts/${encodeURIComponent(userName)}`, input).then((r) => r.data),
  resetPassword: (userName: string, newPassword: string) => api.post(`/accounts/${encodeURIComponent(userName)}/reset-password`, { newPassword }),
  remove: (userName: string) => api.delete(`/accounts/${encodeURIComponent(userName)}`),
}

// ---------- Branches ----------
export interface Branch {
  id: number
  name: string
  address?: string | null
  tel?: string | null
}
export type BranchInput = Omit<Branch, 'id'>
export const branchesApi = {
  list: () => api.get<Branch[]>('/branches').then((r) => r.data),
  create: (input: BranchInput) => api.post<Branch>('/branches', input).then((r) => r.data),
  update: (id: number, input: BranchInput) => api.put(`/branches/${id}`, input),
  remove: (id: number) => api.delete(`/branches/${id}`),
}
