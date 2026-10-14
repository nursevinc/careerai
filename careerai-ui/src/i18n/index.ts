import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import tr from './locales/tr'

void i18n.use(initReactI18next).init({
  resources: { tr: { translation: tr } },
  lng: 'tr',
  fallbackLng: 'tr',
  interpolation: { escapeValue: false }, // React çıktıyı zaten escape eder
})

export default i18n