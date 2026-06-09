import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import toast from 'react-hot-toast'
import { Plus, Search, UserCheck, UserX, KeyRound, Pencil } from 'lucide-react'
import { usuariosApi } from '@/api/usuarios'
import { rolesApi } from '@/api/roles'
import type { Usuario } from '@/types'
import UsuarioModal from './UsuarioModal'
import ResetPasswordModal from './ResetPasswordModal'

export default function UsuariosPage() {
  const qc = useQueryClient()
  const [busqueda, setBusqueda] = useState('')
  const [modalOpen, setModalOpen] = useState(false)
  const [resetModal, setResetModal] = useState<Usuario | null>(null)
  const [editando, setEditando] = useState<Usuario | null>(null)

  const { data, isLoading } = useQuery({
    queryKey: ['usuarios', busqueda],
    queryFn: () => usuariosApi.getAll({ busqueda: busqueda || undefined }),
    staleTime: 30_000,
  })

  const { data: roles = [] } = useQuery({
    queryKey: ['roles'],
    queryFn: rolesApi.getAll,
    staleTime: 300_000,
  })

  const desactivar = useMutation({
    mutationFn: (id: string) => usuariosApi.desactivar(id),
    onSuccess: () => {
      toast.success('Usuario desactivado')
      qc.invalidateQueries({ queryKey: ['usuarios'] })
    },
    onError: (e: unknown) => toast.error((e as { response?: { data?: { error?: string } } })?.response?.data?.error ?? 'Error al desactivar'),
  })

  const activar = useMutation({
    mutationFn: (u: Usuario) =>
      usuariosApi.editar(u.id, {
        nombreCompleto: u.nombreCompleto,
        email: u.email,
        rolId: u.rolId,
        isActivo: true,
        empresaId: u.empresaId,
        profesionalId: u.profesionalId,
      }),
    onSuccess: () => {
      toast.success('Usuario activado')
      qc.invalidateQueries({ queryKey: ['usuarios'] })
    },
  })

  const usuarios = data?.items ?? []

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="h-14 flex items-center justify-between px-6 border-b border-gray-200 bg-white">
        <h1 className="text-base font-semibold text-gray-900">Usuarios</h1>
        <button
          onClick={() => { setEditando(null); setModalOpen(true) }}
          className="flex items-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
        >
          <Plus className="w-4 h-4" />
          Nuevo usuario
        </button>
      </div>

      <div className="flex-1 p-4 md:p-6 overflow-auto">
        {/* Búsqueda */}
        <div className="relative max-w-sm mb-4">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            value={busqueda}
            onChange={(e) => setBusqueda(e.target.value)}
            placeholder="Buscar por nombre o email…"
            className="pl-9 pr-4 py-2 text-sm border border-gray-200 rounded-lg w-full focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
        </div>

        {/* Tabla */}
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          {isLoading ? (
            <div className="p-8 text-center text-sm text-gray-400">Cargando…</div>
          ) : usuarios.length === 0 ? (
            <div className="p-8 text-center text-sm text-gray-400">No hay usuarios.</div>
          ) : (
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-100 bg-gray-50 text-gray-500 text-xs uppercase">
                  <th className="px-4 py-3 text-left">Nombre</th>
                  <th className="px-4 py-3 text-left hidden md:table-cell">Email</th>
                  <th className="px-4 py-3 text-left">Rol</th>
                  <th className="px-4 py-3 text-left hidden sm:table-cell">Último acceso</th>
                  <th className="px-4 py-3 text-center">Estado</th>
                  <th className="px-4 py-3 text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {usuarios.map((u) => (
                  <tr key={u.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-4 py-3 font-medium text-gray-900">{u.nombreCompleto}</td>
                    <td className="px-4 py-3 text-gray-500 hidden md:table-cell">{u.email}</td>
                    <td className="px-4 py-3">
                      <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-indigo-50 text-indigo-700">
                        {u.rolNombre}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-gray-400 hidden sm:table-cell text-xs">
                      {u.ultimoAcceso ? new Date(u.ultimoAcceso).toLocaleDateString('es-AR') : '—'}
                    </td>
                    <td className="px-4 py-3 text-center">
                      {u.isActivo ? (
                        <span className="inline-flex items-center gap-1 text-green-700 text-xs font-medium">
                          <UserCheck className="w-3.5 h-3.5" /> Activo
                        </span>
                      ) : (
                        <span className="inline-flex items-center gap-1 text-red-500 text-xs font-medium">
                          <UserX className="w-3.5 h-3.5" /> Inactivo
                        </span>
                      )}
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex items-center justify-end gap-1">
                        <button
                          onClick={() => { setEditando(u); setModalOpen(true) }}
                          className="p-1.5 rounded hover:bg-gray-100 text-gray-500 hover:text-indigo-600"
                          title="Editar"
                        >
                          <Pencil className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() => setResetModal(u)}
                          className="p-1.5 rounded hover:bg-gray-100 text-gray-500 hover:text-indigo-600"
                          title="Reset contraseña"
                        >
                          <KeyRound className="w-4 h-4" />
                        </button>
                        {u.isActivo ? (
                          <button
                            onClick={() => desactivar.mutate(u.id)}
                            className="p-1.5 rounded hover:bg-red-50 text-gray-500 hover:text-red-600"
                            title="Desactivar"
                          >
                            <UserX className="w-4 h-4" />
                          </button>
                        ) : (
                          <button
                            onClick={() => activar.mutate(u)}
                            className="p-1.5 rounded hover:bg-green-50 text-gray-500 hover:text-green-600"
                            title="Activar"
                          >
                            <UserCheck className="w-4 h-4" />
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>

      {modalOpen && (
        <UsuarioModal
          isOpen={modalOpen}
          onClose={() => { setModalOpen(false); setEditando(null) }}
          usuario={editando}
          roles={roles}
        />
      )}

      {resetModal && (
        <ResetPasswordModal
          usuario={resetModal}
          onClose={() => setResetModal(null)}
        />
      )}
    </div>
  )
}
