import { useMemo } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import ChartCard from '../components/ChartCard'
import {
  ApplicationsOverTimeChart,
  MissingSkillsChart,
  ScoreDistributionChart,
} from '../components/DashboardCharts'
import RecentAnalysesTable from '../components/RecentAnalysesTable'
import StatCard from '../components/StatCard'
import { useAuth } from '../hooks/useAuth'
import { useDashboard } from '../hooks/useDashboard'

// "2026-10" -> "Eki 26"
function monthLabel(key: string): string {
  const [year, month] = key.split('-').map(Number)
  return new Date(year, month - 1, 1).toLocaleDateString('tr-TR', { month: 'short', year: '2-digit' })
}

export default function DashboardPage() {
  const { t } = useTranslation()
  const { user } = useAuth()
  const { data, isLoading, error, retry } = useDashboard()

  const monthlyData = useMemo(
    () => (data?.applicationsOverTime ?? []).map((item) => ({ label: monthLabel(item.month), count: item.count })),
    [data],
  )

  const summarize = (title: string, items: { label: string; count: number }[]) =>
    t('dashboard.charts.summary', {
      title,
      items: items.map((item) => `${item.label}: ${item.count}`).join(', '),
    })

  const header = (
    <div className="flex flex-wrap items-start justify-between gap-4">
      <div>
<h1 className="text-2xl font-semibold text-slate-900">
  {t('dashboard.welcome', { name: user?.name ?? 'Kullanıcı' })}
</h1>        <p className="mt-1 text-slate-500">{t('dashboard.subtitle')}</p>
      </div>
      <Link
        to="/new-analysis"
        className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700"
      >
        {t('dashboard.newAnalysis')}
      </Link>
    </div>
  )

  if (isLoading) {
    return (
      <div className="max-w-6xl">
        {header}
        <div className="flex justify-center py-16" role="status">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-slate-300 border-t-indigo-600" />
        </div>
      </div>
    )
  }

  if (error || !data) {
    return (
      <div className="max-w-6xl">
        {header}
        <div role="alert" className="mt-6 rounded-xl border border-red-200 bg-red-50 p-6 text-center">
          <p className="text-sm text-red-700">{t('dashboard.loadError')}</p>
          {error && <p className="mt-1 text-xs text-red-600">{error}</p>}
          <button
            type="button"
            onClick={retry}
            className="mt-4 rounded-lg border border-red-300 px-4 py-2 text-sm font-medium text-red-700 hover:bg-red-100"
          >
            {t('common.retry')}
          </button>
        </div>
      </div>
    )
  }

  const { stats } = data

  // Hiç ilan yok: grafikler yerine "nasıl başlanır" kartı
  if (stats.totalApplications === 0) {
    return (
      <div className="max-w-6xl">
        {header}
        <section className="mt-6 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="text-base font-semibold text-slate-900">{t('dashboard.onboarding.title')}</h2>
          <p className="mt-1 text-sm text-slate-500">{t('dashboard.onboarding.subtitle')}</p>

          <div className="mt-5 grid gap-4 md:grid-cols-2">
            <div className="rounded-lg border border-slate-200 p-4">
              <p className="font-medium text-slate-900">{t('dashboard.onboarding.step1Title')}</p>
              <p className="mt-1 text-sm text-slate-500">{t('dashboard.onboarding.step1Text')}</p>
              <Link to="/resumes" className="mt-3 inline-block text-sm font-medium text-indigo-600 hover:text-indigo-700">
                {t('dashboard.onboarding.step1Action')}
              </Link>
            </div>
            <div className="rounded-lg border border-slate-200 p-4">
              <p className="font-medium text-slate-900">{t('dashboard.onboarding.step2Title')}</p>
              <p className="mt-1 text-sm text-slate-500">{t('dashboard.onboarding.step2Text')}</p>
              <Link
                to="/new-analysis"
                className="mt-3 inline-block text-sm font-medium text-indigo-600 hover:text-indigo-700"
              >
                {t('dashboard.onboarding.step2Action')}
              </Link>
            </div>
          </div>
        </section>
      </div>
    )
  }

  const noAnalyses = stats.totalAnalyses === 0
  const topThreeMissing = data.topMissingSkills.slice(0, 3)

  const distributionTitle = t('dashboard.charts.scoreDistribution')
  const overTimeTitle = t('dashboard.charts.applicationsOverTime')
  const missingTitle = t('dashboard.charts.topMissingSkills')

  return (
    <div className="max-w-6xl space-y-6">
      {header}

      {/* Özet kartları */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard title={t('dashboard.stats.totalAnalyses')}>
          <p className="text-3xl font-bold text-slate-900">{stats.totalAnalyses}</p>
          <p className="mt-1 text-xs text-slate-500">
            {t('dashboard.stats.savedApplications', { total: stats.totalApplications })}
          </p>
        </StatCard>

        <StatCard title={t('dashboard.stats.averageScore')}>
          <p className="text-3xl font-bold text-slate-900">
            {stats.averageScore === null ? t('dashboard.stats.noData') : `%${stats.averageScore}`}
          </p>
        </StatCard>

        <StatCard title={t('dashboard.stats.highestScore')}>
          <p className="text-3xl font-bold text-slate-900">
            {stats.highestScore === null ? t('dashboard.stats.noData') : `%${stats.highestScore}`}
          </p>
        </StatCard>

        <StatCard title={t('dashboard.stats.topMissing')}>
          {topThreeMissing.length === 0 ? (
            <p className="text-sm text-slate-500">
              {noAnalyses ? t('dashboard.stats.noData') : t('dashboard.stats.noMissing')}
            </p>
          ) : (
            <ul className="flex flex-wrap gap-2">
              {topThreeMissing.map((skill) => (
                <li
                  key={skill.name}
                  className="rounded-full border border-red-200 bg-red-50 px-3 py-1 text-sm text-red-800"
                >
                  {skill.name}
                </li>
              ))}
            </ul>
          )}
        </StatCard>
      </div>

      {/* Grafikler */}
      <div className="grid gap-6 lg:grid-cols-2">
        <ChartCard
          title={distributionTitle}
          summary={summarize(distributionTitle, data.scoreDistribution)}
          isEmpty={noAnalyses}
          emptyText={t('dashboard.charts.emptyAnalyses')}
        >
          <ScoreDistributionChart data={data.scoreDistribution} seriesName={t('dashboard.charts.analysisSeries')} />
        </ChartCard>

        <ChartCard
          title={overTimeTitle}
          summary={summarize(overTimeTitle, monthlyData)}
          isEmpty={false}
          emptyText={t('dashboard.charts.emptyApplications')}
        >
          <ApplicationsOverTimeChart data={monthlyData} seriesName={t('dashboard.charts.applicationSeries')} />
        </ChartCard>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <ChartCard
          title={missingTitle}
          summary={summarize(
            missingTitle,
            data.topMissingSkills.map((skill) => ({ label: skill.name, count: skill.count })),
          )}
          isEmpty={data.topMissingSkills.length === 0}
          emptyText={noAnalyses ? t('dashboard.charts.emptyAnalyses') : t('dashboard.stats.noMissing')}
        >
          <MissingSkillsChart data={data.topMissingSkills} seriesName={t('dashboard.charts.missingSeries')} />
        </ChartCard>

        <div className="lg:col-span-2">
          <RecentAnalysesTable analyses={data.recentAnalyses} />
        </div>
      </div>
    </div>
  )
}