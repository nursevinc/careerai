import { useTranslation } from 'react-i18next'
import ResumeCard from '../components/ResumeCard'
import ResumeUploader from '../components/ResumeUploader'
import { useResumes } from '../hooks/useResumes'
import { getErrorCodeMessage } from '../utils/apiError'
import { MAX_RESUMES } from '../utils/resumeValidation'

export default function ResumesPage() {
  const { t } = useTranslation()
  const { resumes, isLoading, error, reload, upload, remove, download, getText } = useResumes()

  const limitReached = resumes.length >= MAX_RESUMES

  const renderList = () => {
    if (isLoading) {
      return (
        <div className="flex justify-center py-10" role="status">
          <div className="h-7 w-7 animate-spin rounded-full border-4 border-slate-300 border-t-indigo-600" />
        </div>
      )
    }

    if (error) {
      return (
        <div role="alert" className="rounded-xl border border-red-200 bg-red-50 p-6 text-center">
          <p className="text-sm text-red-700">{t('resumes.loadError')}</p>
          <p className="mt-1 text-xs text-red-600">{error}</p>
          <button
            type="button"
            onClick={reload}
            className="mt-4 rounded-lg border border-red-300 px-4 py-2 text-sm font-medium text-red-700 hover:bg-red-100"
          >
            {t('resumes.retry')}
          </button>
        </div>
      )
    }

    if (resumes.length === 0) {
      return (
        <div className="rounded-xl border border-slate-200 bg-white p-10 text-center">
          <p className="text-sm font-medium text-slate-700">{t('resumes.list.empty')}</p>
          <p className="mt-1 text-sm text-slate-500">{t('resumes.list.emptyHint')}</p>
        </div>
      )
    }

    return (
      <>
        <h2 className="text-sm font-medium text-slate-500">
          {t('resumes.count', { current: resumes.length, max: MAX_RESUMES })}
        </h2>
        <ul className="mt-3 space-y-3">
          {resumes.map((resume) => (
            <li key={resume.id}>
              <ResumeCard resume={resume} onDownload={download} onDelete={remove} onLoadText={getText} />
            </li>
          ))}
        </ul>
      </>
    )
  }

  return (
    <div className="max-w-3xl">
      <h1 className="text-2xl font-semibold text-slate-900">{t('resumes.title')}</h1>
      <p className="mt-1 text-slate-500">{t('resumes.subtitle')}</p>

      <div className="mt-6">
        <ResumeUploader onUpload={upload} disabled={isLoading || limitReached} />
        {limitReached && (
          <p className="mt-2 text-sm text-amber-700">{getErrorCodeMessage('resume_limit_reached')}</p>
        )}
      </div>

      <div className="mt-8">{renderList()}</div>
    </div>
  )
}