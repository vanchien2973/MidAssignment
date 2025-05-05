import { Navigate, useLocation } from "react-router-dom";
import { ReactNode } from "react";
import { useAuth } from "../context/AuthContext";

interface ProtectedRouteProps {
  children: ReactNode;
}

const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
  const { isAuthenticated, isInitializing, isLoginLoading, isRegisterLoading } = useAuth();
  const location = useLocation();

  // Only show loading state if initializing
  // and not in the process of logging in or registering
  if (isInitializing && !isLoginLoading && !isRegisterLoading) {
    return <div className="p-4 text-center">Loading...</div>;
  }

  // Redirect to login if not authenticated
  // But do not redirect if in the process of logging in or registering
  if (!isAuthenticated && !isLoginLoading && !isRegisterLoading) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Display content if authenticated
  return children;
};

export default ProtectedRoute; 