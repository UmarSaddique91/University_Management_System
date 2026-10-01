import React from 'react'
import { Navigate } from 'react-router-dom'

const ProtectedRoute = ({ children, allowedRoles = [] }) => {
  const token = localStorage.getItem("token");

  //Check user Authentication
  if (!token) {
    return <Navigate to="/" replace />;
  }

  // Roles Parsing from Local Storage
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

  


  //User should have at least 1 role
  const hasAccess =
    allowedRoles.length === 0 ||
    roles.some((role) => allowedRoles.includes(role));

  if (!hasAccess) {

    
    if (roles.includes("Teacher")) {
      return <Navigate to="/teacher/courses" replace />;
    }
    
    if (roles.includes("Student")) {
      return <Navigate to="/student/courses" replace />;
    }
    
    return <Navigate to="/dashboard" replace />;
  }

  return children;
};

export default ProtectedRoute;