import { useState } from 'react'
import { useTranslation } from 'react-i18next'

interface ConfirmDeleteButtonProps {
  question: string
  onConfirm: () => Promise<void>
  onError: (error: unknown) => void
}

// "Sil" -> "Silinsin mi? Evet / Vazgeç" onayını satırın içinde gösterir
export default function ConfirmDeleteButton({ question, onConfirm, onError }: ConfirmDeleteButtonProps) {
  const { t } = useTranslation()
  const [confirming, setConfirming] = useState(false)
  const [busy, setBusy] = useState(false)

  const handleConfirm = async () => {
    setBusy(true)
    try {
      await onConfirm()
    } catch (error) {
      onError(error)
      setConfirming(false)
    } finally {
      setBusy(false)
    }
  }

  if (!confirming) {
    return (
      <button
        type="button"
        onClick={() => setConfirming(true)}
        className="rounded-lg border border-red-200 px-3 py-1.5 text-sm font-medium text-red-600 hover:bg-red-50"
      >
        {t('common.delete')}
      </button>
    )
  }

  return (
    <div className="flex flex-wrap items-center gap-2">
      <span className="text-sm text-slate-600">{question}</span>
      <button
        type="button"
        disabled={busy}
        onClick={() => void handleConfirm()}
        className="rounded-lg bg-red-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-60"
      >
        {busy ? t('common.deleting') : t('common.confirmYes')}
      </button>
      <button
        type="button"
        disabled={busy}
        onClick={() => setConfirming(false)}
        className="rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 hover:bg-slate-100"
      >
        {t('common.cancel')}
      </button>
    </div>
  )
}