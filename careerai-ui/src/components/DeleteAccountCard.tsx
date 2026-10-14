import { useState } from 'react'
import type { FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import TextField from './TextField'
import { accountService } from '../services/accountService'
import { parseApiError } from '../utils/apiError'
import { tokenStorage } from '../utils/tokenStorage'

interface DeleteAccountCardProps {
  onDeleted: () => void
}

export default function DeleteAccountCard({ onDeleted }: DeleteAccountCardProps) {
  const { t } = useTranslation()
  const [password, setPassword] = useState('')
  const [understood, setUnderstood] = useState(false)
  const [isDeleting, setIsDeleting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const canSubmit = password.length > 0 && understood && !isDeleting

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    if (!canSubmit) return

    setError(null)
    setIsDeleting(true)

    try {
      await accountService.deleteAccount(password)
      tokenStorage.clear() // token artık geçersiz, saklamanın anlamı yok
      onDeleted()
    } catch (err) {
      setError(parseApiError(err).message)
      setIsDeleting(false)
    }
  }

  return (
    <section className="rounded-xl border border-red-200 bg-white p-6 shadow-sm">
      <h2 className="text-base font-semibold text-red-700">{t('profile.delete.title')}</h2>
      <p className="mt-2 text-sm text-slate-600">{t('profile.delete.warning')}</p>

      <ul className="mt-2 list-disc space-y-1 pl-5 text-sm text-slate-600">
        <li>{t('profile.delete.item1')}</li>
        <li>{t('profile.delete.item2')}</li>
        <li>{t('profile.delete.item3')}</li>
        <li>{t('profile.delete.item4')}</li>
      </ul>

      <form onSubmit={(event) => void handleSubmit(event)} noValidate className="mt-5 space-y-4">
        {error && (
          <div role="alert" className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {error}
          </div>
        )}

        <TextField
          label={t('profile.delete.passwordLabel')}
          type="password"
          autoComplete="current-password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
        />

        <label className="flex items-start gap-2 text-sm text-slate-700">
          <input
            type="checkbox"
            checked={understood}
            onChange={(event) => setUnderstood(event.target.checked)}
            className="mt-0.5 h-4 w-4 rounded border-slate-300"
          />
          {t('profile.delete.understood')}
        </label>

        <button
          type="submit"
          disabled={!canSubmit}
          className="rounded-lg bg-red-600 px-5 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
        >
          {isDeleting ? t('profile.delete.submitting') : t('profile.delete.submit')}
        </button>
      </form>
    </section>
  )
}