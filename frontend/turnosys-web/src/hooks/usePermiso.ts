import { useAuthStore } from '@/store/authStore'

/**
 * Verifica si el usuario tiene el permiso solicitado.
 * - SuperAdmin (rol === 'SuperAdmin') tiene acceso a todo.
 * - Para otros roles, comprueba la lista de permisos del store.
 * - Acepta wildcard: "agenda.*" cubre "agenda.ver", "agenda.crear", etc.
 */
export function usePermiso(permiso: string): boolean {
  const { rol, permisos } = useAuthStore()

  if (rol === 'SuperAdmin') return true
  if (permisos.length === 0) return false

  const [modulo] = permiso.split('.')
  return (
    permisos.includes(permiso) ||
    permisos.includes(`${modulo}.*`)
  )
}

/**
 * Devuelve true si el usuario tiene TODOS los permisos listados.
 */
export function useTienePermisos(permisosList: string[]): boolean {
  const { rol, permisos } = useAuthStore()
  if (rol === 'SuperAdmin') return true
  return permisosList.every((p) => {
    const [modulo] = p.split('.')
    return permisos.includes(p) || permisos.includes(`${modulo}.*`)
  })
}
