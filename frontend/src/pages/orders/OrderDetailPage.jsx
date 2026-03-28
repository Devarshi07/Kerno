import { useParams, useNavigate } from 'react-router-dom'
import { useState } from 'react'
import { useAuth } from '../../contexts/AuthContext'
import { useApiQuery } from '../../hooks/useApiQuery'
import { getOrderById, updateOrderStatus, deleteOrder } from '../../api/orders'
import Card from '../../components/ui/Card'
import Spinner from '../../components/ui/Spinner'
import OrderStatusBadge from '../../components/orders/OrderStatusBadge'
import Modal from '../../components/ui/Modal'
import { formatCurrency, formatDate } from '../../utils/formatters'
import { ORDER_STATUSES } from '../../utils/constants'
import { ArrowLeft, Trash2, Check } from 'lucide-react'
import toast from 'react-hot-toast'

export default function OrderDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { isAdmin } = useAuth()
  const { data: order, isLoading, refetch } = useApiQuery(() => getOrderById(id), [id])
  const [showDelete, setShowDelete] = useState(false)

  const handleStatusChange = async (status) => {
    try {
      await updateOrderStatus(id, status)
      toast.success(`Status updated to ${status}`)
      refetch()
    } catch (err) {
      toast.error(err?.error || 'Failed to update status')
    }
  }

  const handleDelete = async () => {
    try {
      await deleteOrder(id)
      toast.success('Order deleted')
      navigate('/orders')
    } catch (err) {
      toast.error(err?.error || 'Failed to delete')
    }
  }

  if (isLoading) return <Spinner />
  if (!order) return <p className="text-center text-gray-400 py-12">Order not found</p>

  const statusIndex = ORDER_STATUSES.indexOf(order.status)
  const isCancelled = order.status === 'Cancelled'

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <button onClick={() => navigate('/orders')} className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700">
        <ArrowLeft className="h-4 w-4" />Back to Orders
      </button>

      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Order Detail</h2>
          <p className="text-sm text-gray-500 font-mono">{order.id}</p>
        </div>
        {isAdmin && (
          <button onClick={() => setShowDelete(true)}
            className="flex items-center gap-2 text-red-600 hover:bg-red-50 px-3 py-2 rounded-lg text-sm">
            <Trash2 className="h-4 w-4" />Delete
          </button>
        )}
      </div>

      {/* Status stepper */}
      <Card>
        <div className="flex items-center justify-between mb-2">
          {ORDER_STATUSES.filter(s => s !== 'Cancelled').map((step, i) => (
            <div key={step} className="flex-1 flex flex-col items-center">
              <div className={`w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold ${
                isCancelled ? 'bg-red-100 text-red-600'
                  : i <= statusIndex ? 'bg-indigo-600 text-white' : 'bg-gray-200 text-gray-400'
              }`}>
                {i <= statusIndex && !isCancelled ? <Check className="h-4 w-4" /> : i + 1}
              </div>
              <span className="text-xs mt-1 text-gray-500">{step}</span>
            </div>
          ))}
        </div>
        {isCancelled && <p className="text-center text-red-600 text-sm font-medium mt-2">This order has been cancelled</p>}
      </Card>

      {/* Order info */}
      <Card>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
          <div><p className="text-xs text-gray-500">Status</p><OrderStatusBadge status={order.status} /></div>
          <div><p className="text-xs text-gray-500">Total</p><p className="font-bold">{formatCurrency(order.totalAmount)}</p></div>
          <div><p className="text-xs text-gray-500">Created</p><p className="text-sm">{formatDate(order.createdAt)}</p></div>
          <div><p className="text-xs text-gray-500">Updated</p><p className="text-sm">{formatDate(order.updatedAt)}</p></div>
        </div>

        <h3 className="text-sm font-semibold text-gray-700 mb-3">Items</h3>
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">Product</th>
              <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">Qty</th>
              <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">Price</th>
              <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">Subtotal</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {order.items?.map((item, i) => (
              <tr key={i}>
                <td className="px-4 py-2 text-sm">{item.productName}</td>
                <td className="px-4 py-2 text-sm">{item.quantity}</td>
                <td className="px-4 py-2 text-sm">{formatCurrency(item.unitPrice)}</td>
                <td className="px-4 py-2 text-sm font-medium">{formatCurrency(item.quantity * item.unitPrice)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </Card>

      {/* Admin: Update status */}
      {isAdmin && !isCancelled && (
        <Card>
          <h3 className="text-sm font-semibold text-gray-700 mb-3">Update Status</h3>
          <div className="flex flex-wrap gap-2">
            {ORDER_STATUSES.map((s) => (
              <button key={s} onClick={() => handleStatusChange(s)} disabled={s === order.status}
                className={`px-3 py-1.5 rounded-lg text-sm font-medium border transition-colors ${
                  s === order.status ? 'bg-indigo-100 text-indigo-700 border-indigo-300' : 'border-gray-300 hover:bg-gray-50'
                }`}>
                {s}
              </button>
            ))}
          </div>
        </Card>
      )}

      <Modal isOpen={showDelete} onClose={() => setShowDelete(false)} title="Delete Order">
        <p className="text-sm text-gray-600 mb-4">Are you sure you want to delete this order? This action cannot be undone.</p>
        <div className="flex justify-end gap-3">
          <button onClick={() => setShowDelete(false)} className="px-4 py-2 border border-gray-300 rounded-lg text-sm">Cancel</button>
          <button onClick={handleDelete} className="px-4 py-2 bg-red-600 text-white rounded-lg text-sm">Delete</button>
        </div>
      </Modal>
    </div>
  )
}
