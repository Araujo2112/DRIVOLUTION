// Helper partilhado pelo Portal do Cliente para transformar um etaUtc (ISO string)
// numa frase relativa amigável ("amanhã", "daqui a 3 dias"...), em vez de uma
// data/hora em bruto. Usa as chaves client.orders.relative.* do i18n.
export function relativeEtaLabel(etaUtc: string, t: (key: string, n?: number) => string): string {
  const targetMs = new Date(etaUtc).getTime()
  const diffMs = targetMs - Date.now()

  if (diffMs <= 0) {
    // Previsão já passou (ex: atraso na linha) — tratamos como "hoje" em vez de
    // mostrar um número negativo, que confundiria o cliente.
    return t('client.orders.relative.today')
  }

  const diffHours = diffMs / 3_600_000
  if (diffHours < 24) {
    const hours = Math.max(1, Math.round(diffHours))
    return t('client.orders.relative.inHours', hours)
  }

  const diffDays = Math.round(diffHours / 24)
  if (diffDays === 1) return t('client.orders.relative.tomorrow')
  return t('client.orders.relative.inDays', diffDays)
}

export function absoluteEtaDate(etaUtc: string): string {
  return new Date(etaUtc).toLocaleString('pt-PT', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}