import { useMemo, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useTranslation } from 'react-i18next'
import type { TFunction } from 'i18next'
import AnalyzingScreen from '../components/AnalyzingScreen'
import ResumeSelect from '../components/ResumeSelect'
import TextAreaField from '../components/TextAreaField'
import TextField from '../components/TextField'
import { useAnalyze } from '../hooks/useAnalyze'
import { useResumes } from '../hooks/useResumes'
import { jobApplicationService } from '../services/jobApplicationService'
import { isCancelledRequest, parseApiError } from '../utils/apiError'
import {
  MAX_DESCRIPTION_LENGTH,
  MAX_NAME_LENGTH,
  MIN_DESCRIPTION_LENGTH,
} from '../utils/jobApplicationLimits'

// Kurallar backend'deki CreateJobApplicationRequestValidator ile aynı
const createSchema = (t: TFunction) =>
  z.object({
    jobTitle: z
      .string()
      .trim()
      .min(1, t('validation.jobTitleRequired'))
      .max(MAX_NAME_LENGTH, t('validation.jobTitleTooLong')),
    companyName: z
      .string()
      .trim()
      .min(1, t('validation.companyRequired'))
      .max(MAX_NAME_LENGTH, t('validation.companyTooLong')),
    jobDescription: z
      .string()
      .trim()
      .min(MIN_DESCRIPTION_LENGTH, t('validation.descriptionMin', { min: MIN_DESCRIPTION_LENGTH }))
      .max(MAX_DESCRIPTION_LENGTH, t('validation.descriptionMax', { max: MAX_DESCRIPTION_LENGTH })),
  })

type FormValues = z.infer<ReturnType<typeof createSchema>>

export default function NewAnalysisPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { resumes, isLoading: resumesLoading, error: resumesError, reload } = useResumes()
  const { analyze, cancel, isAnalyzing } = useAnalyze()

  const [selectedResumeId, setSelectedResumeId] = useState<string | null>(null)
  // İlan kaydedildiyse ID'sini tutuyoruz: analiz başarısız olursa tekrar denemede ilanı İKİNCİ KEZ kaydetmeyiz
  const [createdJobId, setCreatedJobId] = useState<string | null>(null)
  const [serverError, setServerError] = useState<string | null>(null)

  const schema = useMemo(() => createSchema(t), [t])

  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { jobTitle: '', companyName: '', jobDescription: '' },
  })

  const description = useWatch({ control, name: 'jobDescription' })

  // Kullanıcı seçmediyse en yeni CV
  const resumeId = selectedResumeId ?? resumes[0]?.id ?? ''
  const isLocked = createdJobId !== null

  const onSubmit = async (values: FormValues) => {
    if (!resumeId) return
    setServerError(null)

    // Adım 1: ilanı kaydet (daha önce kaydedildiyse atla)
    let jobId = createdJobId
    if (jobId === null) {
      try {
        const created = await jobApplicationService.create(values)
        jobId = created.id
        setCreatedJobId(created.id)
      } catch (error) {
        setServerError(parseApiError(error).message)
        return
      }
    }

    // Adım 2: analiz et
    try {
      await analyze(jobId, resumeId)
      navigate(`/analysis/${jobId}`)
    } catch (error) {
      if (isCancelledRequest(error)) return // kullanıcı Vazgeç'e bastı ya da sayfadan ayrıldı
      setServerError(parseApiError(error).message)
    }
  }

  const renderResumeSection = () => {
    if (resumesLoading) {
      return (
        <div className="flex py-2" role="status">
          <div className="h-6 w-6 animate-spin rounded-full border-4 border-slate-300 border-t-indigo-600" />
        </div>
      )
    }

    if (resumesError) {
      return (
        <div role="alert" className="rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-700">
          <p>{resumesError}</p>
          <button type="button" onClick={reload} className="mt-2 font-medium underline">
            {t('common.retry')}
          </button>
        </div>
      )
    }

    if (resumes.length === 0) {
      return (
        <div className="rounded-lg border border-amber-200 bg-amber-50 p-4 text-sm text-amber-800">
          <p className="font-medium">{t('newAnalysis.noResumesTitle')}</p>
          <p className="mt-1">{t('newAnalysis.noResumesHint')}</p>
          <Link to="/resumes" className="mt-2 inline-block font-medium text-indigo-600 hover:text-indigo-700">
            {t('analysis.uploadResume')}
          </Link>
        </div>
      )
    }

    return (
      <ResumeSelect
        resumes={resumes}
        value={resumeId}
        onChange={setSelectedResumeId}
        disabled={isSubmitting}
      />
    )
  }

  return (
    <div className="max-w-3xl">
      <h1 className="text-2xl font-semibold text-slate-900">{t('newAnalysis.title')}</h1>
      <p className="mt-1 text-slate-500">{t('newAnalysis.subtitle')}</p>

      <div className="mt-6 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        {isLocked && (
          <div role="status" className="mb-4 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
            <p>{t('newAnalysis.savedButFailed')}</p>
            <Link
              to="/applications"
              className="mt-2 inline-block font-medium text-indigo-600 hover:text-indigo-700"
            >
              {t('newAnalysis.toApplications')}
            </Link>
          </div>
        )}

        {serverError && (
          <div role="alert" className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {serverError}
          </div>
        )}

        <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
          {/* readOnly (disabled değil): disabled alanlar react-hook-form değerlerinden düşer */}
          <div className="grid gap-4 sm:grid-cols-2">
            <TextField
              label={t('newAnalysis.jobTitle')}
              placeholder={t('newAnalysis.jobTitlePlaceholder')}
              readOnly={isLocked}
              error={errors.jobTitle?.message}
              {...register('jobTitle')}
            />
            <TextField
              label={t('newAnalysis.companyName')}
              placeholder={t('newAnalysis.companyPlaceholder')}
              readOnly={isLocked}
              error={errors.companyName?.message}
              {...register('companyName')}
            />
          </div>

          <TextAreaField
            label={t('newAnalysis.jobDescription')}
            placeholder={t('newAnalysis.descriptionPlaceholder')}
            rows={12}
            readOnly={isLocked}
            hint={t('newAnalysis.characters', { current: description.length, max: MAX_DESCRIPTION_LENGTH })}
            error={errors.jobDescription?.message}
            {...register('jobDescription')}
          />

          {renderResumeSection()}

          <div>
            <button
              type="submit"
              disabled={isSubmitting || resumesLoading || !resumeId}
              className="rounded-lg bg-indigo-600 px-5 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-60"
            >
              {isLocked ? t('newAnalysis.retry') : t('newAnalysis.submit')}
            </button>
            <p className="mt-3 text-xs text-slate-500">{t('newAnalysis.privacy')}</p>
          </div>
        </form>
      </div>

      {isAnalyzing && <AnalyzingScreen onCancel={cancel} />}
    </div>
  )
}