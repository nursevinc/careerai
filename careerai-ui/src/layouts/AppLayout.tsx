import { Link, NavLink, Outlet } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import Icon from '../components/Icon'
import Logo from '../components/Logo'
import { useAuth } from '../hooks/useAuth'
import { getInitials } from '../utils/initials'

const navItems = [
  { to: '/dashboard', labelKey: 'nav.dashboard', icon: 'grid' },
  { to: '/resumes', labelKey: 'nav.resumes', icon: 'fileText' },
  { to: '/new-analysis', labelKey: 'nav.newAnalysis', icon: 'plusCircle' },
  { to: '/applications', labelKey: 'nav.applications', icon: 'briefcase' },
  { to: '/profile', labelKey: 'nav.profile', icon: 'user' },
] as const

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  `flex items-center gap-3 whitespace-nowrap rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
    isActive ? 'bg-indigo-50 text-indigo-700' : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900'
  }`

export default function AppLayout() {
  const { user, logout } = useAuth()
  const { t } = useTranslation()

  return (
    <div className="flex min-h-screen bg-slate-50">
      {/* Klavye kullanıcıları için: menüyü atlayıp içeriğe geç */}
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:absolute focus:left-4 focus:top-4 focus:z-50 focus:rounded-lg focus:bg-white focus:px-4 focus:py-2 focus:text-sm focus:font-medium focus:text-indigo-700 focus:shadow-lg"
      >
        {t('common.skipToContent')}
      </a>

      {/* Sidebar (geniş ekran) */}
      <aside className="hidden w-64 shrink-0 flex-col border-r border-slate-200 bg-white md:sticky md:top-0 md:flex md:h-screen">
        <div className="flex h-16 items-center border-b border-slate-200 px-6">
          <Link to="/dashboard" aria-label="CareerAI">
            <Logo />
          </Link>
        </div>

        <nav aria-label="Ana menü" className="flex-1 space-y-1 p-3">
          {navItems.map((item) => (
            <NavLink key={item.to} to={item.to} className={navLinkClass}>
              <Icon name={item.icon} className="h-5 w-5 shrink-0" />
              {t(item.labelKey)}
            </NavLink>
          ))}
        </nav>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        {/* Üst çubuk */}
        <header className="sticky top-0 z-20 flex h-16 items-center justify-between border-b border-slate-200 bg-white/90 px-4 backdrop-blur sm:px-6">
          <Link to="/dashboard" aria-label="CareerAI" className="md:hidden">
            <Logo />
          </Link>

          <div className="ml-auto flex items-center gap-3">
            <div className="flex items-center gap-2">
              <span
                aria-hidden="true"
                className="flex h-8 w-8 items-center justify-center rounded-full bg-indigo-100 text-xs font-semibold text-indigo-700"
              >
                {getInitials(user?.name)}
              </span>
              <span className="hidden max-w-40 truncate text-sm font-medium text-slate-700 sm:block">
                {user?.name}
              </span>
            </div>

            <button
              type="button"
              onClick={logout}
              className="inline-flex items-center gap-2 rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100"
            >
              <Icon name="logOut" className="h-4 w-4" />
              <span className="hidden sm:inline">{t('nav.logout')}</span>
              <span className="sr-only sm:hidden">{t('nav.logout')}</span>
            </button>
          </div>
        </header>

        {/* Menü şeridi (dar ekran) */}
        <nav
          aria-label="Ana menü"
          className="flex gap-1 overflow-x-auto border-b border-slate-200 bg-white px-3 py-2 md:hidden"
        >
          {navItems.map((item) => (
            <NavLink key={item.to} to={item.to} className={navLinkClass}>
              <Icon name={item.icon} className="h-4 w-4 shrink-0" />
              {t(item.labelKey)}
            </NavLink>
          ))}
        </nav>

        <main id="main-content" className="flex-1 p-4 sm:p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}