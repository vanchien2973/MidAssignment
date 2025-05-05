import { ApiErrorResponse } from '../types/api';

/**
 * Extract detailed error message from API error response
 * @param error - Error from API
 * @param defaultMessage - Default message if no detailed error is found
 * @returns Error message
 */
export const getDetailedErrorMessage = (error: unknown, defaultMessage = 'Operation failed'): string => {
  if (!error || typeof error !== 'object' || !('response' in error)) {
    return 'An unexpected error occurred';
  }
  
  const errorData = (error as { response: { data: ApiErrorResponse } }).response.data;
  
  // Prioritize error messages from errorMessages array if available
  if (errorData.errorMessages && errorData.errorMessages.length > 0) {
    return errorData.errorMessages[0];
  }
  
  // If no errorMessages, try to get from errors object
  if (errorData.errors) {
    // Get the first error from the errors object
    for (const field in errorData.errors) {
      if (errorData.errors[field] && errorData.errors[field].length > 0) {
        return `${field}: ${errorData.errors[field][0]}`;
      }
    }
  }
  
  // Check for title and message combination
  if (errorData.title && errorData.message) {
    return `${errorData.title}: ${errorData.message}`;
  }
  
  // If no detailed error found, use message or default
  return errorData.message || defaultMessage;
}; 