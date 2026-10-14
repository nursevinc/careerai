import { useCallback, useEffect, useRef, useState } from 'react'
import { analysisService } from '../services/analysisService'
import type { Analysis } from '../types/analysis'

// Analizi başlatır. Hata fırlatabilir: hatayı gösterme işi çağıran bileşende
// (iptal edilmişse isCancelledRequest(error) ile ayırt edilir).
export function useAnalyze() {
  const [isAnalyzing, setIsAnalyzing] = useState(false)
  const controllerRef = useRef<AbortController | null>(null)

  // Kullanıcı sayfadan ayrılırsa devam eden isteği iptal et (gereksiz AI maliyeti olmasın)
  useEffect(() => {
    return () => controllerRef.current?.abort()
  }, [])

  const analyze = useCallback(async (jobApplicationId: string, resumeId: string): Promise<Analysis> => {
    const controller = new AbortController()
    controllerRef.current = controller
    setIsAnalyzing(true)

    try {
      return await analysisService.analyze(jobApplicationId, resumeId, controller.signal)
    } finally {
      if (controllerRef.current === controller) controllerRef.current = null
      setIsAnalyzing(false)
    }
  }, [])

  const cancel = useCallback(() => controllerRef.current?.abort(), [])

  return { analyze, cancel, isAnalyzing }
}