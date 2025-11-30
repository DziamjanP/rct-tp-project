import { defineStore } from 'pinia'
import api, { type LoginInput, type User } from '../api'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: api.getCurrentUser(),
    loading: false
  }),
  actions: {
    async login(creds: LoginInput) {
      this.loading = true
      try {
        const user = await api.login(creds)
        this.user = user
        return user
      } finally {
        this.loading = false
      }
    },
    logout() {
      api.logout()
      this.user = null
    },
    async register(payload: Partial<User>) {
      const user = await api.register(payload)
      // the register endpoint returns created user object
      // for prototype set local user (insecure)
      localStorage.setItem('user', JSON.stringify(user))
      return user
    }
  }
})
