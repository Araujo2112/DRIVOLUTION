import axios from '../axios'

export interface CarModel {
  id: number
  name: string
  version: string | null
  type: string | null
}

export interface Config {
  id: number
  modelId: number
  item: string
}

export interface ConfigOption {
  id: number
  configId: number
  value: string
  isDefault: boolean
}

export interface CreateCarModelDTO {
  name: string
  version?: string
  type?: string
}

export interface CreateConfigDTO {
  modelId: number
  item: string
}

export interface CreateConfigOptionDTO {
  configId: number
  value: string
  isDefault: boolean
}

export const carModelService = {
  getAll: () => axios.get<CarModel[]>('/api/CarModel'),
  getById: (id: number) => axios.get<CarModel>(`/api/CarModel/${id}`),
  getConfigs: (id: number) => axios.get<Config[]>(`/api/CarModel/${id}/configs`),
  create: (dto: CreateCarModelDTO) => axios.post<CarModel>('/api/CarModel', dto),
  update: (id: number, dto: Partial<CreateCarModelDTO>) => axios.put(`/api/CarModel/${id}`, dto),
  delete: (id: number) => axios.delete(`/api/CarModel/${id}`),
}

export const configService = {
  getByModel: (modelId: number) => axios.get<Config[]>(`/api/Config/model/${modelId}`),
  create: (dto: CreateConfigDTO) => axios.post<Config>('/api/Config', dto),
  delete: (id: number) => axios.delete(`/api/Config/${id}`),
}

export const configOptionService = {
  getAll: () => axios.get<ConfigOption[]>('/api/ConfigOption'),
  create: (dto: CreateConfigOptionDTO) => axios.post<ConfigOption>('/api/ConfigOption', dto),
  delete: (id: number) => axios.delete(`/api/ConfigOption/${id}`),
}
