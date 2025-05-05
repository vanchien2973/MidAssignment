import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import UserService from '../services/user.service';
import { useAuth } from './AuthContext';
import { User, UpdateUserProfileRequest, UserResponse } from '../types/user';

// Define context type
interface UserContextType {
  userProfile: User | null;
  isLoading: boolean;
  error: string | null;
  updateProfile: (data: UpdateUserProfileRequest) => Promise<UserResponse>;
  refreshUserProfile: () => Promise<void>;
}

// Create context with default value
const UserContext = createContext<UserContextType | undefined>(undefined);

// Custom hook to use context
export const useUser = () => {
  const context = useContext(UserContext);
  if (!context) {
    throw new Error('useUser must be used within a UserProvider');
  }
  return context;
};

// Provider component
export const UserProvider = ({ children }: { children: ReactNode }) => {
  const { isAuthenticated, currentUser } = useAuth();
  const [userProfile, setUserProfile] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // Get user profile when authenticated
  useEffect(() => {
    if (isAuthenticated) {
      fetchUserProfile();
    } else {
      setUserProfile(null);
      setError(null);
    }
  }, [isAuthenticated]);

  // Use currentUser as fallback if API call fails
  useEffect(() => {
    if (!userProfile && currentUser && !isLoading) {
      // Convert currentUser to UserProfile format
      const profileFromAuth: User = {
        userId: currentUser.userId,
        username: currentUser.username,
        email: currentUser.email,
        fullName: currentUser.fullName,
        userType: currentUser.userType,
        isActive: currentUser.isActive
      };
      
      setUserProfile(profileFromAuth);
      setError(null);
    }
  }, [currentUser, userProfile, isLoading]);

  // Function to fetch user profile
  const fetchUserProfile = async () => {
    try {
      setIsLoading(true);
      setError(null);
    
      const profile = await UserService.getUserProfile();
      
      if (profile.success && profile.data) {
        setUserProfile(profile.data);
      } else {
        console.error('User profile returned null');
        setError('Unable to retrieve user information');
        
        // Use currentUser as fallback
        if (currentUser) {
          const profileFromAuth: User = {
            userId: currentUser.userId,
            username: currentUser.username,
            email: currentUser.email,
            fullName: currentUser.fullName,
            userType: currentUser.userType,
            isActive: currentUser.isActive
          };
          
          setUserProfile(profileFromAuth);
          setError(null); // Clear error since we have a fallback
        }
      }
    } catch (err) {
      console.error('Error fetching user profile:', err);
      setError('Unable to retrieve user information');
    } finally {
      setIsLoading(false);
    }
  };

  // Function to update profile
  const updateProfile = async (data: UpdateUserProfileRequest): Promise<UserResponse> => {
    try {
      setIsLoading(true);
      setError(null);
      const response = await UserService.updateProfile(data);
      
      if (response.success && response.data) {
        setUserProfile(response.data);
      }
      
      return response;
    } catch (err) {
      console.error('Error updating user profile:', err);
      setError('Unable to update user information');
      return {
        success: false,
        message: 'Unable to update user information'
      };
    } finally {
      setIsLoading(false);
    }
  };

  // Function to refresh user profile
  const refreshUserProfile = async () => {
    await fetchUserProfile();
  };

  // Context value
  const value = {
    userProfile,
    isLoading,
    error,
    updateProfile,
    refreshUserProfile
  };

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>;
};

export default UserContext; 