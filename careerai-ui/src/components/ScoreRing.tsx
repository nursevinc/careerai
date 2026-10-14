import { useTranslation } from 'react-i18next'

const RADIUS = 54
const CIRCUMFERENCE = 2 * Math.PI * RADIUS

// Tailwind sınıfları tam metin olarak yazılmalı (dinamik birleştirilen sınıflar derlemeye girmez)
function getTone(score: number) {
  if (score >= 75) {
    return { stroke: 'stroke-emerald-500', text: 'text-emerald-600', labelKey: 'analysis.fit.high' } as const
  }
  if (score >= 50) {
    return { stroke: 'stroke-amber-500', text: 'text-amber-600', labelKey: 'analysis.fit.medium' } as const
  }
  return { stroke: 'stroke-red-500', text: 'text-red-600', labelKey: 'analysis.fit.low' } as const
}

export default function ScoreRing({ score }: { score: number }) {
  const { t } = useTranslation()
  const tone = getTone(score)
  const clamped = Math.min(100, Math.max(0, score))

  return (
    <div className="flex shrink-0 flex-col items-center">
      <div
        role="img"
        aria-label={`${t('analysis.matchScore')}: %${clamped}`}
        className="relative h-40 w-40"
      >
        <svg viewBox="0 0 120 120" className="h-full w-full -rotate-90" aria-hidden="true">
          <circle cx="60" cy="60" r={RADIUS} fill="none" strokeWidth="10" className="stroke-slate-200" />
          <circle
            cx="60"
            cy="60"
            r={RADIUS}
            fill="none"
            strokeWidth="10"
            strokeLinecap="round"
            className={tone.stroke}
            strokeDasharray={CIRCUMFERENCE}
            strokeDashoffset={CIRCUMFERENCE * (1 - clamped / 100)}
          />
        </svg>
        <div className="absolute inset-0 flex items-center justify-center">
          <span className={`text-4xl font-bold ${tone.text}`}>%{clamped}</span>
        </div>
      </div>
      <p className="mt-2 text-sm font-medium text-slate-700">{t('analysis.matchScore')}</p>
      <p className={`text-sm font-semibold ${tone.text}`}>{t(tone.labelKey)}</p>
    </div>
  )
}