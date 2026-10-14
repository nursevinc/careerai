import { useCallback, useEffect, useState } from 'react'
import { jobApplicationService } from '../services/jobApplicationService'
import { parseApiError } from '../utils/apiError'
import type { JobApplication } from '../types/jobApplication'

export function useJobApplications() {
  const [applications, setApplications] = useState<JobApplication[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchApplications = useCallback(async () => {
    try {
      setApplications(await jobApplicationService.getAll())
      setError(null)
    } catch (err) {
      setError(parseApiError(err).message)
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void fetchApplications()
  }, [fetchApplications])

  const reload = useCallback(() => {
    setIsLoading(true)
    void fetchApplications()
  }, [fetchApplications])

  // Hata fırlatabilir: hatayı gösterme işi çağıran bileşende
  const remove = useCallback(async (application: JobApplication) => {
    await jobApplicationService.remove(application.id)
    setApplications((prev) => prev.filter((a) => a.id !== application.id))
  }, [])

  return { applications, isLoading, error, reload, remove }
}