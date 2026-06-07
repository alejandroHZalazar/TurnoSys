import apiClient from './client'
import type { ApiResponse } from '@/types'

export interface PracticaItem {
  id: string
  nombre: string
  descripcion?: string
  precio: number
  duracionMinutos: number
  color?: string
  requiereObservaciones: boolean
  recordatorioRecDias?: number
  isActivo: boolean
  categoriaId?: string
  categoriaNombre?: string
  categoriaColor?: string
}

export interface PracticaForm {
  nombre: string
  descripcion?: string
  precio: number
  duracionMinutos: number
  color?: string
  requiereObservaciones?: boolean
  recordatorioRecDias?: number
  categoriaId?: string
  isActivo?: boolean
}

export const practicasApi = {
  getAll: async (soloActivas = true) => {
    const { data } = await apiClient.get<ApiResponse<PracticaItem[]>>('/practicas', { params: { soloActivas } })
    return data.data
  },
  getById: async (id: string) => {
    const { data } = await apiClient.get<ApiResponse<PracticaItem>>(`/practicas/${id}`)
    return data.data
  },
  crear: async (form: PracticaForm) => {
    const { data } = await apiClient.post<ApiResponse<{ id: string }>>('/practicas', form)
    return data.data
  },
  actualizar: async (id: string, form: PracticaForm) => {
    await apiClient.put(`/practicas/${id}`, form)
  },
  eliminar: async (id: string) => {
    await apiClient.delete(`/practicas/${id}`)
  },
}
