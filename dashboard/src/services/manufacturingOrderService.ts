import axios from '../axios'

export interface ManufacturingOrder {
  id: number
  clientOrderId: number
  customerName: string
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
  getAll: () => axios.get<ManufacturingOrder[]>('/api/ManufacturingOrder'),
  getById: (id: number) => axios.get<ManufacturingOrder>(`/api/ManufacturingOrder/${id}`),
  getByStatus: (status: string) => axios.get<ManufacturingOrder[]>(`/api/ManufacturingOrder/status/${status}`),
  update: (id: number, dto: UpdateManufacturingOrderDTO) => axios.put(`/api/ManufacturingOrder/${id}`, dto),
  delete: (id: number) => axios.delete(`/api/ManufacturingOrder/${id}`),
}