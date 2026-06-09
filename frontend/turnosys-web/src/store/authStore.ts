import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  nombreCompleto: string | null
  rol: string | null
  empresaId: string | null
  permisos: string[]          // lista de permisos del rol; vacío = sin restricción si es SuperAdmin
  isAuthenticated: boolean
  login: (data: {
    accessToken: string
    refreshToken: string
    nombreCompleto: string
    rol: string
    empresaId?: string
    permisos?: string[]
  }) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      refreshToken: null,
      nombreCompleto: null,
      rol: null,
      empresaId: null,
      permisos: [],
      isAuthenticated: false,
      login: (data) =>
        set({
          accessToken: data.accessToken,
          refreshToken: data.refreshToken,
          nombreCompleto: data.nombreCompleto,
          rol: data.rol,
          empresaId: data.empresaId ?? null,
          permisos: data.permisos ?? [],
          isAuthenticated: true,
        }),
      logout: () =>
        set({
          accessToken: null,
          refreshToken: null,
          nombreCompleto: null,
          rol: null,
          empresaId: null,
          permisos: [],
          isAuthenticated: false,
        }),
    }),
    { name: 'turnosys-auth' }
  )
)
