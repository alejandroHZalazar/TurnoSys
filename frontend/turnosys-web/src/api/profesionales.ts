import apiClient from './client'
import type { ApiResponse } from '@/types'

export interface HorarioItem {
  diaSemana: number
  diaNombre: string
  horaInicio: string
  horaFin: string
}

export interface HorarioForm {
  diaSemana: number
  horaInicio: string
  horaFin: string
}

export interface ProfesionalListItem {
  id: string
  nombre: string
  apellido: string
  nombreCompleto: string
  especialidad?: string
  matricula?: string
  telefono?: string
  email?: string
  colorAgenda: string
  isActivo: boolean
  totalTurnos: number
  horarios: HorarioItem[]
}

export interface ProfesionalDetalle extends ProfesionalListItem {
  fotoUrl?: string
  observaciones?: string
  createdAt: string
}

export interface ProfesionalForm {
  nombre: string
  apellido: string
  email?: string
  telefono?: string
  especialidad?: string
  matricula?: string
  colorAgenda: string
  observaciones?: string
  isActivo?: boolean
  horarios: HorarioForm[]
}

export const profesionalesApi = {
  getAll: async (soloActivos = true) => {
    const { data } = await apiClient.get<ApiResponse<ProfesionalListItem[]>>('/profesionales', {
      params: { soloActivos }
    })
    return data.data
  },

  getById: async (id: string) => {
    const { data } = await apiClient.get<ApiResponse<ProfesionalDetalle>>(`/profesionales/${id}`)
    return data.data
  },

  crear: async (form: ProfesionalForm) => {
    const { data } = await apiClient.post<ApiResponse<{ id: string }>>('/profesionales', form)
    return data.data
  },

  actualizar: async (id: string, form: ProfesionalForm) => {
    await apiClient.put(`/profesionales/${id}`, form)
  },

  eliminar: async (id: string) => {
    await apiClient.delete(`/profesionales/${id}`)
  },
}
