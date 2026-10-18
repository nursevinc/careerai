import { useEffect } from 'react'
import { api } from '../services/api'

// Ücretsiz barındırmada sunucu boşta uyur. Ana sayfa açılır açılmaz hafif bir istekle uyandırırız:
// ziyaretçi sayfayı okurken sunucu hazırlanır. /api/health veritabanına dokunmaz (veritabanını uyandırmak vCore harcar).
export function useServerWarmup(): void {
  useEffect(() => {
    api.get('/api/health', { timeout: 90_000 }).catch(() => {
      // Sonuç önemsiz: amaç sadece sunucuyu uyandırmak
    })
  }, [])
}