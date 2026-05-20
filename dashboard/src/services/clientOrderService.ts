import axios from '../axios'

export interface ClientOrder {
  id: number
  orderNumber: string
  orderDate: string
  customerName: string
  quantity: number
}

export interface CreateClientOrderDTO {
  orderNumber: string
  orderDate: string
  customerName: string
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
  customerName: string
  totalQuantity: number
  productsCreated: ProductSummary[]
}

export const clientOrderService = {
  getAll: () => axios.get<ClientOrder[]>('/api/ClientOrder'),
  getById: (id: number) => axios.get<ClientOrder>(`/api/ClientOrder/${id}`),
  create: (dto: CreateClientOrderDTO) => axios.post<CreateClientOrderResult>('/api/ClientOrder', dto),
  delete: (id: number) => axios.delete(`/api/ClientOrder/${id}`),
}
