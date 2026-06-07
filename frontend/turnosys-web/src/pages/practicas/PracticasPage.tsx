import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Pencil, Trash2, Clock, AlertCircle } from 'lucide-react'
import toast from 'react-hot-toast'
import { practicasApi, type PracticaItem } from '@/api/practicas'
import PracticaModal from './PracticaModal'

const DEFAULT_COLOR = '#6366F1'

function formatPrecio(precio: number) {
  return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 0 }).format(precio)
}

function formatDuracion(min: number) {
  if (min < 60) return `${min} min`
  const h = Math.floor(min / 60)
  const m = min % 60
  return m > 0 ? `${h}h ${m}min` : `${h}h`
}

export default function PracticasPage() {
  const qc = useQueryClient()
  const [soloActivas, setSoloActivas] = useState(true)
  const [modalOpen, setModalOpen] = useState(false)
  const [editandoId, setEditandoId] = useState<string | null>(null)

  const { data: practicas = [], isLoading } = useQuery({
    queryKey: ['practicas', soloActivas],
    queryFn: () => practicasApi.getAll(soloActivas),
    staleTime: 30_000,
  })

  const eliminarMutation = useMutation({
    mutationFn: (id: string) => practicasApi.eliminar(id),
    onSuccess: () => {
      toast.success('Práctica eliminada')
      qc.invalidateQueries({ queryKey: ['practicas'] })
    },
    onError: (e: { response?: { data?: { error?: string } } }) =>
      toast.error(e.response?.data?.error ?? 'No se pudo eliminar la práctica'),
  })

  const handleNuevo = () => { setEditandoId(null); setModalOpen(true) }
  const handleEditar = (id: string) => { setEditandoId(id); setModalOpen(true) }
  const confirmarEliminar = (p: PracticaItem) => {
    if (confirm(`¿Eliminar "${p.nombre}"?`)) eliminarMutation.mutate(p.id)
  }

  // Agrupar por categoría
  const grupos = practicas.reduce<Record<string, PracticaItem[]>>((acc, p) => {
    const key = p.categoriaNombre ?? 'Sin categoría'
    if (!acc[key]) acc[key] = []
    acc[key].push(p)
    return acc
  }, {})

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="h-14 flex items-center justify-between px-6 border-b border-gray-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 shrink-0">
        <h1 className="text-base font-semibold text-gray-900 dark:text-white">Prácticas</h1>
        <button
          onClick={handleNuevo}
          className="flex items-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
        >
          <Plus className="w-4 h-4" />
          Nueva práctica
        </button>
      </div>

      {/* Filtro */}
      <div className="px-6 py-3 bg-white dark:bg-zinc-900 border-b border-gray-100 dark:border-zinc-800 flex items-center gap-4 shrink-0">
        <label className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 cursor-pointer">
          <input
            type="checkbox"
            checked={soloActivas}
            onChange={e => setSoloActivas(e.target.checked)}
            className="rounded border-gray-300 text-indigo-600"
          />
          Solo activas
        </label>
        <span className="text-xs text-gray-400 ml-auto">
          {practicas.length} práctica{practicas.length !== 1 ? 's' : ''}
        </span>
      </div>

      {/* Contenido */}
      <div className="flex-1 overflow-auto p-6">
        {isLoading ? (
          <div className="flex justify-center pt-20 text-gray-400 text-sm">Cargando...</div>
        ) : practicas.length === 0 ? (
          <div className="flex flex-col items-center justify-center pt-20 text-gray-400">
            <div className="text-4xl mb-3">🩺</div>
            <p className="text-sm">Todavía no hay prácticas cargadas.</p>
          </div>
        ) : (
          <div className="space-y-6">
            {Object.entries(grupos).map(([categoria, items]) => (
              <div key={categoria}>
                {/* Encabezado de categoría */}
                <div className="flex items-center gap-2 mb-3">
                  {items[0].categoriaColor && (
                    <span
                      className="w-3 h-3 rounded-full shrink-0"
                      style={{ backgroundColor: items[0].categoriaColor }}
                    />
                  )}
                  <h2 className="text-sm font-semibold text-gray-600 dark:text-gray-400 uppercase tracking-wide">
                    {categoria}
                  </h2>
                  <span className="text-xs text-gray-400">({items.length})</span>
                </div>

                {/* Grid de tarjetas */}
                <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-3">
                  {items.map(p => (
                    <PracticaCard
                      key={p.id}
                      practica={p}
                      onEditar={() => handleEditar(p.id)}
                      onEliminar={() => confirmarEliminar(p)}
                    />
                  ))}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {modalOpen && (
        <PracticaModal
          isOpen={modalOpen}
          practicaId={editandoId ?? undefined}
          onClose={() => { setModalOpen(false); setEditandoId(null) }}
        />
      )}
    </div>
  )
}

function PracticaCard({ practica: p, onEditar, onEliminar }: {
  practica: PracticaItem
  onEditar: () => void
  onEliminar: () => void
}) {
  const color = p.color ?? DEFAULT_COLOR

  return (
    <div className="bg-white dark:bg-zinc-900 rounded-xl border border-gray-200 dark:border-zinc-800 overflow-hidden hover:shadow-md transition-shadow">
      {/* Banda de color superior */}
      <div className="h-1.5" style={{ backgroundColor: color }} />

      <div className="p-4 flex flex-col gap-3">
        {/* Nombre y estado */}
        <div className="flex items-start justify-between gap-2">
          <div>
            <p className="font-semibold text-gray-900 dark:text-white text-sm">{p.nombre}</p>
            {p.descripcion && (
              <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5 line-clamp-2">{p.descripcion}</p>
            )}
          </div>
          {!p.isActivo && (
            <span className="shrink-0 text-xs px-2 py-0.5 rounded-full bg-gray-100 dark:bg-zinc-800 text-gray-500">
              Inactiva
            </span>
          )}
        </div>

        {/* Datos */}
        <div className="flex items-center gap-3 text-sm">
          <span className="font-bold text-gray-900 dark:text-white">{formatPrecio(p.precio)}</span>
          <span className="text-gray-300 dark:text-zinc-600">·</span>
          <span className="flex items-center gap-1 text-gray-500 dark:text-gray-400 text-xs">
            <Clock className="w-3.5 h-3.5" />
            {formatDuracion(p.duracionMinutos)}
          </span>
        </div>

        {/* Badges */}
        <div className="flex flex-wrap gap-1.5">
          {p.requiereObservaciones && (
            <span className="inline-flex items-center gap-1 text-xs px-2 py-0.5 rounded-md bg-amber-50 dark:bg-amber-950/30 text-amber-700 dark:text-amber-400">
              <AlertCircle className="w-3 h-3" />
              Requiere observaciones
            </span>
          )}
          {p.recordatorioRecDias && (
            <span className="text-xs px-2 py-0.5 rounded-md bg-indigo-50 dark:bg-indigo-950/30 text-indigo-600 dark:text-indigo-400">
              Recordatorio c/{p.recordatorioRecDias}d
            </span>
          )}
        </div>

        {/* Acciones */}
        <div className="flex items-center justify-end gap-1 border-t border-gray-100 dark:border-zinc-800 pt-2 mt-1">
          <button
            onClick={onEditar}
            className="p-1.5 text-gray-400 hover:text-indigo-600 hover:bg-indigo-50 dark:hover:bg-indigo-950 rounded-lg transition-colors"
          >
            <Pencil className="w-3.5 h-3.5" />
          </button>
          <button
            onClick={onEliminar}
            className="p-1.5 text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-950/30 rounded-lg transition-colors"
          >
            <Trash2 className="w-3.5 h-3.5" />
          </button>
        </div>
      </div>
    </div>
  )
}
