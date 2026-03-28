import { createContext, useContext, useState, useEffect } from 'react'
import * as authApi from '../api/auth'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const saved = localStorage.getItem('nexusgrid_user')
    return saved ? JSON.parse(saved) : null
  })
  const [token, setToken] = useState(() => localStorage.getItem('nexusgrid_token'))
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    if (token) {
      authApi.getMe()
        .then((u) => { setUser(u); localStorage.setItem('nexusgrid_user', JSON.stringify(u)) })
        .catch(() => { logout() })
        .finally(() => setIsLoading(false))
    } else {
      setIsLoading(false)
    }
  }, [])

  const login = async (email, password) => {
    const res = await authApi.login(email, password)
    setToken(res.token)
    setUser(res.user)
    localStorage.setItem('nexusgrid_token', res.token)
    localStorage.setItem('nexusgrid_user', JSON.stringify(res.user))
    return res
  }

  const register = async (email, password, firstName, lastName) => {
    const res = await authApi.register(email, password, firstName, lastName)
    setToken(res.token)
    setUser(res.user)
    localStorage.setItem('nexusgrid_token', res.token)
    localStorage.setItem('nexusgrid_user', JSON.stringify(res.user))
    return res
  }

  const logout = () => {
    setToken(null)
    setUser(null)
    localStorage.removeItem('nexusgrid_token')
    localStorage.removeItem('nexusgrid_user')
  }

  const value = {
    user, token, isLoading,
    isAuthenticated: !!token,
    isAdmin: user?.role === 'Admin',
    login, register, logout,
    refreshUser: async () => {
      const u = await authApi.getMe()
      setUser(u)
      localStorage.setItem('nexusgrid_user', JSON.stringify(u))
    }
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
