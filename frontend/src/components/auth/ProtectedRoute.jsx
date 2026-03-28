import { Navigate } from 'react-router-dom'
import { useAuth } from '../../contexts/AuthContext'

export default function ProtectedRoute({ children, requiredRole }) {
  const { isAuthenticated, isAdmin, isLoading } = useAuth()

  if (isLoading) {
    return <div className="flex items-center justify-center h-screen">
      <div className="animate-spin h-8 w-8 border-4 border-indigo-600 border-t-transparent rounded-full" />
    </div>
  }

  if (!isAuthenticated) return <Navigate to="/login" replace />
  if (requiredRole === 'Admin' && !isAdmin) return <Navigate to="/dashboard" replace />

  return children
}
