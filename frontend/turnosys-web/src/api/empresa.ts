import apiClient from './client'
import type { ApiResponse } from '@/types'

export interface Empresa {
  id: string
  razonSocial: string
  nombreFantasia?: string
  cuit?: string
  direccion?: string
  telefono?: string
  email?: string
  logotipoUrl?: string
  sitioWeb?: string
  instagram?: string
  facebook?: string
  whatsApp?: string
  horarioDesde?: string
  horarioHasta?: string
  timeZone: string
  observaciones?: string
  isActivo: boolean
  tieneLogo: boolean
}

export type EmpresaForm = Omit<Empresa, 'id' | 'isActivo' | 'tieneLogo' | 'logotipoUrl'>

export const empresaApi = {
  get: async () => {
    const { data } = await apiClient.get<ApiResponse<Empresa>>('/empresa')
    return data.data
  },
  actualizar: async (form: EmpresaForm) => {
    await apiClient.put('/empresa', form)
  },
  subirLogo: async (archivo: File) => {
    const fd = new FormData()
    fd.append('archivo', archivo)
    await apiClient.post('/empresa/logo', fd, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
  },
}
