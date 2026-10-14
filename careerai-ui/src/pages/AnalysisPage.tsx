import type { ReactNode } from 'react'
import { Link, Navigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import AnalyzePanel from '../components/AnalyzePanel'
import ScoreRing from '../components/ScoreRing'
import SkillList from '../components/SkillList'
import { useAnalysis } from '../hooks/useAnalysis'

function Card({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
      <h2 className="mb-4 text-base font-semibold text-slate-900">{title}</h2>
      {children}
    </section>
  )
}

function Spinner() {
  return (
    <div className="flex justify-center py-16" role="status">
      <div className="h-8 w-8 animate-spin rounded-full border-4 border-slate-300 border-t-indigo-600" />
    </div>
  )
}

function BackLink() {
  const { t } = useTranslation()
  return (
    <Link to="/applications" className="text-sm font-medium text-indigo-600 hover:text-indigo-700">
      ← {t('analysis.back')}
    </Link>
  )
}

function AnalysisView({ jobId }: { jobId: string }) {
  const { t } = useTranslation()
  const { job, analysis, isLoading, error, refresh, retry } = useAnalysis(jobId)

  const handleCompleted = () => {
    void refresh().then(() => window.scrollTo({ top: 0, behavior: 'smooth' }))
  }

  if (isLoading) return <Spinner />

  if (error) {
    const notFound = error.status === 404
    return (
      <div className="max-w-3xl">
        <BackLink />
        <div role="alert" className="mt-4 rounded-xl border border-red-200 bg-red-50 p-6 text-center">
          <p className="text-sm text-red-700">{notFound ? t('analysis.notFound') : t('analysis.loadError')}</p>
          {!notFound && <p className="mt-1 text-xs text-red-600">{error.message}</p>}
          {!notFound && (
            <button
              type="button"
              onClick={retry}
              className="mt-4 rounded-lg border border-red-300 px-4 py-2 text-sm font-medium text-red-700 hover:bg-red-100"
            >
              {t('common.retry')}
            </button>
          )}
        </div>
      </div>
    )
  }

  if (!job) return null

  // Bu ilan için henüz analiz yok: CV seçip başlat
  if (!analysis) {
    return (
      <div className="max-w-3xl">
        <BackLink />
        <h1 className="mt-3 text-2xl font-semibold text-slate-900">{job.jobTitle}</h1>
        <p className="text-slate-600">{job.companyName}</p>

        <div className="mt-6 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <p className="text-sm font-medium text-slate-700">{t('analysis.empty.title')}</p>
          <p className="mb-4 mt-1 text-sm text-slate-500">{t('analysis.empty.hint')}</p>
          <AnalyzePanel jobId={jobId} variant="first" onCompleted={handleCompleted} />
          <p className="mt-4 text-xs text-slate-500">{t('newAnalysis.privacy')}</p>
        </div>
      </div>
    )
  }

  const levelLabels: Record<string, string> = {
    Intern: t('analysis.levels.Intern'),
    Junior: t('analysis.levels.Junior'),
    Mid: t('analysis.levels.Mid'),
    Senior: t('analysis.levels.Senior'),
    Lead: t('analysis.levels.Lead'),
    Unspecified: t('analysis.levels.Unspecified'),
  }

  const analyzedAt = new Date(analysis.createdAt).toLocaleString('tr-TR', {
    dateStyle: 'long',
    timeStyle: 'short',
  })

  return (
    <div className="max-w-4xl space-y-6">
      <BackLink />

      {/* Skor ve özet */}
      <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <div className="flex flex-col items-center gap-6 sm:flex-row sm:items-start">
          <ScoreRing score={analysis.matchScore} />

          <div className="min-w-0 flex-1">
            <h1 className="text-xl font-semibold text-slate-900">{analysis.jobTitle}</h1>
            <p className="text-slate-600">{analysis.companyName}</p>

            <dl className="mt-3 grid gap-x-6 gap-y-1 text-sm sm:grid-cols-2">
              <div className="flex gap-2">
                <dt className="text-slate-500">{t('analysis.cvLabel')}:</dt>
                <dd className="truncate text-slate-900" title={analysis.resumeFileName}>
                  {analysis.resumeFileName}
                </dd>
              </div>
              <div className="flex gap-2">
                <dt className="text-slate-500">{t('analysis.analyzedAt')}:</dt>
                <dd className="text-slate-900">{analyzedAt}</dd>
              </div>
              <div className="flex gap-2">
                <dt className="text-slate-500">{t('analysis.experienceLevel')}:</dt>
                <dd className="text-slate-900">
                  {levelLabels[analysis.experienceLevel] ?? analysis.experienceLevel}
                </dd>
              </div>
            </dl>

            {analysis.summary && (
              <p className="mt-4 rounded-lg bg-slate-50 p-4 text-sm leading-relaxed text-slate-700">
                {analysis.summary}
              </p>
            )}
          </div>
        </div>
      </section>

      {/* Eşleşen ve eksik beceriler */}
      <div className="grid gap-6 md:grid-cols-2">
        <Card title={`${t('analysis.matchedSkills')} (${analysis.matchedSkills.length})`}>
          <SkillList skills={analysis.matchedSkills} variant="matched" emptyText={t('analysis.noMatched')} />
        </Card>
        <Card title={`${t('analysis.missingSkills')} (${analysis.missingSkills.length})`}>
          <SkillList skills={analysis.missingSkills} variant="missing" emptyText={t('analysis.noMissing')} />
        </Card>
      </div>

      {/* Öneriler */}
      <Card title={t('analysis.recommendations')}>
        {analysis.recommendations.length === 0 ? (
          <p className="text-sm text-slate-500">{t('analysis.noRecommendations')}</p>
        ) : (
          <ol className="space-y-3">
            {analysis.recommendations.map((recommendation, index) => (
              <li key={index} className="flex gap-3 text-sm text-slate-700">
                <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-indigo-50 text-xs font-semibold text-indigo-700">
                  {index + 1}
                </span>
                <span>{recommendation}</span>
              </li>
            ))}
          </ol>
        )}
      </Card>

      {analysis.otherRequirements.length > 0 && (
        <Card title={t('analysis.otherRequirements')}>
          <ul className="list-disc space-y-1 pl-5 text-sm text-slate-700">
            {analysis.otherRequirements.map((requirement, index) => (
              <li key={index}>{requirement}</li>
            ))}
          </ul>
        </Card>
      )}

      {/* Yeniden analiz */}
      <Card title={t('analysis.reanalyzeTitle')}>
        <p className="mb-4 text-sm text-slate-500">{t('analysis.reanalyzeHint')}</p>
        <AnalyzePanel
          jobId={jobId}
          defaultResumeId={analysis.resumeId}
          variant="refresh"
          onCompleted={handleCompleted}
        />
        <p className="mt-4 text-xs text-slate-500">{t('newAnalysis.privacy')}</p>
      </Card>

      <p className="text-xs text-slate-500">{t('analysis.disclaimer')}</p>
    </div>
  )
}

export default function AnalysisPage() {
  const { id } = useParams<{ id: string }>()

  if (!id) return <Navigate to="/applications" replace />

  // key: farklı bir ilana geçilince bileşen sıfırdan başlasın (eski verinin görünmesini önler)
  return <AnalysisView key={id} jobId={id} />
}