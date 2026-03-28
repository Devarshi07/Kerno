import { format, formatDistanceToNow } from 'date-fns'

export function formatCurrency(amount) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount)
}

export function formatDate(dateStr) {
  return format(new Date(dateStr), 'MMM d, yyyy h:mm a')
}

export function formatRelativeTime(dateStr) {
  return formatDistanceToNow(new Date(dateStr), { addSuffix: true })
}

export function truncateId(id) {
  return id?.substring(0, 8) + '...'
}
