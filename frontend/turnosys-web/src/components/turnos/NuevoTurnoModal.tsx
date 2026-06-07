import { useState, useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import toast from 'react-hot-toast'
import { X, Plus, Search } from 'lucide-react'
import { turnosApi } from '@/api/turnos'
import { pacientesApi } from '@/api/pacientes'
import { practicasApi, type PracticaItem } from '@/api/practicas'
import apiClient from '@/api/client'
import type { Profesional } from '@/types'

const schema = z.object({
  profesionalId: z.string().min(1, 'Requerido'),
  pacienteId: z.string().min(1, 'Requerido'),
  practicaId: z.string().min(1, 'Requerido'),
  fechaHoraInicio: z.string().min(1, 'Requerido'),
  observaciones: z.string().optional(),
  proximoControlFecha: z.string().optional(),
})

type FormData = z.infer<typeof schema>

interface Props {
  isOpen: boolean
  onClose: () => void
  defaultDate?: string | null
}

export default function NuevoTurnoModal({ isOpen, onClose, defaultDate }: Props) {
  const qc = useQueryClient()
  const [nuevoPacienteOpen, setNuevoPacienteOpen] = useState(false)
  const [pacienteSel, setPacienteSel] = useState<{ id: string; label: string } | null>(null)

  const { data: profesionales = [] } = useQuery<Profesional[]>({
    queryKey: ['profesionales'],
    queryFn: async () => {
      const { data } = await apiClient.get('/profesionales')
      return data.data
    },
  })

  const { data: practicas = [] } = useQuery<PracticaItem[]>({
    queryKey: ['practicas'],
    queryFn: () => practicasApi.getAll(),
  })

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      // Click en bloque → hora exacta (defaultDate ya viene "YYYY-MM-DDTHH:mm").
      // Botón "Nuevo turno" → primer horario sensato del negocio.
      fechaHoraInicio: defaultDate ?? proximoHorarioSugerido(),
    },
  })

  // Sugerencia de control futuro: al elegir una práctica con días de recordatorio
  // recomendados, pre-llena la fecha de control = fecha del turno + N días.
  const practicaId = watch('practicaId')
  const fechaInicio = watch('fechaHoraInicio')
  useEffect(() => {
    const practica = practicas.find(p => p.id === practicaId)
    const recDias = practica?.recordatorioRecDias
    if (recDias && recDias > 0 && fechaInicio) {
      const base = new Date(fechaInicio)
      base.setDate(base.getDate() + recDias)
      const p = (n: number) => String(n).padStart(2, '0')
      setValue('proximoControlFecha', `${base.getFullYear()}-${p(base.getMonth() + 1)}-${p(base.getDate())}`)
    }
  }, [practicaId, fechaInicio, practicas, setValue])

  const mutation = useMutation({
    mutationFn: (data: FormData) => turnosApi.crear({
      ...data,
      // Wall-clock: enviamos "YYYY-MM-DDTHH:mm:00" tal como lo eligió el usuario,
      // sin convertir a UTC. Backend y BD almacenan hora local del negocio.
      fechaHoraInicio: data.fechaHoraInicio + ':00',
      proximoControlFecha: data.proximoControlFecha || undefined,
    }),
    onSuccess: () => {
      toast.success('Turno creado exitosamente')
      qc.invalidateQueries({ queryKey: ['agenda'] })
      onClose()
    },
    onError: () => toast.error('Error al crear el turno'),
  })

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/50" onClick={onClose} />
      <div className="relative bg-white dark:bg-zinc-900 rounded-2xl shadow-2xl w-full max-w-md mx-4 p-6">
        <div className="flex items-center justify-between mb-5">
          <h2 className="text-base font-semibold text-gray-900 dark:text-white">Nuevo Turno</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 dark:hover:text-white">
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit((d) => mutation.mutate(d))} className="space-y-4">
          <div>
            <div className="flex items-center justify-between mb-1">
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Paciente *</label>
              <button
                type="button"
                onClick={() => setNuevoPacienteOpen(true)}
                className="flex items-center gap-1 text-xs font-medium text-indigo-600 dark:text-indigo-400 hover:text-indigo-700"
              >
                <Plus className="w-3.5 h-3.5" />
                Nuevo
              </button>
            </div>
            <input type="hidden" {...register('pacienteId')} />
            <PacienteSelect
              valueLabel={pacienteSel?.label ?? ''}
              onSelect={(id, label) => {
                setPacienteSel({ id, label })
                setValue('pacienteId', id, { shouldValidate: true })
              }}
            />
            {errors.pacienteId && <p className="text-xs text-red-500 mt-1">{errors.pacienteId.message}</p>}
          </div>

          <Field label="Profesional *" error={errors.profesionalId?.message}>
            <select {...register('profesionalId')} className={fieldClass}>
              <option value="">Seleccionar profesional...</option>
              {profesionales.map((p) => (
                <option key={p.id} value={p.id}>{p.nombreCompleto}</option>
              ))}
            </select>
          </Field>

          <Field label="Práctica *" error={errors.practicaId?.message}>
            <select {...register('practicaId')} className={fieldClass}>
              <option value="">Seleccionar práctica...</option>
              {practicas.map((p) => (
                <option key={p.id} value={p.id}>{p.nombre} — {p.duracionMinutos} min</option>
              ))}
            </select>
          </Field>

          <Field label="Fecha y hora *" error={errors.fechaHoraInicio?.message}>
            <input {...register('fechaHoraInicio')} type="datetime-local" className={fieldClass} />
          </Field>

          <Field label="Observaciones" error={errors.observaciones?.message}>
            <textarea
              {...register('observaciones')}
              rows={2}
              className={`${fieldClass} resize-none`}
              placeholder="Observaciones opcionales..."
            />
          </Field>

          <div className="border-t border-gray-100 dark:border-zinc-800 pt-4">
            <p className="text-xs font-medium text-gray-700 dark:text-gray-400 mb-2">
              ⏰ Control futuro sugerido <span className="text-gray-400 font-normal">(no reserva turno)</span>
            </p>
            <input
              {...register('proximoControlFecha')}
              type="date"
              className={fieldClass}
            />
          </div>

          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 border border-gray-300 dark:border-zinc-700 text-gray-700 dark:text-gray-300 text-sm font-medium py-2.5 rounded-lg hover:bg-gray-50 dark:hover:bg-zinc-800 transition-colors"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={mutation.isPending}
              className="flex-1 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium py-2.5 rounded-lg transition-colors disabled:opacity-50"
            >
              {mutation.isPending ? 'Guardando...' : 'Guardar Turno'}
            </button>
          </div>
        </form>
      </div>

      {nuevoPacienteOpen && (
        <NuevoPacienteRapido
          onClose={() => setNuevoPacienteOpen(false)}
          onCreated={(id, label) => {
            // El combo guarda el seleccionado en su propio estado: queda elegido sí o sí
            setPacienteSel({ id, label })
            setValue('pacienteId', id, { shouldValidate: true })
            qc.invalidateQueries({ queryKey: ['pacientes'] })  // refresca /pacientes en segundo plano
            setNuevoPacienteOpen(false)
          }}
        />
      )}
    </div>
  )
}

function NuevoPacienteRapido({ onClose, onCreated }: {
  onClose: () => void
  onCreated: (id: string, label: string) => void
}) {
  const [nombre, setNombre] = useState('')
  const [apellido, setApellido] = useState('')
  const [dni, setDni] = useState('')
  const [telefono, setTelefono] = useState('')

  const mutation = useMutation({
    mutationFn: () => pacientesApi.crear({
      nombre: nombre.trim(),
      apellido: apellido.trim(),
      dni: dni.trim() || undefined,
      telefono: telefono.trim() || undefined,
    }),
    onSuccess: (data) => {
      toast.success('Paciente creado')
      onCreated(data.id, `${apellido.trim()}, ${nombre.trim()}`)
    },
    onError: (e: { response?: { data?: { error?: string } } }) =>
      toast.error(e.response?.data?.error ?? 'Error al crear el paciente'),
  })

  const submit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!nombre.trim() || !apellido.trim()) {
      toast.error('Nombre y apellido son obligatorios')
      return
    }
    mutation.mutate()
  }

  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center">
      <div className="absolute inset-0 bg-black/50" onClick={onClose} />
      <div className="relative bg-white dark:bg-zinc-900 rounded-2xl shadow-2xl w-full max-w-sm mx-4 p-6">
        <div className="flex items-center justify-between mb-5">
          <h2 className="text-base font-semibold text-gray-900 dark:text-white">Nuevo paciente</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 dark:hover:text-white">
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={submit} className="space-y-4">
          <Field label="Apellido *">
            <input value={apellido} onChange={e => setApellido(e.target.value)} className={fieldClass} autoFocus />
          </Field>
          <Field label="Nombre *">
            <input value={nombre} onChange={e => setNombre(e.target.value)} className={fieldClass} />
          </Field>
          <div className="grid grid-cols-2 gap-3">
            <Field label="DNI">
              <input value={dni} onChange={e => setDni(e.target.value)} className={fieldClass} />
            </Field>
            <Field label="Teléfono">
              <input value={telefono} onChange={e => setTelefono(e.target.value)} className={fieldClass} />
            </Field>
          </div>
          <p className="text-xs text-gray-400">Podés completar el resto de los datos luego en Pacientes.</p>

          <div className="flex gap-3 pt-1">
            <button type="button" onClick={onClose}
              className="flex-1 border border-gray-300 dark:border-zinc-700 text-gray-700 dark:text-gray-300 text-sm font-medium py-2.5 rounded-lg hover:bg-gray-50 dark:hover:bg-zinc-800 transition-colors">
              Cancelar
            </button>
            <button type="submit" disabled={mutation.isPending}
              className="flex-1 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium py-2.5 rounded-lg transition-colors disabled:opacity-50">
              {mutation.isPending ? 'Creando...' : 'Crear y seleccionar'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

// Combo de búsqueda de pacientes (nombre / apellido / DNI / teléfono)
function PacienteSelect({ valueLabel, onSelect }: {
  valueLabel: string
  onSelect: (id: string, label: string) => void
}) {
  const [open, setOpen] = useState(false)
  const [q, setQ] = useState('')
  const [debounced, setDebounced] = useState('')

  useEffect(() => {
    const t = setTimeout(() => setDebounced(q), 250)
    return () => clearTimeout(t)
  }, [q])

  const { data, isFetching } = useQuery({
    queryKey: ['pacientes-search', debounced],
    queryFn: () => pacientesApi.getAll({ busqueda: debounced || undefined, pageSize: 8 }),
    enabled: open,
    staleTime: 30_000,
  })

  const resultados = data?.data ?? []

  return (
    <div className="relative">
      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
        <input
          type="text"
          value={open ? q : valueLabel}
          onChange={e => { setQ(e.target.value); if (!open) setOpen(true) }}
          onFocus={() => { setOpen(true); setQ('') }}
          placeholder="Buscar por nombre, DNI o teléfono..."
          className={`${fieldClass} pl-9`}
        />
      </div>
      {open && (
        <>
          <div className="fixed inset-0 z-10" onClick={() => setOpen(false)} />
          <div className="absolute z-20 mt-1 w-full bg-white dark:bg-zinc-800 border border-gray-200 dark:border-zinc-700 rounded-lg shadow-lg max-h-56 overflow-auto">
            {isFetching && <div className="px-3 py-2 text-xs text-gray-400">Buscando...</div>}
            {!isFetching && resultados.length === 0 && (
              <div className="px-3 py-2 text-xs text-gray-400">
                {debounced ? 'Sin resultados' : 'Escribí para buscar...'}
              </div>
            )}
            {resultados.map(p => (
              <button
                key={p.id}
                type="button"
                onClick={() => { onSelect(p.id, p.nombreCompleto); setOpen(false) }}
                className="w-full text-left px-3 py-2 text-sm hover:bg-indigo-50 dark:hover:bg-indigo-950/40 transition-colors"
              >
                <span className="font-medium text-gray-900 dark:text-white">{p.nombreCompleto}</span>
                {(p.dni || p.telefono) && (
                  <span className="text-xs text-gray-400 ml-2">{p.dni ?? p.telefono}</span>
                )}
              </button>
            ))}
          </div>
        </>
      )}
    </div>
  )
}

// Horario sugerido al abrir desde el botón (sin click en un bloque):
// hoy a la apertura si aún no abrió; próxima media hora si está dentro del horario;
// mañana a la apertura si ya cerró.
function proximoHorarioSugerido(): string {
  const APERTURA = 8, CIERRE = 20
  const now = new Date()
  const d = new Date(now)

  if (now.getHours() < APERTURA) {
    d.setHours(APERTURA, 0, 0, 0)
  } else if (now.getHours() >= CIERRE) {
    d.setDate(d.getDate() + 1)
    d.setHours(APERTURA, 0, 0, 0)
  } else {
    const m = now.getMinutes()
    if (m === 0) d.setMinutes(0, 0, 0)
    else if (m <= 30) d.setMinutes(30, 0, 0)
    else { d.setHours(d.getHours() + 1); d.setMinutes(0, 0, 0) }
  }

  const p = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}`
}

const fieldClass = 'w-full px-3 py-2 border border-gray-300 dark:border-zinc-700 rounded-lg text-sm bg-white dark:bg-zinc-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500'

function Field({ label, error, children }: { label: string; error?: string; children: React.ReactNode }) {
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">{label}</label>
      {children}
      {error && <p className="text-xs text-red-500 mt-1">{error}</p>}
    </div>
  )
}
