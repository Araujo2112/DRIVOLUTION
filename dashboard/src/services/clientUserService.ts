import axios from '../axios'

export interface ClientOption {
  id: number
  name: string
}

export const clientUserService = {
  getClients: () => axios.get<ClientOption[]>('/User/clients'),
}