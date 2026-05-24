import axios from '../axios'

export interface ManufacturingPhase {
  id: number
  name: string
  estimatedDuration: number | null
  maxAcceptableSeverity: string
  reworkSeverity: string
}

export interface PhaseSequence {
  id: number
  order: number
  manufacturingPhaseId: number
  phaseName: string
  modelId: number
}

export interface CreateManufacturingPhaseDTO {
  name: string
  estimatedDuration?: number
  maxAcceptableSeverity: string
  reworkSeverity: string
}

export interface UpdateManufacturingPhaseDTO {
  name?: string
  estimatedDuration?: number
  maxAcceptableSeverity?: string
  reworkSeverity?: string
}

export interface CreatePhaseSequenceDTO {
  order: number
  manufacturingPhaseId: number
  modelId: number
}

export const manufacturingPhaseService = {
  getAll: () => axios.get<ManufacturingPhase[]>('/ManufacturingPhase'),
  getById: (id: number) => axios.get<ManufacturingPhase>(`/ManufacturingPhase/${id}`),
  create: (dto: CreateManufacturingPhaseDTO) => axios.post<ManufacturingPhase>('/ManufacturingPhase', dto),
  update: (id: number, dto: UpdateManufacturingPhaseDTO) => axios.put(`/ManufacturingPhase/${id}`, dto),
  delete: (id: number) => axios.delete(`/ManufacturingPhase/${id}`),
}

export const phaseSequenceService = {
  getByModel: (modelId: number) => axios.get<PhaseSequence[]>(`/PhaseSequence/model/${modelId}`),
  create: (dto: CreatePhaseSequenceDTO) => axios.post<PhaseSequence>('/PhaseSequence', dto),
  delete: (id: number) => axios.delete(`/PhaseSequence/${id}`),
}
