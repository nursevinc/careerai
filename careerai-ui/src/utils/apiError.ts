import axios from 'axios'
import i18n from '../i18n'
import type { ApiProblem } from '../types/api'

// Backend'in döndüğü makine-okunur hata kodları -> çeviri anahtarları
export const ERROR_CODE_KEYS = {
  file_required: 'errors.codes.file_required',
  file_too_large: 'errors.codes.file_too_large',
  file_not_pdf: 'errors.codes.file_not_pdf',
  pdf_unreadable: 'errors.codes.pdf_unreadable',
  pdf_no_text: 'errors.codes.pdf_no_text',
  pdf_too_many_pages: 'errors.codes.pdf_too_many_pages',
  resume_limit_reached: 'errors.codes.resume_limit_reached',
  ai_busy: 'errors.codes.ai_busy',
  ai_unavailable: 'errors.codes.ai_unavailable',
  ai_refused: 'errors.codes.ai_refused',
  ai_invalid_response: 'errors.codes.ai_invalid_response',
  rate_limited: 'errors.codes.rate_limited',
  resume_no_skills: 'errors.codes.resume_no_skills',
  job_no_skills: 'errors.codes.job_no_skills',
    invalid_password: 'errors.codes.invalid_password',
      registration_closed: 'errors.codes.registration_closed',
} as const

export type KnownErrorCode = keyof typeof ERROR_CODE_KEYS

export function isKnownErrorCode(code: unknown): code is KnownErrorCode {
  return typeof code === 'string' && Object.hasOwn(ERROR_CODE_KEYS, code)
}

export function getErrorCodeMessage(code: KnownErrorCode): string {
  return i18n.t(ERROR_CODE_KEYS[code])
}

// Kullanıcı isteği kendisi iptal ettiyse (Vazgeç, sayfadan ayrıldı) bu bir hata değildir
export function isCancelledRequest(error: unknown): boolean {
  return axios.isCancel(error)
}

export interface ParsedApiError {
  status?: number
  code?: KnownErrorCode
  message: string // kullanıcıya gösterilecek, Türkçe
}

// Axios hatasını kullanıcıya gösterilebilir tek bir mesaja çevirir.
// Duruma özel mesajlar (örn. girişteki 401, silmedeki 409) ilgili ekranda belirlenir.
export function parseApiError(error: unknown): ParsedApiError {
  if (!axios.isAxiosError(error)) {
    return { message: i18n.t('errors.generic') }
  }

  // response yok = sunucuya ulaşılamadı ya da istek zaman aşımına uğradı
  if (!error.response) {
    return {
      message: error.code === 'ECONNABORTED' ? i18n.t('errors.timeout') : i18n.t('errors.network'),
    }
  }

  const { status, data } = error.response
  const problem = typeof data === 'object' && data !== null ? (data as ApiProblem) : undefined

  // 1) Backend bilinen bir hata kodu gönderdiyse onu çevir
  const code = problem?.code
  if (isKnownErrorCode(code)) {
    return { status, code, message: getErrorCodeMessage(code) }
  }

  // 2) Değilse durum koduna göre genel mesaj
  if (status === 413) return { status, code: 'file_too_large', message: getErrorCodeMessage('file_too_large') }
  if (status === 404) return { status, message: i18n.t('errors.notFound') }
  if (status === 409) return { status, message: i18n.t('errors.conflict') }
  if (status >= 500) return { status, message: i18n.t('errors.server') }
  if (status === 400) return { status, message: i18n.t('errors.validation') }

  return { status, message: i18n.t('errors.generic') }
}