import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useTranslation } from 'react-i18next'
import type { TFunction } from 'i18next'
import TextField from '../components/TextField'
import { useAuth } from '../hooks/useAuth'
import { parseApiError } from '../utils/apiError'
import SlowRequestHint from '../components/SlowRequestHint'
// Kurallar backend'deki RegisterRequestValidator ile aynı (UX için; asıl doğrulama backend'de)
const createRegisterSchema = (t: TFunction) =>
  z
    .object({
      name: z.string().trim().min(1, t('validation.nameRequired')).max(100, t('validation.nameTooLong')),
      email: z.string().min(1, t('validation.emailRequired')).email(t('validation.emailInvalid')),
      password: z
        .string()
        .min(8, t('validation.passwordMin'))
        .max(64, t('validation.passwordMax'))
        .regex(/[A-Za-z]/, t('validation.passwordLetter'))
        .regex(/[0-9]/, t('validation.passwordDigit')),
      confirmPassword: z.string().min(1, t('validation.confirmRequired')),
    })
    .refine((data) => data.password === data.confirmPassword, {
      message: t('validation.passwordMismatch'),
      path: ['confirmPassword'],
    })

type RegisterFormValues = z.infer<ReturnType<typeof createRegisterSchema>>

export default function RegisterPage() {
  const { t } = useTranslation()
  const { register: registerUser } = useAuth()
  const [serverError, setServerError] = useState<string | null>(null)

  const schema = useMemo(() => createRegisterSchema(t), [t])

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormValues>({ resolver: zodResolver(schema) })

  const onSubmit = async (values: RegisterFormValues) => {
    setServerError(null)
    try {
      // confirmPassword sunucuya gönderilmez
      await registerUser({ name: values.name, email: values.email, password: values.password })
    } catch (error) {
      const parsed = parseApiError(error)
      if (parsed.status === 409) {
        // "e-posta zaten kayıtlı" hatası e-posta alanının altında görünsün
        setError('email', { message: t('auth.errors.emailTaken') })
      } else {
        setServerError(parsed.message)
      }
    }
  }

  return (
    <>
            <h2 className="text-xl font-semibold text-slate-900">{t('auth.register.title')}</h2>
      <p className="mb-6 mt-1 text-sm text-slate-500">{t('auth.registerSubtitle')}</p>

      {serverError && (
        <div role="alert" className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {serverError}
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
        <TextField label={t('auth.name')} autoComplete="name" error={errors.name?.message} {...register('name')} />
        <TextField
          label={t('auth.email')}
          type="email"
          autoComplete="email"
          error={errors.email?.message}
          {...register('email')}
        />
        <TextField
          label={t('auth.password')}
          type="password"
          autoComplete="new-password"
          error={errors.password?.message}
          {...register('password')}
        />
        <TextField
          label={t('auth.confirmPassword')}
          type="password"
          autoComplete="new-password"
          error={errors.confirmPassword?.message}
          {...register('confirmPassword')}
        />
        <button
          type="submit"
          disabled={isSubmitting}
          className="w-full rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-60"
        >
          {isSubmitting ? t('auth.register.submitting') : t('auth.register.submit')}
        </button>
      </form>
        {isSubmitting && <SlowRequestHint />}
      <p className="mt-6 text-center text-sm text-slate-500">
        {t('auth.register.haveAccount')}{' '}
        <Link to="/login" className="font-medium text-indigo-600 hover:text-indigo-700">
          {t('auth.register.signIn')}
        </Link>
      </p>
    </>
  )
}