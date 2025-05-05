import api from '../config/axios';
import { 
  LoginRequest, 
  RegisterRequest, 
  AuthResponse, 
  RefreshTokenRequest, 
  CurrentUser 
} from '../types/auth';
import { getDetailedErrorMessage } from '../utils/errorUtils';

class AuthServiceClass {
  // Login
  login = async (loginData: LoginRequest): Promise<AuthResponse> => {
    try {
      const response = await api.post<AuthResponse>('/Auth/login', loginData);
      
      // Save token to localStorage if login successful
      if (response.data.success && response.data.token) {
        // Clear any existing tokens first
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('tokenExpiry');
        
        // Save new tokens
        localStorage.setItem('token', response.data.token);
        
        if (response.data.refreshToken) {
          localStorage.setItem('refreshToken', response.data.refreshToken);
        }
        
        if (response.data.expiresIn) {
          const expiryTime = new Date().getTime() + response.data.expiresIn * 1000;
          localStorage.setItem('tokenExpiry', expiryTime.toString());
        }
      }
      
      return response.data;
    } catch (error: unknown) {
      console.error('Login error:', error);
      
      // Type guard for error with response property
      if (error && typeof error === 'object' && 'response' in error && error.response) {
        const apiError = error as { response: { status: number; data?: unknown } };
        
        // Handle 401 Unauthorized specifically for login - Incorrect credentials
        if (apiError.response.status === 401) {
          return {
            success: false,
            message: "Invalid username or password",
          };
        }
        
        // Handle 400 Bad Request (validation errors)
        if (apiError.response.status === 400) {
          return {
            success: false,
            message: getDetailedErrorMessage(error, 'Invalid login information'),
          };
        }
      }
      
      // Return standardized response for other errors
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Login failed. Please try again later.'),
      };
    }
  };
  
  // Register
  register = async (registerData: RegisterRequest): Promise<AuthResponse> => {
    try {
      const response = await api.post<AuthResponse>('/Auth/register', registerData);
      return response.data;
    } catch (error: unknown) {
      console.error('Register error:', error);
      
      // Type guard for error with response property
      if (error && typeof error === 'object' && 'response' in error && error.response) {
        const apiError = error as { response: { status: number; data?: unknown } };
        
        // Handle validation errors from the server
        if (apiError.response.status === 400) {
          return {
            success: false,
            message: getDetailedErrorMessage(error, 'Username already exists'),
          };
        }
      }
      
      // Return standardized response for other errors
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Registration failed. Please try again later.'),
      };
    }
  };
  
  // Logout
  logout = (): void => {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('tokenExpiry');
  };
  
  // Refresh token
  refreshToken = async (refreshData: RefreshTokenRequest): Promise<AuthResponse> => {
    try {
      const response = await api.post<AuthResponse>('/Auth/refresh-token', refreshData);
      
      // Update token in localStorage
      if (response.data.success && response.data.token) {
        localStorage.setItem('token', response.data.token);
        
        if (response.data.refreshToken) {
          localStorage.setItem('refreshToken', response.data.refreshToken);
        }
        
        if (response.data.expiresIn) {
          const expiryTime = new Date().getTime() + response.data.expiresIn * 1000;
          localStorage.setItem('tokenExpiry', expiryTime.toString());
        }
      }
      
      return response.data;
    } catch (error: unknown) {
      // Logout in case of refresh token failure
      this.logout();
      
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Token refresh failed. Please login again.'),
      };
    }
  };
  
  // Get current user information
  getCurrentUser = async (): Promise<CurrentUser | null> => {
    try {
      const token = localStorage.getItem('token');
      if (!token) {
        return null;
      }
      
      const response = await api.get('/Auth/me');
      
      if (response.data && response.data.success) {
        // Ensure the user object has the isActive property
        const userData = response.data.data;
        return {
          ...userData,
          isActive: userData.isActive !== undefined ? userData.isActive : true,
        };
      }
      
      return null;
    } catch (error: unknown) {
      console.error('Error getting current user:', error);
      return null;
    }
  };
  
  // Check if token is expired
  isTokenExpired = (): boolean => {
    const expiry = localStorage.getItem('tokenExpiry');
    if (!expiry) return true;
    
    const expiryTime = parseInt(expiry, 10);
    const currentTime = new Date().getTime();
    
    return currentTime > expiryTime;
  };
  
  // Check if user is authenticated
  isAuthenticated = (): boolean => {
    const token = localStorage.getItem('token');
    if (!token) {
      return false;
    }
    
    // Check if token has expired
    if (this.isTokenExpired()) {
      const refreshToken = localStorage.getItem('refreshToken');
      if (!refreshToken) {
        this.logout();
        return false;
      }
      
      return true;
    }
    
    return true;
  };

  // Check if user has SuperUser role
  isSuperUser = async (): Promise<boolean> => {
    try {
      const user = await this.getCurrentUser();
      if (!user) return false;
      
      return user.userType === 'SuperUser';
    } catch (error: unknown) {
      console.error('Error checking user role:', error);
      return false;
    }
  };
}

const AuthService = new AuthServiceClass();
export default AuthService; 