import { defineStore } from 'pinia'
import api from '@/api'

interface User {
  id: string
  name: string
  surname: string
  passport: string
  phone: string
  accessLevel: number
}

interface AuthResponse {
  accessToken: string
  userId: string
  name: string
  surname: string
  passport: string
  phone: string
  accessLevel: number
  refreshToken: string
}


export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token'),
    refreshToken: localStorage.getItem('refreshToken'),
    user: JSON.parse(localStorage.getItem('user') || 'null'),
    loading: false,
  }),
  getters: {
    isAuthenticated: (state): boolean => !!state.token,
    accessLevel: (state): number =>
      state.user?.accessLevel ?? (state.user as any)?.AccessLevel ?? 0,
    isSupport: (state): boolean =>
      (state.user?.accessLevel ?? (state.user as any)?.AccessLevel ?? 0) > 0,
    isAdmin: (state): boolean =>
      (state.user?.accessLevel ?? (state.user as any)?.AccessLevel ?? 0) > 1,
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
        localStorage.setItem('token', res.accessToken)
        localStorage.setItem('user', JSON.stringify(user))
        localStorage.setItem('refreshToken', res.refreshToken)
        this.token = res.accessToken
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
      localStorage.setItem('token', res.accessToken)
      localStorage.setItem('user', JSON.stringify(user))
      localStorage.setItem('refreshToken', res.refreshToken)
      this.token = res.accessToken
      this.user = user
      return user
    },
  },
})
