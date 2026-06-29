import axios from '../axios'

export interface ClientOrder {
  id: number
  orderNumber: string
  orderDate: string
  appUserId: number
  clientName: string
  quantity: number
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
  getAll: () => axios.get<ClientOrder[]>('/ClientOrder'),
  getById: (id: number) => axios.get<ClientOrder>(`/ClientOrder/${id}`),
  create: (dto: CreateClientOrderDTO) => axios.post<CreateClientOrderResult>('/ClientOrder', dto),
  delete: (id: number) => axios.delete(`/ClientOrder/${id}`),
}