import apiClient from './client'
import type { ApiResponse } from '@/types'

export interface DashboardKPIs {
  totalTurnos: number
  atendidos: number
  cancelados: number
  reservados: number
  ingresoEstimado: number
  porcentajeOcupacion: number
  pacientesNuevos: number
}

export interface TurnosPorDia {
  fecha: string
  total: number
  atendidos: number
  cancelados: number
  reservados: number
  ingresoEstimado: number
}

export interface HeatmapCelda {
  diaSemana: number
  hora: number
  cantidad: number
}

export interface EstadisticaProfesional {
  profesionalId: string
  nombre: string
  colorAgenda: string
  totalTurnos: number
  atendidos: number
  cancelados: number
  ingresoEstimado: number
  porcentajeOcupacion: number
}

export interface EstadisticasPacientes {
  totalPacientes: number
  pacientesNuevos: number
  pacientesRecurrentes: number
  topPacientes: { pacienteId: string; nombre: string; totalTurnos: number; turnosAtendidos: number }[]
}

const fmt = (d: Date) => d.toISOString()

export const estadisticasApi = {
  getKPIs: async (desde: Date, hasta: Date) => {
    const { data } = await apiClient.get<ApiResponse<DashboardKPIs>>('/estadisticas/kpis', {
      params: { desde: fmt(desde), hasta: fmt(hasta) }
    })
    return data.data
  },
  getTurnosPorPeriodo: async (desde: Date, hasta: Date, profesionalId?: string) => {
    const { data } = await apiClient.get<ApiResponse<TurnosPorDia[]>>('/estadisticas/turnos-por-periodo', {
      params: { desde: fmt(desde), hasta: fmt(hasta), profesionalId }
    })
    return data.data
  },
  getHeatmap: async (diasAtras = 90) => {
    const { data } = await apiClient.get<ApiResponse<HeatmapCelda[]>>('/estadisticas/heatmap', {
      params: { diasAtras }
    })
    return data.data
  },
  getProfesionales: async (desde: Date, hasta: Date) => {
    const { data } = await apiClient.get<ApiResponse<EstadisticaProfesional[]>>('/estadisticas/profesionales', {
      params: { desde: fmt(desde), hasta: fmt(hasta) }
    })
    return data.data
  },
  getPacientes: async (desde: Date, hasta: Date) => {
    const { data } = await apiClient.get<ApiResponse<EstadisticasPacientes>>('/estadisticas/pacientes', {
      params: { desde: fmt(desde), hasta: fmt(hasta) }
    })
    return data.data
  },
}
