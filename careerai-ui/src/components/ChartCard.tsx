import type { ReactNode } from 'react'

interface ChartCardProps {
  title: string
  summary: string // ekran okuyucular için grafiğin metin özeti
  isEmpty: boolean
  emptyText: string
  children: ReactNode
  className?: string
}

export default function ChartCard({ title, summary, isEmpty, emptyText, children, className = '' }: ChartCardProps) {
  return (
    <section className={`rounded-xl border border-slate-200 bg-white p-5 shadow-sm ${className}`}>
      <h2 className="mb-4 text-base font-semibold text-slate-900">{title}</h2>

      {isEmpty ? (
        <p className="flex h-64 items-center justify-center px-4 text-center text-sm text-slate-500">{emptyText}</p>
      ) : (
        // SVG grafik ekran okuyucuya görünmez: sayıları özetleyen bir etiket veriyoruz.
        // Yükseklik sabit olmalı: ResponsiveContainer ebeveyninin boyutuna göre çizer.
        <div role="img" aria-label={summary} className="h-64">
          {children}
        </div>
      )}
    </section>
  )
}