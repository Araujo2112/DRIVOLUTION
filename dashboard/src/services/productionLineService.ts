import axios from '../axios'

export interface ProductionLine {
  id: number
  name: string
  location: string | null
  status: string | null
  capacity: number | null
}

export interface Workstation {
  id: number
  productionLineId: number
  productionLineName: string | null
  type: string | null
}

export interface CreateProductionLineDTO {
  name: string
  location?: string
  status?: string
  capacity?: number
}

export interface CreateWorkstationDTO {
  productionLineId: number
  type?: string
}

export const productionLineService = {
  getAll: () => axios.get<ProductionLine[]>('/api/ProductionLine'),
  create: (dto: CreateProductionLineDTO) => axios.post<ProductionLine>('/api/ProductionLine', dto),
  update: (id: number, dto: Partial<CreateProductionLineDTO>) => axios.put(`/api/ProductionLine/${id}`, dto),
  delete: (id: number) => axios.delete(`/api/ProductionLine/${id}`),
}

export const workstationService = {
  getByLine: (lineId: number) => axios.get<Workstation[]>(`/api/Workstation/line/${lineId}`),
  create: (dto: CreateWorkstationDTO) => axios.post<Workstation>('/api/Workstation', dto),
  delete: (id: number) => axios.delete(`/api/Workstation/${id}`),
}