import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import FullPageSpinner from './FullPageSpinner'

// Zaten giriş yapmış kullanıcı /login ve /register'ı görmesin
export default function GuestRoute() {
  const { isAuthenticated, isInitializing } = useAuth()

  if (isInitializing) return <FullPageSpinner />
  if (isAuthenticated) return <Navigate to="/dashboard" replace />

  return <Outlet />
}