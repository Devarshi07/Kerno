import { useAuth } from '../../contexts/AuthContext'
import { useApiQuery } from '../../hooks/useApiQuery'
import { getNotificationsByUser } from '../../api/notifications'
import Card from '../../components/ui/Card'
import Spinner from '../../components/ui/Spinner'
import Badge from '../../components/ui/Badge'
import { formatRelativeTime } from '../../utils/formatters'
import { Bell, Package, UserPlus, AlertCircle } from 'lucide-react'

const TYPE_ICONS = {
  OrderCreated: Package,
  OrderStatusChanged: Package,
  OrderCancelled: AlertCircle,
  AccountCreated: UserPlus,
  System: Bell,
}

export default function NotificationInboxPage() {
  const { user } = useAuth()
  const { data: notifications, isLoading } = useApiQuery(
    () => getNotificationsByUser(user.id), [user?.id])

  if (isLoading) return <Spinner />

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <h2 className="text-2xl font-bold text-gray-900">Notifications</h2>

      {notifications?.length > 0 ? (
        <div className="space-y-3">
          {notifications.map((n) => {
            const Icon = TYPE_ICONS[n.type] || Bell
            const isRead = n.status === 'Read'
            return (
              <Card key={n.id} className={`flex gap-4 ${isRead ? 'opacity-60' : ''}`}>
                <div className="p-2 bg-indigo-50 rounded-lg h-fit"><Icon className="h-5 w-5 text-indigo-600" /></div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between gap-2">
                    <p className={`text-sm ${isRead ? 'text-gray-600' : 'font-semibold text-gray-900'}`}>{n.title}</p>
                    <Badge className={n.status === 'Pending' ? 'bg-yellow-100 text-yellow-800' : 'bg-gray-100 text-gray-600'}>
                      {n.status}
                    </Badge>
                  </div>
                  <p className="text-sm text-gray-500 mt-1">{n.message}</p>
                  <p className="text-xs text-gray-400 mt-2">{formatRelativeTime(n.createdAt)}</p>
                </div>
              </Card>
            )
          })}
        </div>
      ) : (
        <Card className="text-center py-12">
          <Bell className="h-12 w-12 text-gray-300 mx-auto mb-3" />
          <p className="text-gray-400">No notifications yet</p>
        </Card>
      )}
    </div>
  )
}
