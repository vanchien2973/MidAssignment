import api from '../config/axios';
import { 
  UpdateUserProfileRequest, 
  UserResponse, 
  UserActivityLog,
  UserActivityLogSearchParams,
  UpdatePasswordRequest,
  User,
  UserType,
  UserSearchParams
} from '../types/user';
import { getDetailedErrorMessage } from '../utils/errorUtils';
import { PaginatedResponse } from '../types/api';

// User service
const UserService = {
  // Get user profile information
  getUserProfile: async (): Promise<UserResponse> => {
    try {
      const response = await api.get<UserResponse>('/User/profile');
      return response.data;
    } catch (error: unknown) {
      console.error('Error getting profile:', error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Failed to get profile'),
        data: undefined
      };
    }
  },
  
  // Update user profile information
  updateProfile: async (profileData: UpdateUserProfileRequest): Promise<UserResponse> => {
    try {
    const response = await api.put<UserResponse>('/User/profile', profileData);
    return response.data;
    } catch (error: unknown) {
      console.error('Error updating profile:', error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Failed to update profile'),
        data: undefined
      };
    }
  },
  
  // Update user password
  updatePassword: async (passwordData: UpdatePasswordRequest): Promise<UserResponse> => {
    try {
    const response = await api.put<UserResponse>('/User/password', passwordData);
    return response.data;
    } catch (error: unknown) {
      console.error('Error updating password:', error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Failed to update password'),
        data: undefined
      };
    }
  },
  
  // Get user activity logs
  getUserActivityLogs: async (searchParams?: UserActivityLogSearchParams): Promise<UserActivityLog[]> => {
    try {
      const params: Record<string, number> = {
        pageNumber: searchParams?.pageNumber || 1,
        pageSize: searchParams?.pageSize || 10
      };
    
      const response = await api.get('/User/activity-logs', { params });
      
      if (response.data && Array.isArray(response.data)) {
        return response.data;
      }
      
      return [];
    } catch (error: unknown) {
      console.error('Error fetching user activity logs:', error);
      return [];
    }
  },

  // ==================== ADMIN USER MANAGEMENT =====================

  // Get all users (Admin only)
  getAllUsers: async (searchParams?: UserSearchParams): Promise<PaginatedResponse<User>> => {
    try {
      // Đảm bảo pageNumber của parameters luôn đúng giá trị được truyền vào
      const pageNumber = searchParams?.pageNumber || 1;
      
      // Create basic query parameters
      const params: Record<string, any> = {
        pageNumber: pageNumber,
        pageSize: searchParams?.pageSize || 5,
        searchTerm: searchParams?.searchTerm || ''
      };
      
      // Add optional sorting parameters if present
      if (searchParams?.sortBy) {
        params.sortBy = searchParams.sortBy;
      }
      
      if (searchParams?.sortOrder) {
        params.sortOrder = searchParams.sortOrder;
      }
      
      // Call API with processed parameters
      const response = await api.get<PaginatedResponse<User>>('/Admin/users', { 
        params
      });
      
      // Process response to ensure userType is in correct format
      if (response.data && response.data.data) {
        return {
          ...response.data,
          data: response.data.data.map(user => ({
            ...user,
            userType: typeof user.userType === 'number' && user.userType === 2 ? 'SuperUser' : 'NormalUser'
          }))
        };
      }
      
      // Fallback response
      return {
        success: false,
        data: [],
        message: "Failed to process response data"
      };
    } catch (error: unknown) {
      console.error('Error fetching users:', error);
      return {
        success: false,
        data: [],
        message: getDetailedErrorMessage(error, 'Failed to fetch users')
      };
    }
  },

  // Update user role (Admin only)
  updateUserRole: async (userId: number, userType: UserType): Promise<UserResponse> => {
    try {
      // Convert userType string to integer value
      const userTypeValue = userType === 'SuperUser' ? 2 : 1;
      
      const response = await api.put<UserResponse>(`/Admin/users/${userId}/role`, { userType: userTypeValue });
      return response.data;
    } catch (error: unknown) {
      console.error(`Error updating user ${userId} role:`, error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, `Failed to update user ${userId} role`)
      };
    }
  },

  // Activate user (Admin only)
  activateUser: async (userId: number): Promise<UserResponse> => {
    try {
      const response = await api.post<UserResponse>(`/Admin/users/${userId}/activate`);
      return response.data;
    } catch (error: unknown) {
      console.error(`Error activating user ${userId}:`, error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, `Failed to activate user ${userId}`)
      };
    }
  },

  // Deactivate user (Admin only)
  deactivateUser: async (userId: number): Promise<UserResponse> => {
    try {
      const response = await api.post<UserResponse>(`/Admin/users/${userId}/deactivate`);
      return response.data;
    } catch (error: unknown) {
      console.error(`Error deactivating user ${userId}:`, error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, `Failed to deactivate user ${userId}`)
      };
    }
  },

  // Delete user (Admin only)
  deleteUser: async (userId: number): Promise<UserResponse> => {
    try {
      const response = await api.delete<UserResponse>(`/Admin/users/${userId}`);
      return response.data;
    } catch (error: unknown) {
      console.error(`Error deleting user ${userId}:`, error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, `Failed to delete user ${userId}`)
      };
    }
  }
};

export default UserService; 