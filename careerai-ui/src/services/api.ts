import axios from 'axios'
import { tokenStorage } from '../utils/tokenStorage'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  timeout: 15_000, // 15 sn. Uzun süren isteklerde (dosya yükleme, AI) istek bazında artırıyoruz
})

// Oturum geçersiz olduğunda AuthProvider'ın logout'unu çağırmak için.
// (api.ts'in React'e bağımlı olmaması için callback ile bağlıyoruz.)
let onUnauthorized: (() => void) | null = null
export function setUnauthorizedHandler(handler: (() => void) | null): void {
  onUnauthorized = handler
}

// Her isteğe token'ı otomatik ekle
api.interceptors.request.use((config) => {
  const token = tokenStorage.get()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

const AUTH_PATHS = ['/api/auth/login', '/api/auth/register']

// 401 gelirse oturumu kapat. Ama login/register'daki 401 (yanlış şifre) bir form hatasıdır, oturum sonu değil.
api.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      const url = error.config?.url ?? ''
      if (!AUTH_PATHS.some((path) => url.startsWith(path))) {
        tokenStorage.clear()
        onUnauthorized?.()
      }
    }
    return Promise.reject(error)
  },
)