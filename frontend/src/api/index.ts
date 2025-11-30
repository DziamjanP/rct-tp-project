import axios, { type AxiosResponse, type InternalAxiosRequestConfig } from 'axios'
const BASE = import.meta.env.VITE_API_BASE ?? 'http://localhost:5000'
const api = axios.create({ baseURL: BASE, headers: { 'Content-Type': 'application/json' } })

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers = config.headers || {}
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})


api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem('token')

  if (token) {
    config.headers = config.headers ?? {}
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

export interface User {
  id: number | string
  phone: string
  password: string
  [key: string]: unknown
}

export interface RegisterResult {
  id: number | string
  status: string
}

export interface LoginInput {
  phone: string
  password: string
}

export type EntityName = string
export type ID = string | number

function setToken(token: string): void {
  localStorage.setItem('token', token)
}

function getUser(): User | null {
  const raw = localStorage.getItem('user')
  return raw ? JSON.parse(raw) : null
}

export default {
  // Auth
  register(payload: Record<string, any>) { return api.post('/auth/register', payload).then(r => r.data) },
  login(payload: { phone: string; password: string }) { return api.post('/auth/login', payload).then(r => r.data) },
  logout() { localStorage.removeItem('token'); localStorage.removeItem('user') },

  /* ---------------------------
     Generic Entity Helpers
  ---------------------------- */

  list<T = unknown>(entity: EntityName): Promise<T[]> {
    return api.get(`/${entity}`).then(r => r.data)
  },

  get<T = unknown>(entity: EntityName, id: ID): Promise<T> {
    return api.get(`/${entity}/${id}`).then(r => r.data)
  },

  create<T = unknown, P = unknown>(
    entity: EntityName,
    payload: P
  ): Promise<T> {
    return api.post(`/${entity}`, payload).then(r => r.data)
  },

  update<T = unknown, P = unknown>(
    entity: EntityName,
    id: ID,
    payload: P
  ): Promise<T> {
    // supports fallback for servers without PUT
    if (api.put) {
      return api.put(`/${entity}/${id}`, payload).then(r => r.data)
    }

    return api.post(`/${entity}/${id}`, payload).then(r => r.data)
  },

  remove<T = unknown>(entity: EntityName, id: ID): Promise<T> {
    return api.delete(`/${entity}/${id}`).then(r => r.data)
  },

  /* ---------------------------
     Ticket Workflows
  ---------------------------- */

  createTicket<T = unknown, P = unknown>(payload: P): Promise<T> {
    return api.post('/tickets', payload).then(r => r.data)
  },

  createTicketLock<T = unknown, P = unknown>(payload: P): Promise<T> {
    return api.post('/ticketlocks', payload).then(r => r.data)
  },

  createPayment<T = unknown, P = unknown>(payload: P): Promise<T> {
    return api.post('/payments', payload).then(r => r.data)
  },

  getTimeTable<T = unknown>(params?: Record<string, unknown>): Promise<T> {
    return api.get('/timetable', { params }).then(r => r.data)
  },

  /* ---------------------------
     Price Policy Perks
  ---------------------------- */

  addPerkToPolicy(
    policyId: ID,
    perkId: ID
  ): Promise<void> {
    return api.post(`/pricepolicies/${policyId}/perkgroups/${perkId}`)
      .then(() => undefined)
  },

  removePerkFromPolicy(
    policyId: ID,
    perkId: ID
  ): Promise<void> {
    return api.delete(`/pricepolicies/${policyId}/perkgroups/${perkId}`)
      .then(() => undefined)
  }
}
