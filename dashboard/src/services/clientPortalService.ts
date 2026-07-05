import axios from '@/axios'

export interface ClientOrderSummary {
  id: number
  orderNumber: string
  orderDate: string
  totalCars: number
  completedCars: number
  status: string
  modelName: string
  etaUtc: string | null
  etaIsMlPrediction: boolean
}

export interface ClientProduct {
  productId: number
  serialNumber: string
  modelName: string
  currentPhase: string
  isCompleted: boolean
  etaUtc: string | null
  etaIsMlPrediction: boolean
}

export interface ClientOrderDetail {
  orderId: number
  orderNumber: string
  orderDate: string
  products: ClientProduct[]
}

export interface ClientCarModel {
  id: number
  name: string
  version: string | null
  type: string | null
}

export interface ClientConfigOption {
  id: number
  configId: number
  value: string
  isDefault: boolean
}

export interface ClientModelConfig {
  id: number
  item: string
  allowMultiple: boolean
  options: ClientConfigOption[]
}

export interface ClientAccount {
  id: number
  name: string
  email: string
  role: string
  status: string
  createdAt: string
}

export interface ClientNotification {
  id: number
  type: 'order_started' | 'order_completed' | 'car_completed'
  message: string
  clientOrderId: number | null
  productId: number | null
  isRead: boolean
  createdAt: string
}

export interface PagedResult<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
}

export const clientPortalService = {
  // Portal do cliente (role=client)
  getMyOrders(): Promise<ClientOrderSummary[]> {
    return axios.get('/client/orders').then(r => r.data?.$values ?? r.data ?? [])
  },

  getOrderDetail(orderId: number): Promise<ClientOrderDetail> {
    return axios.get(`/client/orders/${orderId}/products`).then(r => r.data)
  },

  // Fluxo "Nova Encomenda" — catálogo de modelos e respetivas configurações.
  getModels(): Promise<ClientCarModel[]> {
    return axios.get('/client/models').then(r => r.data?.$values ?? r.data ?? [])
  },

  getModel(modelId: number): Promise<ClientCarModel> {
    return axios.get(`/client/models/${modelId}`).then(r => r.data)
  },

  getModelConfigs(modelId: number): Promise<ClientModelConfig[]> {
    return axios.get(`/client/models/${modelId}/configs`).then(r => r.data?.$values ?? r.data ?? [])
  },

  // Gestão de contas de cliente (admin/manager) — paginado, com busca por nome
  getClientsPaged(params: { page?: number; pageSize?: number; search?: string }): Promise<PagedResult<ClientAccount>> {
    return axios.get('/client-accounts', { params }).then(r => r.data)
  },

  // Lista completa, sem paginação (uso pontual, ex: exports)
  getClients(): Promise<ClientAccount[]> {
    return axios.get('/client-accounts/all').then(r => r.data?.$values ?? r.data ?? [])
  },

  // A password não é escolhida — é gerada automaticamente pelo backend (igual
  // ao registo de Manager/Operator). A resposta inclui temporaryPassword.
  createClient(data: { name: string; email: string }): Promise<{ user: ClientAccount; temporaryPassword: string }> {
    return axios.post('/client-accounts', data).then(r => r.data)
  },

  updateClient(id: number, data: { name: string; email: string; status?: string }): Promise<void> {
    return axios.put(`/client-accounts/${id}`, data)
  },

  toggleStatus(id: number): Promise<{ status: string }> {
    return axios.put(`/client-accounts/${id}/toggle-status`).then(r => r.data)
  },

  // Reset gera password temporária nova automaticamente — sem corpo no request.
  resetPassword(id: number): Promise<{ temporaryPassword: string }> {
    return axios.put(`/client-accounts/${id}/reset-password`).then(r => r.data)
  },

  // Sino de notificações (Card N) — encomenda iniciada/concluída, carro concluído.
  getNotifications(): Promise<{ items: ClientNotification[]; unreadCount: number }> {
    return axios.get('/client/notifications').then(r => ({
      items: r.data?.items?.$values ?? r.data?.items ?? [],
      unreadCount: r.data?.unreadCount ?? 0,
    }))
  },

  markNotificationRead(id: number): Promise<void> {
    return axios.post(`/client/notifications/${id}/read`)
  },

  markAllNotificationsRead(): Promise<void> {
    return axios.post('/client/notifications/read-all')
  },

  clearAllNotifications(): Promise<void> {
    return axios.delete('/client/notifications')
  }
}