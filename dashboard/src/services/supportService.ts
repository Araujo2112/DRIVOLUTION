import axios from '../axios'

export interface Support {
  id: number
  productionLineId: number
  rfidTag: string | null
  type: string | null
}

export interface SupportedProduct {
  id: number
  supportId: number
  productId: number | null
  serialNumber: string | null
  modelName: string | null
  datetimeIni: string
  datetimeEnd: string | null
}

export interface SupportPaged {
  id: number
  productionLineId: number
  productionLineName: string
  rfidTag: string | null
  type: string | null
  isOccupied: boolean
  currentProductId: number | null
  currentSerialNumber: string | null
  currentModelName: string | null
}

export interface PagedResult<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
}

export interface CreateSupportDTO {
  productionLineId: number
  rfidTag?: string | null
  type?: string | null
}

export interface CreateSupportedProductDTO {
  supportId: number
  productId: number
}

export const supportService = {
  getPaged: (params: {
    page?: number
    pageSize?: number
    search?: string
    productionLineId?: number
    occupied?: boolean
  }) => axios.get<PagedResult<SupportPaged>>('/Support', { params }),

  getAll: () => axios.get<Support[]>('/Support/all'),
  create: (dto: CreateSupportDTO) => axios.post<Support>('/Support', dto),
  delete: (id: number) => axios.delete(`/Support/${id}`),
}

export const supportedProductService = {
  getCurrent: (supportId: number) => axios.get<SupportedProduct>(`/SupportedProduct/support/${supportId}/current`),
  associate: (dto: CreateSupportedProductDTO) => axios.post<SupportedProduct>('/SupportedProduct', dto),
  release: (id: number) => axios.put(`/SupportedProduct/${id}/close`),
}