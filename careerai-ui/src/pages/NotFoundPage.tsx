import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import Logo from '../components/Logo'

export default function NotFoundPage() {
  const { t } = useTranslation()

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-slate-50 p-6 text-center">
      <Link to="/" aria-label="CareerAI">
        <Logo />
      </Link>

      <p className="mt-10 text-sm font-semibold text-indigo-600">404</p>
      <h1 className="mt-2 text-3xl font-bold tracking-tight text-slate-900">{t('notFound.title')}</h1>
      <p className="mt-3 max-w-md text-slate-600">{t('notFound.text')}</p>

      <Link
        to="/"
        className="mt-8 inline-flex items-center justify-center rounded-lg bg-indigo-600 px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-indigo-700"
      >
        {t('notFound.action')}
      </Link>
    </div>
  )
}