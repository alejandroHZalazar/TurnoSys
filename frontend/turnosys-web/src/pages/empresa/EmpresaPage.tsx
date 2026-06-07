import { useEffect, useRef, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import toast from 'react-hot-toast'
import {
  Building2, Phone, Globe, Clock, AtSign, Share2,
  MessageCircle, Save, Image as ImageIcon, Upload,
} from 'lucide-react'
import { empresaApi, type EmpresaForm } from '@/api/empresa'
import { useAuthStore } from '@/store/authStore'

const schema = z.object({
  razonSocial:    z.string().min(1, 'Requerido').max(200),
  nombreFantasia: z.string().max(200).optional().or(z.literal('')),
  cuit:           z.string().max(20).optional().or(z.literal('')),
  direccion:      z.string().max(300).optional().or(z.literal('')),
  telefono:       z.string().max(50).optional().or(z.literal('')),
  email:          z.string().email('Email inválido').optional().or(z.literal('')),
  sitioWeb:       z.string().max(300).optional().or(z.literal('')),
  instagram:      z.string().max(200).optional().or(z.literal('')),
  facebook:       z.string().max(200).optional().or(z.literal('')),
  whatsApp:       z.string().max(50).optional().or(z.literal('')),
  horarioDesde:   z.string().optional().or(z.literal('')),
  horarioHasta:   z.string().optional().or(z.literal('')),
  timeZone:       z.string().optional().or(z.literal('')),
  observaciones:  z.string().optional().or(z.literal('')),
})

type FormData = z.infer<typeof schema>

export default function EmpresaPage() {
  const qc = useQueryClient()
  const empresaId = useAuthStore(s => s.empresaId)
  const fileRef = useRef<HTMLInputElement>(null)
  const [logoVersion, setLogoVersion] = useState(0)
  const [preview, setPreview] = useState<string | null>(null)

  const { data: empresa, isLoading } = useQuery({
    queryKey: ['empresa'],
    queryFn: () => empresaApi.get(),
  })

  const { register, handleSubmit, reset, formState: { errors, isDirty } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  // URL del logo servido desde la BD (con cache-buster al actualizar)
  const logoSrc = empresa?.tieneLogo && empresaId
    ? `/api/v1/empresa/logo?empresaId=${empresaId}&v=${logoVersion}`
    : null

  const subirLogoMutation = useMutation({
    mutationFn: (file: File) => empresaApi.subirLogo(file),
    onSuccess: () => {
      toast.success('Logotipo actualizado')
      setLogoVersion(v => v + 1)
      setPreview(null)
      qc.invalidateQueries({ queryKey: ['empresa'] })
    },
    onError: (e: { response?: { data?: { error?: string } } }) =>
      toast.error(e.response?.data?.error ?? 'Error al subir el logotipo'),
  })

  const handleFile = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return
    if (file.size > 2 * 1024 * 1024) {
      toast.error('El logo no puede superar los 2 MB')
      return
    }
    setPreview(URL.createObjectURL(file))
    subirLogoMutation.mutate(file)
  }

  useEffect(() => {
    if (empresa) {
      reset({
        razonSocial:    empresa.razonSocial,
        nombreFantasia: empresa.nombreFantasia ?? '',
        cuit:           empresa.cuit ?? '',
        direccion:      empresa.direccion ?? '',
        telefono:       empresa.telefono ?? '',
        email:          empresa.email ?? '',
        sitioWeb:       empresa.sitioWeb ?? '',
        instagram:      empresa.instagram ?? '',
        facebook:       empresa.facebook ?? '',
        whatsApp:       empresa.whatsApp ?? '',
        horarioDesde:   empresa.horarioDesde ?? '',
        horarioHasta:   empresa.horarioHasta ?? '',
        timeZone:       empresa.timeZone ?? '',
        observaciones:  empresa.observaciones ?? '',
      })
    }
  }, [empresa, reset])

  const mutation = useMutation({
    mutationFn: (data: FormData) => {
      const payload: EmpresaForm = {
        razonSocial:    data.razonSocial,
        nombreFantasia: data.nombreFantasia || undefined,
        cuit:           data.cuit || undefined,
        direccion:      data.direccion || undefined,
        telefono:       data.telefono || undefined,
        email:          data.email || undefined,
        sitioWeb:       data.sitioWeb || undefined,
        instagram:      data.instagram || undefined,
        facebook:       data.facebook || undefined,
        whatsApp:       data.whatsApp || undefined,
        horarioDesde:   data.horarioDesde || undefined,
        horarioHasta:   data.horarioHasta || undefined,
        timeZone:       data.timeZone || 'America/Argentina/Buenos_Aires',
        observaciones:  data.observaciones || undefined,
      }
      return empresaApi.actualizar(payload)
    },
    onSuccess: () => {
      toast.success('Datos de empresa guardados')
      qc.invalidateQueries({ queryKey: ['empresa'] })
    },
    onError: (e: { response?: { data?: { error?: string } } }) =>
      toast.error(e.response?.data?.error ?? 'Error al guardar'),
  })

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="h-14 flex items-center justify-between px-4 md:px-6 border-b border-gray-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 shrink-0">
        <h1 className="text-base font-semibold text-gray-900 dark:text-white">Empresa</h1>
        <button
          form="empresa-form"
          type="submit"
          disabled={mutation.isPending || !isDirty}
          className="flex items-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors disabled:opacity-50"
        >
          <Save className="w-4 h-4" />
          {mutation.isPending ? 'Guardando...' : 'Guardar cambios'}
        </button>
      </div>

      <div className="flex-1 overflow-auto p-4 md:p-6">
        {isLoading ? (
          <div className="flex justify-center pt-20 text-gray-400 text-sm">Cargando...</div>
        ) : (
          <form id="empresa-form" onSubmit={handleSubmit(d => mutation.mutate(d))}
            className="max-w-4xl mx-auto space-y-6">

            {/* Identificación */}
            <Card icon={Building2} title="Identificación">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <Field label="Razón social *" error={errors.razonSocial?.message}>
                  <input {...register('razonSocial')} className={inp} placeholder="Mi Empresa S.R.L." />
                </Field>
                <Field label="Nombre de fantasía" error={errors.nombreFantasia?.message}>
                  <input {...register('nombreFantasia')} className={inp} placeholder="Clínica Centro" />
                </Field>
                <Field label="CUIT" error={errors.cuit?.message}>
                  <input {...register('cuit')} className={inp} placeholder="30-12345678-9" />
                </Field>
                <Field label="Email" error={errors.email?.message}>
                  <input {...register('email')} type="email" className={inp} placeholder="info@empresa.com" />
                </Field>
              </div>
            </Card>

            {/* Logotipo */}
            <Card icon={ImageIcon} title="Logotipo">
              <div className="flex items-center gap-5">
                <div className="w-24 h-24 rounded-xl border border-gray-200 dark:border-zinc-700 bg-gray-50 dark:bg-zinc-800 flex items-center justify-center overflow-hidden shrink-0">
                  {preview ?? logoSrc
                    ? <img src={preview ?? logoSrc!} alt="Logo" className="w-full h-full object-contain" />
                    : <ImageIcon className="w-8 h-8 text-gray-300" />}
                </div>
                <div className="flex-1">
                  <input
                    ref={fileRef}
                    type="file"
                    accept="image/png,image/jpeg,image/webp,image/gif,image/svg+xml"
                    onChange={handleFile}
                    className="hidden"
                  />
                  <button
                    type="button"
                    onClick={() => fileRef.current?.click()}
                    disabled={subirLogoMutation.isPending}
                    className="flex items-center gap-2 px-4 py-2 text-sm font-medium border border-gray-300 dark:border-zinc-700 rounded-lg hover:bg-gray-50 dark:hover:bg-zinc-800 transition-colors disabled:opacity-50 text-gray-700 dark:text-gray-300"
                  >
                    <Upload className="w-4 h-4" />
                    {subirLogoMutation.isPending ? 'Subiendo...' : 'Subir logotipo'}
                  </button>
                  <p className="text-xs text-gray-400 mt-2">
                    PNG, JPG, WEBP, GIF o SVG · máx. 2 MB · se guarda en la base de datos
                  </p>
                </div>
              </div>
            </Card>

            {/* Contacto */}
            <Card icon={Phone} title="Contacto y ubicación">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <Field label="Teléfono" error={errors.telefono?.message}>
                  <input {...register('telefono')} className={inp} placeholder="(011) 4000-0000" />
                </Field>
                <Field label="WhatsApp" error={errors.whatsApp?.message}>
                  <input {...register('whatsApp')} className={inp} placeholder="+54 9 11 ..." />
                </Field>
                <Field label="Dirección" error={errors.direccion?.message} className="md:col-span-2">
                  <input {...register('direccion')} className={inp} placeholder="Av. Siempreviva 742, CABA" />
                </Field>
              </div>
            </Card>

            {/* Redes / web */}
            <Card icon={Globe} title="Presencia online">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <Field label="Sitio web" error={errors.sitioWeb?.message}>
                  <InputIcon icon={Globe}><input {...register('sitioWeb')} className={inpIcon} placeholder="https://empresa.com" /></InputIcon>
                </Field>
                <Field label="Instagram" error={errors.instagram?.message}>
                  <InputIcon icon={AtSign}><input {...register('instagram')} className={inpIcon} placeholder="@empresa" /></InputIcon>
                </Field>
                <Field label="Facebook" error={errors.facebook?.message}>
                  <InputIcon icon={Share2}><input {...register('facebook')} className={inpIcon} placeholder="/empresa" /></InputIcon>
                </Field>
                <Field label="WhatsApp Business" error={errors.whatsApp?.message}>
                  <InputIcon icon={MessageCircle}><input {...register('whatsApp')} className={inpIcon} placeholder="Número WhatsApp" /></InputIcon>
                </Field>
              </div>
            </Card>

            {/* Horarios */}
            <Card icon={Clock} title="Horario de atención">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <Field label="Apertura" error={errors.horarioDesde?.message}>
                  <input {...register('horarioDesde')} type="time" className={inp} />
                </Field>
                <Field label="Cierre" error={errors.horarioHasta?.message}>
                  <input {...register('horarioHasta')} type="time" className={inp} />
                </Field>
                <Field label="Zona horaria" error={errors.timeZone?.message}>
                  <input {...register('timeZone')} className={inp} placeholder="America/Argentina/Buenos_Aires" />
                </Field>
              </div>
              <Field label="Observaciones" error={errors.observaciones?.message} className="mt-4">
                <textarea {...register('observaciones')} rows={3} className={`${inp} resize-none`}
                  placeholder="Notas internas sobre la empresa..." />
              </Field>
            </Card>
          </form>
        )}
      </div>
    </div>
  )
}

const inp = 'w-full px-3 py-2 border border-gray-300 dark:border-zinc-700 rounded-lg text-sm bg-white dark:bg-zinc-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500'
const inpIcon = 'w-full pl-9 pr-3 py-2 border border-gray-300 dark:border-zinc-700 rounded-lg text-sm bg-white dark:bg-zinc-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500'

function Card({ icon: Icon, title, children }: { icon: React.ElementType; title: string; children: React.ReactNode }) {
  return (
    <div className="bg-white dark:bg-zinc-900 rounded-xl border border-gray-200 dark:border-zinc-800 p-5">
      <div className="flex items-center gap-2 mb-4">
        <Icon className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
        <h2 className="text-sm font-semibold text-gray-700 dark:text-gray-300">{title}</h2>
      </div>
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

function InputIcon({ icon: Icon, children }: { icon: React.ElementType; children: React.ReactNode }) {
  return (
    <div className="relative">
      <Icon className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
      {children}
    </div>
  )
}
