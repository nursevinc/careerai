export interface Resume {
  id: string
  fileName: string
  createdAt: string
  extractedTextLength: number
}

export interface ResumeDetail extends Resume {
  extractedText: string
}