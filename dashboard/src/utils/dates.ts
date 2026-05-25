/**
 * Converte uma string UTC da API para hora local do browser.
 * A API devolve sempre UTC (ex: "2026-05-25T11:38:12").
 * O browser converte automaticamente para a timezone do utilizador.
 */
export function formatDateTime(utcString: string | null | undefined): string {
  if (!utcString) return '—'
  // Garantir que o string é tratado como UTC (adicionar Z se não tiver)
  const normalized = utcString.endsWith('Z') ? utcString : utcString + 'Z'
  return new Date(normalized).toLocaleString('pt-PT', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export function formatDate(utcString: string | null | undefined): string {
  if (!utcString) return '—'
  const normalized = utcString.endsWith('Z') ? utcString : utcString + 'Z'
  return new Date(normalized).toLocaleDateString('pt-PT', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  })
}

export function formatDuration(seconds: number | null | undefined): string {
  if (seconds == null) return '—'
  const h = Math.floor(seconds / 3600)
  const m = Math.floor((seconds % 3600) / 60)
  const s = seconds % 60
  if (h > 0) return `${h}h ${m}m`
  if (m > 0) return `${m}m ${s}s`
  return `${s}s`
}

export function elapsedSeconds(utcString: string | null | undefined): number {
  if (!utcString) return 0
  const normalized = utcString.endsWith('Z') ? utcString : utcString + 'Z'
  return Math.floor((Date.now() - new Date(normalized).getTime()) / 1000)
}