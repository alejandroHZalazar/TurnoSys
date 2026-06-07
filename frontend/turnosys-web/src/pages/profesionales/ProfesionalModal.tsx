import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import toast from 'react-hot-toast'
import { X, Plus, Trash2 } from 'lucide-react'
import { profesionalesApi, type ProfesionalForm, type HorarioForm } from '@/api/profesionales'

const DIAS_SEMANA = [
  { value: 1, label: 'Lunes' },
  { value: 2, label: 'Martes' },
  { value: 3, label: 'Miércoles' },
  { value: 4, label: 'Jueves' },
  { value: 5, label: 'Viernes' },
  { value: 6, label: 'Sábado' },
  { value: 0, label: 'Domingo' },
]

const COLORES_PRESET = [
  '#4F46E5', '#3B82F6', '#06B6D4', '#10B981',
  '#F59E0B', '#EF4444', '#EC4899', '#8B5CF6',
]

const schema = z.object({
  nombre:       z.string().min(1, 'Requerido').max(100),
  apellido:     z.string().min(1, 'Requerido').max(100),
  email:        z.string().email('Email inválido').optional().or(z.literal('')),
  telefono:     z.string().max(50).optional().or(z.literal('')),
  especialidad: z.string().max(200).optional().or(z.literal('')),
  matricula:    z.string().max(100).optional().or(z.literal('')),
  colorAgenda:  z.string().regex(/^#[0-9A-Fa-f]{6}$/, 'Color HEX inválido'),
  observaciones:z.string().optional().or(z.literal('')),
  isActivo:     z.boolean().optional(),
})

type FormData = z.infer<typeof schema>

interface Props {
  isOpen: boolean
  profesionalId?: string
  onClose: () => void
}

export default function ProfesionalModal({ isOpen, profesionalId, onClose }: Props) {
  const qc = useQueryClient()
  const isEdit = !!profesionalId
  const [horarios, setHorarios] = useState<HorarioForm[]>([])
  const [colorSeleccionado, setColorSeleccionado] = useState('#4F46E5')

  const { data: profesional } = useQuery({
    queryKey: ['profesional', profesionalId],
    queryFn: () => profesionalesApi.getById(profesionalId!),
    enabled: isEdit,
  })

  const { register, handleSubmit, reset, setValue, watch, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { colorAgenda: '#4F46E5', isActivo: true },
  })

  const colorActual = watch('colorAgenda')

  useEffect(() => {
    if (profesional) {
      reset({
        nombre:       profesional.nombre,
        apellido:     profesional.apellido,
        email:        profesional.email ?? '',
        telefono:     profesional.telefono ?? '',
        especialidad: profesional.especialidad ?? '',
        matricula:    profesional.matricula ?? '',
        colorAgenda:  profesional.colorAgenda,
        observaciones:profesional.observaciones ?? '',
        isActivo:     profesional.isActivo,
      })
      setColorSeleccionado(profesional.colorAgenda)
      setHorarios(
        profesional.horarios.map(h => ({
          diaSemana: h.diaSemana,
          horaInicio: h.horaInicio,
          horaFin: h.horaFin,
        }))
      )
    }
  }, [profesional, reset])

  const agregarHorario = () => {
    const diaLibre = DIAS_SEMANA.find(d => !horarios.some(h => h.diaSemana === d.value))
    setHorarios(prev => [...prev, {
      diaSemana: diaLibre?.value ?? 1,
      horaInicio: '08:00',
      horaFin: '18:00',
    }])
  }

  const quitarHorario = (idx: number) =>
    setHorarios(prev => prev.filter((_, i) => i !== idx))

  const actualizarHorario = (idx: number, field: keyof HorarioForm, value: string | number) =>
    setHorarios(prev => prev.map((h, i) => i === idx ? { ...h, [field]: value } : h))

  const mutation = useMutation({
    mutationFn: async (data: FormData) => {
      const payload: ProfesionalForm = {
        ...data,
        email:        data.email || undefined,
        telefono:     data.telefono || undefined,
        especialidad: data.especialidad || undefined,
        matricula:    data.matricula || undefined,
        observaciones:data.observaciones || undefined,
        isActivo:     data.isActivo ?? true,
        horarios,
      }
      if (isEdit) await profesionalesApi.actualizar(profesionalId!, payload)
      else await profesionalesApi.crear(payload)
    },
    onSuccess: () => {
      toast.success(isEdit ? 'Profesional actualizado' : 'Profesional creado')
      qc.invalidateQueries({ queryKey: ['profesionales'] })
      if (isEdit) qc.invalidateQueries({ queryKey: ['profesional', profesionalId] })
      onClose()
    },
    onError: (e: { response?: { data?: { error?: string } } }) =>
      toast.error(e.response?.data?.error ?? 'Error al guardar el profesional'),
  })

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/50" onClick={onClose} />
      <div className="relative bg-white dark:bg-zinc-900 rounded-2xl shadow-2xl w-full max-w-2xl mx-4 max-h-[90vh] flex flex-col">

        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 dark:border-zinc-800 shrink-0">
          <h2 className="text-base font-semibold text-gray-900 dark:text-white">
            {isEdit ? 'Editar profesional' : 'Nuevo profesional'}
          </h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 dark:hover:text-white">
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit(d => mutation.mutate(d))} className="overflow-y-auto">
          <div className="px-6 py-5 space-y-5">

            {/* Datos personales */}
            <Section title="Datos del profesional">
              <div className="grid grid-cols-2 gap-4">
                <Field label="Apellido *" error={errors.apellido?.message}>
                  <input {...register('apellido')} className={inp} placeholder="García" />
                </Field>
                <Field label="Nombre *" error={errors.nombre?.message}>
                  <input {...register('nombre')} className={inp} placeholder="Martín" />
                </Field>
                <Field label="Especialidad" error={errors.especialidad?.message}>
                  <input {...register('especialidad')} className={inp} placeholder="Médico Clínico" />
                </Field>
                <Field label="Matrícula" error={errors.matricula?.message}>
                  <input {...register('matricula')} className={inp} placeholder="MN 12345" />
                </Field>
                <Field label="Teléfono" error={errors.telefono?.message}>
                  <input {...register('telefono')} className={inp} placeholder="11-1234-5678" />
                </Field>
                <Field label="Email" error={errors.email?.message}>
                  <input {...register('email')} type="email" className={inp} placeholder="prof@email.com" />
                </Field>
                <Field label="Observaciones" error={errors.observaciones?.message} className="col-span-2">
                  <textarea {...register('observaciones')} rows={2} className={`${inp} resize-none`} />
                </Field>
              </div>
            </Section>

            {/* Color de agenda */}
            <Section title="Color en agenda">
              <div className="flex items-center gap-3 flex-wrap">
                {COLORES_PRESET.map(c => (
                  <button
                    key={c}
                    type="button"
                    onClick={() => { setColorSeleccionado(c); setValue('colorAgenda', c) }}
                    className="w-8 h-8 rounded-full border-2 transition-transform hover:scale-110"
                    style={{
                      backgroundColor: c,
                      borderColor: colorActual === c ? 'white' : 'transparent',
                      boxShadow: colorActual === c ? `0 0 0 2px ${c}` : 'none',
                    }}
                  />
                ))}
                {/* Color personalizado */}
                <div className="flex items-center gap-2 ml-2">
                  <input
                    type="color"
                    value={colorSeleccionado}
                    onChange={e => { setColorSeleccionado(e.target.value); setValue('colorAgenda', e.target.value) }}
                    className="w-8 h-8 rounded-full cursor-pointer border-0 p-0"
                    title="Color personalizado"
                  />
                  <span className="text-xs text-gray-400 font-mono">{colorActual}</span>
                </div>
              </div>
              {errors.colorAgenda && <p className="text-xs text-red-500 mt-1">{errors.colorAgenda.message}</p>}
            </Section>

            {/* Horarios laborales */}
            <Section title="Horarios laborales">
              <div className="space-y-2">
                {horarios.map((h, idx) => (
                  <div key={idx} className="flex items-center gap-2">
                    <select
                      value={h.diaSemana}
                      onChange={e => actualizarHorario(idx, 'diaSemana', parseInt(e.target.value))}
                      className={`${inp} w-36`}
                    >
                      {DIAS_SEMANA.map(d => (
                        <option key={d.value} value={d.value}>{d.label}</option>
                      ))}
                    </select>
                    <input
                      type="time"
                      value={h.horaInicio}
                      onChange={e => actualizarHorario(idx, 'horaInicio', e.target.value)}
                      className={`${inp} w-32`}
                    />
                    <span className="text-gray-400 text-sm">a</span>
                    <input
                      type="time"
                      value={h.horaFin}
                      onChange={e => actualizarHorario(idx, 'horaFin', e.target.value)}
                      className={`${inp} w-32`}
                    />
                    <button
                      type="button"
                      onClick={() => quitarHorario(idx)}
                      className="p-1.5 text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-950/30 rounded-lg transition-colors"
                    >
                      <Trash2 className="w-3.5 h-3.5" />
                    </button>
                  </div>
                ))}
                <button
                  type="button"
                  onClick={agregarHorario}
                  className="flex items-center gap-2 text-sm text-indigo-600 dark:text-indigo-400 hover:text-indigo-700 mt-1"
                >
                  <Plus className="w-4 h-4" />
                  Agregar horario
                </button>
              </div>
            </Section>

            {/* Estado (solo edición) */}
            {isEdit && (
              <label className="flex items-center gap-3 cursor-pointer">
                <input {...register('isActivo')} type="checkbox" className="rounded border-gray-300 text-indigo-600 w-4 h-4" />
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Profesional activo</span>
              </label>
            )}
          </div>

          {/* Footer */}
          <div className="flex gap-3 px-6 py-4 border-t border-gray-100 dark:border-zinc-800 bg-gray-50 dark:bg-zinc-800/50 rounded-b-2xl shrink-0">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 border border-gray-300 dark:border-zinc-700 text-gray-700 dark:text-gray-300 text-sm font-medium py-2.5 rounded-lg hover:bg-gray-100 dark:hover:bg-zinc-700 transition-colors"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={mutation.isPending}
              className="flex-1 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium py-2.5 rounded-lg transition-colors disabled:opacity-50"
            >
              {mutation.isPending ? 'Guardando...' : isEdit ? 'Guardar cambios' : 'Crear profesional'}
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
