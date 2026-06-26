import axios from 'axios'
import router from './router'

const _axios = axios.create({
  baseURL: import.meta.env.VITE_API_URL
})

_axios.interceptors.request.use(config => {
  const token = localStorage.getItem('drivolution_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

_axios.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      localStorage.removeItem('drivolution_token')
      localStorage.removeItem('drivolution_user')
      router.push('/login')
    }
    return Promise.reject(error)
  }
)

export default _axios