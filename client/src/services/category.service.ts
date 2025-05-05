import api from '../config/axios';
import { 
  CategoryCreateDto, 
  CategoryUpdateDto, 
  CategoryListResponse, 
  CategoryResponse, 
} from '../types/category';
import { MutationResponse } from '../types/api';
import { getDetailedErrorMessage } from '../utils/errorUtils';

class CategoryService {
  // Get categories with pagination
  getCategories = async (
    pageNumber: number = 1,
    pageSize: number = 10,
    sortBy?: string,
    sortOrder?: string,
    searchTerm?: string
  ): Promise<CategoryListResponse> => {
    try {
      let url = `/Category?pageNumber=${pageNumber}&pageSize=${pageSize}`;
      
      if (sortBy) {
        url += `&sortBy=${sortBy}`;
      }
      
      if (sortOrder) {
        url += `&sortOrder=${sortOrder}`;
      }
      
      if (searchTerm && searchTerm.trim() !== '') {
        url += `&searchTerm=${encodeURIComponent(searchTerm.trim())}`;
      }
      
      const response = await api.get(url);
      
      // Parse pagination headers
      const totalCount = response.headers['x-total-count'];
      const respPageNumber = response.headers['x-page-number'];
      const respPageSize = response.headers['x-page-size'];
      
      return {
        success: true,
        data: response.data,
        totalCount: totalCount ? parseInt(totalCount, 10) : undefined,
        pageNumber: respPageNumber ? parseInt(respPageNumber, 10) : pageNumber,
        pageSize: respPageSize ? parseInt(respPageSize, 10) : pageSize
      };
    } catch (error: unknown) {
      console.error('Error fetching categories:', error);
      return {
        success: false,
        data: [],
        message: getDetailedErrorMessage(error, 'Failed to fetch categories')
      };
    }
  };
  
  // Get details of a category
  getCategory = async (categoryId: string): Promise<CategoryResponse> => {
    try {
      const response = await api.get(`/Category/${categoryId}`);
      return {
        success: true,
        data: response.data
      };
    } catch (error: unknown) {
      console.error(`Error fetching category ${categoryId}:`, error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Failed to fetch category details')
      };
    }
  };
  
  // Create a new category
  createCategory = async (categoryData: CategoryCreateDto): Promise<MutationResponse> => {
    try {
      const response = await api.post('/Category', categoryData);
      return {
        success: true,
        message: response.data.message || 'Category created successfully'
      };
    } catch (error: unknown) {
      console.error('Error creating category:', error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Failed to create category')
      };
    }
  };
  
  // Update a category
  updateCategory = async (categoryData: CategoryUpdateDto): Promise<MutationResponse> => {
    try {
      const response = await api.put('/Category', categoryData);
      return {
        success: true,
        message: response.data.message || 'Category updated successfully'
      };
    } catch (error: unknown) {
      console.error(`Error updating category ${categoryData.categoryId}:`, error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Failed to update category')
      };
    }
  };
  
  // Delete a category
  deleteCategory = async (categoryId: string): Promise<MutationResponse> => {
    try {
      const response = await api.delete(`/Category/${categoryId}`);
      return {
        success: true,
        message: response.data.message || 'Category deleted successfully'
      };
    } catch (error: unknown) {
      console.error(`Error deleting category ${categoryId}:`, error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Failed to delete category')
      };
    }
  };
}

export default new CategoryService(); 