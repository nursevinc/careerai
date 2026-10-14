import { useCallback, useEffect, useState } from 'react'
import { resumeService } from '../services/resumeService'
import { saveBlob } from '../utils/downloadFile'
import { parseApiError } from '../utils/apiError'
import type { Resume } from '../types/resume'

export function useResumes() {
  const [resumes, setResumes] = useState<Resume[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchResumes = useCallback(async () => {
    try {
      setResumes(await resumeService.getAll())
      setError(null)
    } catch (err) {
      setError(parseApiError(err).message)
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void fetchResumes()
  }, [fetchResumes])

  const reload = useCallback(() => {
    setIsLoading(true)
    void fetchResumes()
  }, [fetchResumes])

  // Aşağıdaki fonksiyonlar hata fırlatabilir: hatayı gösterme işi onları çağıran bileşende.
  const upload = useCallback(async (file: File) => {
    const created = await resumeService.upload(file)
    setResumes((prev) => [created, ...prev])
  }, [])

  const remove = useCallback(async (resume: Resume) => {
    await resumeService.remove(resume.id)
    setResumes((prev) => prev.filter((r) => r.id !== resume.id))
  }, [])

  const download = useCallback(async (resume: Resume) => {
    const blob = await resumeService.download(resume.id)
    saveBlob(blob, resume.fileName)
  }, [])

  const getText = useCallback(async (resume: Resume) => {
    const detail = await resumeService.getById(resume.id)
    return detail.extractedText
  }, [])

  return { resumes, isLoading, error, reload, upload, remove, download, getText }
}