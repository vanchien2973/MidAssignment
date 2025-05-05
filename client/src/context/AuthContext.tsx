import React, { createContext, useState, useContext, useEffect, ReactNode } from 'react';
import AuthService from '../services/auth.service';
import { CurrentUser, LoginRequest, RegisterRequest, AuthResponse } from '../types/auth';
import { getDetailedErrorMessage } from '../utils/errorUtils';

// Define extended login response interface
interface LoginResponse extends AuthResponse {
  user?: CurrentUser;
}

// Define context type
interface AuthContextProps {
  isAuthenticated: boolean;
  isInitializing: boolean;
  isLoginLoading: boolean;
  isRegisterLoading: boolean;
  isAdmin: boolean;
  currentUser: CurrentUser | null;
  login: (data: LoginRequest) => Promise<LoginResponse>;
  register: (data: RegisterRequest) => Promise<{ success: boolean; message: string }>;
  logout: () => void;
  refreshUserData: () => Promise<void>;
}

// Create context with default value
const AuthContext = createContext<AuthContextProps>({
  isAuthenticated: false,
  isInitializing: true,
  isLoginLoading: false,
  isRegisterLoading: false,
  isAdmin: false,
  currentUser: null,
  login: async () => ({ success: false, message: 'Auth context not initialized' }),
  register: async () => ({ success: false, message: 'Auth context not initialized' }),
  logout: () => {},
  refreshUserData: async () => {},
});

// Custom hook to use context
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

// Provider component
export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [currentUser, setCurrentUser] = useState<CurrentUser | null>(null);
  const [isInitializing, setIsInitializing] = useState(true);
  const [isLoginLoading, setIsLoginLoading] = useState(false);
  const [isRegisterLoading, setIsRegisterLoading] = useState(false);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);

  // Initialize auth state
  useEffect(() => {
    const initializeAuth = async () => {
      try {
        if (AuthService.isAuthenticated()) {
          const user = await AuthService.getCurrentUser();
          
          if (user) {
            setCurrentUser(user);
            setIsAuthenticated(true);
            
            // Check if user has admin role (SuperUser)
            const isUserAdmin = user.userType === 'SuperUser';
            setIsAdmin(isUserAdmin);
          } else {
            handleLogout();
          }
        } else {
          setIsAuthenticated(false);
          setIsAdmin(false);
          setCurrentUser(null);
        }
      } catch (error) {
        console.error('Error initializing auth state:', error);
        handleLogout();
      } finally {
        setIsInitializing(false);
      }
    };

    initializeAuth();
  }, []);

  // Function to refresh user data
  const refreshUserData = async () => {
    try {
      if (AuthService.isAuthenticated()) {
        const user = await AuthService.getCurrentUser();
        
        if (user) {
          setCurrentUser(user);
          setIsAuthenticated(true);
          
          // Check if user has admin role (SuperUser)
          const isUserAdmin = user.userType === 'SuperUser';
          setIsAdmin(isUserAdmin);
        } else {
          handleLogout();
        }
      } else {
        setIsAuthenticated(false);
        setIsAdmin(false);
        setCurrentUser(null);
      }
    } catch (error) {
      console.error('Error refreshing user data:', error);
      handleLogout();
    }
  };

  // Login function
  const login = async (loginData: LoginRequest): Promise<LoginResponse> => {
    try {
      setIsLoginLoading(true);
      const response = await AuthService.login(loginData);
      
      if (response.success) {
        // Lấy thông tin user sau khi đăng nhập thành công
        const user = await AuthService.getCurrentUser();
        
        if (user) {
          setCurrentUser(user);
          setIsAuthenticated(true);
          
          // Check if user has admin role (SuperUser)
          const hasAdminRole = user.userType === 'SuperUser';
          setIsAdmin(hasAdminRole);
          
          return {
            ...response,
            user
          };
        }
      }
      
      // For login failures, pass through the exact error message
      return {
        success: false,
        message: response.message || "Invalid username or password"
      };
    } catch (error) {
      console.error('Login error:', error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Login failed. Please try again later.') 
      };
    } finally {
      setIsLoginLoading(false);
    }
  };

  // Register function
  const register = async (registerData: RegisterRequest): Promise<{ success: boolean; message: string }> => {
    try {
      setIsRegisterLoading(true);
      const result = await AuthService.register(registerData);
      
      // Đảm bảo trả về cả thông báo lỗi từ server
      return {
        success: result.success,
        message: result.message || (result.success ? 'Registration successful' : 'Registration failed')
      };
    } catch (error) {
      console.error('Register error:', error);
      return { 
        success: false, 
        message: getDetailedErrorMessage(error, 'An error occurred during registration. Please try again later.')
      };
    } finally {
      setIsRegisterLoading(false);
    }
  };

  // Logout function
  const handleLogout = () => {
    AuthService.logout();
    setCurrentUser(null);
    setIsAuthenticated(false);
    setIsAdmin(false);
  };

  // Context value
  const value = {
    isAuthenticated,
    isInitializing,
    isLoginLoading,
    isRegisterLoading,
    isAdmin,
    currentUser,
    login,
    register,
    logout: handleLogout,
    refreshUserData,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export default AuthContext;