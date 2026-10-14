import type tr from './locales/tr'

// t('...') çağrılarında anahtarlar otomatik tamamlanır, yanlış yazılan anahtar derleme hatası verir.
declare module 'i18next' {
  interface CustomTypeOptions {
    defaultNS: 'translation'
    resources: { translation: typeof tr }
  }
}