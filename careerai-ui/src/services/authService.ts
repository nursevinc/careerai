import { api } from './api'
import type { AuthResponse, LoginRequest, RegisterRequest, User } from '../types/auth'

export const authService = {
  async register(data: RegisterRequest): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/api/auth/register', data)
    return response.data
  },

  async login(data: LoginRequest): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/api/auth/login', data)
    return response.data
  },

  async me(): Promise<User> {
    const response = await api.get<User>('/api/auth/me')
    return response.data
  },
}