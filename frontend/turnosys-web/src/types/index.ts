export interface TurnoAgenda {
  id: string
  titulo: string
  inicio: string
  fin: string
  color: string
  estado: string
  estadoId: number
  profesionalId: string
  profesionalNombre: string
  profesionalColor: string
  pacienteId: string
  pacienteNombre: string
  pacienteTelefono?: string
  practicaId: string
  practicaNombre: string
  duracionMinutos: number
  observaciones?: string
  proximoControlFecha?: string
}

export interface Profesional {
  id: string
  nombre: string
  apellido: string
  nombreCompleto: string
  email?: string
  telefono?: string
  especialidad?: string
  colorAgenda: string
  isActivo: boolean
}

export interface Paciente {
  id: string
  nombre: string
  apellido: string
  nombreCompleto: string
  dni?: string
  telefono?: string
  email?: string
  obraSocial?: string
}

export interface Practica {
  id: string
  nombre: string
  precio: number
  duracionMinutos: number
  color?: string
  isActivo: boolean
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  nombreCompleto: string
  rol: string
  empresaId?: string
}

export interface ApiResponse<T> {
  success: boolean
  data: T
  errors?: string[]
  message?: string
}

export type EstadoTurno = 'Disponible' | 'Reservado' | 'Cancelado' | 'Atendido'
