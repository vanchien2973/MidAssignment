import { Navigate, useLocation } from "react-router-dom";
import { ReactNode, useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";

interface AdminProtectedRouteProps {
  children: ReactNode;
}

const AdminProtectedRoute = ({ children }: AdminProtectedRouteProps) => {
  const { isAuthenticated, isInitializing, isLoginLoading, isRegisterLoading, isAdmin, currentUser, refreshUserData } = useAuth();
  const [isChecking, setIsChecking] = useState(true);
  const location = useLocation();

  useEffect(() => {
    const checkAdminStatus = async () => {
      try {
        await refreshUserData();
      } catch (error) {
        console.error("Error checking admin status:", error);
      } finally {
        setIsChecking(false);
      }
    };
    
    checkAdminStatus();
  }, []);

  // Only show loading state if initializing or checking admin status
  // and not in the process of logging in or registering
  if ((isInitializing || isChecking) && !isLoginLoading && !isRegisterLoading) {
    return <div className="p-4 text-center">Loading...</div>;
  }

  // Redirect to login if not authenticated and not in the process of logging in/registration
  if (!isAuthenticated && !isLoginLoading && !isRegisterLoading) {
    // Save the current location to redirect back after login
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Redirect to home if not an admin
  if (!isAdmin && currentUser?.userType !== "SuperUser") {
    return <Navigate to="/" replace />;
  }

  // Display content if authenticated and has admin rights
  return children;
};

export default AdminProtectedRoute; 