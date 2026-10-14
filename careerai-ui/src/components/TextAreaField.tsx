import { forwardRef } from 'react'
import type { TextareaHTMLAttributes } from 'react'

interface TextAreaFieldProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label: string
  error?: string
  hint?: string // sağ altta küçük bilgi (örn. karakter sayacı)
}

const TextAreaField = forwardRef<HTMLTextAreaElement, TextAreaFieldProps>(function TextAreaField(
  { label, error, hint, id, ...props },
  ref,
) {
  const fieldId = id ?? props.name

  return (
    <div>
      <label htmlFor={fieldId} className="block text-sm font-medium text-slate-700">
        {label}
      </label>
      <textarea
        ref={ref}
        id={fieldId}
        aria-invalid={error ? true : undefined}
        className={`mt-1 block w-full rounded-lg border px-3 py-2 text-sm shadow-sm outline-none focus:ring-2 focus:ring-indigo-500 ${
          error ? 'border-red-400' : 'border-slate-300'
        }`}
        {...props}
      />
      <div className="mt-1 flex items-start justify-between gap-4 text-sm">
        {error ? <p className="text-red-600">{error}</p> : <span />}
        {hint && <p className="ml-auto shrink-0 text-xs text-slate-400">{hint}</p>}
      </div>
    </div>
  )
})

export default TextAreaField