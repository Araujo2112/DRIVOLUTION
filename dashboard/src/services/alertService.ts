import axios from '../axios'

export interface Alert {
  id: number
  type: string
  status: string
  productId: number
  productPhaseId: number
  triggeredAt: string
  acknowledgedAt: string | null
  resolvedAt: string | null
  notes: string | null
  productSerial: string
  phaseName: string
  thresholdPct: number | null
  estimatedDuration: number | null
  orderFrom: number | null
  orderTo: number | null
}

export interface PagedResult<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
}

export const alertService = {
  getPaged: (params: {
    page?: number
    pageSize?: number
    type?: string
    status?: string
  }) => axios.get<PagedResult<Alert>>('/Alert', { params }),

  acknowledge: (id: number) => axios.put(`/Alert/${id}/acknowledge`),
  resolve: (id: number) => axios.put(`/Alert/${id}/resolve`),
}