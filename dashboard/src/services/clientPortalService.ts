import axios from '@/axios'

export interface ClientOrderSummary {
  id: number
  orderNumber: string
  orderDate: string
  quantity: number
  completedProducts: number
  status: string
}

export interface ClientOrderProduct {
  vin: string
  currentPhase: string
  estimatedFinish?: string | null
}

export const clientPortalService = {
  getOrders() {
    return axios.get<ClientOrderSummary[]>('/client/orders')
  },

  getOrderProducts(orderId: number) {
    return axios.get<ClientOrderProduct[]>(`/client/orders/${orderId}/products`)
  },
}