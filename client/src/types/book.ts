import { PaginatedResponse } from './api';

export interface Book {
  bookId: number;
  title: string;
  author: string;
  categoryId: string;
  categoryName?: string;
  isbn: string;
  publishedYear: number | null;
  publisher: string | null;
  description: string | null;
  totalCopies: number;
  availableCopies: number;
  createdDate: string;
  createdBy: number;
  updatedDate: string | null;
  updatedBy: number | null;
  isActive: boolean;
}


// DTO for creating a new book
export interface BookCreateDto {
  title: string;
  author: string;
  categoryId: string;
  isbn: string;
  publishedYear?: number | null;
  publisher?: string;
  description?: string;
  totalCopies: number;
}

// DTO for updating a book
export interface BookUpdateDto {
  bookId: string;
  title: string;
  author: string;
  categoryId: string;
  isbn: string;
  publishedYear?: number | null;
  publisher?: string;
  description?: string;
  totalCopies: number;
}

// DTO for book details
export interface BookDetailsDto extends Book {
  createdDate: string;
}

// Response for book list
export type BookListResponse = PaginatedResponse<Book>;

// DTO for book query parameters
export interface BookQueryParametersDto {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string;
  searchTerm?: string;
  categoryId?: string;
} 