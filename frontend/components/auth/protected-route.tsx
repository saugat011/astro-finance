"use client"

import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/lib/context/auth-context'
import { Loader2 } from 'lucide-react'

interface ProtectedRouteProps {
  children: React.ReactNode
  requiredRoles?: string[]
}

export function ProtectedRoute({ children, requiredRoles = [] }: ProtectedRouteProps) {
  const { user, isLoading, isAuthenticated } = useAuth()
  const router = useRouter()

  useEffect(() => {
    // If not loading and not authenticated, redirect to login
    if (!isLoading && !isAuthenticated) {
      router.push('/login')
    }

    // If authenticated but doesn't have required role, redirect to dashboard
    if (
      !isLoading &&
      isAuthenticated &&
      requiredRoles.length > 0 &&
      user &&
      !requiredRoles.includes(user.role)
    ) {
      router.push('/dashboard')
    }
  }, [isLoading, isAuthenticated, router, user, requiredRoles])

  // Show loading state
  if (isLoading || !isAuthenticated) {
    return (
      <div className="flex h-screen w-full items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    )
  }

  // If role check is required and user doesn't have the required role
  if (
    requiredRoles.length > 0 &&
    user &&
    !requiredRoles.includes(user.role)
  ) {
    return (
      <div className="flex h-screen w-full flex-col items-center justify-center gap-4">
        <h1 className="text-2xl font-bold">Access Denied</h1>
        <p className="text-muted-foreground">
          You don't have permission to access this page.
        </p>
      </div>
    )
  }

  // If authenticated and has required role, render children
  return <>{children}</>
}