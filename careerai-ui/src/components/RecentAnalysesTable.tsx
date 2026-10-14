import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { getScoreBadgeClass } from '../utils/scoreTone'
import type { RecentAnalysis } from '../types/dashboard'

export default function RecentAnalysesTable({ analyses }: { analyses: RecentAnalysis[] }) {
  const { t } = useTranslation()

  return (
    <section className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
      <div className="mb-4 flex items-center justify-between gap-4">
        <h2 className="text-base font-semibold text-slate-900">{t('dashboard.recent.title')}</h2>
        <Link to="/applications" className="text-sm font-medium text-indigo-600 hover:text-indigo-700">
          {t('dashboard.recent.viewAll')}
        </Link>
      </div>

      {analyses.length === 0 ? (
        <p className="py-6 text-center text-sm text-slate-500">{t('dashboard.recent.empty')}</p>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-slate-200 text-sm">
            <thead className="text-left text-xs font-medium uppercase tracking-wide text-slate-500">
              <tr>
                <th className="py-2 pr-4">{t('dashboard.recent.columns.company')}</th>
                <th className="py-2 pr-4">{t('dashboard.recent.columns.position')}</th>
                <th className="py-2 pr-4">{t('dashboard.recent.columns.score')}</th>
                <th className="py-2">{t('dashboard.recent.columns.date')}</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {analyses.map((analysis) => (
                <tr key={analysis.jobApplicationId}>
                  <td className="py-3 pr-4 font-medium text-slate-900">{analysis.companyName}</td>
                  <td className="py-3 pr-4 text-slate-700">{analysis.jobTitle}</td>
                  <td className="py-3 pr-4">
                    <Link
                      to={`/analysis/${analysis.jobApplicationId}`}
                      className={`inline-block rounded-full px-2.5 py-0.5 text-xs font-semibold hover:underline ${getScoreBadgeClass(analysis.matchScore)}`}
                    >
                      %{analysis.matchScore}
                    </Link>
                  </td>
                  <td className="whitespace-nowrap py-3 text-slate-500">
                    {new Date(analysis.analyzedAt).toLocaleDateString('tr-TR', {
                      day: 'numeric',
                      month: 'long',
                      year: 'numeric',
                    })}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}