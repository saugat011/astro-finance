import axios, { AxiosError } from 'axios';
import { ApiError } from '@/lib/api/types';

/**
 * Extract error message from API error response
 */
export function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const axiosError = error as AxiosError<ApiError>;
    
    // Check if it's a structured API error
    if (axiosError.response?.data) {
      const apiError = axiosError.response.data;
      
      // Return the main error message if available
      if (apiError.message) {
        return apiError.message;
      }
      
      // If there are validation errors, return the first one
      if (apiError.errors) {
        const firstErrorKey = Object.keys(apiError.errors)[0];
        if (firstErrorKey && apiError.errors[firstErrorKey].length > 0) {
          return apiError.errors[firstErrorKey][0];
        }
      }
    }
    
    // Fallback to status text or generic message
    return axiosError.response?.statusText || 'An error occurred with the API request';
  }
  
  // For non-Axios errors, return the error message or a generic one
  if (error instanceof Error) {
    return error.message;
  }
  
  return 'An unknown error occurred';
}

/**
 * Format API data for form submission
 * Handles date formatting and other common transformations
 */
export function formatApiData<T extends Record<string, any>>(data: T): T {
  const formattedData = { ...data };
  
  // Format dates to ISO strings for API
  Object.keys(formattedData).forEach(key => {
    const value = formattedData[key];
    
    // Check if value is a Date object
    if (value instanceof Date) {
      formattedData[key] = value.toISOString();
    }
  });
  
  return formattedData;
}

/**
 * Parse API response data
 * Handles date parsing and other common transformations
 */
export function parseApiResponse<T extends Record<string, any>>(data: T): T {
  const parsedData = { ...data };
  
  // Parse date strings to Date objects
  Object.keys(parsedData).forEach(key => {
    const value = parsedData[key];
    
    // Check if value is a date string (simple check)
    if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/.test(value)) {
      parsedData[key] = new Date(value);
    }
  });
  
  return parsedData;
}