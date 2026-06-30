import axios from '../axios'

export interface ClientOption {
  id: number
  name: string
}

export interface TeamMember {
  id: number
  name: string
  email: string
  role: 'admin' | 'manager' | 'operator'
  status: string
  mustChangePassword: boolean
  createdAt: string
}

export interface PagedResult<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
}

export const clientUserService = {
  getClients: () => axios.get<ClientOption[]>('/User/clients'),
  getTeamPaged(params: { page?: number; pageSize?: number; search?: string; role?: string }): Promise<PagedResult<TeamMember>> {
    return axios.get('/User', { params }).then(r => r.data)
  },
}