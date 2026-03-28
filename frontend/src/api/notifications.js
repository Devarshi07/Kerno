import client from './client'

export async function getNotificationsByUser(userId, limit = 50) {
  const { data } = await client.get(`/api/v1/notifications/user/${userId}?limit=${limit}`)
  return data
}

export async function createNotification(userId, type, title, message, metadata = {}) {
  const { data } = await client.post('/api/v1/notifications', { userId, type, title, message, metadata })
  return data
}

export async function updateNotificationStatus(userId, createdAt, id, status) {
  await client.patch(`/api/v1/notifications/${userId}/${createdAt}/${id}/status`, { status })
}
