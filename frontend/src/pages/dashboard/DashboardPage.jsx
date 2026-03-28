import { useAuth } from '../../contexts/AuthContext'
import { useApiQuery } from '../../hooks/useApiQuery'
import { getOrders, getOrdersByUser } from '../../api/orders'
import { getUsers } from '../../api/users'
import Card from '../../components/ui/Card'
import Spinner from '../../components/ui/Spinner'
import OrderStatusBadge from '../../components/orders/OrderStatusBadge'
import { formatCurrency, formatRelativeTime, truncateId } from '../../utils/formatters'
import { ORDER_STATUSES } from '../../utils/constants'
import { ShoppingCart, Users, Clock, DollarSign } from 'lucide-react'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, Cell } from 'recharts'
import { useNavigate } from 'react-router-dom'

const CHART_COLORS = ['#fbbf24', '#3b82f6', '#8b5cf6', '#6366f1', '#22c55e', '#ef4444']

export default function DashboardPage() {
  const { user, isAdmin } = useAuth()
  const navigate = useNavigate()

  const orders = useApiQuery(() =>
    isAdmin ? getOrders(1, 100) : getOrdersByUser(user.id, 1, 100), [isAdmin, user?.id])

  const users = useApiQuery(() => isAdmin ? getUsers(1, 1) : Promise.resolve(null), [isAdmin])

  if (orders.isLoading) return <Spinner />

  const allOrders = orders.data?.items || []
  const totalRevenue = allOrders.reduce((sum, o) => sum + o.totalAmount, 0)
  const pendingCount = allOrders.filter(o => o.status === 'Pending').length

  const statusData = ORDER_STATUSES.map((s, i) => ({
    name: s, count: allOrders.filter(o => o.status === s).length, fill: CHART_COLORS[i]
  })).filter(d => d.count > 0)

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-gray-900">Dashboard</h2>

      {/* Stats */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card className="flex items-center gap-4">
          <div className="p-3 bg-indigo-50 rounded-lg"><ShoppingCart className="h-6 w-6 text-indigo-600" /></div>
          <div>
            <p className="text-sm text-gray-500">{isAdmin ? 'Total Orders' : 'My Orders'}</p>
            <p className="text-2xl font-bold">{orders.data?.totalCount || 0}</p>
          </div>
        </Card>

        <Card className="flex items-center gap-4">
          <div className="p-3 bg-green-50 rounded-lg"><DollarSign className="h-6 w-6 text-green-600" /></div>
          <div>
            <p className="text-sm text-gray-500">Revenue</p>
            <p className="text-2xl font-bold">{formatCurrency(totalRevenue)}</p>
          </div>
        </Card>

        <Card className="flex items-center gap-4">
          <div className="p-3 bg-yellow-50 rounded-lg"><Clock className="h-6 w-6 text-yellow-600" /></div>
          <div>
            <p className="text-sm text-gray-500">Pending</p>
            <p className="text-2xl font-bold">{pendingCount}</p>
          </div>
        </Card>

        {isAdmin && (
          <Card className="flex items-center gap-4">
            <div className="p-3 bg-purple-50 rounded-lg"><Users className="h-6 w-6 text-purple-600" /></div>
            <div>
              <p className="text-sm text-gray-500">Total Users</p>
              <p className="text-2xl font-bold">{users.data?.totalCount || 0}</p>
            </div>
          </Card>
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Chart */}
        <Card>
          <h3 className="text-lg font-semibold mb-4">Orders by Status</h3>
          {statusData.length > 0 ? (
            <ResponsiveContainer width="100%" height={250}>
              <BarChart data={statusData}>
                <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                <YAxis allowDecimals={false} />
                <Tooltip />
                <Bar dataKey="count" radius={[4, 4, 0, 0]}>
                  {statusData.map((entry, i) => <Cell key={i} fill={entry.fill} />)}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-gray-400 text-center py-12">No orders yet</p>
          )}
        </Card>

        {/* Recent orders */}
        <Card>
          <h3 className="text-lg font-semibold mb-4">Recent Orders</h3>
          {allOrders.length > 0 ? (
            <div className="space-y-3">
              {allOrders.slice(0, 5).map((order) => (
                <div key={order.id}
                  onClick={() => navigate(`/orders/${order.id}`)}
                  className="flex items-center justify-between p-3 rounded-lg hover:bg-gray-50 cursor-pointer transition-colors">
                  <div>
                    <p className="font-medium text-sm">{truncateId(order.id)}</p>
                    <p className="text-xs text-gray-500">{formatRelativeTime(order.createdAt)}</p>
                  </div>
                  <div className="flex items-center gap-3">
                    <span className="text-sm font-medium">{formatCurrency(order.totalAmount)}</span>
                    <OrderStatusBadge status={order.status} />
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-gray-400 text-center py-12">No orders yet</p>
          )}
        </Card>
      </div>
    </div>
  )
}
