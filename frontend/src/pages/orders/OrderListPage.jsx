import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../contexts/AuthContext'
import { useApiQuery } from '../../hooks/useApiQuery'
import { getOrders, getOrdersByUser } from '../../api/orders'
import OrderStatusBadge from '../../components/orders/OrderStatusBadge'
import Spinner from '../../components/ui/Spinner'
import { formatCurrency, formatDate, truncateId } from '../../utils/formatters'
import { Plus, ChevronLeft, ChevronRight } from 'lucide-react'

export default function OrderListPage() {
  const { user, isAdmin } = useAuth()
  const navigate = useNavigate()
  const [page, setPage] = useState(1)
  const pageSize = 10

  const { data, isLoading } = useApiQuery(
    () => isAdmin ? getOrders(page, pageSize) : getOrdersByUser(user.id, page, pageSize),
    [page, isAdmin, user?.id]
  )

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-gray-900">Orders</h2>
        <button onClick={() => navigate('/orders/new')}
          className="flex items-center gap-2 bg-indigo-600 text-white px-4 py-2 rounded-lg hover:bg-indigo-700 transition-colors text-sm font-medium">
          <Plus className="h-4 w-4" />New Order
        </button>
      </div>

      {isLoading ? <Spinner /> : (
        <>
          <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Order ID</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Items</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Total</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Created</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {data?.items?.length > 0 ? data.items.map((order) => (
                    <tr key={order.id} onClick={() => navigate(`/orders/${order.id}`)}
                      className="hover:bg-gray-50 cursor-pointer transition-colors">
                      <td className="px-6 py-4 text-sm font-mono">{truncateId(order.id)}</td>
                      <td className="px-6 py-4"><OrderStatusBadge status={order.status} /></td>
                      <td className="px-6 py-4 text-sm text-gray-600">{order.items?.length || 0} item(s)</td>
                      <td className="px-6 py-4 text-sm font-medium">{formatCurrency(order.totalAmount)}</td>
                      <td className="px-6 py-4 text-sm text-gray-500">{formatDate(order.createdAt)}</td>
                    </tr>
                  )) : (
                    <tr><td colSpan={5} className="px-6 py-12 text-center text-gray-400">No orders found</td></tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {/* Pagination */}
          {data && data.totalPages > 1 && (
            <div className="flex items-center justify-between">
              <p className="text-sm text-gray-500">
                Page {data.page} of {data.totalPages} ({data.totalCount} total)
              </p>
              <div className="flex gap-2">
                <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page === 1}
                  className="p-2 border border-gray-300 rounded-lg disabled:opacity-50 hover:bg-gray-50">
                  <ChevronLeft className="h-4 w-4" />
                </button>
                <button onClick={() => setPage(p => Math.min(data.totalPages, p + 1))} disabled={page === data.totalPages}
                  className="p-2 border border-gray-300 rounded-lg disabled:opacity-50 hover:bg-gray-50">
                  <ChevronRight className="h-4 w-4" />
                </button>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  )
}
