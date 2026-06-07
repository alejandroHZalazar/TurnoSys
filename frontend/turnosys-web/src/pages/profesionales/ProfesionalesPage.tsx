import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Pencil, Trash2, Phone, Mail, Clock, BadgeCheck } from 'lucide-react'
import toast from 'react-hot-toast'
import { profesionalesApi, type ProfesionalListItem } from '@/api/profesionales'
import ProfesionalModal from './ProfesionalModal'

const DIAS = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb']

export default function ProfesionalesPage() {
  const qc = useQueryClient()
  const [soloActivos, setSoloActivos] = useState(true)
  const [modalOpen, setModalOpen] = useState(false)
  const [editandoId, setEditandoId] = useState<string | null>(null)

  const { data: profesionales = [], isLoading } = useQuery({
    queryKey: ['profesionales', soloActivos],
    queryFn: () => profesionalesApi.getAll(soloActivos),
    staleTime: 30_000,
  })

  const eliminarMutation = useMutation({
    mutationFn: (id: string) => profesionalesApi.eliminar(id),
    onSuccess: () => {
      toast.success('Profesional eliminado')
      qc.invalidateQueries({ queryKey: ['profesionales'] })
    },
    onError: (e: { response?: { data?: { error?: string } } }) =>
      toast.error(e.response?.data?.error ?? 'No se pudo eliminar el profesional'),
  })

  const handleEditar = (id: string) => { setEditandoId(id); setModalOpen(true) }
  const handleNuevo  = () => { setEditandoId(null); setModalOpen(true) }

  const confirmarEliminar = (p: ProfesionalListItem) => {
    if (confirm(`¿Eliminar a ${p.nombreCompleto}?`)) eliminarMutation.mutate(p.id)
  }

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="h-14 flex items-center justify-between px-6 border-b border-gray-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 shrink-0">
        <h1 className="text-base font-semibold text-gray-900 dark:text-white">Profesionales</h1>
        <button
          onClick={handleNuevo}
          className="flex items-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
        >
          <Plus className="w-4 h-4" />
          Nuevo profesional
        </button>
      </div>

      {/* Filtro */}
      <div className="px-6 py-3 bg-white dark:bg-zinc-900 border-b border-gray-100 dark:border-zinc-800 flex items-center gap-4 shrink-0">
        <label className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 cursor-pointer">
          <input
            type="checkbox"
            checked={soloActivos}
            onChange={e => setSoloActivos(e.target.checked)}
            className="rounded border-gray-300 text-indigo-600"
          />
          Solo activos
        </label>
        <span className="text-xs text-gray-400 ml-auto">
          {profesionales.length} profesional{profesionales.length !== 1 ? 'es' : ''}
        </span>
      </div>

      {/* Contenido */}
      <div className="flex-1 overflow-auto p-6">
        {isLoading ? (
          <div className="flex justify-center pt-20 text-gray-400 text-sm">Cargando...</div>
        ) : profesionales.length === 0 ? (
          <div className="flex flex-col items-center justify-center pt-20 text-gray-400">
            <div className="text-4xl mb-3">👨‍⚕️</div>
            <p className="text-sm">Todavía no hay profesionales cargados.</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
            {profesionales.map(p => (
              <ProfesionalCard
                key={p.id}
                profesional={p}
                onEditar={() => handleEditar(p.id)}
                onEliminar={() => confirmarEliminar(p)}
              />
            ))}
          </div>
        )}
      </div>

      {modalOpen && (
        <ProfesionalModal
          isOpen={modalOpen}
          profesionalId={editandoId ?? undefined}
          onClose={() => { setModalOpen(false); setEditandoId(null) }}
        />
      )}
    </div>
  )
}

function ProfesionalCard({ profesional: p, onEditar, onEliminar }: {
  profesional: ProfesionalListItem
  onEditar: () => void
  onEliminar: () => void
}) {
  return (
    <div className="bg-white dark:bg-zinc-900 rounded-xl border border-gray-200 dark:border-zinc-800 p-5 flex flex-col gap-4 hover:shadow-md transition-shadow">
      {/* Cabecera */}
      <div className="flex items-start justify-between gap-3">
        <div className="flex items-center gap-3">
          {/* Avatar con color de agenda */}
          <div
            className="w-11 h-11 rounded-full flex items-center justify-center text-white text-sm font-bold shrink-0"
            style={{ backgroundColor: p.colorAgenda }}
          >
            {p.apellido[0]}{p.nombre[0]}
          </div>
          <div>
            <p className="font-semibold text-gray-900 dark:text-white text-sm">{p.nombreCompleto}</p>
            {p.especialidad && (
              <p className="text-xs text-indigo-600 dark:text-indigo-400 font-medium">{p.especialidad}</p>
            )}
          </div>
        </div>
        <span className={`shrink-0 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${
          p.isActivo
            ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400'
            : 'bg-gray-100 text-gray-500 dark:bg-zinc-800 dark:text-gray-500'
        }`}>
          {p.isActivo ? 'Activo' : 'Inactivo'}
        </span>
      </div>

      {/* Info */}
      <div className="space-y-1.5">
        {p.matricula && (
          <div className="flex items-center gap-2 text-xs text-gray-500 dark:text-gray-400">
            <BadgeCheck className="w-3.5 h-3.5 shrink-0" />
            Matrícula: {p.matricula}
          </div>
        )}
        {p.telefono && (
          <div className="flex items-center gap-2 text-xs text-gray-500 dark:text-gray-400">
            <Phone className="w-3.5 h-3.5 shrink-0" /> {p.telefono}
          </div>
        )}
        {p.email && (
          <div className="flex items-center gap-2 text-xs text-gray-500 dark:text-gray-400">
            <Mail className="w-3.5 h-3.5 shrink-0" /> {p.email}
          </div>
        )}
      </div>

      {/* Horarios */}
      {p.horarios.length > 0 && (
        <div className="border-t border-gray-100 dark:border-zinc-800 pt-3">
          <div className="flex items-center gap-1.5 mb-2">
            <Clock className="w-3.5 h-3.5 text-gray-400" />
            <span className="text-xs font-medium text-gray-500 dark:text-gray-400">Horarios</span>
          </div>
          <div className="flex flex-wrap gap-1">
            {p.horarios.map(h => (
              <span
                key={h.diaSemana}
                className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-indigo-50 dark:bg-indigo-950/40 text-indigo-700 dark:text-indigo-300 text-xs"
              >
                <span className="font-medium">{DIAS[h.diaSemana]}</span>
                <span className="text-indigo-400">{h.horaInicio}–{h.horaFin}</span>
              </span>
            ))}
          </div>
        </div>
      )}

      {/* Footer */}
      <div className="flex items-center justify-between border-t border-gray-100 dark:border-zinc-800 pt-3 mt-auto">
        <span className="text-xs text-gray-400">{p.totalTurnos} turno{p.totalTurnos !== 1 ? 's' : ''} totales</span>
        <div className="flex gap-1">
          <button
            onClick={onEditar}
            className="p-1.5 text-gray-400 hover:text-indigo-600 hover:bg-indigo-50 dark:hover:bg-indigo-950 rounded-lg transition-colors"
            title="Editar"
          >
            <Pencil className="w-3.5 h-3.5" />
          </button>
          <button
            onClick={onEliminar}
            className="p-1.5 text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-950/30 rounded-lg transition-colors"
            title="Eliminar"
          >
            <Trash2 className="w-3.5 h-3.5" />
          </button>
        </div>
      </div>
    </div>
  )
}
