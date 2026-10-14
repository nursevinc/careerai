import { useCallback, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { AuthContext } from './authContext'
import type { AuthContextValue } from './authContext'
import { authService } from '../services/authService'
import { setUnauthorizedHandler } from '../services/api'
import { tokenStorage } from '../utils/tokenStorage'
import type { AuthResponse, LoginRequest, RegisterRequest, User } from '../types/auth'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  // Kayıtlı bir token varsa, kullanıcıyı sunucudan doğrulayana kadar "başlatılıyor" say
  const [isInitializing, setIsInitializing] = useState(() => tokenStorage.get() !== null)

  const logout = useCallback(() => {
    tokenStorage.clear()
    setUser(null)
  }, [])

  // api.ts'deki interceptor 401 görürse logout'u çağırsın
  useEffect(() => {
    setUnauthorizedHandler(logout)
    return () => setUnauthorizedHandler(null)
  }, [logout])

  // Sayfa yenilenince oturumu geri yükle
  useEffect(() => {
    if (tokenStorage.get() === null) return

    let cancelled = false
    authService
      .me()
      .then((currentUser) => {
        if (!cancelled) setUser(currentUser)
      })
      .catch(() => tokenStorage.clear()) // token geçersiz: temizle, kullanıcı login'e düşer
      .finally(() => {
        if (!cancelled) setIsInitializing(false)
      })

    return () => {
      cancelled = true
    }
  }, [])

  const applyAuthResponse = useCallback((response: AuthResponse) => {
    tokenStorage.set(response.accessToken, response.expiresAtUtc)
    setUser(response.user)
  }, [])

  const login = useCallback(
    async (data: LoginRequest) => applyAuthResponse(await authService.login(data)),
    [applyAuthResponse],
  )

  const register = useCallback(
    async (data: RegisterRequest) => applyAuthResponse(await authService.register(data)),
    [applyAuthResponse],
  )

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isAuthenticated: user !== null,
      isInitializing,
      login,
      register,
      logout,
      applyAuthResponse,
      updateUser: setUser,
    }),
    [user, isInitializing, login, register, logout, applyAuthResponse],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}