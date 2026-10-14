import type { ReactNode } from 'react'

interface StatCardProps {
  title: string
  children: ReactNode
}

export default function StatCard({ title, children }: StatCardProps) {
  return (
    <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
      <p className="text-sm font-medium text-slate-500">{title}</p>
      <div className="mt-2">{children}</div>
    </div>
  )
}