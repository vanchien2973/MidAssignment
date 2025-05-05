export type UserType = 'SuperUser' | 'NormalUser';

export interface User {
  userId: number;
  username: string;
  email: string;
  fullName: string;
  userType?: string;
  isActive?: boolean;
  avatarUrl?: string;
  lastLoginDate?: string | null;
  createdDate?: string;
}

export interface UpdateUserProfileRequest {
  fullName?: string;
  email?: string;
  avatarUrl?: string;
}

export interface UpdatePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface UserResponse {
  success: boolean;
  message: string;
  data?: User;
}

export interface UserActivityLog {
  logId: number;
  userId: number;
  username: string;
  activityType: string;
  activityDate: string | Date;
  details?: string;
  ipAddress?: string;
}

export interface UserActivityLogSearchParams {
  userId?: number;
  activityType?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface UserSearchParams {
  searchTerm?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
} 