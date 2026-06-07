import { useMutation, useQueryClient } from '@tanstack/react-query'
import { X, CheckCircle2, XCircle, CalendarClock, User, Stethoscope, Bell } from 'lucide-react'
import toast from 'react-hot-toast'
import { turnosApi } from '@/api/turnos'
import type { TurnoAgenda } from '@/types'

interface Props {
  turno: TurnoAgenda
  onClose: () => void
}

const COLOR_ESTADO: Record<number, { label: string; bg: string; text: string }> = {
  1: { label: 'Disponible', bg: 'bg-green-100 dark:bg-green-900/30', text: 'text-green-700 dark:text-green-400' },
  2: { label: 'Reservado',  bg: 'bg-blue-100 dark:bg-blue-900/30',   text: 'text-blue-700 dark:text-blue-400' },
  3: { label: 'Cancelado',  bg: 'bg-red-100 dark:bg-red-900/30',     text: 'text-red-700 dark:text-red-400' },
  4: { label: 'Atendido',   bg: 'bg-violet-100 dark:bg-violet-900/30', text: 'text-violet-700 dark:text-violet-400' },
}

export default function DetalleTurnoModal({ turno, onClose }: Props) {
  const qc = useQueryClient()
  const estado = COLOR_ESTADO[turno.estadoId] ?? COLOR_ESTADO[2]

  const refresh = () => qc.invalidateQueries({ queryKey: ['agenda'] })

  const atender = useMutation({
    mutationFn: () => turnosApi.atender(turno.id),
    onSuccess: () => { toast.success('Turno marcado como atendido'); refresh(); onClose() },
    onError: (e: { response?: { data?: { error?: string } } }) =>
      toast.error(e.response?.data?.error ?? 'No se pudo marcar como atendido'),
  })

  const cancelar = useMutation({
    mutationFn: (motivo: string) => turnosApi.cancelar(turno.id, motivo),
    onSuccess: () => { toast.success('Turno cancelado'); refresh(); onClose() },
    onError: (e: { response?: { data?: { error?: string } } }) =>
      toast.error(e.response?.data?.error ?? 'No se pudo cancelar'),
  })

  const reagendar = useMutation({
    mutationFn: (nuevaIso: string) => turnosApi.reagendar(turno.id, nuevaIso),
    onSuccess: () => { toast.success('Turno reagendado'); refresh(); onClose() },
    onError: (e: { response?: { data?: { error?: string } } }) =>
      toast.error(e.response?.data?.error ?? 'No se pudo reagendar'),
  })

  const handleCancelar = () => {
    const motivo = prompt('Motivo de la cancelación (opcional):') ?? ''
    if (confirm('¿Cancelar este turno?')) cancelar.mutate(motivo)
  }

  const handleReagendar = () => {
    // Toma "YYYY-MM-DDTHH:mm" de la fecha actual del turno como sugerencia
    const sugerencia = turno.inicio.substring(0, 16)
    const nueva = prompt('Nueva fecha y hora (formato YYYY-MM-DDTHH:mm):', sugerencia)
    if (!nueva) return
    if (!/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/.test(nueva)) {
      toast.error('Formato inválido. Usá YYYY-MM-DDTHH:mm')
      return
    }
    reagendar.mutate(nueva + ':00')
  }

  const fechaFmt = new Date(turno.inicio).toLocaleString('es-AR', {
    dateStyle: 'long', timeStyle: 'short',
  })

  const puedeOperar = turno.estadoId === 2  // solo Reservado

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/50" onClick={onClose} />
      <div className="relative bg-white dark:bg-zinc-900 rounded-2xl shadow-2xl w-full max-w-md mx-4 p-6">
        <div className="flex items-start justify-between mb-5">
          <div>
            <h2 className="text-base font-semibold text-gray-900 dark:text-white">Detalle del turno</h2>
            <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium mt-1 ${estado.bg} ${estado.text}`}>
              {estado.label}
            </span>
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 dark:hover:text-white">
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="space-y-3 mb-6">
          <Info icon={CalendarClock} label="Fecha y hora" value={fechaFmt} />
          <Info icon={User} label="Paciente" value={turno.pacienteNombre} extra={turno.pacienteTelefono} />
          <Info icon={Stethoscope} label="Profesional" value={turno.profesionalNombre} dot={turno.profesionalColor} />
          <Info icon={Stethoscope} label="Práctica" value={`${turno.practicaNombre} — ${turno.duracionMinutos} min`} />
          <Info
            icon={Bell}
            label="Próximo control / recordatorio"
            value={turno.proximoControlFecha ? fmtFecha(turno.proximoControlFecha) : 'Sin recordatorio'}
          />
          {turno.observaciones && (
            <div className="text-xs text-gray-500 dark:text-gray-400 bg-gray-50 dark:bg-zinc-800 rounded-lg p-3">
              {turno.observaciones}
            </div>
          )}
        </div>

        {puedeOperar ? (
          <div className="grid grid-cols-2 gap-2">
            <button
              onClick={() => atender.mutate()}
              disabled={atender.isPending}
              className="flex items-center justify-center gap-2 bg-violet-600 hover:bg-violet-700 text-white text-sm font-medium py-2.5 rounded-lg transition-colors disabled:opacity-50"
            >
              <CheckCircle2 className="w-4 h-4" />
              Atender
            </button>
            <button
              onClick={handleReagendar}
              disabled={reagendar.isPending}
              className="flex items-center justify-center gap-2 border border-gray-300 dark:border-zinc-700 text-gray-700 dark:text-gray-300 text-sm font-medium py-2.5 rounded-lg hover:bg-gray-50 dark:hover:bg-zinc-800 transition-colors disabled:opacity-50"
            >
              <CalendarClock className="w-4 h-4" />
              Reagendar
            </button>
            <button
              onClick={handleCancelar}
              disabled={cancelar.isPending}
              className="col-span-2 flex items-center justify-center gap-2 border border-red-200 dark:border-red-900 text-red-600 dark:text-red-400 text-sm font-medium py-2.5 rounded-lg hover:bg-red-50 dark:hover:bg-red-950/30 transition-colors disabled:opacity-50"
            >
              <XCircle className="w-4 h-4" />
              Cancelar turno
            </button>
          </div>
        ) : (
          <p className="text-xs text-gray-400 text-center">
            Sin acciones disponibles para un turno {estado.label.toLowerCase()}.
          </p>
        )}
      </div>
    </div>
  )
}

// "2026-06-09" → "09/06/2026" (sin pasar por Date para evitar desfase de timezone)
function fmtFecha(iso: string): string {
  const [y, m, d] = iso.substring(0, 10).split('-')
  return `${d}/${m}/${y}`
}

function Info({ icon: Icon, label, value, extra, dot }: {
  icon: React.ElementType; label: string; value: string; extra?: string; dot?: string
}) {
  return (
    <div className="flex items-start gap-3">
      <Icon className="w-4 h-4 text-gray-400 mt-0.5 shrink-0" />
      <div className="flex-1 min-w-0">
        <p className="text-xs text-gray-400">{label}</p>
        <p className="text-sm text-gray-800 dark:text-gray-200 flex items-center gap-2 truncate">
          {dot && <span className="w-2 h-2 rounded-full shrink-0" style={{ background: dot }} />}
          {value}
        </p>
        {extra && <p className="text-xs text-gray-400">{extra}</p>}
      </div>
    </div>
  )
}
