import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import toast from 'react-hot-toast'
import { X } from 'lucide-react'
import { practicasApi, type PracticaForm } from '@/api/practicas'
import apiClient from '@/api/client'

const COLORES_PRESET = [
  '#4F46E5', '#3B82F6', '#06B6D4', '#10B981',
  '#F59E0B', '#EF4444', '#EC4899', '#8B5CF6',
  '#84CC16', '#F97316',
]

const DURACIONES = [10, 15, 20, 30, 45, 60, 90, 120]

const schema = z.object({
  nombre:               z.string().min(1, 'Requerido').max(200),
  descripcion:          z.string().optional(),
  precio:               z.string().min(1, 'Requerido'),
  duracionMinutos:      z.string().min(1, 'Requerido'),
  color:                z.string().optional(),
  requiereObservaciones:z.boolean().optional(),
  recordatorioRecDias:  z.string().optional(),
  isActivo:             z.boolean().optional(),
})

type FormData = z.infer<typeof schema>

interface Props {
  isOpen: boolean
  practicaId?: string
  onClose: () => void
}

export default function PracticaModal({ isOpen, practicaId, onClose }: Props) {
  const qc = useQueryClient()
  const isEdit = !!practicaId
  const [color, setColor] = useState('#4F46E5')
  const [categoriaId, setCategoriaId] = useState<string>('')

  const { data: categorias = [] } = useQuery({
    queryKey: ['practicas-categorias'],
    queryFn: async () => {
      const { data } = await apiClient.get<{ data: { id: string; nombre: string; color?: string }[] }>('/practicas/categorias')
      return data.data
    },
  })

  const { data: practica } = useQuery({
    queryKey: ['practica', practicaId],
    queryFn: () => practicasApi.getById(practicaId!),
    enabled: isEdit,
  })

  const { register, handleSubmit, reset, setValue, watch, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { precio: '0', duracionMinutos: '30', requiereObservaciones: false, isActivo: true },
  })

  const colorActual = watch('color')

  useEffect(() => {
    if (practica) {
      reset({
        nombre:               practica.nombre,
        descripcion:          practica.descripcion ?? '',
        precio:               String(practica.precio),
        duracionMinutos:      String(practica.duracionMinutos),
        color:                practica.color ?? '#4F46E5',
        requiereObservaciones:practica.requiereObservaciones,
        recordatorioRecDias:  practica.recordatorioRecDias ? String(practica.recordatorioRecDias) : '',
        isActivo:             practica.isActivo,
      })
      setColor(practica.color ?? '#4F46E5')
      setCategoriaId(practica.categoriaId ?? '')
    }
  }, [practica, reset])

  const seleccionarColor = (c: string) => {
    setColor(c)
    setValue('color', c)
  }

  const mutation = useMutation({
    mutationFn: async (data: FormData) => {
      const payload: PracticaForm = {
        nombre:               data.nombre,
        descripcion:          data.descripcion || undefined,
        precio:               Number(data.precio),
        duracionMinutos:      Number(data.duracionMinutos),
        color:                data.color || color,
        requiereObservaciones:data.requiereObservaciones ?? false,
        recordatorioRecDias:  data.recordatorioRecDias ? Number(data.recordatorioRecDias) : undefined,
        categoriaId:          categoriaId || undefined,
        isActivo:             data.isActivo ?? true,
      }
      if (isEdit) await practicasApi.actualizar(practicaId!, payload)
      else await practicasApi.crear(payload)
    },
    onSuccess: () => {
      toast.success(isEdit ? 'Práctica actualizada' : 'Práctica creada')
      qc.invalidateQueries({ queryKey: ['practicas'] })
      if (isEdit) qc.invalidateQueries({ queryKey: ['practica', practicaId] })
      onClose()
    },
    onError: (e: { response?: { data?: { error?: string } } }) =>
      toast.error(e.response?.data?.error ?? 'Error al guardar la práctica'),
  })

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/50" onClick={onClose} />
      <div className="relative bg-white dark:bg-zinc-900 rounded-2xl shadow-2xl w-full max-w-lg mx-4 max-h-[90vh] flex flex-col">

        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 dark:border-zinc-800 shrink-0">
          <div className="flex items-center gap-3">
            {/* Preview del color */}
            <span className="w-4 h-4 rounded-full shrink-0" style={{ backgroundColor: colorActual ?? color }} />
            <h2 className="text-base font-semibold text-gray-900 dark:text-white">
              {isEdit ? 'Editar práctica' : 'Nueva práctica'}
            </h2>
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 dark:hover:text-white">
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit(d => mutation.mutate(d))} className="overflow-y-auto">
          <div className="px-6 py-5 space-y-5">

            {/* Nombre y descripción */}
            <Section title="Información">
              <Field label="Nombre *" error={errors.nombre?.message}>
                <input {...register('nombre')} className={inp} placeholder="Consulta general" autoFocus />
              </Field>
              <Field label="Descripción" error={errors.descripcion?.message} className="mt-3">
                <textarea {...register('descripcion')} rows={2} className={`${inp} resize-none`}
                  placeholder="Descripción breve de la práctica..." />
              </Field>
            </Section>

            {/* Precio y duración */}
            <Section title="Configuración">
              {/* Categoría */}
              {categorias.length > 0 && (
                <Field label="Categoría" className="mb-4">
                  <select
                    value={categoriaId}
                    onChange={e => setCategoriaId(e.target.value)}
                    className={inp}
                  >
                    <option value="">Sin categoría</option>
                    {categorias.map(c => (
                      <option key={c.id} value={c.id}>{c.nombre}</option>
                    ))}
                  </select>
                </Field>
              )}
              <div className="grid grid-cols-2 gap-4">
                <Field label="Precio ($)" error={errors.precio?.message}>
                  <input {...register('precio')} type="number" min="0" step="0.01" className={inp} placeholder="0" />
                </Field>
                <Field label="Duración" error={errors.duracionMinutos?.message}>
                  <select {...register('duracionMinutos')} className={inp}>
                    {DURACIONES.map(d => (
                      <option key={d} value={d}>
                        {d < 60 ? `${d} min` : d === 60 ? '1 hora' : d === 90 ? '1h 30min' : `${d / 60}h`}
                      </option>
                    ))}
                    <option value="custom">Personalizada</option>
                  </select>
                </Field>
                <Field label="Recordatorio control (días)" error={errors.recordatorioRecDias?.message}
                       className="col-span-2">
                  <input {...register('recordatorioRecDias')} type="text" inputMode="numeric"
                    pattern="[0-9]*" className={inp}
                    placeholder="Ej: 30 (dejar vacío si no aplica)" />
                </Field>
              </div>

              {/* Checkbox observaciones */}
              <label className="flex items-center gap-3 mt-3 cursor-pointer">
                <input {...register('requiereObservaciones')} type="checkbox"
                  className="rounded border-gray-300 text-indigo-600 w-4 h-4" />
                <div>
                  <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Requiere observaciones</span>
                  <p className="text-xs text-gray-400">El turno pedirá observaciones obligatorias al reservar</p>
                </div>
              </label>
            </Section>

            {/* Color */}
            <Section title="Color en agenda">
              <div className="flex flex-wrap gap-2">
                {COLORES_PRESET.map(c => (
                  <button
                    key={c}
                    type="button"
                    onClick={() => seleccionarColor(c)}
                    className="w-7 h-7 rounded-full border-2 transition-transform hover:scale-110"
                    style={{
                      backgroundColor: c,
                      borderColor: (colorActual ?? color) === c ? 'white' : 'transparent',
                      boxShadow: (colorActual ?? color) === c ? `0 0 0 2px ${c}` : 'none',
                    }}
                  />
                ))}
                <input
                  type="color"
                  value={colorActual ?? color}
                  onChange={e => seleccionarColor(e.target.value)}
                  className="w-7 h-7 rounded-full cursor-pointer border-0 p-0"
                  title="Color personalizado"
                />
              </div>
            </Section>

            {/* Estado (solo edición) */}
            {isEdit && (
              <label className="flex items-center gap-3 cursor-pointer">
                <input {...register('isActivo')} type="checkbox" className="rounded border-gray-300 text-indigo-600 w-4 h-4" />
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Práctica activa</span>
              </label>
            )}
          </div>

          {/* Footer */}
          <div className="flex gap-3 px-6 py-4 border-t border-gray-100 dark:border-zinc-800 bg-gray-50 dark:bg-zinc-800/50 rounded-b-2xl shrink-0">
            <button type="button" onClick={onClose}
              className="flex-1 border border-gray-300 dark:border-zinc-700 text-gray-700 dark:text-gray-300 text-sm font-medium py-2.5 rounded-lg hover:bg-gray-100 dark:hover:bg-zinc-700 transition-colors">
              Cancelar
            </button>
            <button type="submit" disabled={mutation.isPending}
              className="flex-1 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium py-2.5 rounded-lg transition-colors disabled:opacity-50">
              {mutation.isPending ? 'Guardando...' : isEdit ? 'Guardar cambios' : 'Crear práctica'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

const inp = 'w-full px-3 py-2 border border-gray-300 dark:border-zinc-700 rounded-lg text-sm bg-white dark:bg-zinc-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500'

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div>
      <h3 className="text-xs font-semibold text-gray-400 dark:text-gray-500 uppercase tracking-wider mb-3">{title}</h3>
      {children}
    </div>
  )
}

function Field({ label, error, children, className }: {
  label: string; error?: string; children: React.ReactNode; className?: string
}) {
  return (
    <div className={className}>
      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">{label}</label>
      {children}
      {error && <p className="text-xs text-red-500 mt-1">{error}</p>}
    </div>
  )
}
