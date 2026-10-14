export interface JobApplication {
  id: string
  jobTitle: string
  companyName: string
  matchScore: number | null // null = henüz analiz edilmedi
  createdAt: string
}

export interface JobApplicationDetail extends JobApplication {
  jobDescription: string
}

export interface CreateJobApplicationRequest {
  jobTitle: string
  companyName: string
  jobDescription: string
}