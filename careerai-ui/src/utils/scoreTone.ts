// Skor rozetinin renkleri (Tailwind sınıfları tam metin olarak yazılmalı). Renk tek başına bilgi taşımaz: yanında sayı var.
export function getScoreBadgeClass(score: number): string {
  if (score >= 75) return 'bg-emerald-50 text-emerald-700'
  if (score >= 50) return 'bg-amber-50 text-amber-700'
  return 'bg-red-50 text-red-700'
}