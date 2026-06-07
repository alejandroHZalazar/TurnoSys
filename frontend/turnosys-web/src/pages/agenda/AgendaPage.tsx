import FullCalendar from '@fullcalendar/react'
import dayGridPlugin from '@fullcalendar/daygrid'
import timeGridPlugin from '@fullcalendar/timegrid'
import listPlugin from '@fullcalendar/list'
import interactionPlugin from '@fullcalendar/interaction'
import esLocale from '@fullcalendar/core/locales/es'
import { useQuery } from '@tanstack/react-query'
import { useRef, useState } from 'react'
import { format } from 'date-fns'
import { Plus } from 'lucide-react'
import { turnosApi } from '@/api/turnos'
import { configuracionApi } from '@/api/configuracion'
import type { TurnoAgenda } from '@/types'
import NuevoTurnoModal from '@/components/turnos/NuevoTurnoModal'
import DetalleTurnoModal from '@/components/turnos/DetalleTurnoModal'

export default function AgendaPage() {
  const calendarRef = useRef<FullCalendar>(null)
  const [rango, setRango] = useState({
    desde: format(new Date(), "yyyy-MM-dd'T'00:00:00"),
    hasta: format(new Date(), "yyyy-MM-dd'T'23:59:59"),
  })
  const [modalOpen, setModalOpen] = useState(false)
  const [selectedDate, setSelectedDate] = useState<string | null>(null)
  const [turnoDetalle, setTurnoDetalle] = useState<TurnoAgenda | null>(null)

  const { data: turnos = [] } = useQuery({
    queryKey: ['agenda', rango.desde, rango.hasta],
    queryFn: () => turnosApi.getAgenda(rango.desde, rango.hasta),
    staleTime: 30_000,
  })

  // Horarios de atención configurados (sección Turnos)
  const { data: config } = useQuery({
    queryKey: ['configuracion'],
    queryFn: () => configuracionApi.get(),
    staleTime: 300_000,
  })

  const getParam = (clave: string, def: string) =>
    config?.flatMap(s => s.parametros).find(p => p.clave === clave)?.valor || def

  const horaInicio = getParam('TURNO_HORA_INICIO', '08:00')
  const horaFin = getParam('TURNO_HORA_FIN', '20:00')

  const events = turnos.map((t: TurnoAgenda) => ({
    id: t.id,
    title: t.titulo,
    start: t.inicio,
    end: t.fin,
    backgroundColor: t.profesionalColor,
    borderColor: t.profesionalColor,
    extendedProps: t,
  }))

  const handleDatesSet = (info: { startStr: string; endStr: string }) => {
    setRango({ desde: info.startStr, hasta: info.endStr })
  }

  const handleDateClick = (info: { date: Date; allDay: boolean }) => {
    // En timeGrid el click trae la hora exacta del bloque; en mes/lista (allDay)
    // usamos la hora de apertura como base.
    const base = info.allDay
      ? new Date(info.date.getFullYear(), info.date.getMonth(), info.date.getDate(), 8, 0)
      : info.date
    setSelectedDate(toLocalInput(base))
    setModalOpen(true)
  }

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="h-14 flex items-center justify-between px-6 border-b border-gray-200 dark:border-zinc-800 bg-white dark:bg-zinc-900">
        <h1 className="text-base font-semibold text-gray-900 dark:text-white">Agenda</h1>
        <button
          onClick={() => { setSelectedDate(null); setModalOpen(true) }}
          className="flex items-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
        >
          <Plus className="w-4 h-4" />
          Nuevo turno
        </button>
      </div>

      {/* Calendar */}
      <div className="flex-1 p-2 md:p-4 overflow-auto md:overflow-hidden">
        <div className="bg-white dark:bg-zinc-900 rounded-xl border border-gray-200 dark:border-zinc-800 p-2 md:p-4
                        h-[calc(100svh-7.5rem)] md:h-full min-h-0">
          <FullCalendar
            ref={calendarRef}
            plugins={[dayGridPlugin, timeGridPlugin, listPlugin, interactionPlugin]}
            initialView="listWeek"
            windowResizeDelay={100}
            views={{
              listWeek: { buttonText: 'Lista' },
              timeGridDay: { buttonText: 'Día' },
              timeGridWeek: { buttonText: 'Semana' },
              dayGridMonth: { buttonText: 'Mes' },
            }}
            locale={esLocale}
            headerToolbar={{
              left: 'prev,next',
              center: 'title',
              right: 'timeGridDay,timeGridWeek,dayGridMonth,listWeek',
            }}
            events={events}
            datesSet={handleDatesSet}
            dateClick={handleDateClick}
            eventClick={(info) => {
              const ext = info.event.extendedProps as TurnoAgenda
              setTurnoDetalle(ext)
            }}
            editable={true}
            selectable={true}
            height="100%"
            allDaySlot={false}
            slotMinTime={`${horaInicio}:00`}
            slotMaxTime={`${horaFin}:00`}
            slotDuration="00:30:00"
            nowIndicator={true}
            businessHours={{
              daysOfWeek: [1, 2, 3, 4, 5],
              startTime: horaInicio,
              endTime: horaFin,
            }}
            eventTimeFormat={{ hour: '2-digit', minute: '2-digit', meridiem: false }}
          />
        </div>
      </div>

      {modalOpen && (
        <NuevoTurnoModal
          isOpen={modalOpen}
          onClose={() => setModalOpen(false)}
          defaultDate={selectedDate}
        />
      )}

      {turnoDetalle && (
        <DetalleTurnoModal
          turno={turnoDetalle}
          onClose={() => setTurnoDetalle(null)}
        />
      )}
    </div>
  )
}

// Formatea un Date a "YYYY-MM-DDTHH:mm" en hora local (formato de <input datetime-local>)
function toLocalInput(d: Date): string {
  const p = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}`
}
