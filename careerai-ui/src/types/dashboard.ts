export interface DashboardStats {
  totalApplications: number
  totalAnalyses: number // en az bir kez analiz edilmiş ilan sayısı
  averageScore: number | null
  highestScore: number | null
}

export interface ScoreBucket {
  label: string
  count: number
}

export interface MonthlyCount {
  month: string // "2026-10"
  count: number
}

export interface MissingSkillCount {
  name: string
  count: number
}

export interface RecentAnalysis {
  jobApplicationId: string
  companyName: string
  jobTitle: string
  matchScore: number
  analyzedAt: string
}

export interface Dashboard {
  stats: DashboardStats
  scoreDistribution: ScoreBucket[]
  applicationsOverTime: MonthlyCount[]
  topMissingSkills: MissingSkillCount[]
  recentAnalyses: RecentAnalysis[]
}