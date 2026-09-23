import React from 'react'
import { Navigate } from 'react-router-dom'

const ProtectedRoute = ({ children, allowedRoles = [] }) => {
  const token = localStorage.getItem("token");

  // If user is not authenticated at all, redirect to Login
  if (!token) {
    return <Navigate to="/" replace />;
  }

  // Safely parse roles from localStorage
  let roles = [];
  try {
    const stored = localStorage.getItem("roles");
    roles = stored ? JSON.parse(stored) : [];
    if (!Array.isArray(roles)) {
      roles = typeof roles === "string" ? [roles] : [];
    }
  } catch {
    roles = [];
  }

  // Fallback: decode JWT token payload if roles is empty in localStorage
  if (roles.length === 0) {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const roleClaim =
        payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
        payload.role;
      roles = Array.isArray(roleClaim) ? roleClaim : [roleClaim];
    } catch {
      roles = [];
    }
  }

  // Check if user has at least one of the allowed roles
  const hasAccess =
    allowedRoles.length === 0 ||
    roles.some((role) => allowedRoles.includes(role));

  if (!hasAccess) {
    // If logged in but unauthorized for this specific route (e.g. Admin visiting SuperAdmin-only page),
    // redirect to dashboard instead of kicking to login
    return <Navigate to="/dashboard" replace />;
  }

  return children;
};

export default ProtectedRoute;