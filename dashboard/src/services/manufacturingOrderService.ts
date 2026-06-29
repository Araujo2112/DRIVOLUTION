import axios from '../axios'

export interface ManufacturingOrder {
  id: number
  clientOrderId: number
  clientName: string
  manufacturingOrderNumber: string
  startDate: string
  endDate: string | null
  status: string | null
}

export interface UpdateManufacturingOrderDTO {
  status?: string
  endDate?: string
}

export const manufacturingOrderService = {
  getAll: () => axios.get<ManufacturingOrder[]>('/ManufacturingOrder'),
  getById: (id: number) => axios.get<ManufacturingOrder>(`/ManufacturingOrder/${id}`),
  getByStatus: (status: string) => axios.get<ManufacturingOrder[]>(`/ManufacturingOrder/status/${status}`),
  getDetails: (id: number) => axios.get(`/ManufacturingOrder/${id}/details`),
  update: (id: number, dto: UpdateManufacturingOrderDTO) => axios.put(`/ManufacturingOrder/${id}`, dto),
  delete: (id: number) => axios.delete(`/ManufacturingOrder/${id}`),
}