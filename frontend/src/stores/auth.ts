import { defineStore } from 'pinia'
import api from '@/api'

// Define the shape of the User object
interface User {
  id: string
  name: string
  surname: string
  passport: string
  phone: string
  accessLevel: number
}

// Define the shape of the login/register response from the API
interface AuthResponse {
  token: string
  userId: string
  name: string
  surname: string
  passport: string
  phone: string
  accessLevel: number
}

// Define the state type
interface AuthState {
  user: User | null
  token: string | null
  loading: boolean
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    user: JSON.parse(localStorage.getItem('user') || 'null'),
    token: localStorage.getItem('token') || null,
    loading: false,
  }),
  getters: {
    isAuthenticated: (state): boolean => !!state.token,
    isAdmin: (state): boolean =>
      (state.user?.accessLevel ?? (state.user as any)?.AccessLevel ?? 0) > 0,
  },
  actions: {
    async login(payload: { phone: string; password: string }): Promise<User> {
      this.loading = true
      try {
        const res: AuthResponse = await api.login(payload)
        const user: User = {
          id: res.userId,
          name: res.name,
          surname: res.surname,
          phone: res.phone,
          passport: res.passport,
          accessLevel: res.accessLevel,
        }
        localStorage.setItem('token', res.token)
        localStorage.setItem('user', JSON.stringify(user))
        this.token = res.token
        this.user = user
        return user
      } finally {
        this.loading = false
      }
    },
    logout(): void {
      api.logout()
      this.user = null
      this.token = null
      localStorage.removeItem('token')
      localStorage.removeItem('user')
    },
    async register(payload: Record<string, any>): Promise<User> {
      const res: AuthResponse = await api.register(payload)
      const user: User = {
        id: res.userId,
        name: res.name,
        surname: res.surname,
        phone: res.phone,
        passport: res.passport,
        accessLevel: res.accessLevel,
      }
      localStorage.setItem('token', res.token)
      localStorage.setItem('user', JSON.stringify(user))
      this.token = res.token
      this.user = user
      return user
    },
  },
})
