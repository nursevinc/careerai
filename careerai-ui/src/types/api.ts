// Backend'in hata formatı (RFC 7807 ProblemDetails)
export interface ApiProblem {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  traceId?: string
  code?: string // makine-okunur hata kodu (örn. "file_not_pdf")
  errors?: Record<string, string[]>
}