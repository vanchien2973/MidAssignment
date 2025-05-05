import { SingleItemResponse, PaginatedResponse } from './api';

// Category types
export interface Category {
  categoryId: number;
  categoryName: string;
  description: string | null;
  createdDate: string;
  createdBy: number;
  updatedDate: string | null;
  updatedBy: number | null;
}


// DTO for creating a new category
export interface CategoryCreateDto {
  categoryName: string;
  description: string;
}

// DTO for updating a category
export interface CategoryUpdateDto {
  categoryId: string;
  categoryName: string;
  description: string;
}

// DTO for category details
export interface CategoryDetailsDto extends Category {
  createdDate: string;
}

// Response for category list
export type CategoryListResponse = PaginatedResponse<Category>;

// Response for category details
export type CategoryResponse = SingleItemResponse<CategoryDetailsDto>;

// DTO for category query parameters
export interface CategoryQueryParametersDto {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string; 
  searchTerm?: string;
} 