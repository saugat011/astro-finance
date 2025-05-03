import api from '../axios';
import { 
  ApiResponse, 
  CreateLoanRequest, 
  Loan, 
  LoanStatus,
  LoanType,
  PaginatedResponse,
  PaymentSchedule
} from '../types';

export const loanService = {
  /**
   * Get all loans with pagination
   */
  getLoans: async (page = 1, pageSize = 10, filters?: { 
    status?: LoanStatus, 
    type?: LoanType,
    customerId?: string,
    startDate?: string,
    endDate?: string
  }): Promise<ApiResponse<PaginatedResponse<Loan>>> => {
    const response = await api.get<ApiResponse<PaginatedResponse<Loan>>>('/loans', {
      params: { page, pageSize, ...filters }
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
   * Get loan summary statistics
   */
  getLoanSummary: async (): Promise<ApiResponse<{
    totalActiveLoans: number;
    totalDisbursedAmount: number;
    flatLoans: {
      count: number;
      amount: number;
      averageInterestRate: number;
    };
    diminishingLoans: {
      count: number;
      amount: number;
      averageInterestRate: number;
    };
  }>> => {
    const response = await api.get<ApiResponse<any>>('/loans/summary');
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
   * Update loan
   */
  updateLoan: async (id: string, data: Partial<CreateLoanRequest>): Promise<ApiResponse<Loan>> => {
    const response = await api.put<ApiResponse<Loan>>(`/loans/${id}`, data);
    return response.data;
  },

  /**
   * Update loan status
   */
  updateLoanStatus: async (id: string, status: LoanStatus): Promise<ApiResponse<Loan>> => {
    const response = await api.patch<ApiResponse<Loan>>(`/loans/${id}/status`, { status });
    return response.data;
  },

  /**
   * Get payment schedule for a loan
   */
  getPaymentSchedule: async (loanId: string): Promise<ApiResponse<PaymentSchedule[]>> => {
    const response = await api.get<ApiResponse<PaymentSchedule[]>>(`/loans/${loanId}/payment-schedule`);
    return response.data;
  },

  /**
   * Calculate loan EMI and payment schedule
   */
  calculateLoan: async (data: {
    amount: number;
    interestRate: number;
    term: number;
    type: LoanType;
    startDate: string;
  }): Promise<ApiResponse<{
    emi: number;
    totalRepayment: number;
    totalInterest: number;
    paymentSchedule: PaymentSchedule[];
  }>> => {
    const response = await api.post<ApiResponse<any>>('/loans/calculate', data);
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