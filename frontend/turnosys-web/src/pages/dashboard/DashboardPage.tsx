import { useState, useMemo } from 'react'
import { useQuery } from '@tanstack/react-query'
import {
  CalendarCheck, CheckCircle2, XCircle,
  DollarSign, UserPlus, Activity, Users,
} from 'lucide-react'
import {
  ResponsiveContainer, AreaChart, Area, BarChart, Bar,
  PieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip, Legend,
} from 'recharts'
import { format, subDays, startOfDay, endOfDay } from 'date-fns'
import { es } from 'date-fns/locale'
import { estadisticasApi } from '@/api/estadisticas'

const RANGOS = [
  { label: 'Últimos 7 días', dias: 7 },
  { label: 'Últimos 30 días', dias: 30 },
  { label: 'Últimos 90 días', dias: 90 },
]

const DIAS_SEMANA = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb']
const HORAS = Array.from({ length: 14 }, (_, i) => i + 7) // 07:00 a 20:00

const COLOR = {
  indigo: '#4F46E5',
  blue: '#3B82F6',
  green: '#22C55E',
  red: '#EF4444',
  amber: '#F59E0B',
  violet: '#8B5CF6',
}

function fmtMoney(n: number) {
  return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 0 }).format(n)
}

export default function DashboardPage() {
  const [diasRango, setDiasRango] = useState(30)

  const { desde, hasta } = useMemo(() => ({
    desde: startOfDay(subDays(new Date(), diasRango - 1)),
    hasta: endOfDay(new Date()),
  }), [diasRango])

  const { data: kpis, isLoading: loadingKpis } = useQuery({
    queryKey: ['stats-kpis', diasRango],
    queryFn: () => estadisticasApi.getKPIs(desde, hasta),
    staleTime: 120_000,
  })

  const { data: serie = [] } = useQuery({
    queryKey: ['stats-serie', diasRango],
    queryFn: () => estadisticasApi.getTurnosPorPeriodo(desde, hasta),
    staleTime: 120_000,
  })

  const { data: heatmap = [] } = useQuery({
    queryKey: ['stats-heatmap'],
    queryFn: () => estadisticasApi.getHeatmap(90),
    staleTime: 600_000,
  })

  const { data: profesionales = [] } = useQuery({
    queryKey: ['stats-profesionales', diasRango],
    queryFn: () => estadisticasApi.getProfesionales(desde, hasta),
    staleTime: 120_000,
  })

  const { data: pacientes } = useQuery({
    queryKey: ['stats-pacientes', diasRango],
    queryFn: () => estadisticasApi.getPacientes(desde, hasta),
    staleTime: 120_000,
  })

  // Datos para la torta de estados
  const dataEstados = kpis ? [
    { name: 'Atendidos', value: kpis.atendidos, color: COLOR.violet },
    { name: 'Reservados', value: kpis.reservados, color: COLOR.blue },
    { name: 'Cancelados', value: kpis.cancelados, color: COLOR.red },
  ].filter(d => d.value > 0) : []

  // Serie temporal formateada
  const serieChart = serie.map(s => ({
    fecha: format(new Date(s.fecha + 'T00:00:00'), 'dd/MM', { locale: es }),
    Turnos: s.total,
    Atendidos: s.atendidos,
  }))

  // Top 6 profesionales por turnos
  const topProf = profesionales.slice(0, 6).map(p => ({
    nombre: p.nombre.split(',')[0],
    Turnos: p.totalTurnos,
    color: p.colorAgenda,
  }))

  // Mapa de heatmap: clave "dia-hora" -> cantidad
  const heatMap = useMemo(() => {
    const m = new Map<string, number>()
    let max = 0
    for (const c of heatmap) {
      m.set(`${c.diaSemana}-${c.hora}`, c.cantidad)
      if (c.cantidad > max) max = c.cantidad
    }
    return { m, max }
  }, [heatmap])

  const heatColor = (cant: number) => {
    if (cant === 0) return 'transparent'
    const intensidad = heatMap.max === 0 ? 0 : cant / heatMap.max
    if (intensidad > 0.66) return COLOR.red
    if (intensidad > 0.33) return COLOR.amber
    return COLOR.green
  }

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="min-h-14 flex flex-wrap items-center justify-between gap-2 px-4 md:px-6 py-2 border-b border-gray-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 shrink-0">
        <h1 className="text-base font-semibold text-gray-900 dark:text-white">Dashboard</h1>
        <div className="flex gap-1 bg-gray-100 dark:bg-zinc-800 rounded-lg p-0.5">
          {RANGOS.map(r => (
            <button
              key={r.dias}
              onClick={() => setDiasRango(r.dias)}
              className={`px-3 py-1.5 text-xs font-medium rounded-md transition-colors ${
                diasRango === r.dias
                  ? 'bg-white dark:bg-zinc-700 text-indigo-600 dark:text-white shadow-sm'
                  : 'text-gray-500 hover:text-gray-700 dark:hover:text-gray-300'
              }`}
            >
              {r.label}
            </button>
          ))}
        </div>
      </div>

      {/* Contenido */}
      <div className="flex-1 overflow-auto p-4 md:p-6 space-y-6">
        {/* KPI Cards */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 md:gap-4">
          <KpiCard icon={CalendarCheck} label="Turnos" value={kpis?.totalTurnos ?? 0}
            color={COLOR.indigo} loading={loadingKpis} />
          <KpiCard icon={CheckCircle2} label="Atendidos" value={kpis?.atendidos ?? 0}
            color={COLOR.violet} loading={loadingKpis} />
          <KpiCard icon={XCircle} label="Cancelados" value={kpis?.cancelados ?? 0}
            color={COLOR.red} loading={loadingKpis} />
          <KpiCard icon={Activity} label="Ocupación" value={`${kpis?.porcentajeOcupacion ?? 0}%`}
            color={COLOR.green} loading={loadingKpis} />
          <KpiCard icon={DollarSign} label="Ingresos estim." value={fmtMoney(kpis?.ingresoEstimado ?? 0)}
            color={COLOR.amber} loading={loadingKpis} small />
          <KpiCard icon={CalendarCheck} label="Reservados" value={kpis?.reservados ?? 0}
            color={COLOR.blue} loading={loadingKpis} />
          <KpiCard icon={UserPlus} label="Pacientes nuevos" value={kpis?.pacientesNuevos ?? 0}
            color={COLOR.indigo} loading={loadingKpis} />
          <KpiCard icon={Users} label="Recurrentes" value={pacientes?.pacientesRecurrentes ?? 0}
            color={COLOR.violet} loading={loadingKpis} />
        </div>

        {/* Gráficos fila 1 */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
          {/* Serie temporal */}
          <Panel title="Turnos por día" className="lg:col-span-2">
            {serieChart.length === 0 ? <Empty /> : (
              <ResponsiveContainer width="100%" height={260}>
                <AreaChart data={serieChart} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                  <defs>
                    <linearGradient id="gTurnos" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor={COLOR.indigo} stopOpacity={0.3} />
                      <stop offset="95%" stopColor={COLOR.indigo} stopOpacity={0} />
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" strokeOpacity={0.4} />
                  <XAxis dataKey="fecha" tick={{ fontSize: 11 }} stroke="#9ca3af" />
                  <YAxis tick={{ fontSize: 11 }} stroke="#9ca3af" allowDecimals={false} />
                  <Tooltip contentStyle={tooltipStyle} />
                  <Area type="monotone" dataKey="Turnos" stroke={COLOR.indigo} strokeWidth={2} fill="url(#gTurnos)" />
                  <Area type="monotone" dataKey="Atendidos" stroke={COLOR.violet} strokeWidth={2} fillOpacity={0} />
                </AreaChart>
              </ResponsiveContainer>
            )}
          </Panel>

          {/* Distribución por estado */}
          <Panel title="Distribución por estado">
            {dataEstados.length === 0 ? <Empty /> : (
              <ResponsiveContainer width="100%" height={260}>
                <PieChart>
                  <Pie data={dataEstados} dataKey="value" nameKey="name" cx="50%" cy="45%"
                    innerRadius={50} outerRadius={80} paddingAngle={2}>
                    {dataEstados.map((d, i) => <Cell key={i} fill={d.color} />)}
                  </Pie>
                  <Tooltip contentStyle={tooltipStyle} />
                  <Legend iconType="circle" wrapperStyle={{ fontSize: 12 }} />
                </PieChart>
              </ResponsiveContainer>
            )}
          </Panel>
        </div>

        {/* Gráficos fila 2 */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {/* Top profesionales */}
          <Panel title="Top profesionales por turnos">
            {topProf.length === 0 ? <Empty /> : (
              <ResponsiveContainer width="100%" height={260}>
                <BarChart data={topProf} layout="vertical" margin={{ top: 5, right: 20, left: 10, bottom: 5 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" strokeOpacity={0.4} horizontal={false} />
                  <XAxis type="number" tick={{ fontSize: 11 }} stroke="#9ca3af" allowDecimals={false} />
                  <YAxis type="category" dataKey="nombre" tick={{ fontSize: 11 }} stroke="#9ca3af" width={90} />
                  <Tooltip contentStyle={tooltipStyle} cursor={{ fill: '#6366f115' }} />
                  <Bar dataKey="Turnos" radius={[0, 4, 4, 0]}>
                    {topProf.map((p, i) => <Cell key={i} fill={p.color} />)}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            )}
          </Panel>

          {/* Heatmap */}
          <Panel title="Ocupación por hora y día (90 días)">
            {heatmap.length === 0 ? <Empty /> : (
              <div className="overflow-x-auto">
                <table className="border-separate border-spacing-0.5 text-[10px]">
                  <thead>
                    <tr>
                      <th className="w-8" />
                      {DIAS_SEMANA.map(d => (
                        <th key={d} className="text-gray-400 font-medium pb-1 w-9">{d}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {HORAS.map(hora => (
                      <tr key={hora}>
                        <td className="text-gray-400 pr-1 text-right">{String(hora).padStart(2, '0')}</td>
                        {DIAS_SEMANA.map((_, dia) => {
                          const cant = heatMap.m.get(`${dia}-${hora}`) ?? 0
                          return (
                            <td key={dia}>
                              <div
                                className="w-8 h-5 rounded-sm border border-gray-100 dark:border-zinc-800 flex items-center justify-center"
                                style={{ backgroundColor: heatColor(cant) }}
                                title={`${DIAS_SEMANA[dia]} ${hora}:00 — ${cant} turnos`}
                              >
                                {cant > 0 && <span className="text-white text-[9px] font-medium">{cant}</span>}
                              </div>
                            </td>
                          )
                        })}
                      </tr>
                    ))}
                  </tbody>
                </table>
                <div className="flex items-center gap-3 mt-3 text-[10px] text-gray-400">
                  <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm" style={{ background: COLOR.green }} /> Baja</span>
                  <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm" style={{ background: COLOR.amber }} /> Media</span>
                  <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm" style={{ background: COLOR.red }} /> Alta</span>
                </div>
              </div>
            )}
          </Panel>
        </div>

        {/* Tablas fila 3 */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {/* Productividad profesionales */}
          <Panel title="Productividad por profesional">
            {profesionales.length === 0 ? <Empty /> : (
              <div className="overflow-x-auto -mx-1">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="text-left text-xs text-gray-400 border-b border-gray-100 dark:border-zinc-800">
                      <th className="py-2 px-1 font-medium">Profesional</th>
                      <th className="py-2 px-1 font-medium text-right">Turnos</th>
                      <th className="py-2 px-1 font-medium text-right">Atend.</th>
                      <th className="py-2 px-1 font-medium text-right">Ingresos</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-50 dark:divide-zinc-800/50">
                    {profesionales.map(p => (
                      <tr key={p.profesionalId}>
                        <td className="py-2 px-1">
                          <div className="flex items-center gap-2">
                            <span className="w-2.5 h-2.5 rounded-full shrink-0" style={{ backgroundColor: p.colorAgenda }} />
                            <span className="text-gray-700 dark:text-gray-300 truncate">{p.nombre}</span>
                          </div>
                        </td>
                        <td className="py-2 px-1 text-right text-gray-700 dark:text-gray-300">{p.totalTurnos}</td>
                        <td className="py-2 px-1 text-right text-gray-500">{p.atendidos}</td>
                        <td className="py-2 px-1 text-right text-gray-700 dark:text-gray-300 font-medium">{fmtMoney(p.ingresoEstimado)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </Panel>

          {/* Pacientes frecuentes */}
          <Panel title="Pacientes frecuentes">
            {!pacientes?.topPacientes.length ? <Empty /> : (
              <div className="space-y-1">
                {pacientes.topPacientes.slice(0, 8).map((p, i) => (
                  <div key={p.pacienteId} className="flex items-center gap-3 py-1.5">
                    <span className="w-5 text-xs text-gray-400 text-center">{i + 1}</span>
                    <span className="flex-1 text-sm text-gray-700 dark:text-gray-300 truncate">{p.nombre}</span>
                    <span className="text-xs text-gray-400">{p.turnosAtendidos} atend.</span>
                    <span className="text-sm font-medium text-indigo-600 dark:text-indigo-400 w-12 text-right">
                      {p.totalTurnos}
                    </span>
                  </div>
                ))}
              </div>
            )}
          </Panel>
        </div>
      </div>
    </div>
  )
}

const tooltipStyle = {
  fontSize: 12,
  borderRadius: 8,
  border: '1px solid #e5e7eb',
  boxShadow: '0 4px 12px rgba(0,0,0,0.08)',
}

function KpiCard({ icon: Icon, label, value, color, loading, small }: {
  icon: React.ElementType
  label: string
  value: string | number
  color: string
  loading?: boolean
  small?: boolean
}) {
  return (
    <div className="bg-white dark:bg-zinc-900 rounded-xl border border-gray-200 dark:border-zinc-800 p-4">
      <div className="flex items-center gap-2 mb-2">
        <span className="w-7 h-7 rounded-lg flex items-center justify-center shrink-0"
          style={{ backgroundColor: `${color}1a` }}>
          <Icon className="w-4 h-4" style={{ color }} />
        </span>
        <span className="text-xs text-gray-500 dark:text-gray-400 truncate">{label}</span>
      </div>
      {loading
        ? <div className="h-7 w-16 bg-gray-100 dark:bg-zinc-800 rounded animate-pulse" />
        : <p className={`font-bold text-gray-900 dark:text-white ${small ? 'text-lg' : 'text-2xl'}`}>{value}</p>
      }
    </div>
  )
}

function Panel({ title, children, className = '' }: {
  title: string; children: React.ReactNode; className?: string
}) {
  return (
    <div className={`bg-white dark:bg-zinc-900 rounded-xl border border-gray-200 dark:border-zinc-800 p-4 ${className}`}>
      <h3 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-4">{title}</h3>
      {children}
    </div>
  )
}

function Empty() {
  return (
    <div className="flex items-center justify-center h-[200px] text-gray-300 dark:text-zinc-600 text-sm">
      Sin datos en el período
    </div>
  )
}
