const TOKEN_KEY = 'careerai.accessToken'
const EXPIRES_KEY = 'careerai.expiresAt'

function clear(): void {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(EXPIRES_KEY)
}

function get(): string | null {
  const token = localStorage.getItem(TOKEN_KEY)
  const expiresAt = localStorage.getItem(EXPIRES_KEY)
  if (!token || !expiresAt) return null

  // Süresi dolmuş token'ı hiç kullanma
  if (new Date(expiresAt).getTime() <= Date.now()) {
    clear()
    return null
  }
  return token
}

function set(token: string, expiresAtUtc: string): void {
  localStorage.setItem(TOKEN_KEY, token)
  localStorage.setItem(EXPIRES_KEY, expiresAtUtc)
}

// Token'a erişen TEK modül. İleride cookie'ye geçersek sadece burası değişir.
export const tokenStorage = { get, set, clear }