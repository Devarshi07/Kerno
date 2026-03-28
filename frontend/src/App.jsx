import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import { AuthProvider } from './contexts/AuthContext'
import ProtectedRoute from './components/auth/ProtectedRoute'
import AppLayout from './components/layout/AppLayout'
import LoginPage from './pages/auth/LoginPage'
import RegisterPage from './pages/auth/RegisterPage'
import DashboardPage from './pages/dashboard/DashboardPage'
import OrderListPage from './pages/orders/OrderListPage'
import CreateOrderPage from './pages/orders/CreateOrderPage'
import OrderDetailPage from './pages/orders/OrderDetailPage'
import ProfilePage from './pages/users/ProfilePage'
import NotificationInboxPage from './pages/notifications/NotificationInboxPage'
import UserListPage from './pages/users/UserListPage'
import AuditLogPage from './pages/audit/AuditLogPage'
import HealthPage from './pages/health/HealthPage'

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Toaster position="top-right" />
        <Routes>
          {/* Public */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* Authenticated */}
          <Route path="/" element={<ProtectedRoute><AppLayout /></ProtectedRoute>}>
            <Route index element={<Navigate to="/dashboard" replace />} />
            <Route path="dashboard" element={<DashboardPage />} />
            <Route path="orders" element={<OrderListPage />} />
            <Route path="orders/new" element={<CreateOrderPage />} />
            <Route path="orders/:id" element={<OrderDetailPage />} />
            <Route path="profile" element={<ProfilePage />} />
            <Route path="notifications" element={<NotificationInboxPage />} />
            <Route path="health" element={<HealthPage />} />

            {/* Admin only */}
            <Route path="users" element={<ProtectedRoute requiredRole="Admin"><UserListPage /></ProtectedRoute>} />
            <Route path="audit" element={<ProtectedRoute requiredRole="Admin"><AuditLogPage /></ProtectedRoute>} />
          </Route>

          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}
