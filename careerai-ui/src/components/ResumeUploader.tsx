import { useRef, useState } from 'react'
import type { ChangeEvent, DragEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { getErrorCodeMessage, parseApiError } from '../utils/apiError'
import { validateResumeFile } from '../utils/resumeValidation'

interface ResumeUploaderProps {
  onUpload: (file: File) => Promise<void>
  disabled?: boolean
}

export default function ResumeUploader({ onUpload, disabled = false }: ResumeUploaderProps) {
  const { t } = useTranslation()
  const inputRef = useRef<HTMLInputElement>(null)
  const [isDragging, setIsDragging] = useState(false)
  const [isUploading, setIsUploading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleFile = async (file: File | undefined) => {
    if (!file) return
    setError(null)

    // Hızlı ön kontrol (sunucuya gitmeden). Asıl doğrulama backend'de.
    const code = validateResumeFile(file)
    if (code) {
      setError(getErrorCodeMessage(code))
      return
    }

    setIsUploading(true)
    try {
      await onUpload(file)
    } catch (err) {
      setError(parseApiError(err).message)
    } finally {
      setIsUploading(false)
    }
  }

  const handleInputChange = (event: ChangeEvent<HTMLInputElement>) => {
    void handleFile(event.target.files?.[0])
    event.target.value = '' // aynı dosyayı tekrar seçebilmek için sıfırla
  }

  const handleDrop = (event: DragEvent<HTMLDivElement>) => {
    event.preventDefault()
    setIsDragging(false)
    if (disabled || isUploading) return
    void handleFile(event.dataTransfer.files[0])
  }

  const isBusy = disabled || isUploading

  return (
    <div>
      <div
        onDragOver={(event) => {
          event.preventDefault() // bırakmaya izin vermek için gerekli
          if (!isBusy) setIsDragging(true)
        }}
        onDragLeave={() => setIsDragging(false)}
        onDrop={handleDrop}
        className={`rounded-xl border-2 border-dashed p-8 text-center transition-colors ${
          isDragging ? 'border-indigo-500 bg-indigo-50' : 'border-slate-300 bg-white'
        } ${disabled ? 'opacity-60' : ''}`}
      >
        {isUploading ? (
          <div className="flex flex-col items-center gap-3" role="status">
            <div className="h-7 w-7 animate-spin rounded-full border-4 border-slate-300 border-t-indigo-600" />
            <p className="text-sm text-slate-600">{t('resumes.upload.uploading')}</p>
          </div>
        ) : (
          <>
            <p className="text-sm font-medium text-slate-700">{t('resumes.upload.dropHere')}</p>
            <p className="my-2 text-xs text-slate-400">{t('resumes.upload.or')}</p>
            <button
              type="button"
              disabled={isBusy}
              onClick={() => inputRef.current?.click()}
              className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {t('resumes.upload.choose')}
            </button>
            <p className="mt-3 text-xs text-slate-500">{t('resumes.upload.hint')}</p>
          </>
        )}

        <input
          ref={inputRef}
          type="file"
          accept="application/pdf,.pdf"
          className="hidden"
          onChange={handleInputChange}
        />
      </div>

      {error && (
        <div role="alert" className="mt-3 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}
    </div>
  )
}