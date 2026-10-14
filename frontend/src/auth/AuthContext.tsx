import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react'
import { api, TOKEN_KEY, USER_KEY } from '../api/client'

export interface UserInfo {
  userName: string
  fullName?: string | null
  isAdministrator: boolean
  branchId: number
  branchName: string
}

interface AuthState {
  user: UserInfo | null
  /** Branch currently selected for filtering (admins may choose 0 = "Tất cả"). */
  branchId: number
  setBranchId: (id: number) => void
  login: (userName: string, password: string) => Promise<UserInfo>
  logout: () => void
}

const AuthContext = createContext<AuthState | undefined>(undefined)
const BRANCH_KEY = 'duchanh.branchId'

function readUser(): UserInfo | null {
  try {
    const raw = localStorage.getItem(USER_KEY)
    return raw ? (JSON.parse(raw) as UserInfo) : null
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserInfo | null>(readUser)
  const [branchId, setBranchState] = useState<number>(() => {
    const saved = localStorage.getItem(BRANCH_KEY)
    if (saved !== null) return Number(saved)
    return readUser()?.branchId ?? 0
  })

  const setBranchId = useCallback((id: number) => {
    setBranchState(id)
    localStorage.setItem(BRANCH_KEY, String(id))
  }, [])

  const login = useCallback(async (userName: string, password: string) => {
    const res = await api.post<{ token: string; user: UserInfo }>('/auth/login', { userName, password })
    localStorage.setItem(TOKEN_KEY, res.data.token)
    localStorage.setItem(USER_KEY, JSON.stringify(res.data.user))
    setUser(res.data.user)
    // Receptionists are locked to their branch; admins default to their own branch but may switch.
    setBranchId(res.data.user.branchId)
    return res.data.user
  }, [setBranchId])

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
    localStorage.removeItem(BRANCH_KEY)
    setUser(null)
  }, [])

  const value = useMemo(() => ({ user, branchId, setBranchId, login, logout }), [user, branchId, setBranchId, login, logout])
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthState {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}
