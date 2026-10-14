import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'

const STEP_KEYS = [
  'analysis.progress.step1',
  'analysis.progress.step2',
  'analysis.progress.step3',
  'analysis.progress.step4',
] as const

interface AnalyzingScreenProps {
  onCancel: () => void
}

// Analiz sürerken tüm ekranı kaplayan bekleme penceresi.
// Adımlar GERÇEK ilerleme değil, süreye göre ilerleyen bir ipucudur (sunucu ilerleme bildirmiyor).
export default function AnalyzingScreen({ onCancel }: AnalyzingScreenProps) {
  const { t } = useTranslation()
  const [step, setStep] = useState(0)

  useEffect(() => {
    const timer = setInterval(() => setStep((current) => Math.min(current + 1, STEP_KEYS.length - 1)), 5000)
    return () => clearInterval(timer)
  }, [])

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 p-6">
      <div
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="analyzing-title"
        aria-live="polite"
        className="w-full max-w-md rounded-xl bg-white p-8 shadow-xl"
      >
        <div className="flex items-center gap-3">
          <div className="h-6 w-6 animate-spin rounded-full border-4 border-slate-200 border-t-indigo-600" />
          <h2 id="analyzing-title" className="text-lg font-semibold text-slate-900">
            {t('analysis.progress.title')}
          </h2>
        </div>

        <ul className="mt-6 space-y-2 text-sm">
          {STEP_KEYS.map((key, index) => (
            <li
              key={key}
              className={
                index < step ? 'text-emerald-700' : index === step ? 'font-medium text-slate-900' : 'text-slate-400'
              }
            >
              <span aria-hidden="true" className="mr-2 inline-block w-4 text-center">
                {index < step ? '✓' : index === step ? '•' : ''}
              </span>
              {t(key)}
            </li>
          ))}
        </ul>

        <p className="mt-6 text-xs text-slate-500">{t('analysis.progress.hint')}</p>

        <button
          type="button"
          onClick={onCancel}
          className="mt-4 rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 hover:bg-slate-100"
        >
          {t('analysis.progress.cancel')}
        </button>
      </div>
    </div>
  )
}