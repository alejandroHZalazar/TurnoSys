import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import toast from 'react-hot-toast'
import { Shield, ChevronDown, ChevronUp, Save } from 'lucide-react'
import { rolesApi } from '@/api/roles'
import type { Rol } from '@/types'

// Catálogo de permisos agrupados por módulo
const CATALOGO: { modulo: string; label: string; permisos: { key: string; label: string }[] }[] = [
  {
    modulo: 'agenda',
    label: 'Agenda',
    permisos: [
      { key: 'agenda.ver', label: 'Ver agenda' },
      { key: 'agenda.crear', label: 'Crear turno' },
      { key: 'agenda.editar', label: 'Editar turno' },
      { key: 'agenda.cancelar', label: 'Cancelar turno' },
    ],
  },
  {
    modulo: 'pacientes',
    label: 'Pacientes',
    permisos: [
      { key: 'pacientes.ver', label: 'Ver pacientes' },
      { key: 'pacientes.crear', label: 'Crear paciente' },
      { key: 'pacientes.editar', label: 'Editar paciente' },
      { key: 'pacientes.eliminar', label: 'Eliminar paciente' },
    ],
  },
  {
    modulo: 'profesionales',
    label: 'Profesionales',
    permisos: [
      { key: 'profesionales.ver', label: 'Ver profesionales' },
      { key: 'profesionales.crear', label: 'Crear profesional' },
      { key: 'profesionales.editar', label: 'Editar profesional' },
      { key: 'profesionales.eliminar', label: 'Eliminar profesional' },
    ],
  },
  {
    modulo: 'practicas',
    label: 'Prácticas',
    permisos: [
      { key: 'practicas.ver', label: 'Ver prácticas' },
      { key: 'practicas.crear', label: 'Crear práctica' },
      { key: 'practicas.editar', label: 'Editar práctica' },
      { key: 'practicas.eliminar', label: 'Eliminar práctica' },
    ],
  },
  {
    modulo: 'dashboard',
    label: 'Dashboard',
    permisos: [
      { key: 'dashboard.ver', label: 'Ver estadísticas' },
    ],
  },
  {
    modulo: 'empresa',
    label: 'Empresa',
    permisos: [
      { key: 'empresa.ver', label: 'Ver empresa' },
      { key: 'empresa.editar', label: 'Editar empresa' },
    ],
  },
  {
    modulo: 'configuracion',
    label: 'Configuración',
    permisos: [
      { key: 'configuracion.ver', label: 'Ver configuración' },
      { key: 'configuracion.editar', label: 'Editar configuración' },
    ],
  },
  {
    modulo: 'usuarios',
    label: 'Usuarios',
    permisos: [
      { key: 'usuarios.ver', label: 'Ver usuarios' },
      { key: 'usuarios.crear', label: 'Crear usuario' },
      { key: 'usuarios.editar', label: 'Editar usuario' },
      { key: 'usuarios.desactivar', label: 'Desactivar usuario' },
    ],
  },
]

function parsePermisos(raw: string | null | undefined): string[] {
  if (!raw) return []
  try { return JSON.parse(raw) as string[] } catch { return [] }
}

interface RolCardProps {
  rol: Rol
}

function RolCard({ rol }: RolCardProps) {
  const qc = useQueryClient()
  const [expanded, setExpanded] = useState(false)
  const [permisos, setPermisos] = useState<string[]>(() => parsePermisos(rol.permisos))
  const isSuperAdmin = rol.id === 1

  const mutation = useMutation({
    mutationFn: () => rolesApi.actualizarPermisos(rol.id, {
      permisos: isSuperAdmin ? null : JSON.stringify(permisos),
    }),
    onSuccess: () => {
      toast.success('Permisos actualizados')
      qc.invalidateQueries({ queryKey: ['roles'] })
    },
    onError: () => toast.error('Error al guardar permisos'),
  })

  const toggle = (key: string) => {
    setPermisos((prev) =>
      prev.includes(key) ? prev.filter((p) => p !== key) : [...prev, key]
    )
  }

  const toggleModulo = (modulo: string, keys: string[]) => {
    const todosActivos = keys.every((k) => permisos.includes(k))
    if (todosActivos) {
      setPermisos((prev) => prev.filter((p) => !keys.includes(p)))
    } else {
      setPermisos((prev) => [...new Set([...prev, ...keys])])
    }
  }

  return (
    <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
      <button
        className="w-full flex items-center justify-between px-5 py-4 hover:bg-gray-50 transition-colors"
        onClick={() => setExpanded((v) => !v)}
      >
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 rounded-lg bg-indigo-50 flex items-center justify-center">
            <Shield className="w-4 h-4 text-indigo-600" />
          </div>
          <div className="text-left">
            <p className="font-semibold text-gray-900 text-sm">{rol.nombre}</p>
            <p className="text-xs text-gray-400">{rol.descripcion}</p>
          </div>
        </div>
        <div className="flex items-center gap-3">
          {isSuperAdmin ? (
            <span className="text-xs text-indigo-600 font-medium bg-indigo-50 px-2 py-0.5 rounded-full">Acceso total</span>
          ) : (
            <span className="text-xs text-gray-400">{permisos.length} permiso{permisos.length !== 1 ? 's' : ''}</span>
          )}
          {expanded ? <ChevronUp className="w-4 h-4 text-gray-400" /> : <ChevronDown className="w-4 h-4 text-gray-400" />}
        </div>
      </button>

      {expanded && !isSuperAdmin && (
        <div className="border-t border-gray-100 px-5 py-4 space-y-4">
          {CATALOGO.map(({ modulo, label, permisos: items }) => {
            const keys = items.map((i) => i.key)
            const todosActivos = keys.every((k) => permisos.includes(k))
            return (
              <div key={modulo}>
                <div className="flex items-center gap-2 mb-2">
                  <input
                    type="checkbox"
                    id={`${rol.id}-${modulo}`}
                    checked={todosActivos}
                    onChange={() => toggleModulo(modulo, keys)}
                    className="rounded accent-indigo-600"
                  />
                  <label htmlFor={`${rol.id}-${modulo}`} className="text-xs font-semibold text-gray-700 uppercase tracking-wide cursor-pointer">
                    {label}
                  </label>
                </div>
                <div className="grid grid-cols-2 gap-1 pl-5">
                  {items.map(({ key, label: lbl }) => (
                    <label key={key} className="flex items-center gap-2 text-xs text-gray-600 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={permisos.includes(key)}
                        onChange={() => toggle(key)}
                        className="rounded accent-indigo-600"
                      />
                      {lbl}
                    </label>
                  ))}
                </div>
              </div>
            )
          })}

          <div className="flex justify-end pt-2">
            <button
              onClick={() => mutation.mutate()}
              disabled={mutation.isPending}
              className="flex items-center gap-2 px-4 py-2 text-sm font-medium bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg transition-colors disabled:opacity-50"
            >
              <Save className="w-4 h-4" />
              {mutation.isPending ? 'Guardando…' : 'Guardar permisos'}
            </button>
          </div>
        </div>
      )}
    </div>
  )
}

export default function RolesPage() {
  const { data: roles = [], isLoading } = useQuery({
    queryKey: ['roles'],
    queryFn: rolesApi.getAll,
    staleTime: 300_000,
  })

  return (
    <div className="flex flex-col h-full">
      <div className="h-14 flex items-center px-6 border-b border-gray-200 bg-white">
        <h1 className="text-base font-semibold text-gray-900">Roles y permisos</h1>
      </div>

      <div className="flex-1 p-4 md:p-6 overflow-auto">
        <p className="text-sm text-gray-500 mb-4">
          Configurá qué puede hacer cada rol. SuperAdmin siempre tiene acceso total.
        </p>
        {isLoading ? (
          <div className="text-sm text-gray-400">Cargando…</div>
        ) : (
          <div className="space-y-3 max-w-2xl">
            {roles.map((r) => <RolCard key={r.id} rol={r} />)}
          </div>
        )}
      </div>
    </div>
  )
}
