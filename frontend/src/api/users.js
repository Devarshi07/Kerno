import client from './client'

export async function getUsers(page = 1, pageSize = 20) {
  const { data } = await client.get(`/api/v1/users?page=${page}&pageSize=${pageSize}`)
  return data
}

export async function updateProfile(firstName, lastName) {
  const { data } = await client.put('/api/v1/users/me', { firstName, lastName })
  return data
}

export async function deleteUser(id) {
  await client.delete(`/api/v1/users/${id}`)
}
