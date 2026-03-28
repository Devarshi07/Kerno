import { useState, useEffect } from 'react'
import Card from '../../components/ui/Card'
import { Activity, CheckCircle, XCircle, RefreshCw } from 'lucide-react'

const SERVICES = [
  { name: 'API Gateway', url: '/health', port: 5000 },
  { name: 'Order Service', url: '/health', port: 5101 },
  { name: 'User Service', url: '/health', port: 5102 },
  { name: 'Notification Service', url: '/health', port: 5103 },
]

export default function HealthPage() {
  const [statuses, setStatuses] = useState({})
  const [checking, setChecking] = useState(false)

  const checkAll = async () => {
    setChecking(true)
    const results = {}
    for (const svc of SERVICES) {
      try {
        const res = await fetch(svc.url, { signal: AbortSignal.timeout(5000) })
        results[svc.name] = res.ok ? 'Healthy' : 'Unhealthy'
      } catch {
        results[svc.name] = 'Unreachable'
      }
    }
    setStatuses(results)
    setChecking(false)
  }

  useEffect(() => { checkAll() }, [])

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-gray-900">System Health</h2>
        <button onClick={checkAll} disabled={checking}
          className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg text-sm hover:bg-gray-50 disabled:opacity-50">
          <RefreshCw className={`h-4 w-4 ${checking ? 'animate-spin' : ''}`} />Refresh
        </button>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        {SERVICES.map((svc) => {
          const status = statuses[svc.name]
          const isHealthy = status === 'Healthy'
          const isChecking = !status

          return (
            <Card key={svc.name} className="flex items-center gap-4">
              <div className={`p-3 rounded-lg ${isHealthy ? 'bg-green-50' : isChecking ? 'bg-gray-50' : 'bg-red-50'}`}>
                {isChecking ? <Activity className="h-6 w-6 text-gray-400 animate-pulse" />
                  : isHealthy ? <CheckCircle className="h-6 w-6 text-green-600" />
                  : <XCircle className="h-6 w-6 text-red-600" />}
              </div>
              <div>
                <p className="font-medium text-sm">{svc.name}</p>
                <p className={`text-xs ${isHealthy ? 'text-green-600' : isChecking ? 'text-gray-400' : 'text-red-600'}`}>
                  {status || 'Checking...'}
                </p>
                <p className="text-xs text-gray-400">Port {svc.port}</p>
              </div>
            </Card>
          )
        })}
      </div>
    </div>
  )
}
