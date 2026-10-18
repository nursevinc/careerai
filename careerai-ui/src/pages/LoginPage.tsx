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
const createLoginSchema = (t: TFunction) =>
  z.object({
    email: z.string().min(1, t('validation.emailRequired')).email(t('validation.emailInvalid')),
    password: z.string().min(1, t('validation.passwordRequired')),
  })

type LoginFormValues = z.infer<ReturnType<typeof createLoginSchema>>

export default function LoginPage() {
  const { t } = useTranslation()
  const { login } = useAuth()
  const [serverError, setServerError] = useState<string | null>(null)

  const schema = useMemo(() => createLoginSchema(t), [t])

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({ resolver: zodResolver(schema) })

  const onSubmit = async (values: LoginFormValues) => {
    setServerError(null)
    try {
      await login(values)
      // Başarılıysa GuestRoute kullanıcıyı otomatik olarak /dashboard'a yönlendirir
    } catch (error) {
      const parsed = parseApiError(error)
      // Giriş ekranındaki 401, oturum sonu değil yanlış e-posta/şifre demektir
      setServerError(parsed.status === 401 ? t('auth.errors.invalidCredentials') : parsed.message)
    }
  }

  return (
    <>
            <h2 className="text-xl font-semibold text-slate-900">{t('auth.login.title')}</h2>
      <p className="mb-6 mt-1 text-sm text-slate-500">{t('auth.loginSubtitle')}</p>

      {serverError && (
        <div role="alert" className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {serverError}
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
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
          autoComplete="current-password"
          error={errors.password?.message}
          {...register('password')}
        />
        <button
          type="submit"
          disabled={isSubmitting}
          className="w-full rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-60"
        >
          {isSubmitting ? t('auth.login.submitting') : t('auth.login.submit')}
        </button>
      </form>
              {isSubmitting && <SlowRequestHint />}

      <p className="mt-6 text-center text-sm text-slate-500">
        {t('auth.login.noAccount')}{' '}
        <Link to="/register" className="font-medium text-indigo-600 hover:text-indigo-700">
          {t('auth.login.createAccount')}
        </Link>
      </p>
    </>
  )
}