import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import Icon from '../components/Icon'
import Logo from '../components/Logo'
import ScoreRing from '../components/ScoreRing'
import SkillList from '../components/SkillList'
import type { SkillMatch } from '../types/analysis'
import { useServerWarmup } from '../hooks/useServerWarmup'
const primaryButton =
  'inline-flex items-center justify-center rounded-lg bg-indigo-600 px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-indigo-700'
const secondaryButton =
  'inline-flex items-center justify-center rounded-lg border border-slate-300 bg-white px-5 py-2.5 text-sm font-semibold text-slate-700 transition-colors hover:bg-slate-50'

// Örnek analiz, gerçek algoritmayla tutarlıdır:
// zorunlu 5 (4 eşleşti) + tercih 1 (eksik) -> (4*3) / (5*3 + 1*1) = 12 / 16 = %75
const SAMPLE_MATCHED: SkillMatch[] = [
  { name: 'C#', isRequired: true },
  { name: 'ASP.NET Core', isRequired: true },
  { name: 'SQL Server', isRequired: true },
  { name: 'React', isRequired: true },
]
const SAMPLE_MISSING: SkillMatch[] = [
  { name: 'Docker', isRequired: true },
  { name: 'Azure', isRequired: false },
]
const SAMPLE_SCORE = 75

const STEPS = [
  { icon: 'upload', titleKey: 'landing.how.step1.title', textKey: 'landing.how.step1.text' },
  { icon: 'clipboard', titleKey: 'landing.how.step2.title', textKey: 'landing.how.step2.text' },
  { icon: 'barChart', titleKey: 'landing.how.step3.title', textKey: 'landing.how.step3.text' },
] as const

const FEATURES = [
  { icon: 'target', titleKey: 'landing.features.scoring.title', textKey: 'landing.features.scoring.text' },
  { icon: 'list', titleKey: 'landing.features.skills.title', textKey: 'landing.features.skills.text' },
  { icon: 'message', titleKey: 'landing.features.advice.title', textKey: 'landing.features.advice.text' },
  { icon: 'barChart', titleKey: 'landing.features.history.title', textKey: 'landing.features.history.text' },
  { icon: 'globe', titleKey: 'landing.features.languages.title', textKey: 'landing.features.languages.text' },
  { icon: 'refresh', titleKey: 'landing.features.reanalyze.title', textKey: 'landing.features.reanalyze.text' },
] as const

const PRIVACY_POINTS = [
  'landing.privacy.point1',
  'landing.privacy.point2',
  'landing.privacy.point3',
  'landing.privacy.point4',
] as const

export default function LandingPage() {
  const { t } = useTranslation()
    useServerWarmup()

  return (
    <div className="min-h-screen bg-white text-slate-900">
      {/* Üst menü */}
      <header className="sticky top-0 z-30 border-b border-slate-200/70 bg-white/80 backdrop-blur">
        <div className="mx-auto flex h-16 max-w-6xl items-center justify-between px-6">
          <Link to="/" aria-label="CareerAI">
            <Logo />
          </Link>

          <nav aria-label={t('landing.nav.label')} className="hidden items-center gap-8 text-sm font-medium text-slate-600 md:flex">
            <a href="#how" className="hover:text-slate-900">
              {t('landing.nav.how')}
            </a>
            <a href="#features" className="hover:text-slate-900">
              {t('landing.nav.features')}
            </a>
            <a href="#privacy" className="hover:text-slate-900">
              {t('landing.nav.privacy')}
            </a>
          </nav>

          <div className="flex items-center gap-2 sm:gap-3">
            <Link to="/login" className="rounded-lg px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100">
              {t('landing.nav.login')}
            </Link>
            <Link to="/register" className={primaryButton}>
              {t('landing.nav.start')}
            </Link>
          </div>
        </div>
      </header>

      <main>
        {/* Hero */}
        <section className="bg-gradient-to-b from-indigo-50 via-white to-white">
          <div className="mx-auto grid max-w-6xl items-center gap-12 px-6 py-16 lg:grid-cols-2 lg:py-24">
            <div>
              <span className="inline-flex items-center rounded-full border border-indigo-200 bg-white px-3 py-1 text-xs font-medium text-indigo-700">
                {t('landing.hero.badge')}
              </span>
              <h1 className="mt-5 text-4xl font-bold tracking-tight text-slate-900 sm:text-5xl">
                {t('landing.hero.title')}
              </h1>
              <p className="mt-5 max-w-xl text-lg leading-relaxed text-slate-600">{t('landing.hero.subtitle')}</p>

              <div className="mt-8 flex flex-wrap gap-3">
                <Link to="/register" className={primaryButton}>
                  {t('landing.hero.primary')}
                </Link>
                <a href="#how" className={secondaryButton}>
                  {t('landing.hero.secondary')}
                </a>
              </div>
            </div>

            {/* Örnek analiz önizlemesi */}
            <div className="relative">
              <div
                aria-hidden="true"
                className="absolute -inset-4 rounded-3xl bg-gradient-to-tr from-indigo-200/60 to-violet-200/40 blur-2xl"
              />
              <div className="relative rounded-2xl border border-slate-200 bg-white p-6 shadow-xl">
                <div className="mb-5 flex items-start justify-between gap-3">
                  <div className="min-w-0">
                    <p className="truncate text-sm font-semibold text-slate-900">{t('landing.hero.sampleJob')}</p>
                    <p className="text-xs text-slate-500">{t('landing.hero.sampleCompany')}</p>
                  </div>
                  <span className="shrink-0 rounded-full bg-slate-100 px-2.5 py-0.5 text-xs font-medium text-slate-600">
                    {t('landing.hero.sampleLabel')}
                  </span>
                </div>

                <div className="flex flex-col items-center gap-6 sm:flex-row sm:items-start">
                  <ScoreRing score={SAMPLE_SCORE} />

                  <div className="min-w-0 flex-1 space-y-4">
                    <div>
                      <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-slate-500">
                        {t('analysis.matchedSkills')}
                      </p>
                      <SkillList skills={SAMPLE_MATCHED} variant="matched" emptyText="" />
                    </div>
                    <div>
                      <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-slate-500">
                        {t('analysis.missingSkills')}
                      </p>
                      <SkillList skills={SAMPLE_MISSING} variant="missing" emptyText="" />
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* Nasıl çalışır */}
        <section id="how" className="scroll-mt-16 bg-white py-20">
          <div className="mx-auto max-w-6xl px-6">
            <div className="mx-auto max-w-2xl text-center">
              <h2 className="text-3xl font-bold tracking-tight text-slate-900">{t('landing.how.title')}</h2>
              <p className="mt-3 text-slate-600">{t('landing.how.subtitle')}</p>
            </div>

            <ol className="mt-12 grid gap-6 md:grid-cols-3">
              {STEPS.map((step, index) => (
                <li key={step.titleKey} className="rounded-2xl border border-slate-200 bg-slate-50 p-6">
                  <div className="flex items-center gap-3">
                    <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-indigo-600 text-white">
                      <Icon name={step.icon} className="h-5 w-5" />
                    </span>
                    <span className="text-sm font-semibold text-indigo-600">0{index + 1}</span>
                  </div>
                  <h3 className="mt-4 text-lg font-semibold text-slate-900">{t(step.titleKey)}</h3>
                  <p className="mt-2 text-sm leading-relaxed text-slate-600">{t(step.textKey)}</p>
                </li>
              ))}
            </ol>
          </div>
        </section>

        {/* Özellikler */}
        <section id="features" className="scroll-mt-16 border-y border-slate-200 bg-slate-50 py-20">
          <div className="mx-auto max-w-6xl px-6">
            <div className="mx-auto max-w-2xl text-center">
              <h2 className="text-3xl font-bold tracking-tight text-slate-900">{t('landing.features.title')}</h2>
              <p className="mt-3 text-slate-600">{t('landing.features.subtitle')}</p>
            </div>

            <ul className="mt-12 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {FEATURES.map((feature) => (
                <li key={feature.titleKey} className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
                  <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-indigo-50 text-indigo-600">
                    <Icon name={feature.icon} className="h-5 w-5" />
                  </span>
                  <h3 className="mt-4 text-base font-semibold text-slate-900">{t(feature.titleKey)}</h3>
                  <p className="mt-2 text-sm leading-relaxed text-slate-600">{t(feature.textKey)}</p>
                </li>
              ))}
            </ul>
          </div>
        </section>

        {/* Gizlilik ve güvenlik */}
        <section id="privacy" className="scroll-mt-16 bg-slate-900 py-20 text-white">
          <div className="mx-auto grid max-w-6xl gap-12 px-6 lg:grid-cols-2 lg:items-center">
            <div>
              <h2 className="text-3xl font-bold tracking-tight">{t('landing.privacy.title')}</h2>
              <p className="mt-3 max-w-md text-slate-300">{t('landing.privacy.subtitle')}</p>
            </div>

            <ul className="space-y-4">
              {PRIVACY_POINTS.map((key) => (
                <li key={key} className="flex items-start gap-3 text-slate-200">
                  <span className="mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-emerald-500/20 text-emerald-400">
                    <Icon name="check" className="h-3.5 w-3.5" />
                  </span>
                  <span className="text-sm leading-relaxed">{t(key)}</span>
                </li>
              ))}
            </ul>
          </div>
        </section>

        {/* Son çağrı */}
        <section className="bg-white py-20">
          <div className="mx-auto max-w-4xl px-6">
            <div className="rounded-3xl bg-gradient-to-br from-indigo-600 to-violet-700 px-8 py-14 text-center text-white shadow-lg">
              <h2 className="text-3xl font-bold tracking-tight">{t('landing.cta.title')}</h2>
              <p className="mx-auto mt-3 max-w-xl text-indigo-100">{t('landing.cta.text')}</p>
              <Link
                to="/register"
                className="mt-8 inline-flex items-center justify-center rounded-lg bg-white px-6 py-3 text-sm font-semibold text-indigo-700 shadow-sm transition-colors hover:bg-indigo-50"
              >
                {t('landing.cta.action')}
              </Link>
            </div>
          </div>
        </section>
      </main>

      <footer className="border-t border-slate-200 bg-white">
        <div className="mx-auto flex max-w-6xl flex-col gap-3 px-6 py-8 text-sm text-slate-500 md:flex-row md:items-center md:justify-between">
          <p>
            © {new Date().getFullYear()} {t('landing.footer.rights')}
          </p>
          <p className="max-w-xl md:text-right">{t('landing.footer.disclaimer')}</p>
        </div>
      </footer>
    </div>
  )
}