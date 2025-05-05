// Common API response interfaces

// Interface for API error
export interface ApiErrorResponse {
  success: boolean;
  title?: string;
  status?: number;
  errors?: Record<string, string[]>;
  message?: string;
  errorMessages?: string[];
}

// Interface for basic mutation response
export interface MutationResponse {
  success: boolean;
  message: string;
}

// Interface for paginated response
export interface PaginatedResponse<T> {
  success: boolean;
  data: T[];
  message?: string;
  totalCount?: number;
  pageNumber?: number;
  pageSize?: number;
  headers?: Record<string, string>;
}

// Interface for single item response
export interface SingleItemResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
}

// Interface for basic pagination query parameters
export interface PaginationParams {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string;
  searchTerm?: string;
} 