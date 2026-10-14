import axios, { AxiosError } from 'axios'

export const TOKEN_KEY = 'duchanh.token'
export const USER_KEY = 'duchanh.user'

export const api = axios.create({
  baseURL: '/api',
  timeout: 60000,
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY)
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  (res) => res,
  (error: AxiosError<{ message?: string }>) => {
    if (error.response?.status === 401 && !error.config?.url?.includes('/auth/login')) {
      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem(USER_KEY)
      window.location.href = '/login'
    }
    return Promise.reject(error)
  },
)

/** Extract a Vietnamese error message from an API error. */
export function errorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as { message?: string; title?: string; errors?: Record<string, string[]> } | undefined
    if (data?.message) return data.message
    if (data?.errors) return Object.values(data.errors).flat().join('; ')
    if (data?.title) return data.title
    if (error.code === 'ECONNABORTED') return 'Hết thời gian chờ máy chủ'
    return error.message
  }
  return error instanceof Error ? error.message : 'Lỗi không xác định'
}

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

/** Download a binary response (e.g. Excel export) as a file. */
export async function downloadFile(url: string, fileName: string, params?: Record<string, unknown>) {
  const res = await api.get(url, { params, responseType: 'blob' })
  const blobUrl = URL.createObjectURL(res.data as Blob)
  const a = document.createElement('a')
  a.href = blobUrl
  a.download = fileName
  document.body.appendChild(a)
  a.click()
  a.remove()
  URL.revokeObjectURL(blobUrl)
}
