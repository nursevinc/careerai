export interface SkillMatch {
  name: string
  isRequired: boolean // ilanda zorunlu mu, tercih mi?
}

export interface Analysis {
  id: string
  jobApplicationId: string
  resumeId: string
  jobTitle: string
  companyName: string
  resumeFileName: string
  matchScore: number
  matchedSkills: SkillMatch[]
  missingSkills: SkillMatch[]
  experienceLevel: string // Intern | Junior | Mid | Senior | Lead | Unspecified
  otherRequirements: string[]
  summary: string | null
  recommendations: string[]
  createdAt: string
}