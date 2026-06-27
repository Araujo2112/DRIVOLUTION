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

    // O backend recusou o pedido porque a conta ainda tem password temporária
    // (ver MustChangePasswordMiddleware). Garante que o frontend não fica "preso"
    // a tentar chamadas que vão continuar a falhar e leva o user para o sítio certo.
    if (error.response?.status === 403 && error.response?.data?.code === 'PASSWORD_CHANGE_REQUIRED') {
      const userStr = localStorage.getItem('drivolution_user')
      try {
        const user = JSON.parse(userStr ?? 'null')
        if (user) {
          user.mustChangePassword = true
          localStorage.setItem('drivolution_user', JSON.stringify(user))
        }
      } catch {
        // ignora; pior caso, o router vai depender só do token
      }
      if (router.currentRoute.value.name !== 'ChangePassword') {
        router.push('/change-password')
      }
    }

    return Promise.reject(error)
  }
)

export default _axios