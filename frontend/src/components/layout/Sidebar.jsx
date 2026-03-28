import { NavLink } from 'react-router-dom'
import { useAuth } from '../../contexts/AuthContext'
import { LayoutDashboard, ShoppingCart, User, Bell, Shield, Activity, LogOut, X } from 'lucide-react'
import { cn } from '../../utils/cn'

const navItems = [
  { to: '/dashboard', icon: LayoutDashboard, label: 'Dashboard' },
  { to: '/orders', icon: ShoppingCart, label: 'Orders' },
  { to: '/notifications', icon: Bell, label: 'Notifications' },
  { to: '/profile', icon: User, label: 'Profile' },
  { to: '/health', icon: Activity, label: 'System Health' },
]

const adminItems = [
  { to: '/users', icon: Shield, label: 'User Management' },
  { to: '/audit', icon: Shield, label: 'Audit Log' },
]

export default function Sidebar({ mobile, onClose }) {
  const { isAdmin, logout, user } = useAuth()

  const linkClass = ({ isActive }) => cn(
    'flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors',
    isActive ? 'bg-indigo-50 text-indigo-700' : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900'
  )

  return (
    <aside className={cn(
      'flex flex-col h-full bg-white border-r border-gray-200',
      mobile ? 'w-64' : 'w-64 hidden lg:flex'
    )}>
      <div className="flex items-center justify-between p-4 border-b border-gray-200">
        <h1 className="text-xl font-bold text-indigo-600">NexusGrid</h1>
        {mobile && <button onClick={onClose}><X className="h-5 w-5 text-gray-500" /></button>}
      </div>

      <nav className="flex-1 p-3 space-y-1">
        {navItems.map((item) => (
          <NavLink key={item.to} to={item.to} className={linkClass} onClick={onClose}>
            <item.icon className="h-5 w-5" />{item.label}
          </NavLink>
        ))}

        {isAdmin && (
          <>
            <div className="pt-4 pb-1 px-3 text-xs font-semibold text-gray-400 uppercase">Admin</div>
            {adminItems.map((item) => (
              <NavLink key={item.to} to={item.to} className={linkClass} onClick={onClose}>
                <item.icon className="h-5 w-5" />{item.label}
              </NavLink>
            ))}
          </>
        )}
      </nav>

      <div className="p-3 border-t border-gray-200">
        <div className="px-3 py-2 text-sm text-gray-500 truncate">{user?.email}</div>
        <button onClick={logout} className="flex items-center gap-3 w-full px-3 py-2 rounded-lg text-sm font-medium text-red-600 hover:bg-red-50 transition-colors">
          <LogOut className="h-5 w-5" />Sign Out
        </button>
      </div>
    </aside>
  )
}
