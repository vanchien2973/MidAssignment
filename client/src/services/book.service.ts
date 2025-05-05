import api from '../config/axios';
import { Book } from '../types/book';
import { 
  BookCreateDto, 
  BookUpdateDto, 
  BookListResponse, 
} from '../types/book';
import { MutationResponse } from '../types/api';
import { getDetailedErrorMessage } from '../utils/errorUtils';
import CategoryService from './category.service';

class BookService {
  // Get books with pagination
  getBooks = async (
    pageNumber: number = 1,
    pageSize: number = 10,
    sortBy?: string,
    sortOrder?: string
  ): Promise<BookListResponse> => {
    try {
      let url = `/Book?pageNumber=${pageNumber}&pageSize=${pageSize}`;
      
      if (sortBy) {
        url += `&sortBy=${sortBy}`;
      }
      
      if (sortOrder) {
        url += `&sortOrder=${sortOrder}`;
      }
      
      const response = await api.get(url);
      
      // Extract total count from headers
      const totalCount = response.headers['x-total-count'] 
        ? parseInt(response.headers['x-total-count']) 
        : 0;
      
      // Create a map of categoryId -> categoryName from API to ensure complete information
      const categoryMap = new Map<string, string>();
      
      // Get all categories
      const categoriesResponse = await CategoryService.getCategories(1, 100);
      if (categoriesResponse.success) {
        categoriesResponse.data.forEach(category => {
          categoryMap.set(category.categoryId.toString(), category.categoryName);
        });
      }
      
      // Ensure each book has a categoryName
      const booksWithCategoryNames = response.data.map((book: Book) => {
        return {
          ...book,
          categoryName: categoryMap.get(book.categoryId) || 'Unknown Category'
        };
      });
      
      return {
        success: true,
        data: booksWithCategoryNames,
        totalCount,
        headers: {
          'x-total-count': response.headers['x-total-count'],
          'x-page-number': response.headers['x-page-number'],
          'x-page-size': response.headers['x-page-size']
        }
      };
    } catch (error: unknown) {
      console.error('Error fetching books:', error);
      return {
        success: false,
        data: [],
        message: getDetailedErrorMessage(error, 'Failed to fetch books')
      };
    }
  };
  
  // Get books by category
  getBooksByCategory = async (
    categoryId: string,
    pageNumber: number = 1,
    pageSize: number = 10
  ): Promise<BookListResponse> => {
    try {
      const url = `/Book/by-category/${categoryId}?pageNumber=${pageNumber}&pageSize=${pageSize}`;
      const response = await api.get(url);
      
      // Extract total count from headers
      const totalCount = response.headers['x-total-count'] 
        ? parseInt(response.headers['x-total-count']) 
        : 0;
      
      // Get category information to ensure each book has a categoryName
      const categoryResponse = await CategoryService.getCategory(categoryId);
      const categoryName = categoryResponse.success ? categoryResponse.data?.categoryName : 'Unknown Category';
      
      // Ensure each book has a categoryName
      const booksWithCategoryNames = response.data.map((book: Book) => {
        return {
          ...book,
          categoryName: categoryName
        };
      });
      
      return {
        success: true,
        data: booksWithCategoryNames,
        totalCount,
        headers: {
          'x-total-count': response.headers['x-total-count'],
          'x-page-number': response.headers['x-page-number'],
          'x-page-size': response.headers['x-page-size']
        }
      };
    } catch (error: unknown) {
      console.error(`Error fetching books for category ${categoryId}:`, error);
      return {
        success: false,
        data: [],
        message: getDetailedErrorMessage(error, 'Failed to fetch books for this category')
      };
    }
  };
  
  // Get available books
  getAvailableBooks = async (
    pageNumber: number = 1,
    pageSize: number = 10
  ): Promise<BookListResponse> => {
    try {
      const url = `/Book/available?pageNumber=${pageNumber}&pageSize=${pageSize}`;
      const response = await api.get(url);

      let totalCount = 0;
      
      if (response.headers['x-total-count']) {
        totalCount = parseInt(response.headers['x-total-count']);
      } else if (response.headers['X-Total-Count']) {
        totalCount = parseInt(response.headers['X-Total-Count']);
      }

      // Create a map of categoryId -> categoryName from API to ensure complete information
      const categoryMap = new Map<string, string>();
      
      // Get all categories
      const categoriesResponse = await CategoryService.getCategories(1, 100);
      if (categoriesResponse.success) {
        categoriesResponse.data.forEach(category => {
          categoryMap.set(category.categoryId.toString(), category.categoryName);
        });
      }
      
      // Ensure each book has a categoryName
      const booksWithCategoryNames = response.data.map((book: Book) => {
        return {
          ...book,
          categoryName: categoryMap.get(book.categoryId) || 'Unknown Category'
        };
      });
      
      return {
        success: true,
        data: booksWithCategoryNames,
        totalCount,
        message: `Found ${totalCount} books`,
        headers: {
          'x-total-count': totalCount.toString(),
          'x-page-number': pageNumber.toString(),
          'x-page-size': pageSize.toString()
        }
      };
    } catch (error: unknown) {
      console.error('Error fetching available books:', error);
      return {
        success: false,
        data: [],
        message: getDetailedErrorMessage(error, 'Failed to fetch available books')
      };
    }
  };
  
  // Create a new book
  createBook = async (bookData: BookCreateDto): Promise<MutationResponse> => {
    try {
      const response = await api.post('/Book', bookData);
      return {
        success: true,
        message: response.data.message || 'Book created successfully'
      };
    } catch (error: unknown) {
      console.error('Error creating book:', error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Failed to create book')
      };
    }
  };
  
  // Update a book
  updateBook = async (bookData: BookUpdateDto): Promise<MutationResponse> => {
    try {
      const response = await api.put('/Book', bookData);
      return {
        success: true,
        message: response.data.message || 'Book updated successfully'
      };
    } catch (error: unknown) {
      console.error(`Error updating book ${bookData.bookId}:`, error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Failed to update book')
      };
    }
  };
  
  // Delete a book
  deleteBook = async (bookId: string): Promise<MutationResponse> => {
    try {
      const response = await api.delete(`/Book/${bookId}`);
      return {
        success: true,
        message: response.data.message || 'Book deleted successfully'
      };
    } catch (error: unknown) {
      console.error(`Error deleting book ${bookId}:`, error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Failed to delete book')
      };
    }
  };
}

export default new BookService(); 