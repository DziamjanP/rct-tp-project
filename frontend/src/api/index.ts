import axios, { type AxiosResponse, type InternalAxiosRequestConfig } from 'axios'
const BASE = import.meta.env.VITE_API_BASE ?? 'http://localhost:5000'
const api = axios.create({ baseURL: BASE, headers: { 'Content-Type': 'application/json' } })

api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem('token')

  if (token) {
    config.headers = config.headers ?? {}
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

let isRefreshing = false
let refreshQueue: { resolve: (token: any) => void; reject: (reason?: any) => void }[] = []

function processQueue(error: unknown, token = null) {
  refreshQueue.forEach(promise => {
    if (error) {
      promise.reject(error)
    } else {
      promise.resolve(token)
    }
  })
  refreshQueue = []
}

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

api.interceptors.response.use(
  response => response,

  async error => {
    const originalRequest = error.config
    const status = error.response?.status

    if (status !== 401 || originalRequest._retry) {
      return Promise.reject(error)
    }

    originalRequest._retry = true

    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        refreshQueue.push({
          resolve: token => {
            originalRequest.headers.Authorization = `Bearer ${token}`
            resolve(api(originalRequest))
          },
          reject
        })
      })
    }

    isRefreshing = true

    try {
      const refreshToken = localStorage.getItem('refreshToken')

      const response = await axios.post(`${BASE}/auth/refresh`, {
        refreshToken
      })

      const newAccessToken = response.data.accessToken
      const newRefreshToken = response.data.refreshToken

      localStorage.setItem('token', newAccessToken)
      localStorage.setItem('refreshToken', newRefreshToken)

      api.defaults.headers.Authorization = `Bearer ${newAccessToken}`

      processQueue(null, newAccessToken)

      originalRequest.headers.Authorization = `Bearer ${newAccessToken}`
      return api(originalRequest)

    } catch (err) {
      processQueue(err, null)

      localStorage.clear()
      window.location.href = '/login'

      return Promise.reject(err)
    } finally {
      isRefreshing = false
    }
  }
)

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
  register(payload: Record<string, any>) { return api.post('/auth/register', payload).then(r => r.data) },
  login(payload: { phone: string; password: string }) { return api.post('/auth/login', payload).then(r => r.data) },
  logout() { localStorage.removeItem('token'); localStorage.removeItem('user') },

  list<T = unknown>(entity: EntityName, params?: Record<string, any>): Promise<T[]> {
    return api.get(`/${entity}`, { params }).then(r => r.data)
  },

  get<T = unknown>(entity: EntityName, id: ID): Promise<T> {
    return api.get(`/${entity}/${id}`).then(r => r.data)
  },

  estimatePrice<T = unknown>(ticket: ID, perkGroup?: ID): Promise<T> {
    let params: Record<string, unknown> = { entryId: ticket, perkGroupId: perkGroup };
    return api.get('/price', { params }).then(r => r.data)
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
    if (api.put) {
      return api.put(`/${entity}/${id}`, payload).then(r => r.data)
    }

    return api.post(`/${entity}/${id}`, payload).then(r => r.data)
  },

  remove<T = unknown>(entity: EntityName, id: ID): Promise<T> {
    return api.delete(`/${entity}/${id}`).then(r => r.data)
  },

  createTicket<T = unknown, P = unknown>(payload: P): Promise<T> {
    return api.post('/tickets', payload).then(r => r.data)
  },

  createTicketLock<T = unknown, P = unknown>(payload: P): Promise<T> {
    return api.post('/ticketlocks', payload).then(r => r.data)
  },

  createPayment<T = unknown, P = unknown>(payload: P): Promise<T> {
    return api.post('/pay', payload).then(r => r.data)
  },

  refundPayment<T = unknown, P = unknown>(ticketId: ID, payload: P): Promise<T> {
    return api.post(`/tickets/refund/${ticketId}`, payload).then(r => r.data)
  },

  getTimeTable<T = unknown>(params?: Record<string, unknown>): Promise<T> {
    return api.get('/timetable', { params }).then(r => r.data)
  },

  getReport<T = unknown>(params?: Record<string, unknown>): Promise<T> {
    return api.get('/report', { params }).then(r => r.data)
  },

  searchTimetable(params?: Record<string, unknown>) {
    return api
      .get('/timetable/search', { params })
      .then(res => res.data)
  },

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
