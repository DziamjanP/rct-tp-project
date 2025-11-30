import axios, {
  type AxiosInstance,
  type InternalAxiosRequestConfig,
  type AxiosResponse
} from 'axios'

/* -------------------------------------------------
   Environment
-------------------------------------------------- */

const BASE: string =
  import.meta.env.VITE_API_BASE ?? 'http://localhost:5041'

/* -------------------------------------------------
   Core Axios Instance
-------------------------------------------------- */

const api: AxiosInstance = axios.create({
  baseURL: BASE,
  headers: {
    'Content-Type': 'application/json'
  }
})

/* -------------------------------------------------
   Interceptor (attach token)
-------------------------------------------------- */

api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem('token')

  if (token) {
    config.headers = config.headers ?? {}
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

/* -------------------------------------------------
   Types
-------------------------------------------------- */

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

/* -------------------------------------------------
   LocalStorage Helpers
-------------------------------------------------- */

function setToken(token: string): void {
  localStorage.setItem('token', token)
}

function getUser(): User | null {
  const raw = localStorage.getItem('user')
  return raw ? JSON.parse(raw) : null
}

/* -------------------------------------------------
   API Object
-------------------------------------------------- */

const ApiService = {
  /* ---------------------------
      Auth (prototype)
  ---------------------------- */

  register<T = RegisterResult>(user: Partial<User>): Promise<T> {
    return api.post('/users', user).then(r => r.data)
  },

  async login({ phone, password }: LoginInput): Promise<User> {
    // Prototype auth: find user client-side (INSECURE)
    const res: AxiosResponse<User[]> = await api.get('/users')

    const user = res.data.find(
      u => u.phone === phone && u.password === password
    )

    if (!user) {
      throw new Error('Invalid credentials (dev only)')
    }

    const token = `dev-${user.id}`

    setToken(token)
    localStorage.setItem('user', JSON.stringify(user))

    return user
  },

  logout(): void {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
  },

  getCurrentUser(): User | null {
    return getUser()
  },

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

export default ApiService
