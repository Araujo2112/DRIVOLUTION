import { defineStore } from 'pinia'
import { ref } from 'vue'
import axios from '@/axios'

interface Alert {
  id: number
  type: string
  status: string
  productId: number
  productPhaseId: number
  triggeredAt: string
  productSerial?: string
  phaseName?: string
  thresholdPct?: number
  estimatedDuration?: number
  orderFrom?: number
  orderTo?: number
}

export const useAlertStore = defineStore('alert', () => {
  const activeAlerts = ref<Alert[]>([])
  const allAlerts = ref<Alert[]>([])
  const acknowledgedProductIds = ref<Set<number>>(new Set())
  const knownIds = ref<Set<number>>(new Set())
  let pollingInterval: ReturnType<typeof setInterval> | null = null

  const fetchOpenAlerts = async () => {
    try {
      const res = await axios.get('/Alert/open')
      const openAlerts: Alert[] = res.data?.$values ?? res.data ?? []

      const newAlerts = openAlerts.filter(a => !knownIds.value.has(a.id))
      newAlerts.forEach(a => {
        knownIds.value.add(a.id)
        activeAlerts.value.unshift(a)
      })
    } catch (err) {
      console.error('Erro ao buscar alertas:', err)
    }
  }

  const fetchAllAlerts = async () => {
    try {
      const res = await axios.get('/Alert/all')
      allAlerts.value = res.data?.$values ?? res.data ?? []
    } catch (err) {
      console.error('Erro ao buscar histórico de alertas:', err)
    }
  }

  const acknowledge = async (id: number) => {
    try {
      await axios.put(`/Alert/${id}/acknowledge`)
      const alert = activeAlerts.value.find(a => a.id === id)
      if (alert) acknowledgedProductIds.value.add(alert.productId)
      activeAlerts.value = activeAlerts.value.filter(a => a.id !== id)
    } catch (err) {
      console.error('Erro ao confirmar alerta:', err)
    }
  }

  const startPolling = () => {
    if (pollingInterval) return
    fetchOpenAlerts()
    pollingInterval = setInterval(fetchOpenAlerts, 15000)
  }

  const stopPolling = () => {
    if (pollingInterval) {
      clearInterval(pollingInterval)
      pollingInterval = null
    }
  }

  return {
    activeAlerts,
    allAlerts,
    acknowledgedProductIds,
    fetchOpenAlerts,
    fetchAllAlerts,
    acknowledge,
    startPolling,
    stopPolling,
  }
})