import apiClient from './client'
import type { TurnoAgenda, ApiResponse } from '@/types'

export const turnosApi = {
  getAgenda: async (desde: string, hasta: string, profesionalId?: string) => {
    const params: Record<string, string> = { desde, hasta }
    if (profesionalId) params.profesionalId = profesionalId
    const { data } = await apiClient.get<ApiResponse<TurnoAgenda[]>>('/turnos/agenda', { params })
    return data.data
  },

  crear: async (payload: {
    profesionalId: string
    pacienteId: string
    practicaId: string
    fechaHoraInicio: string
    observaciones?: string
    proximoControlFecha?: string
  }) => {
    const { data } = await apiClient.post<ApiResponse<{ id: string }>>('/turnos', payload)
    return data.data
  },

  cancelar: async (id: string, motivo?: string) => {
    await apiClient.put(`/turnos/${id}/cancelar`, { motivo })
  },

  reagendar: async (id: string, nuevaFechaHoraInicio: string) => {
    await apiClient.put(`/turnos/${id}/reagendar`, { nuevaFechaHoraInicio })
  },

  atender: async (id: string) => {
    await apiClient.put(`/turnos/${id}/atender`)
  },
}
