import { useEffect, useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import toast from 'react-hot-toast'
import { Save, CalendarClock, Bell, Mail, Send, KeyRound } from 'lucide-react'
import { configuracionApi, type SeccionConfig } from '@/api/configuracion'

const DIAS = [
  { v: 1, l: 'Lun' }, { v: 2, l: 'Mar' }, { v: 3, l: 'Mié' }, { v: 4, l: 'Jue' },
  { v: 5, l: 'Vie' }, { v: 6, l: 'Sáb' }, { v: 0, l: 'Dom' },
]

const SECCION_META: Record<string, { icon: React.ElementType; desc: string }> = {
  Turnos:        { icon: CalendarClock, desc: 'Parámetros por defecto de la agenda y los turnos.' },
  Recordatorios: { icon: Bell,          desc: 'Cuándo y cómo se envían los recordatorios automáticos.' },
  Email:         { icon: Mail,          desc: 'Proveedor de email y credenciales de envío.' },
}

export default function ConfiguracionPage() {
  const qc = useQueryClient()
  const [valores, setValores] = useState<Record<string, string>>({})
  const [emailPrueba, setEmailPrueba] = useState('')

  const { data: secciones = [], isLoading } = useQuery({
    queryKey: ['configuracion'],
    queryFn: () => configuracionApi.get(),
  })

  // Inicializa el estado local con los valores recibidos
  useEffect(() => {
    if (secciones.length) {
      const init: Record<string, string> = {}
      for (const s of secciones)
        for (const p of s.parametros)
          init[p.clave] = p.valor
      setValores(init)
    }
  }, [secciones])

  const setVal = (clave: string, valor: string) =>
    setValores(prev => ({ ...prev, [clave]: valor }))

  const guardar = useMutation({
    mutationFn: () => {
      // Solo claves con cambios o no secretas; los secretos vacíos se omiten
      const payload = Object.entries(valores).map(([clave, valor]) => ({ clave, valor }))
      return configuracionApi.actualizar(payload)
    },
    onSuccess: () => {
      toast.success('Configuración guardada')
      qc.invalidateQueries({ queryKey: ['configuracion'] })
    },
    onError: (e: { response?: { data?: { error?: string } } }) =>
      toast.error(e.response?.data?.error ?? 'Error al guardar'),
  })

  const probarEmail = useMutation({
    mutationFn: () => configuracionApi.probarEmail(emailPrueba || undefined),
    onSuccess: () => toast.success('Email de prueba enviado'),
    onError: (e: { response?: { data?: { error?: string } } }) =>
      toast.error(e.response?.data?.error ?? 'No se pudo enviar el email de prueba'),
  })

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="h-14 flex items-center justify-between px-4 md:px-6 border-b border-gray-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 shrink-0">
        <h1 className="text-base font-semibold text-gray-900 dark:text-white">Configuración</h1>
        <button
          onClick={() => guardar.mutate()}
          disabled={guardar.isPending || isLoading}
          className="flex items-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors disabled:opacity-50"
        >
          <Save className="w-4 h-4" />
          {guardar.isPending ? 'Guardando...' : 'Guardar cambios'}
        </button>
      </div>

      <div className="flex-1 overflow-auto p-4 md:p-6">
        {isLoading ? (
          <div className="flex justify-center pt-20 text-gray-400 text-sm">Cargando...</div>
        ) : (
          <div className="max-w-3xl mx-auto space-y-6">
            {secciones.map(seccion => (
              <SeccionCard
                key={seccion.seccion}
                seccion={seccion}
                valores={valores}
                setVal={setVal}
                emailPrueba={emailPrueba}
                setEmailPrueba={setEmailPrueba}
                onProbarEmail={() => probarEmail.mutate()}
                probandoEmail={probarEmail.isPending}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

function SeccionCard({ seccion, valores, setVal, emailPrueba, setEmailPrueba, onProbarEmail, probandoEmail }: {
  seccion: SeccionConfig
  valores: Record<string, string>
  setVal: (clave: string, valor: string) => void
  emailPrueba: string
  setEmailPrueba: (v: string) => void
  onProbarEmail: () => void
  probandoEmail: boolean
}) {
  const meta = SECCION_META[seccion.seccion] ?? { icon: CalendarClock, desc: '' }
  const Icon = meta.icon

  return (
    <div className="bg-white dark:bg-zinc-900 rounded-xl border border-gray-200 dark:border-zinc-800 p-5">
      <div className="flex items-start gap-3 mb-5">
        <span className="w-9 h-9 rounded-lg bg-indigo-50 dark:bg-indigo-950/40 flex items-center justify-center shrink-0">
          <Icon className="w-4.5 h-4.5 text-indigo-600 dark:text-indigo-400" />
        </span>
        <div>
          <h2 className="text-sm font-semibold text-gray-900 dark:text-white">{seccion.seccion}</h2>
          <p className="text-xs text-gray-400">{meta.desc}</p>
        </div>
      </div>

      <div className="space-y-4">
        {seccion.parametros.map(p => (
          <div key={p.clave}>
            <label className="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              {p.esSecreto && <KeyRound className="w-3.5 h-3.5 text-gray-400" />}
              {p.label}
              {p.esSecreto && p.configurado && (
                <span className="text-[10px] px-1.5 py-0.5 rounded bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-400">
                  Configurado
                </span>
              )}
            </label>
            <ParamInput param={p} valor={valores[p.clave] ?? ''} onChange={v => setVal(p.clave, v)} />
            {p.ayuda && <p className="text-xs text-gray-400 mt-1">{p.ayuda}</p>}
          </div>
        ))}
      </div>

      {/* Probar email */}
      {seccion.seccion === 'Email' && (
        <div className="mt-5 pt-5 border-t border-gray-100 dark:border-zinc-800">
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Probar envío</label>
          <div className="flex gap-2">
            <input
              value={emailPrueba}
              onChange={e => setEmailPrueba(e.target.value)}
              type="email"
              placeholder="tu@email.com (vacío = tu usuario)"
              className={inp}
            />
            <button
              type="button"
              onClick={onProbarEmail}
              disabled={probandoEmail}
              className="flex items-center gap-2 px-4 py-2 text-sm font-medium border border-indigo-200 dark:border-indigo-900 text-indigo-600 dark:text-indigo-400 rounded-lg hover:bg-indigo-50 dark:hover:bg-indigo-950/40 transition-colors disabled:opacity-50 shrink-0"
            >
              <Send className="w-4 h-4" />
              {probandoEmail ? 'Enviando...' : 'Enviar prueba'}
            </button>
          </div>
        </div>
      )}
    </div>
  )
}

function ParamInput({ param, valor, onChange }: {
  param: SeccionConfig['parametros'][number]
  valor: string
  onChange: (v: string) => void
}) {
  switch (param.tipo) {
    case 'bool':
      return (
        <label className="flex items-center gap-2 cursor-pointer">
          <input
            type="checkbox"
            checked={valor === 'true'}
            onChange={e => onChange(e.target.checked ? 'true' : 'false')}
            className="rounded border-gray-300 text-indigo-600 w-4 h-4"
          />
          <span className="text-sm text-gray-500">{valor === 'true' ? 'Activado' : 'Desactivado'}</span>
        </label>
      )
    case 'time':
      return <input type="time" value={valor} onChange={e => onChange(e.target.value)} className={inp} />
    case 'int':
      return <input type="number" value={valor} onChange={e => onChange(e.target.value)} className={inp} />
    case 'select':
      return (
        <select value={valor} onChange={e => onChange(e.target.value)} className={inp}>
          {(param.opciones ?? '').split(',').map(o => (
            <option key={o} value={o}>{o}</option>
          ))}
        </select>
      )
    case 'dias':
      return <DiasSelector valor={valor} onChange={onChange} />
    default:
      return (
        <input
          type={param.esSecreto ? 'password' : 'text'}
          value={valor}
          onChange={e => onChange(e.target.value)}
          placeholder={param.esSecreto && param.configurado ? '•••••••• (dejar vacío para no cambiar)' : ''}
          className={inp}
          // Evita que el gestor de contraseñas autocomplete estos campos
          // (antes inyectaba la contraseña del login en las API keys/SMTP).
          name={`cfg_${param.clave}`}
          autoComplete={param.esSecreto ? 'new-password' : 'off'}
          data-lpignore="true"
          data-1p-ignore="true"
          data-form-type="other"
        />
      )
  }
}

function DiasSelector({ valor, onChange }: { valor: string; onChange: (v: string) => void }) {
  const seleccionados = valor.split(',').filter(Boolean).map(Number)
  const toggle = (dia: number) => {
    const set = new Set(seleccionados)
    set.has(dia) ? set.delete(dia) : set.add(dia)
    onChange([...set].sort((a, b) => a - b).join(','))
  }
  return (
    <div className="flex flex-wrap gap-1.5">
      {DIAS.map(d => {
        const activo = seleccionados.includes(d.v)
        return (
          <button
            key={d.v}
            type="button"
            onClick={() => toggle(d.v)}
            className={`px-3 py-1.5 text-xs font-medium rounded-lg border transition-colors ${
              activo
                ? 'bg-indigo-600 border-indigo-600 text-white'
                : 'border-gray-300 dark:border-zinc-700 text-gray-500 hover:bg-gray-50 dark:hover:bg-zinc-800'
            }`}
          >
            {d.l}
          </button>
        )
      })}
    </div>
  )
}

const inp = 'w-full px-3 py-2 border border-gray-300 dark:border-zinc-700 rounded-lg text-sm bg-white dark:bg-zinc-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500'
