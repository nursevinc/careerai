import { api } from './api'
import type { AuthResponse, User } from '../types/auth'

export interface ChangePasswordData {
  currentPassword: string
  newPassword: string
}

export const accountService = {
  async updateProfile(data: { name: string }): Promise<User> {
    const response = await api.put<User>('/api/account/profile', data)
    return response.data
  },

  // Başarılı olursa diğer cihazlardaki oturumlar kapanır; bu cihaz için yeni token döner
  async changePassword(data: ChangePasswordData): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/api/account/change-password', data)
    return response.data
  },

  async deleteAccount(password: string): Promise<void> {
    await api.delete('/api/account', { data: { password } })
  },
}