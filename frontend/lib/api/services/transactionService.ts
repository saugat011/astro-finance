import api from '../axios';
import { 
  ApiResponse, 
  PaginatedResponse, 
  Transaction, 
  TransactionType 
} from '../types';

export const transactionService = {
  /**
   * Get all transactions with pagination
   */
  getTransactions: async (page = 1, pageSize = 10): Promise<ApiResponse<PaginatedResponse<Transaction>>> => {
    const response = await api.get<ApiResponse<PaginatedResponse<Transaction>>>('/transactions', {
      params: { page, pageSize }
    });
    return response.data;
  },

  /**
   * Get transactions by loan ID
   */
  getTransactionsByLoanId: async (loanId: string): Promise<ApiResponse<Transaction[]>> => {
    const response = await api.get<ApiResponse<Transaction[]>>(`/transactions/loan/${loanId}`);
    return response.data;
  },

  /**
   * Create a new transaction
   */
  createTransaction: async (data: {
    loanId?: string;
    customerId: string;
    amount: number;
    type: TransactionType;
    description: string;
    date: string;
  }): Promise<ApiResponse<Transaction>> => {
    const response = await api.post<ApiResponse<Transaction>>('/transactions', data);
    return response.data;
  },

  /**
   * Get daily transactions summary
   */
  getDailyTransactions: async (date: string): Promise<ApiResponse<{
    totalDisbursed: number;
    totalRepaid: number;
    totalFees: number;
    transactions: Transaction[];
  }>> => {
    const response = await api.get<ApiResponse<{
      totalDisbursed: number;
      totalRepaid: number;
      totalFees: number;
      transactions: Transaction[];
    }>>('/transactions/daily', {
      params: { date }
    });
    return response.data;
  }
};