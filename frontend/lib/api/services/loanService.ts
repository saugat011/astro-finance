import api from '../axios';
import { 
  ApiResponse, 
  CreateLoanRequest, 
  Loan, 
  PaginatedResponse 
} from '../types';

export const loanService = {
  /**
   * Get all loans with pagination
   */
  getLoans: async (page = 1, pageSize = 10): Promise<ApiResponse<PaginatedResponse<Loan>>> => {
    const response = await api.get<ApiResponse<PaginatedResponse<Loan>>>('/loans', {
      params: { page, pageSize }
    });
    return response.data;
  },

  /**
   * Get loan by ID
   */
  getLoanById: async (id: string): Promise<ApiResponse<Loan>> => {
    const response = await api.get<ApiResponse<Loan>>(`/loans/${id}`);
    return response.data;
  },

  /**
   * Create a new loan
   */
  createLoan: async (data: CreateLoanRequest): Promise<ApiResponse<Loan>> => {
    const response = await api.post<ApiResponse<Loan>>('/loans', data);
    return response.data;
  },

  /**
   * Update loan status
   */
  updateLoanStatus: async (id: string, status: string): Promise<ApiResponse<Loan>> => {
    const response = await api.patch<ApiResponse<Loan>>(`/loans/${id}/status`, { status });
    return response.data;
  },

  /**
   * Delete loan
   */
  deleteLoan: async (id: string): Promise<ApiResponse<void>> => {
    const response = await api.delete<ApiResponse<void>>(`/loans/${id}`);
    return response.data;
  }
};