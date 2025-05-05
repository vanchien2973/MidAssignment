export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  fullName: string;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  token?: string;
  refreshToken?: string;
  expiresIn?: number;
  user?: {
    userId: number;
    username: string;
    email: string;
    fullName: string;
    userType: string;
  };
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface CurrentUser {
  userId: number;
  username: string;
  email: string;
  fullName: string;
  userType: string;
  isActive: boolean;
} 