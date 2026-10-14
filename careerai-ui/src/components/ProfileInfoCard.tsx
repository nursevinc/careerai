import { useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useTranslation } from 'react-i18next'
import type { TFunction } from 'i18next'
import TextField from './TextField'
import { useAuth } from '../hooks/useAuth'
import { accountService } from '../services/accountService'
import { parseApiError } from '../utils/apiError'

const createSchema = (t: TFunction) =>
  z.object({
    name: z.string().trim().min(1, t('validation.nameRequired')).max(100, t('validation.nameTooLong')),
  })

type FormValues = z.infer<ReturnType<typeof createSchema>>

export default function ProfileInfoCard() {
  const { t } = useTranslation()
  const { user, updateUser } = useAuth()
  const [serverError, setServerError] = useState<string | null>(null)
  const [saved, setSaved] = useState(false)

  const schema = useMemo(() => createSchema(t), [t])

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting, isDirty },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: user?.name ?? '' },
  })

  const onSubmit = async (values: FormValues) => {
    setServerError(null)
    setSaved(false)

    try {
      const updated = await accountService.updateProfile(values)
      updateUser(updated) // üst çubuktaki ad da hemen güncellensin
      reset({ name: updated.name })
      setSaved(true)
    } catch (error) {
      setServerError(parseApiError(error).message)
    }
  }

  return (
    <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
      <h2 className="text-base font-semibold text-slate-900">{t('profile.info.title')}</h2>

      <dl className="mt-4 space-y-2 text-sm">
        <div className="flex gap-2">
          <dt className="w-32 shrink-0 text-slate-500">{t('profile.info.email')}</dt>
          <dd className="min-w-0 break-words text-slate-900">
            {user?.email}
            <span className="block text-xs text-slate-400">{t('profile.info.emailHint')}</span>
          </dd>
        </div>
        <div className="flex gap-2">
          <dt className="w-32 shrink-0 text-slate-500">{t('profile.info.memberSince')}</dt>
          <dd className="text-slate-900">
            {user ? new Date(user.createdAt).toLocaleDateString('tr-TR', { dateStyle: 'long' }) : ''}
          </dd>
        </div>
      </dl>

      <form onSubmit={handleSubmit(onSubmit)} noValidate className="mt-5 space-y-4 border-t border-slate-100 pt-5">
        {serverError && (
          <div role="alert" className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {serverError}
          </div>
        )}
        {saved && (
          <div role="status" className="rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
            {t('profile.info.saved')}
          </div>
        )}

        <TextField label={t('auth.name')} autoComplete="name" error={errors.name?.message} {...register('name')} />

        <button
          type="submit"
          disabled={isSubmitting || !isDirty}
          className="rounded-lg bg-indigo-600 px-5 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-60"
        >
          {isSubmitting ? t('profile.info.saving') : t('profile.info.save')}
        </button>
      </form>
    </section>
  )
}