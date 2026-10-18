import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'

// Yalnızca bir istek sürerken gösterilir (mount/unmount ile). 6 saniyeden uzun sürerse açıklama çıkar.
export default function SlowRequestHint() {
  const { t } = useTranslation()
  const [visible, setVisible] = useState(false)

  useEffect(() => {
    const timer = setTimeout(() => setVisible(true), 6000)
    return () => clearTimeout(timer)
  }, [])

  if (!visible) return null

  return (
    <p role="status" className="mt-3 text-xs text-slate-500">
      {t('common.slowRequest')}
    </p>
  )
}