import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../contexts/AuthContext'
import { createOrder } from '../../api/orders'
import Card from '../../components/ui/Card'
import { Plus, Trash2 } from 'lucide-react'
import { formatCurrency } from '../../utils/formatters'
import toast from 'react-hot-toast'

const emptyItem = () => ({ productId: '', productName: '', quantity: 1, unitPrice: 0 })

export default function CreateOrderPage() {
  const { user } = useAuth()
  const navigate = useNavigate()
  const [items, setItems] = useState([emptyItem()])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const updateItem = (index, field, value) => {
    const updated = [...items]
    updated[index] = { ...updated[index], [field]: field === 'quantity' || field === 'unitPrice' ? Number(value) : value }
    setItems(updated)
  }

  const total = items.reduce((sum, i) => sum + i.quantity * i.unitPrice, 0)

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    if (items.some(i => !i.productName || i.quantity < 1 || i.unitPrice <= 0)) {
      setError('All items must have a name, quantity >= 1, and price > 0')
      return
    }
    setLoading(true)
    try {
      const order = await createOrder(user.id, items)
      toast.success('Order created!')
      navigate(`/orders/${order.id}`)
    } catch (err) {
      setError(err?.error || 'Failed to create order')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <h2 className="text-2xl font-bold text-gray-900">Create Order</h2>

      <form onSubmit={handleSubmit}>
        <Card className="space-y-4">
          {error && <div className="bg-red-50 text-red-700 px-4 py-3 rounded-lg text-sm">{error}</div>}

          <div className="space-y-3">
            {items.map((item, i) => (
              <div key={i} className="grid grid-cols-12 gap-2 items-end">
                <div className="col-span-2">
                  {i === 0 && <label className="block text-xs font-medium text-gray-500 mb-1">Product ID</label>}
                  <input value={item.productId} onChange={(e) => updateItem(i, 'productId', e.target.value)}
                    placeholder="SKU-001" className="w-full px-2 py-2 border border-gray-300 rounded-lg text-sm outline-none focus:ring-2 focus:ring-indigo-500" />
                </div>
                <div className="col-span-4">
                  {i === 0 && <label className="block text-xs font-medium text-gray-500 mb-1">Product Name</label>}
                  <input value={item.productName} onChange={(e) => updateItem(i, 'productName', e.target.value)} required
                    placeholder="Product name" className="w-full px-2 py-2 border border-gray-300 rounded-lg text-sm outline-none focus:ring-2 focus:ring-indigo-500" />
                </div>
                <div className="col-span-2">
                  {i === 0 && <label className="block text-xs font-medium text-gray-500 mb-1">Qty</label>}
                  <input type="number" min="1" value={item.quantity} onChange={(e) => updateItem(i, 'quantity', e.target.value)}
                    className="w-full px-2 py-2 border border-gray-300 rounded-lg text-sm outline-none focus:ring-2 focus:ring-indigo-500" />
                </div>
                <div className="col-span-3">
                  {i === 0 && <label className="block text-xs font-medium text-gray-500 mb-1">Unit Price</label>}
                  <input type="number" min="0.01" step="0.01" value={item.unitPrice} onChange={(e) => updateItem(i, 'unitPrice', e.target.value)}
                    className="w-full px-2 py-2 border border-gray-300 rounded-lg text-sm outline-none focus:ring-2 focus:ring-indigo-500" />
                </div>
                <div className="col-span-1">
                  {items.length > 1 && (
                    <button type="button" onClick={() => setItems(items.filter((_, j) => j !== i))}
                      className="p-2 text-red-500 hover:bg-red-50 rounded-lg"><Trash2 className="h-4 w-4" /></button>
                  )}
                </div>
              </div>
            ))}
          </div>

          <button type="button" onClick={() => setItems([...items, emptyItem()])}
            className="flex items-center gap-2 text-sm text-indigo-600 hover:text-indigo-700 font-medium">
            <Plus className="h-4 w-4" />Add Item
          </button>

          <div className="border-t border-gray-200 pt-4 flex items-center justify-between">
            <p className="text-lg font-bold">Total: {formatCurrency(total)}</p>
            <div className="flex gap-3">
              <button type="button" onClick={() => navigate('/orders')}
                className="px-4 py-2 border border-gray-300 rounded-lg text-sm hover:bg-gray-50">Cancel</button>
              <button type="submit" disabled={loading}
                className="px-6 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 disabled:opacity-50">
                {loading ? 'Creating...' : 'Create Order'}
              </button>
            </div>
          </div>
        </Card>
      </form>
    </div>
  )
}
