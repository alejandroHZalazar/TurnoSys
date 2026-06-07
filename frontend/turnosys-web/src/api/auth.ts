import apiClient from './client'
import type { LoginResponse, ApiResponse } from '@/types'

export const authApi = {
  login: async (email: string, password: string) => {
    const { data } = await apiClient.post<ApiResponse<LoginResponse>>('/auth/login', { email, password })
    return data.data
  },
}
