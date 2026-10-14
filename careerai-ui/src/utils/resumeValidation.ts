import type { KnownErrorCode } from './apiError'

// Bu değerler backend'deki ResumeLimits ile aynı tutulmalı.
// Bu kontroller sadece hızlı geri bildirim içindir; asıl doğrulama backend'de.
export const MAX_RESUME_SIZE_MB = 5
export const MAX_RESUMES = 10

export function validateResumeFile(file: File): KnownErrorCode | null {
  if (file.size === 0) return 'file_required'
  if (file.size > MAX_RESUME_SIZE_MB * 1024 * 1024) return 'file_too_large'
  if (!file.name.toLowerCase().endsWith('.pdf')) return 'file_not_pdf'
  return null
}