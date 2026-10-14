import { useCallback, useEffect, useState } from 'react'
import { dashboardService } from '../services/dashboardService'
import type { Dashboard } from '../types/dashboard'

export function useDashboard() {
  const [data, setData] = useState<Dashboard | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchDashboard = useCallback(async () => {
    setIsLoading(true)
    setError(null)

    try {
      const result = await dashboardService.get()
      setData(result)
    } catch (err: unknown) {
      setError(
        err instanceof Error ? err.message : 'Dashboard verileri yüklenemedi.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void fetchDashboard()
  }, [fetchDashboard])

  return {
    data,
    isLoading,
    error,
    retry: fetchDashboard,
  }
}