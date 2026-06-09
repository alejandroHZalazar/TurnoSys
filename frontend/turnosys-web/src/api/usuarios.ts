import apiClient from './client'
import type { ApiResponse, Usuario } from '@/types'

interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalItems: number
}

export const usuariosApi = {
  getAll: async (params?: { page?: number; pageSize?: number; busqueda?: string; soloActivos?: boolean }) => {
    const { data } = await apiClient.get<ApiResponse<PagedResult<Usuario>>>('/usuarios', { params })
    return data.data
  },

  crear: async (body: {
    nombreCompleto: string
    email: string
    password: string
    rolId: number
    empresaId?: string
    profesionalId?: string
  }) => {
    const { data } = await apiClient.post<ApiResponse<string>>('/usuarios', body)
    return data.data
  },

  editar: async (id: string, body: {
    nombreCompleto: string
    email: string
    rolId: number
    isActivo: boolean
    empresaId?: string
    profesionalId?: string
  }) => {
    await apiClient.put(`/usuarios/${id}`, body)
  },

  desactivar: async (id: string) => {
    await apiClient.patch(`/usuarios/${id}/desactivar`)
  },

  resetPassword: async (id: string, nuevaPassword: string) => {
    await apiClient.patch(`/usuarios/${id}/reset-password`, { nuevaPassword })
  },
}
