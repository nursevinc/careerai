const firstLetter = (word: string | undefined): string => (word ? (Array.from(word)[0] ?? '') : '')

// "Nur Sevinç" -> "NS", "Nur" -> "N"
export function getInitials(name: string | null | undefined): string {
  const words = (name ?? '').trim().split(/\s+/).filter(Boolean)
  const letters = firstLetter(words[0]) + (words.length > 1 ? firstLetter(words[words.length - 1]) : '')

  return letters.toLocaleUpperCase('tr-TR') || '?'
}