import api from '../axios';
import { 
  ApiResponse, 
  PaginatedResponse, 
  SmsHistory, 
  SmsTemplate 
} from '../types';

export const smsService = {
  /**
   * Get all SMS templates
   */
  getTemplates: async (): Promise<ApiResponse<SmsTemplate[]>> => {
    const response = await api.get<ApiResponse<SmsTemplate[]>>('/sms/templates');
    return response.data;
  },

  /**
   * Create a new SMS template
   */
  createTemplate: async (data: { name: string; content: string }): Promise<ApiResponse<SmsTemplate>> => {
    const response = await api.post<ApiResponse<SmsTemplate>>('/sms/templates', data);
    return response.data;
  },

  /**
   * Update an SMS template
   */
  updateTemplate: async (id: string, data: { name: string; content: string }): Promise<ApiResponse<SmsTemplate>> => {
    const response = await api.put<ApiResponse<SmsTemplate>>(`/sms/templates/${id}`, data);
    return response.data;
  },

  /**
   * Delete an SMS template
   */
  deleteTemplate: async (id: string): Promise<ApiResponse<void>> => {
    const response = await api.delete<ApiResponse<void>>(`/sms/templates/${id}`);
    return response.data;
  },

  /**
   * Get SMS history with pagination
   */
  getSmsHistory: async (page = 1, pageSize = 10): Promise<ApiResponse<PaginatedResponse<SmsHistory>>> => {
    const response = await api.get<ApiResponse<PaginatedResponse<SmsHistory>>>('/sms/history', {
      params: { page, pageSize }
    });
    return response.data;
  },

  /**
   * Send bulk SMS
   */
  sendBulkSms: async (data: { 
    templateId?: string; 
    message?: string; 
    recipients: { phoneNumber: string; name?: string }[] 
  }): Promise<ApiResponse<{ successCount: number; failedCount: number }>> => {
    const response = await api.post<ApiResponse<{ successCount: number; failedCount: number }>>('/sms/send-bulk', data);
    return response.data;
  }
};