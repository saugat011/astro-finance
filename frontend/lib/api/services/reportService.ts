import api from '../axios';
import { ApiResponse, ReportParams } from '../types';

export const reportService = {
  /**
   * Get balance sheet report
   */
  getBalanceSheet: async (params: ReportParams): Promise<ApiResponse<any>> => {
    const response = await api.get<ApiResponse<any>>('/reports/balance-sheet', { params });
    return response.data;
  },

  /**
   * Get trial balance report
   */
  getTrialBalance: async (params: ReportParams): Promise<ApiResponse<any>> => {
    const response = await api.get<ApiResponse<any>>('/reports/trial-balance', { params });
    return response.data;
  },

  /**
   * Get day book report
   */
  getDayBook: async (params: ReportParams): Promise<ApiResponse<any>> => {
    const response = await api.get<ApiResponse<any>>('/reports/day-book', { params });
    return response.data;
  },

  /**
   * Export report to PDF
   */
  exportToPdf: async (reportType: string, params: ReportParams): Promise<Blob> => {
    const response = await api.get(`/reports/${reportType}/export`, {
      params,
      responseType: 'blob'
    });
    return response.data;
  },

  /**
   * Export report to Excel
   */
  exportToExcel: async (reportType: string, params: ReportParams): Promise<Blob> => {
    const response = await api.get(`/reports/${reportType}/export`, {
      params: { ...params, format: 'excel' },
      responseType: 'blob'
    });
    return response.data;
  }
};