import apiClient from './client'
import type { ApiResponse } from '@/types'

export interface PacienteListItem {
  id: string
  nombre: string
  apellido: string
  nombreCompleto: string
  dni?: string
  telefono?: string
  email?: string
  obraSocial?: string
  edad?: number
  isActivo: boolean
  totalTurnos: number
}

export interface PacienteDetalle {
  id: string
  nombre: string
  apellido: string
  nombreCompleto: string
  dni?: string
  fechaNacimiento?: string
  edad?: number
  telefono?: string
  email?: string
  direccion?: string
  obraSocial?: string
  numeroAfiliado?: string
  contactoEmergenciaNombre?: string
  contactoEmergenciaTelefono?: string
  observaciones?: string
  restricciones?: string
  consentimientoFirmado: boolean
  fechaConsentimiento?: string
  isActivo: boolean
  createdAt: string
}

export interface PacienteForm {
  nombre: string
  apellido: string
  dni?: string
  fechaNacimiento?: string
  telefono?: string
  email?: string
  direccion?: string
  obraSocial?: string
  numeroAfiliado?: string
  contactoEmergenciaNombre?: string
  contactoEmergenciaTelefono?: string
  observaciones?: string
  restricciones?: string
  isActivo?: boolean
}

export interface PacientesResponse {
  data: PacienteListItem[]
  pagination: { page: number; pageSize: number; totalItems: number; totalPages: number }
}

export const pacientesApi = {
  getAll: async (params?: { busqueda?: string; soloActivos?: boolean; page?: number; pageSize?: number }) => {
    const { data } = await apiClient.get<PacientesResponse>('/pacientes', { params })
    return data
  },

  getById: async (id: string) => {
    const { data } = await apiClient.get<ApiResponse<PacienteDetalle>>(`/pacientes/${id}`)
    return data.data
  },

  crear: async (form: PacienteForm) => {
    const { data } = await apiClient.post<ApiResponse<{ id: string }>>('/pacientes', form)
    return data.data
  },

  actualizar: async (id: string, form: PacienteForm) => {
    await apiClient.put(`/pacientes/${id}`, form)
  },

  eliminar: async (id: string) => {
    await apiClient.delete(`/pacientes/${id}`)
  },
}
