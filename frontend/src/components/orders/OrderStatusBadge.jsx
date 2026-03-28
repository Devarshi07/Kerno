import Badge from '../ui/Badge'
import { ORDER_STATUS_COLORS } from '../../utils/constants'

export default function OrderStatusBadge({ status }) {
  return <Badge className={ORDER_STATUS_COLORS[status] || 'bg-gray-100 text-gray-800'}>{status}</Badge>
}
