import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import ChangePasswordCard from '../components/ChangePasswordCard'
import DeleteAccountCard from '../components/DeleteAccountCard'
import ProfileInfoCard from '../components/ProfileInfoCard'
import { useAuth } from '../hooks/useAuth'

export default function ProfilePage() {
  const { t } = useTranslation()
  const { logout } = useAuth()
  const [isDeleted, setIsDeleted] = useState(false)

  // Hesap silindi: onay göster. "Giriş sayfasına git" oturum durumunu da temizler ve /login'e yönlendirir.
  if (isDeleted) {
    return (
      <div className="max-w-xl">
        <div role="status" className="rounded-xl border border-slate-200 bg-white p-8 text-center shadow-sm">
          <h1 className="text-xl font-semibold text-slate-900">{t('profile.deleted.title')}</h1>
          <p className="mt-2 text-sm text-slate-600">{t('profile.deleted.text')}</p>
          <button
            type="button"
            onClick={logout}
            className="mt-6 rounded-lg bg-indigo-600 px-5 py-2 text-sm font-medium text-white hover:bg-indigo-700"
          >
            {t('profile.deleted.toLogin')}
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="max-w-3xl space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">{t('profile.title')}</h1>
        <p className="mt-1 text-slate-500">{t('profile.subtitle')}</p>
      </div>

      <ProfileInfoCard />
      <ChangePasswordCard />
      <DeleteAccountCard onDeleted={() => setIsDeleted(true)} />
    </div>
  )
}