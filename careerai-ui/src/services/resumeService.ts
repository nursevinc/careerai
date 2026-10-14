import { api } from './api'
import type { Resume, ResumeDetail } from '../types/resume'

export const resumeService = {
  async getAll(): Promise<Resume[]> {
    const response = await api.get<Resume[]>('/api/resumes')
    return response.data
  },

  async getById(id: string): Promise<ResumeDetail> {
    const response = await api.get<ResumeDetail>(`/api/resumes/${id}`)
    return response.data
  },

  async upload(file: File): Promise<Resume> {
    const formData = new FormData()
    formData.append('file', file) // backend'deki parametre adı: "file"

    const response = await api.post<Resume>('/api/resumes/upload', formData, {
      timeout: 60_000, // dosya yükleme + PDF ayrıştırma için daha geniş süre
    })
    return response.data
  },

  async download(id: string): Promise<Blob> {
    const response = await api.get<Blob>(`/api/resumes/${id}/download`, { responseType: 'blob' })
    return response.data
  },

  async remove(id: string): Promise<void> {
    await api.delete(`/api/resumes/${id}`)
  },
}