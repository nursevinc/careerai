import { useTranslation } from 'react-i18next'
import type { SkillMatch } from '../types/analysis'

interface SkillListProps {
  skills: SkillMatch[]
  variant: 'matched' | 'missing'
  emptyText: string
}

const CHIP_STYLES = {
  matched: 'border-emerald-200 bg-emerald-50 text-emerald-800',
  missing: 'border-red-200 bg-red-50 text-red-800',
} as const

export default function SkillList({ skills, variant, emptyText }: SkillListProps) {
  const { t } = useTranslation()

  if (skills.length === 0) {
    return <p className="text-sm text-slate-500">{emptyText}</p>
  }

  return (
    <ul className="flex flex-wrap gap-2">
      {skills.map((skill) => (
        <li
          key={skill.name}
          className={`inline-flex items-center gap-1.5 rounded-full border px-3 py-1 text-sm ${CHIP_STYLES[variant]}`}
        >
          {/* İşaret ve etiket: anlam sadece renge bağlı değil */}
          <span aria-hidden="true">{variant === 'matched' ? '✓' : '✕'}</span>
          {skill.name}
          <span className="text-xs opacity-70">
            {skill.isRequired ? t('analysis.required') : t('analysis.preferred')}
          </span>
        </li>
      ))}
    </ul>
  )
}