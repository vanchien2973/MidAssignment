import axios from 'axios';
import { API_URL } from '../constants/api';

// Initialize Axios client with default settings
const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000, 
});

// CORS expose headers configuration
api.defaults.headers.common['Access-Control-Expose-Headers'] = 'Content-Disposition, X-Total-Count, X-Page-Number, X-Page-Size';

// Check if we already have redirected to prevent infinite redirect loops
let hasRedirectedToLogin = false;

// Request interceptor
api.interceptors.request.use(
  (config) => {
    // Get token from localStorage
    const token = localStorage.getItem('token');
    
    // Add token to header if available
    config.headers.Authorization = `Bearer ${token}`;
    
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor
api.interceptors.response.use(
  (response) => {
    return response;
  },
  async (error) => {
    const originalRequest = error.config;
    
    // If no response, could be a network error or CORS issue
    if (!error.response) {
      return Promise.reject(error);
    }
    
    // Check if error is 401 (Unauthorized) and not retried yet
    if (error.response.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        // Get refresh token
        const refreshToken = localStorage.getItem('refreshToken');
        
        if (!refreshToken) {
          // If no refresh token, log out the user
          localStorage.removeItem('token');
          localStorage.removeItem('refreshToken');
          
          if (!hasRedirectedToLogin) {
            hasRedirectedToLogin = true;
            window.location.href = '/login';
          }
          
          return Promise.reject(error);
        }
        
        // Call API to refresh token
        const response = await axios.post(`${API_URL}/Auth/refresh-token`, {
          refreshToken
        });
        
        // If token refresh successful
        if (response.data.success) {
          // Save new token to localStorage
          localStorage.setItem('token', response.data.token);
          
          if (response.data.refreshToken) {
            localStorage.setItem('refreshToken', response.data.refreshToken);
          }
          
          // Reset redirect flag since we have a valid token now
          hasRedirectedToLogin = false;
          
          // Update token in the original request header
          originalRequest.headers.Authorization = `Bearer ${response.data.token}`;
          
          // Retry the original request with new token
          return api(originalRequest);
        } else {
          // If refresh token fails but has response
          localStorage.removeItem('token');
          localStorage.removeItem('refreshToken');
          
          if (!hasRedirectedToLogin) {
            hasRedirectedToLogin = true;
            window.location.href = '/login';
          } else {
            console.warn('Prevented multiple redirects to login page');
          }
          
          return Promise.reject(error);
        }
      } catch (refreshError) {
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        
        if (!hasRedirectedToLogin) {
          hasRedirectedToLogin = true;
          window.location.href = '/login';
        } else {
          console.warn('Prevented multiple redirects to login page');
        }
        
        return Promise.reject(refreshError);
      }
    }
    
    // Handle other HTTP errors
    if (error.response.status === 403) {
      // Forbidden - No access rights
      console.error('Access forbidden:', error.response.data);
    } else if (error.response.status === 404) {
      // Not Found
      console.error('Resource not found:', error.response.data);
    } else if (error.response.status >= 500) {
      // Server error
      console.error('Server error:', error.response.data);
    }
    
    return Promise.reject(error);
  }
);

export default api; 