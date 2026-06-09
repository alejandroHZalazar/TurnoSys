import apiClient from './client'
import type { LoginResponse, ApiResponse } from '@/types'

/** Decodifica el payload del JWT sin verificar firma (solo para leer claims en el cliente). */
function parseJwtPermisos(token: string): string[] {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    const raw: string = payload['permisos'] ?? ''
    if (!raw) return []
    return JSON.parse(raw) as string[]
  } catch {
    return []
  }
}

export const authApi = {
  login: async (email: string, password: string): Promise<LoginResponse> => {
    const { data } = await apiClient.post<ApiResponse<LoginResponse>>('/auth/login', { email, password })
    const resp = data.data
    // Extraer permisos del JWT para no necesitar un endpoint extra
    resp.permisos = parseJwtPermisos(resp.accessToken)
    return resp
  },
}
