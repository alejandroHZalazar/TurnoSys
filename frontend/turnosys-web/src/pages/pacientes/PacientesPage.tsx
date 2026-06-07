import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Search, Pencil, Trash2, Phone, Mail, Shield } from 'lucide-react'
import toast from 'react-hot-toast'
import { pacientesApi, type PacienteListItem } from '@/api/pacientes'
import PacienteModal from './PacienteModal'

export default function PacientesPage() {
  const qc = useQueryClient()
  const [busqueda, setBusqueda] = useState('')
  const [soloActivos, setSoloActivos] = useState(true)
  const [page, setPage] = useState(1)
  const [modalOpen, setModalOpen] = useState(false)
  const [editando, setEditando] = useState<PacienteListItem | null>(null)

  const { data, isLoading } = useQuery({
    queryKey: ['pacientes', busqueda, soloActivos, page],
    queryFn: () => pacientesApi.getAll({ busqueda: busqueda || undefined, soloActivos, page, pageSize: 20 }),
    staleTime: 30_000,
  })

  const eliminarMutation = useMutation({
    mutationFn: (id: string) => pacientesApi.eliminar(id),
    onSuccess: () => {
      toast.success('Paciente eliminado')
      qc.invalidateQueries({ queryKey: ['pacientes'] })
    },
    onError: (e: { response?: { data?: { error?: string } } }) =>
      toast.error(e.response?.data?.error ?? 'No se pudo eliminar el paciente'),
  })

  const handleEditar = (p: PacienteListItem) => { setEditando(p); setModalOpen(true) }
  const handleNuevo = () => { setEditando(null); setModalOpen(true) }

  const confirmarEliminar = (p: PacienteListItem) => {
    if (confirm(`¿Eliminar a ${p.nombreCompleto}?`))
      eliminarMutation.mutate(p.id)
  }

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="h-14 flex items-center justify-between px-6 border-b border-gray-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 shrink-0">
        <h1 className="text-base font-semibold text-gray-900 dark:text-white">Pacientes</h1>
        <button
          onClick={handleNuevo}
          className="flex items-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
        >
          <Plus className="w-4 h-4" />
          Nuevo paciente
        </button>
      </div>

      {/* Filtros */}
      <div className="px-6 py-3 bg-white dark:bg-zinc-900 border-b border-gray-100 dark:border-zinc-800 flex items-center gap-4 shrink-0">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            value={busqueda}
            onChange={e => { setBusqueda(e.target.value); setPage(1) }}
            placeholder="Buscar por nombre, apellido, DNI o teléfono..."
            className="w-full pl-9 pr-3 py-2 text-sm border border-gray-300 dark:border-zinc-700 rounded-lg bg-white dark:bg-zinc-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
        </div>
        <label className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 cursor-pointer">
          <input
            type="checkbox"
            checked={soloActivos}
            onChange={e => { setSoloActivos(e.target.checked); setPage(1) }}
            className="rounded border-gray-300 text-indigo-600"
          />
          Solo activos
        </label>
        {data && (
          <span className="text-xs text-gray-400 ml-auto">
            {data.pagination.totalItems} paciente{data.pagination.totalItems !== 1 ? 's' : ''}
          </span>
        )}
      </div>

      {/* Tabla */}
      <div className="flex-1 overflow-auto p-6">
        {isLoading ? (
          <div className="flex justify-center pt-20 text-gray-400 text-sm">Cargando...</div>
        ) : !data?.data.length ? (
          <div className="flex flex-col items-center justify-center pt-20 text-gray-400">
            <div className="text-4xl mb-3">🔍</div>
            <p className="text-sm">{busqueda ? 'Sin resultados para la búsqueda.' : 'Todavía no hay pacientes.'}</p>
          </div>
        ) : (
          <>
            <div className="bg-white dark:bg-zinc-900 rounded-xl border border-gray-200 dark:border-zinc-800 overflow-hidden">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-100 dark:border-zinc-800 bg-gray-50 dark:bg-zinc-800/50">
                    <th className="text-left px-4 py-3 font-medium text-gray-600 dark:text-gray-400">Paciente</th>
                    <th className="text-left px-4 py-3 font-medium text-gray-600 dark:text-gray-400">Contacto</th>
                    <th className="text-left px-4 py-3 font-medium text-gray-600 dark:text-gray-400">Obra social</th>
                    <th className="text-left px-4 py-3 font-medium text-gray-600 dark:text-gray-400">Turnos</th>
                    <th className="text-left px-4 py-3 font-medium text-gray-600 dark:text-gray-400">Estado</th>
                    <th className="px-4 py-3" />
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100 dark:divide-zinc-800">
                  {data.data.map(p => (
                    <tr key={p.id} className="hover:bg-gray-50 dark:hover:bg-zinc-800/40 transition-colors">
                      {/* Paciente */}
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-3">
                          <div className="w-9 h-9 rounded-full bg-indigo-100 dark:bg-indigo-900/40 flex items-center justify-center text-sm font-bold text-indigo-600 dark:text-indigo-400 shrink-0">
                            {p.apellido[0]}{p.nombre[0]}
                          </div>
                          <div>
                            <p className="font-medium text-gray-900 dark:text-white">{p.nombreCompleto}</p>
                            <p className="text-xs text-gray-400">
                              {p.dni ? `DNI ${p.dni}` : 'Sin DNI'}
                              {p.edad != null && ` · ${p.edad} años`}
                            </p>
                          </div>
                        </div>
                      </td>
                      {/* Contacto */}
                      <td className="px-4 py-3">
                        <div className="space-y-0.5">
                          {p.telefono && (
                            <div className="flex items-center gap-1.5 text-xs text-gray-600 dark:text-gray-400">
                              <Phone className="w-3 h-3" /> {p.telefono}
                            </div>
                          )}
                          {p.email && (
                            <div className="flex items-center gap-1.5 text-xs text-gray-500 dark:text-gray-500">
                              <Mail className="w-3 h-3" /> {p.email}
                            </div>
                          )}
                          {!p.telefono && !p.email && <span className="text-xs text-gray-300">—</span>}
                        </div>
                      </td>
                      {/* Obra social */}
                      <td className="px-4 py-3">
                        {p.obraSocial ? (
                          <div className="flex items-center gap-1.5 text-xs text-gray-600 dark:text-gray-400">
                            <Shield className="w-3 h-3" /> {p.obraSocial}
                          </div>
                        ) : <span className="text-xs text-gray-300">—</span>}
                      </td>
                      {/* Turnos */}
                      <td className="px-4 py-3">
                        <span className="text-sm text-gray-700 dark:text-gray-300 font-medium">{p.totalTurnos}</span>
                      </td>
                      {/* Estado */}
                      <td className="px-4 py-3">
                        <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${
                          p.isActivo
                            ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400'
                            : 'bg-gray-100 text-gray-500 dark:bg-zinc-800 dark:text-gray-500'
                        }`}>
                          {p.isActivo ? 'Activo' : 'Inactivo'}
                        </span>
                      </td>
                      {/* Acciones */}
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-1 justify-end">
                          <button
                            onClick={() => handleEditar(p)}
                            className="p-1.5 text-gray-400 hover:text-indigo-600 hover:bg-indigo-50 dark:hover:bg-indigo-950 rounded-lg transition-colors"
                            title="Editar"
                          >
                            <Pencil className="w-3.5 h-3.5" />
                          </button>
                          <button
                            onClick={() => confirmarEliminar(p)}
                            className="p-1.5 text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-950/30 rounded-lg transition-colors"
                            title="Eliminar"
                          >
                            <Trash2 className="w-3.5 h-3.5" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Paginación */}
            {data.pagination.totalPages > 1 && (
              <div className="flex items-center justify-between mt-4">
                <p className="text-sm text-gray-500">
                  Página {data.pagination.page} de {data.pagination.totalPages}
                </p>
                <div className="flex gap-2">
                  <button
                    disabled={page === 1}
                    onClick={() => setPage(p => p - 1)}
                    className="px-3 py-1.5 text-sm border border-gray-300 dark:border-zinc-700 rounded-lg disabled:opacity-40 hover:bg-gray-50 dark:hover:bg-zinc-800 transition-colors"
                  >
                    Anterior
                  </button>
                  <button
                    disabled={page >= data.pagination.totalPages}
                    onClick={() => setPage(p => p + 1)}
                    className="px-3 py-1.5 text-sm border border-gray-300 dark:border-zinc-700 rounded-lg disabled:opacity-40 hover:bg-gray-50 dark:hover:bg-zinc-800 transition-colors"
                  >
                    Siguiente
                  </button>
                </div>
              </div>
            )}
          </>
        )}
      </div>

      {modalOpen && (
        <PacienteModal
          isOpen={modalOpen}
          pacienteId={editando?.id}
          onClose={() => { setModalOpen(false); setEditando(null) }}
        />
      )}
    </div>
  )
}
