import { api } from './api'
import type { Analysis } from '../types/analysis'

export const analysisService = {
  // Backend'de 3 AI çağrısı sırayla çalışabilir: genel 15 sn yerine uzun zaman aşımı
  async analyze(jobApplicationId: string, resumeId: string, signal?: AbortSignal): Promise<Analysis> {
    const response = await api.post<Analysis>(
      `/api/job-applications/${jobApplicationId}/analyze`,
      { resumeId },
      { timeout: 180_000, signal },
    )
    return response.data
  },

  // İlanın en son analizi. Analiz yoksa 404 döner.
  async getLatest(jobApplicationId: string): Promise<Analysis> {
    const response = await api.get<Analysis>(`/api/job-applications/${jobApplicationId}/analysis`)
    return response.data
  },
}