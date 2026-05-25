import axios from '../axios'

export interface ProductionLine {
  id: number
  name: string
  location?: string
  status?: string
  capacity?: number
}

export interface Workstation {
  id: number
  productionLineId: number
  type?: string
  manufacturingPhaseId?: number | null
  phaseName?: string | null
}

export interface CreateProductionLineDTO {
  name: string
  location?: string
  status?: string
  capacity?: number
}

export interface UpdateProductionLineDTO {
  name?: string
  location?: string
  status?: string
  capacity?: number
}

export interface CreateWorkstationDTO {
  productionLineId: number
  type?: string
  manufacturingPhaseId?: number | null
}

export interface UpdateWorkstationDTO {
  type?: string
  manufacturingPhaseId?: number | null
}

export const productionLineService = {
  getAll: () => axios.get<ProductionLine[]>('/ProductionLine'),
  getById: (id: number) => axios.get<ProductionLine>(`/ProductionLine/${id}`),
  create: (dto: CreateProductionLineDTO) => axios.post<ProductionLine>('/ProductionLine', dto),
  update: (id: number, dto: UpdateProductionLineDTO) => axios.put(`/ProductionLine/${id}`, dto),
  delete: (id: number) => axios.delete(`/ProductionLine/${id}`),
}

export const workstationService = {
  getAll: () => axios.get<Workstation[]>('/Workstation'),
  getById: (id: number) => axios.get<Workstation>(`/Workstation/${id}`),
  getByLine: (lineId: number) => axios.get<Workstation[]>(`/Workstation/line/${lineId}`),
  create: (dto: CreateWorkstationDTO) => axios.post<Workstation>('/Workstation', dto),
  update: (id: number, dto: UpdateWorkstationDTO) => axios.put(`/Workstation/${id}`, dto),
  delete: (id: number) => axios.delete(`/Workstation/${id}`),
}