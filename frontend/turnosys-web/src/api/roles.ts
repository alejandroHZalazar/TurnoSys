import apiClient from './client'
import type { ApiResponse, Rol } from '@/types'

export const rolesApi = {
  getAll: async () => {
    const { data } = await apiClient.get<ApiResponse<Rol[]>>('/roles')
    return data.data
  },

  actualizarPermisos: async (id: number, body: { descripcion?: string; permisos: string | null }) => {
    await apiClient.put(`/roles/${id}/permisos`, body)
  },
}
