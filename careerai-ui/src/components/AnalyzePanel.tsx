import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAnalyze } from '../hooks/useAnalyze'
import { useResumes } from '../hooks/useResumes'
import { isCancelledRequest, parseApiError } from '../utils/apiError'
import AnalyzingScreen from './AnalyzingScreen'
import ResumeSelect from './ResumeSelect'

interface AnalyzePanelProps {
  jobId: string
  defaultResumeId?: string
  variant: 'first' | 'refresh'
  onCompleted: () => void
}

// Mevcut bir ilan için CV seçip analizi başlatır / yeniler
export default function AnalyzePanel({ jobId, defaultResumeId, variant, onCompleted }: AnalyzePanelProps) {
  const { t } = useTranslation()
  const { resumes, isLoading, error: loadError, reload } = useResumes()
  const { analyze, cancel, isAnalyzing } = useAnalyze()
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  // Kullanıcı seçmediyse: varsayılan CV (önceki analizdeki), o yoksa en yeni CV
  const fallbackId = resumes.find((resume) => resume.id === defaultResumeId)?.id ?? resumes[0]?.id ?? ''
  const resumeId = selectedId ?? fallbackId

  const handleAnalyze = async () => {
    if (!resumeId) return
    setError(null)

    try {
      await analyze(jobId, resumeId)
      onCompleted()
    } catch (err) {
      if (isCancelledRequest(err)) return
      setError(parseApiError(err).message)
    }
  }

  if (isLoading) {
    return (
      <div className="flex justify-center py-4" role="status">
        <div className="h-6 w-6 animate-spin rounded-full border-4 border-slate-300 border-t-indigo-600" />
      </div>
    )
  }

  if (loadError) {
    return (
      <div role="alert" className="rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-700">
        <p>{loadError}</p>
        <button type="button" onClick={reload} className="mt-2 font-medium underline">
          {t('common.retry')}
        </button>
      </div>
    )
  }

  if (resumes.length === 0) {
    return (
      <div className="rounded-lg border border-slate-200 bg-slate-50 p-4 text-sm text-slate-600">
        <p>{t('analysis.noResumes')}</p>
        <Link to="/resumes" className="mt-2 inline-block font-medium text-indigo-600 hover:text-indigo-700">
          {t('analysis.uploadResume')}
        </Link>
      </div>
    )
  }

  return (
    <>
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end">
        <div className="flex-1">
          <ResumeSelect
            id={`resume-select-${variant}`}
            resumes={resumes}
            value={resumeId}
            onChange={setSelectedId}
            disabled={isAnalyzing}
          />
        </div>
        <button
          type="button"
          disabled={!resumeId || isAnalyzing}
          onClick={() => void handleAnalyze()}
          className="rounded-lg bg-indigo-600 px-5 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-60"
        >
          {variant === 'first' ? t('analysis.startAnalysis') : t('analysis.reanalyze')}
        </button>
      </div>

      {error && (
        <div role="alert" className="mt-3 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {isAnalyzing && <AnalyzingScreen onCancel={cancel} />}
    </>
  )
}