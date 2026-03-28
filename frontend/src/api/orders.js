import client from './client'

export async function getOrders(page = 1, pageSize = 20) {
  const { data } = await client.get(`/api/v1/orders?page=${page}&pageSize=${pageSize}`)
  return data
}

export async function getOrderById(id) {
  const { data } = await client.get(`/api/v1/orders/${id}`)
  return data
}

export async function getOrdersByUser(userId, page = 1, pageSize = 20) {
  const { data } = await client.get(`/api/v1/orders/user/${userId}?page=${page}&pageSize=${pageSize}`)
  return data
}

export async function createOrder(userId, items) {
  const { data } = await client.post('/api/v1/orders', { userId, items })
  return data
}

export async function updateOrderStatus(id, status) {
  const { data } = await client.patch(`/api/v1/orders/${id}/status`, { status })
  return data
}

export async function deleteOrder(id) {
  await client.delete(`/api/v1/orders/${id}`)
}
