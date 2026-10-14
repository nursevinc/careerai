import axios from 'axios'
import { useCallback, useEffect, useState } from 'react'
import { analysisService } from '../services/analysisService'
import { jobApplicationService } from '../services/jobApplicationService'
import { parseApiError } from '../utils/apiError'
import type { ParsedApiError } from '../utils/apiError'
import type { Analysis } from '../types/analysis'
import type { JobApplicationDetail } from '../types/jobApplication'

// Bir ilanı ve (varsa) en son analizini yükler.
export function useAnalysis(jobId: string) {
  const [job, setJob] = useState<JobApplicationDetail | null>(null)
  const [analysis, setAnalysis] = useState<Analysis | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<ParsedApiError | null>(null)

  const load = useCallback(async () => {
    try {
      // Önce ilan: yoksa ya da başkasınınsa gerçek "bulunamadı"
      const jobDetail = await jobApplicationService.getById(jobId)

      let latest: Analysis | null = null
      try {
        latest = await analysisService.getLatest(jobId)
      } catch (err) {
        // 404 = bu ilan için henüz analiz yok: hata değil, normal bir durum
        if (!(axios.isAxiosError(err) && err.response?.status === 404)) throw err
      }

      setJob(jobDetail)
      setAnalysis(latest)
      setError(null)
    } catch (err) {
      setError(parseApiError(err))
    } finally {
      setIsLoading(false)
    }
  }, [jobId])

  useEffect(() => {
    void load()
  }, [load])

  const retry = useCallback(() => {
    setIsLoading(true)
    void load()
  }, [load])

  // refresh: ekranı yükleniyor durumuna sokmadan veriyi yeniler
  return { job, analysis, isLoading, error, refresh: load, retry }
}