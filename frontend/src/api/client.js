import axios from 'axios'

const client = axios.create({
  headers: { 'Content-Type': 'application/json' },
})

client.interceptors.request.use((config) => {
  const token = localStorage.getItem('nexusgrid_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  config.headers['X-Correlation-Id'] = crypto.randomUUID()
  return config
})

client.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('nexusgrid_token')
      localStorage.removeItem('nexusgrid_user')
      window.location.href = '/login'
    }
    const data = error.response?.data || { error: 'Network error', code: 'NETWORK_ERROR' }
    return Promise.reject(data)
  }
)

export default client
