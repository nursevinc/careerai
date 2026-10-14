import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import ConfirmDeleteButton from '../components/ConfirmDeleteButton'
import { useJobApplications } from '../hooks/useJobApplications'
import { parseApiError } from '../utils/apiError'

export default function ApplicationsPage() {
  const { t } = useTranslation()
  const { applications, isLoading, error, reload, remove } = useJobApplications()
  const [actionError, setActionError] = useState<string | null>(null)

  const renderContent = () => {
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
          <p className="text-sm text-red-700">{t('applications.loadError')}</p>
          <p className="mt-1 text-xs text-red-600">{error}</p>
          <button
            type="button"
            onClick={reload}
            className="mt-4 rounded-lg border border-red-300 px-4 py-2 text-sm font-medium text-red-700 hover:bg-red-100"
          >
            {t('common.retry')}
          </button>
        </div>
      )
    }

    if (applications.length === 0) {
      return (
        <div className="rounded-xl border border-slate-200 bg-white p-10 text-center">
          <p className="text-sm font-medium text-slate-700">{t('applications.empty')}</p>
          <p className="mt-1 text-sm text-slate-500">{t('applications.emptyHint')}</p>
          <Link
            to="/new-analysis"
            className="mt-4 inline-block rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700"
          >
            {t('applications.newAnalysis')}
          </Link>
        </div>
      )
    }

    return (
      <div className="overflow-x-auto rounded-xl border border-slate-200 bg-white shadow-sm">
        <table className="min-w-full divide-y divide-slate-200 text-sm">
          <thead className="bg-slate-50 text-left text-xs font-medium uppercase tracking-wide text-slate-500">
            <tr>
              <th className="px-4 py-3">{t('applications.columns.company')}</th>
              <th className="px-4 py-3">{t('applications.columns.position')}</th>
              <th className="px-4 py-3">{t('applications.columns.score')}</th>
              <th className="px-4 py-3">{t('applications.columns.date')}</th>
              <th className="px-4 py-3" />
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {applications.map((application) => (
              <tr key={application.id}>
                <td className="px-4 py-3 font-medium text-slate-900">{application.companyName}</td>
                <td className="px-4 py-3 text-slate-700">{application.jobTitle}</td>
                <td className="px-4 py-3">
                  {application.matchScore === null ? (
                    <Link
                      to={`/analysis/${application.id}`}
                      className="rounded-lg border border-indigo-200 px-3 py-1 text-xs font-medium text-indigo-700 hover:bg-indigo-50"
                    >
                      {t('applications.analyze')}
                    </Link>
                  ) : (
                    <Link
                      to={`/analysis/${application.id}`}
                      className="font-semibold text-indigo-700 hover:underline"
                      title={t('applications.viewAnalysis')}
                    >
                      %{application.matchScore}
                    </Link>
                  )}
                </td>
                <td className="whitespace-nowrap px-4 py-3 text-slate-500">
                  {new Date(application.createdAt).toLocaleDateString('tr-TR', {
                    day: 'numeric',
                    month: 'long',
                    year: 'numeric',
                  })}
                </td>
                <td className="px-4 py-3 text-right">
                  <ConfirmDeleteButton
                    question={t('applications.confirmDelete')}
                    onConfirm={() => remove(application)}
                    onError={(err) => setActionError(parseApiError(err).message)}
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    )
  }

  return (
    <div className="max-w-5xl">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-slate-900">{t('applications.title')}</h1>
          <p className="mt-1 text-slate-500">{t('applications.subtitle')}</p>
        </div>
        <Link
          to="/new-analysis"
          className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700"
        >
          {t('applications.newAnalysis')}
        </Link>
      </div>

      {actionError && (
        <div role="alert" className="mt-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {actionError}
        </div>
      )}

      <div className="mt-6">{renderContent()}</div>
    </div>
  )
}