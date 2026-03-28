import client from './client'

export async function getAuditEvents(tenantId, date, limit = 50) {
  const params = new URLSearchParams({ limit })
  if (date) params.append('date', date)
  const { data } = await client.get(`/api/v1/audit/tenant/${tenantId}?${params}`)
  return data
}

export async function createAuditEvent(tenantId, eventType, actorId, resourceType, resourceId, description) {
  const { data } = await client.post('/api/v1/audit', { tenantId, eventType, actorId, resourceType, resourceId, description })
  return data
}
