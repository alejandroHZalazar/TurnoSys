import apiClient from './client'
import type { ApiResponse } from '@/types'

export interface ParametroConfig {
  clave: string
  label: string
  tipo: string          // string | int | bool | time | dias | select
  valor: string
  esSecreto: boolean
  configurado: boolean
  opciones?: string
  ayuda?: string
}

export interface SeccionConfig {
  seccion: string
  parametros: ParametroConfig[]
}

export const configuracionApi = {
  get: async () => {
    const { data } = await apiClient.get<ApiResponse<SeccionConfig[]>>('/configuracion')
    return data.data
  },
  actualizar: async (parametros: { clave: string; valor: string | null }[]) => {
    await apiClient.put('/configuracion', parametros)
  },
  probarEmail: async (email?: string) => {
    await apiClient.post('/configuracion/probar-email', { email })
  },
}
