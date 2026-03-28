import { useState } from 'react'
import { useApiQuery } from '../../hooks/useApiQuery'
import { getAuditEvents } from '../../api/audit'
import Card from '../../components/ui/Card'
import Spinner from '../../components/ui/Spinner'
import Badge from '../../components/ui/Badge'
import { formatDate } from '../../utils/formatters'
import { Shield } from 'lucide-react'

export default function AuditLogPage() {
  const [tenantId, setTenantId] = useState('default')
  const [date, setDate] = useState(new Date().toISOString().split('T')[0])

  const { data: events, isLoading, refetch } = useApiQuery(
    () => getAuditEvents(tenantId, date), [tenantId, date])

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <h2 className="text-2xl font-bold text-gray-900">Audit Log</h2>

      <Card className="flex flex-wrap gap-3 items-end">
        <div>
          <label className="block text-xs font-medium text-gray-500 mb-1">Tenant ID</label>
          <input value={tenantId} onChange={(e) => setTenantId(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-lg text-sm outline-none focus:ring-2 focus:ring-indigo-500" />
        </div>
        <div>
          <label className="block text-xs font-medium text-gray-500 mb-1">Date</label>
          <input type="date" value={date} onChange={(e) => setDate(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-lg text-sm outline-none focus:ring-2 focus:ring-indigo-500" />
        </div>
        <button onClick={refetch} className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700">
          Search
        </button>
      </Card>

      {isLoading ? <Spinner /> : events?.length > 0 ? (
        <div className="relative border-l-2 border-gray-200 ml-4 space-y-6">
          {events.map((event) => (
            <div key={event.id} className="relative pl-8">
              <div className="absolute -left-2.5 top-1 w-5 h-5 bg-indigo-600 rounded-full flex items-center justify-center">
                <Shield className="h-3 w-3 text-white" />
              </div>
              <Card className="space-y-2">
                <div className="flex items-center justify-between">
                  <Badge className="bg-indigo-100 text-indigo-800">{event.eventType}</Badge>
                  <span className="text-xs text-gray-400">{formatDate(event.eventTime)}</span>
                </div>
                <p className="text-sm text-gray-700">{event.description}</p>
                <div className="flex gap-4 text-xs text-gray-500">
                  <span>Actor: <strong>{event.actorId}</strong></span>
                  <span>Resource: <strong>{event.resourceType}/{event.resourceId}</strong></span>
                </div>
              </Card>
            </div>
          ))}
        </div>
      ) : (
        <Card className="text-center py-12">
          <Shield className="h-12 w-12 text-gray-300 mx-auto mb-3" />
          <p className="text-gray-400">No audit events found for this date</p>
        </Card>
      )}
    </div>
  )
}
