import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { parseApiError } from '../utils/apiError'
import type { Resume } from '../types/resume'

interface ResumeCardProps {
  resume: Resume
  onDownload: (resume: Resume) => Promise<void>
  onDelete: (resume: Resume) => Promise<void>
  onLoadText: (resume: Resume) => Promise<string>
}

export default function ResumeCard({ resume, onDownload, onDelete, onLoadText }: ResumeCardProps) {
  const { t } = useTranslation()
  const [busy, setBusy] = useState<'download' | 'delete' | null>(null)
  const [confirmingDelete, setConfirmingDelete] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [showText, setShowText] = useState(false)
  const [text, setText] = useState<string | null>(null)
  const [isLoadingText, setIsLoadingText] = useState(false)

  const run = async (action: 'download' | 'delete', work: () => Promise<void>) => {
    setBusy(action)
    setError(null)
    try {
      await work()
    } catch (err) {
      const parsed = parseApiError(err)
      // Silmedeki 409: CV bir analizde kullanılıyor
      setError(action === 'delete' && parsed.status === 409 ? t('resumes.errors.inUse') : parsed.message)
      setConfirmingDelete(false)
    } finally {
      setBusy(null)
    }
  }

  const toggleText = async () => {
    if (showText) {
      setShowText(false)
      return
    }

    setShowText(true)
    if (text !== null) return // daha önce yüklendi

    setIsLoadingText(true)
    setError(null)
    try {
      setText(await onLoadText(resume))
    } catch (err) {
      setError(parseApiError(err).message)
      setShowText(false)
    } finally {
      setIsLoadingText(false)
    }
  }

  const uploadedAt = new Date(resume.createdAt).toLocaleDateString('tr-TR', {
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  })

  return (
    <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex min-w-0 items-center gap-3">
          <svg
            aria-hidden="true"
            viewBox="0 0 24 24"
            className="h-8 w-8 shrink-0 text-red-500"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.5"
          >
            <path d="M14 3H7a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8l-5-5z" />
            <path d="M14 3v5h5" />
          </svg>
          <div className="min-w-0">
            <p className="truncate text-sm font-medium text-slate-900" title={resume.fileName}>
              {resume.fileName}
            </p>
            <p className="text-xs text-slate-500">
              {uploadedAt} ·{' '}
              {t('resumes.list.characters', { chars: resume.extractedTextLength.toLocaleString('tr-TR') })}
            </p>
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <button
            type="button"
            onClick={() => void toggleText()}
            className="rounded-lg px-3 py-1.5 text-sm font-medium text-indigo-600 hover:bg-indigo-50"
          >
            {showText ? t('resumes.list.hideText') : t('resumes.list.showText')}
          </button>

          <button
            type="button"
            disabled={busy !== null}
            onClick={() => void run('download', () => onDownload(resume))}
            className="rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 hover:bg-slate-100 disabled:opacity-60"
          >
            {busy === 'download' ? t('resumes.list.downloading') : t('resumes.list.download')}
          </button>

          {confirmingDelete ? (
            <div className="flex items-center gap-2">
              <span className="text-sm text-slate-600">{t('resumes.list.confirmDelete')}</span>
              <button
                type="button"
                disabled={busy !== null}
                onClick={() => void run('delete', () => onDelete(resume))}
                className="rounded-lg bg-red-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-60"
              >
                {busy === 'delete' ? t('resumes.list.deleting') : t('resumes.list.confirmYes')}
              </button>
              <button
                type="button"
                disabled={busy !== null}
                onClick={() => setConfirmingDelete(false)}
                className="rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 hover:bg-slate-100"
              >
                {t('resumes.list.cancel')}
              </button>
            </div>
          ) : (
            <button
              type="button"
              disabled={busy !== null}
              onClick={() => setConfirmingDelete(true)}
              className="rounded-lg border border-red-200 px-3 py-1.5 text-sm font-medium text-red-600 hover:bg-red-50 disabled:opacity-60"
            >
              {t('resumes.list.delete')}
            </button>
          )}
        </div>
      </div>

      {error && (
        <div role="alert" className="mt-3 rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
          {error}
        </div>
      )}

      {showText && (
        <div className="mt-4 border-t border-slate-100 pt-4">
          <p className="mb-2 text-xs text-slate-500">{t('resumes.list.textInfo')}</p>
          {isLoadingText ? (
            <p className="text-sm text-slate-400">{t('resumes.list.loadingText')}</p>
          ) : (
            // {text}: React içeriği otomatik escape eder. CV metni güvenilmez girdidir, HTML olarak ASLA basılmaz.
            <pre className="max-h-72 overflow-auto whitespace-pre-wrap break-words rounded-lg bg-slate-50 p-3 text-xs text-slate-700">
              {text}
            </pre>
          )}
        </div>
      )}
    </div>
  )
}