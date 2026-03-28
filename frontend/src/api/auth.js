import client from './client'

export async function login(email, password) {
  const { data } = await client.post('/api/v1/auth/login', { email, password })
  return data
}

export async function register(email, password, firstName, lastName) {
  const { data } = await client.post('/api/v1/auth/register', { email, password, firstName, lastName })
  return data
}

export async function getMe() {
  const { data } = await client.get('/api/v1/users/me')
  return data
}
