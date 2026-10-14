import { api } from './api'
import type {
  CreateJobApplicationRequest,
  JobApplication,
  JobApplicationDetail,
} from '../types/jobApplication'

export const jobApplicationService = {
  async create(data: CreateJobApplicationRequest): Promise<JobApplication> {
    const response = await api.post<JobApplication>('/api/job-applications', data)
    return response.data
  },

  async getAll(): Promise<JobApplication[]> {
    const response = await api.get<JobApplication[]>('/api/job-applications')
    return response.data
  },

  async getById(id: string): Promise<JobApplicationDetail> {
    const response = await api.get<JobApplicationDetail>(`/api/job-applications/${id}`)
    return response.data
  },

  async remove(id: string): Promise<void> {
    await api.delete(`/api/job-applications/${id}`)
  },
}