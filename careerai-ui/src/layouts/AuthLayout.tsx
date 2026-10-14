import { Link, Outlet } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import Icon from '../components/Icon'
import Logo from '../components/Logo'

const POINT_KEYS = ['auth.panel.point1', 'auth.panel.point2', 'auth.panel.point3'] as const

export default function AuthLayout() {
  const { t } = useTranslation()

  return (
    <div className="flex min-h-screen">
      {/* Marka paneli (geniş ekranlarda) */}
      <aside className="relative hidden w-1/2 max-w-xl flex-col justify-between overflow-hidden bg-gradient-to-br from-indigo-700 via-indigo-600 to-violet-700 p-12 text-white lg:flex">
        <div aria-hidden="true" className="pointer-events-none absolute -right-24 -top-24 h-72 w-72 rounded-full bg-white/10" />
        <div aria-hidden="true" className="pointer-events-none absolute -bottom-32 -left-16 h-80 w-80 rounded-full bg-white/5" />

        <Link to="/" aria-label="CareerAI" className="relative w-fit">
          <Logo variant="light" />
        </Link>

        <div className="relative">
          <h2 className="text-3xl font-semibold leading-tight">{t('auth.panel.title')}</h2>
          <ul className="mt-8 space-y-4">
            {POINT_KEYS.map((key) => (
              <li key={key} className="flex items-start gap-3 text-indigo-50">
                <span className="mt-0.5 flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-white/20">
                  <Icon name="check" className="h-3 w-3" />
                </span>
                {t(key)}
              </li>
            ))}
          </ul>
        </div>

        <p className="relative text-sm text-indigo-200">{t('auth.panel.footer')}</p>
      </aside>

      {/* Form alanı */}
      <main className="flex flex-1 items-center justify-center bg-slate-50 p-6">
        <div className="w-full max-w-md">
          <Link to="/" aria-label="CareerAI" className="mb-8 block w-fit lg:hidden">
            <Logo />
          </Link>

          <div className="rounded-xl border border-slate-200 bg-white p-8 shadow-sm">
            <Outlet />
          </div>

          <p className="mt-6 text-center text-sm">
            <Link to="/" className="text-slate-500 hover:text-slate-700">
              ← {t('auth.backToHome')}
            </Link>
          </p>
        </div>
      </main>
    </div>
  )
}