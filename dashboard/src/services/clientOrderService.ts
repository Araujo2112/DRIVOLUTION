import axios from '../axios'

export type OrderStatus = 'pending' | 'in_progress' | 'completed' | 'cancelled'

export interface ClientOrder {
  id: number
  orderNumber: string
  orderDate: string
  appUserId: number
  clientName: string
  quantity: number
  status: OrderStatus
}

export interface PagedResult<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
}

export interface GetOrdersParams {
  page?: number
  pageSize?: number
  search?: string
  status?: string
  dateFrom?: string
  dateTo?: string
}

export interface CreateClientOrderDTO {
  orderNumber: string
  orderDate: string
  appUserId: number
  quantity: number
  modelId: number
  configs: { configOptionId: number }[]
}

export interface ProductSummary {
  productId: number
  serialNumber: string
  moNumber: string
}

export interface CreateClientOrderResult {
  orderId: number
  clientName: string
  totalQuantity: number
  productsCreated: ProductSummary[]
}

export const clientOrderService = {
  getAll: () => axios.get<ClientOrder[]>('/ClientOrder/all'),
  getPaged: (params: GetOrdersParams = {}) =>
    axios.get<PagedResult<ClientOrder>>('/ClientOrder', { params }),
  getById: (id: number) => axios.get<ClientOrder>(`/ClientOrder/${id}`),
  create: (dto: CreateClientOrderDTO) => axios.post<CreateClientOrderResult>('/ClientOrder', dto),
  cancel: (id: number) => axios.patch(`/ClientOrder/${id}/cancel`),
  delete: (id: number) => axios.delete(`/ClientOrder/${id}`),
}