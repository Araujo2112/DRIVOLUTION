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
  getAll: () => axios.get<Support[]>('/Support'),
  create: (dto: CreateSupportDTO) => axios.post<Support>('/Support', dto),
  delete: (id: number) => axios.delete(`/Support/${id}`),
}

export const supportedProductService = {
  getCurrent: (supportId: number) => axios.get<SupportedProduct>(`/SupportedProduct/support/${supportId}/current`),
  associate: (dto: CreateSupportedProductDTO) => axios.post<SupportedProduct>('/SupportedProduct', dto),
  release: (id: number) => axios.put(`/SupportedProduct/${id}/close`),
}