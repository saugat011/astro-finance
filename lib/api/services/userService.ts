import api from '../axios';
import { ApiResponse, PaginatedResponse, User } from '../types';

export const userService = {
  /**
   * Get all users with pagination
   */
  getUsers: async (page = 1, pageSize = 10): Promise<ApiResponse<PaginatedResponse<User>>> => {
    const response = await api.get<ApiResponse<PaginatedResponse<User>>>('/users', {
      params: { page, pageSize }
    });
    return response.data;
  },

  /**
   * Get user by ID
   */
  getUserById: async (id: string): Promise<ApiResponse<User>> => {
    const response = await api.get<ApiResponse<User>>(`/users/${id}`);
    return response.data;
  },

  /**
   * Create a new user
   */
  createUser: async (data: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    role: string;
  }): Promise<ApiResponse<User>> => {
    const response = await api.post<ApiResponse<User>>('/users', data);
    return response.data;
  },

  /**
   * Update user
   */
  updateUser: async (id: string, data: {
    email?: string;
    firstName?: string;
    lastName?: string;
    role?: string;
  }): Promise<ApiResponse<User>> => {
    const response = await api.put<ApiResponse<User>>(`/users/${id}`, data);
    return response.data;
  },

  /**
   * Delete user
   */
  deleteUser: async (id: string): Promise<ApiResponse<void>> => {
    const response = await api.delete<ApiResponse<void>>(`/users/${id}`);
    return response.data;
  },

  /**
   * Change user password
   */
  changePassword: async (id: string, data: {
    currentPassword: string;
    newPassword: string;
  }): Promise<ApiResponse<void>> => {
    const response = await api.post<ApiResponse<void>>(`/users/${id}/change-password`, data);
    return response.data;
  }
};