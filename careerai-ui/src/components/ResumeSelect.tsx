import { useTranslation } from 'react-i18next'
import type { Resume } from '../types/resume'

interface ResumeSelectProps {
  resumes: Resume[]
  value: string
  onChange: (resumeId: string) => void
  disabled?: boolean
  id?: string
}

export default function ResumeSelect({ resumes, value, onChange, disabled = false, id = 'resume-select' }: ResumeSelectProps) {
  const { t } = useTranslation()

  return (
    <div>
      <label htmlFor={id} className="block text-sm font-medium text-slate-700">
        {t('analysis.resumeLabel')}
      </label>
      <select
        id={id}
        value={value}
        disabled={disabled}
        onChange={(event) => onChange(event.target.value)}
        className="mt-1 block w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm shadow-sm outline-none focus:ring-2 focus:ring-indigo-500 disabled:opacity-60"
      >
        {resumes.map((resume) => (
          <option key={resume.id} value={resume.id}>
            {resume.fileName} · {new Date(resume.createdAt).toLocaleDateString('tr-TR')}
          </option>
        ))}
      </select>
    </div>
  )
}