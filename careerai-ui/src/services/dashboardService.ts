import { api } from './api'
import type { Dashboard } from '../types/dashboard'

export const dashboardService = {
  async get(): Promise<Dashboard> {
    const response = await api.get<Dashboard>('/api/dashboard')
    return response.data
  },
}