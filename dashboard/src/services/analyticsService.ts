import axios from '@/axios'

export type PhaseDuration = {
  manufacturingPhaseId: number
  phaseName: string
  averageDurationMinutes: number
  completedCount: number
}

export type ReworkRate = {
  manufacturingPhaseId: number
  phaseName: string
  totalChecks: number
  failedChecks: number
  reworkRatePercent: number
}

export type Throughput = {
  period: string
  completedProducts: number
}

export const analyticsService = {
  getPhaseDurations() {
    return axios.get<PhaseDuration[]>('/analytics/phase-durations')
  },

  getReworkRate() {
    return axios.get<ReworkRate[]>('/analytics/rework-rate')
  },

  getThroughput() {
    return axios.get<Throughput[]>('/analytics/throughput')
  },
}