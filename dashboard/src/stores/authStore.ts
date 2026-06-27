import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import axios from '@/axios'
import router from '@/router'

export interface AuthUser {
  id: number
  name: string
  email: string
  role: 'admin' | 'operator' | 'client' | 'manager'
  status: string
}

const TOKEN_KEY = 'drivolution_token'
const USER_KEY  = 'drivolution_user'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))
  const user  = ref<AuthUser | null>(
    (() => {
      try { return JSON.parse(localStorage.getItem(USER_KEY) ?? 'null') }
      catch { return null }
    })()
  )

  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const isAdmin         = computed(() => user.value?.role === 'admin')
  const isOperator      = computed(() => user.value?.role === 'operator')
  const isManager       = computed(() => user.value?.role === 'manager')

  async function login(email: string, password: string): Promise<void> {
    const res = await axios.post('/Auth/login', { email, password })
    const { token: jwt, user: userData } = res.data

    token.value = jwt
    user.value  = userData

    localStorage.setItem(TOKEN_KEY, jwt)
    localStorage.setItem(USER_KEY,  JSON.stringify(userData))

    if (userData.role === 'client') {
  await router.push('/client')
} else if (userData.role === 'operator') {
  await router.push('/dashboard/production-line-status')
} else if (userData.role === 'manager') {
  await router.push('/dashboard/orders')
} else {
  await router.push('/dashboard')
}
  }

  function logout(): void {
    token.value = null
    user.value  = null
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
    router.push('/login')
  }

  return {
  token,
  user,
  isAuthenticated,
  isAdmin,
  isOperator,
  isManager,
  login,
  logout,
}
})