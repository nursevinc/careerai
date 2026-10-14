import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import FullPageSpinner from './FullPageSpinner'

// Giriş yapmamış kullanıcıyı /login'e yönlendirir
export default function ProtectedRoute() {
  const { isAuthenticated, isInitializing } = useAuth()

  if (isInitializing) return <FullPageSpinner />
  if (!isAuthenticated) return <Navigate to="/login" replace />

  return <Outlet />
}