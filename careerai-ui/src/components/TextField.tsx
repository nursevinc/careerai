import { forwardRef, useState } from 'react'
import type { InputHTMLAttributes } from 'react'
import { useTranslation } from 'react-i18next'
import Icon from './Icon'

interface TextFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string
  error?: string
}

// react-hook-form'un register() fonksiyonu ref gerektirir, o yüzden forwardRef
const TextField = forwardRef<HTMLInputElement, TextFieldProps>(function TextField(
  { label, error, id, type, ...props },
  ref,
) {
  const { t } = useTranslation()
  const [passwordVisible, setPasswordVisible] = useState(false)

  const inputId = id ?? props.name
  const isPassword = type === 'password'

  return (
    <div>
      <label htmlFor={inputId} className="block text-sm font-medium text-slate-700">
        {label}
      </label>

      <div className="relative mt-1">
        <input
          ref={ref}
          id={inputId}
          type={isPassword && passwordVisible ? 'text' : type}
          aria-invalid={error ? true : undefined}
          aria-describedby={error ? `${inputId}-error` : undefined}
          className={`block w-full rounded-lg border bg-white px-3 py-2 text-sm text-slate-900 shadow-sm outline-none transition placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/30 read-only:bg-slate-50 read-only:text-slate-500 ${
            isPassword ? 'pr-10' : ''
          } ${error ? 'border-red-400' : 'border-slate-300'}`}
          {...props}
        />

        {isPassword && (
          <button
            type="button"
            onClick={() => setPasswordVisible((visible) => !visible)}
            aria-label={passwordVisible ? t('common.hidePassword') : t('common.showPassword')}
            aria-pressed={passwordVisible}
            className="absolute inset-y-0 right-0 flex items-center rounded-r-lg px-3 text-slate-400 hover:text-slate-600"
          >
            <Icon name={passwordVisible ? 'eyeOff' : 'eye'} className="h-4 w-4" />
          </button>
        )}
      </div>

      {error && (
        <p id={`${inputId}-error`} className="mt-1 text-sm text-red-600">
          {error}
        </p>
      )}
    </div>
  )
})

export default TextField