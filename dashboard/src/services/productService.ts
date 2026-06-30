import axios from '../axios'

export interface Product {
  id: number
  manufacturingOrderId: number
  modelId: number
  modelName: string | null
  serialNumber: string | null
  lotNumber: string | null
  productionDate: string | null
}

export interface PagedResult<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
}

export interface ProductPhase {
  productPhaseId: number
  productId: number
  serialNumber: string
  phaseName: string
  workstation: string
  startedAt: string
  endedAt: string | null
  durationSeconds: number | null
  result: string | null
  notes: string | null
}

export interface ProductTimeline {
  productId: number
  serialNumber: string
  status: string
  phases: ProductPhase[]
}

export const productService = {
  getPaged: (params: {
    page?: number
    pageSize?: number
    search?: string
    modelId?: number
    dateFrom?: string
    dateTo?: string
  }) => axios.get<PagedResult<Product>>('/Product', { params }),

  getById: (id: number) => axios.get<Product>(`/Product/${id}`),
  getTimeline: (id: number) => axios.get<ProductTimeline>(`/products/${id}/timeline`),
}