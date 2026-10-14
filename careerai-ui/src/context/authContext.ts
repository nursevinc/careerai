import { createContext } from 'react'
import type { AuthResponse, LoginRequest, RegisterRequest, User } from '../types/auth'

export interface AuthContextValue {
  user: User | null
  isAuthenticated: boolean
  isInitializing: boolean // sayfa yenilenince oturum geri yükleniyor mu?
  login: (data: LoginRequest) => Promise<void>
  register: (data: RegisterRequest) => Promise<void>
  logout: () => void
  // Şifre değişince sunucu yeni token verir: oturumu onunla sürdür
  applyAuthResponse: (response: AuthResponse) => void
  // Profil güncellenince (örn. ad) ekrandaki kullanıcı bilgisini yenile
  updateUser: (user: User) => void
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined)