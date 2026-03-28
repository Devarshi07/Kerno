export const ORDER_STATUSES = ['Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered', 'Cancelled']

export const ORDER_STATUS_COLORS = {
  Pending: 'bg-yellow-100 text-yellow-800',
  Confirmed: 'bg-blue-100 text-blue-800',
  Processing: 'bg-purple-100 text-purple-800',
  Shipped: 'bg-indigo-100 text-indigo-800',
  Delivered: 'bg-green-100 text-green-800',
  Cancelled: 'bg-red-100 text-red-800',
}

export const NOTIFICATION_TYPES = ['OrderCreated', 'OrderStatusChanged', 'OrderCancelled', 'AccountCreated', 'PasswordReset', 'System']

export const ROLES = { USER: 'User', ADMIN: 'Admin' }
