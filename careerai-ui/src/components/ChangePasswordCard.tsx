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

// Kurallar backend'deki PasswordRules ile aynı (UX için; asıl doğrulama backend'de)
const createSchema = (t: TFunction) =>
  z
    .object({
      currentPassword: z.string().min(1, t('validation.currentPasswordRequired')),
      newPassword: z
        .string()
        .min(8, t('validation.passwordMin'))
        .max(64, t('validation.passwordMax'))
        .regex(/[A-Za-z]/, t('validation.passwordLetter'))
        .regex(/[0-9]/, t('validation.passwordDigit')),
      confirmPassword: z.string().min(1, t('validation.confirmRequired')),
    })
    .refine((data) => data.newPassword === data.confirmPassword, {
      message: t('validation.passwordMismatch'),
      path: ['confirmPassword'],
    })
    .refine((data) => data.newPassword !== data.currentPassword, {
      message: t('validation.newPasswordSame'),
      path: ['newPassword'],
    })

type FormValues = z.infer<ReturnType<typeof createSchema>>

export default function ChangePasswordCard() {
  const { t } = useTranslation()
  const { applyAuthResponse } = useAuth()
  const [serverError, setServerError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  const schema = useMemo(() => createSchema(t), [t])

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { currentPassword: '', newPassword: '', confirmPassword: '' },
  })

  const onSubmit = async (values: FormValues) => {
    setServerError(null)
    setSuccess(false)

    try {
      const response = await accountService.changePassword({
        currentPassword: values.currentPassword,
        newPassword: values.newPassword,
      })
      applyAuthResponse(response) // diğer cihazlar çıkış yapar, bu cihaz yeni token ile devam eder
      reset()
      setSuccess(true)
    } catch (error) {
      const parsed = parseApiError(error)
      if (parsed.code === 'invalid_password') {
        setError('currentPassword', { message: parsed.message })
      } else {
        setServerError(parsed.message)
      }
    }
  }

  return (
    <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
      <h2 className="text-base font-semibold text-slate-900">{t('profile.password.title')}</h2>

      <form onSubmit={handleSubmit(onSubmit)} noValidate className="mt-4 space-y-4">
        {serverError && (
          <div role="alert" className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {serverError}
          </div>
        )}
        {success && (
          <div role="status" className="rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
            {t('profile.password.success')}
          </div>
        )}

        <TextField
          label={t('profile.password.currentPassword')}
          type="password"
          autoComplete="current-password"
          error={errors.currentPassword?.message}
          {...register('currentPassword')}
        />
        <TextField
          label={t('profile.password.newPassword')}
          type="password"
          autoComplete="new-password"
          error={errors.newPassword?.message}
          {...register('newPassword')}
        />
        <TextField
          label={t('profile.password.confirmPassword')}
          type="password"
          autoComplete="new-password"
          error={errors.confirmPassword?.message}
          {...register('confirmPassword')}
        />

        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-lg bg-indigo-600 px-5 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-60"
        >
          {isSubmitting ? t('profile.password.submitting') : t('profile.password.submit')}
        </button>
      </form>
    </section>
  )
}