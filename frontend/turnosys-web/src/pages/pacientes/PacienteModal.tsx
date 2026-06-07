import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import toast from 'react-hot-toast'
import { X } from 'lucide-react'
import { pacientesApi, type PacienteForm } from '@/api/pacientes'

const schema = z.object({
  nombre:                    z.string().min(1, 'Requerido').max(100),
  apellido:                  z.string().min(1, 'Requerido').max(100),
  dni:                       z.string().max(20).optional().or(z.literal('')),
  fechaNacimiento:           z.string().optional().or(z.literal('')),
  telefono:                  z.string().max(50).optional().or(z.literal('')),
  email:                     z.string().email('Email inválido').optional().or(z.literal('')),
  direccion:                 z.string().max(300).optional().or(z.literal('')),
  obraSocial:                z.string().max(200).optional().or(z.literal('')),
  numeroAfiliado:            z.string().max(100).optional().or(z.literal('')),
  contactoEmergenciaNombre:  z.string().max(200).optional().or(z.literal('')),
  contactoEmergenciaTelefono:z.string().max(50).optional().or(z.literal('')),
  observaciones:             z.string().optional().or(z.literal('')),
  restricciones:             z.string().optional().or(z.literal('')),
  isActivo:                  z.boolean().optional(),
})

type FormData = z.infer<typeof schema>

interface Props {
  isOpen: boolean
  pacienteId?: string
  onClose: () => void
}

export default function PacienteModal({ isOpen, pacienteId, onClose }: Props) {
  const qc = useQueryClient()
  const isEdit = !!pacienteId

  const { data: paciente } = useQuery({
    queryKey: ['paciente', pacienteId],
    queryFn: () => pacientesApi.getById(pacienteId!),
    enabled: isEdit,
  })

  const { register, handleSubmit, reset, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { isActivo: true },
  })

  useEffect(() => {
    if (paciente) {
      reset({
        nombre:                     paciente.nombre,
        apellido:                   paciente.apellido,
        dni:                        paciente.dni ?? '',
        fechaNacimiento:            paciente.fechaNacimiento ?? '',
        telefono:                   paciente.telefono ?? '',
        email:                      paciente.email ?? '',
        direccion:                  paciente.direccion ?? '',
        obraSocial:                 paciente.obraSocial ?? '',
        numeroAfiliado:             paciente.numeroAfiliado ?? '',
        contactoEmergenciaNombre:   paciente.contactoEmergenciaNombre ?? '',
        contactoEmergenciaTelefono: paciente.contactoEmergenciaTelefono ?? '',
        observaciones:              paciente.observaciones ?? '',
        restricciones:              paciente.restricciones ?? '',
        isActivo:                   paciente.isActivo,
      })
    }
  }, [paciente, reset])

  const buildPayload = (data: FormData): PacienteForm => ({
    ...data,
    dni:             data.dni || undefined,
    fechaNacimiento: data.fechaNacimiento || undefined,
    telefono:        data.telefono || undefined,
    email:           data.email || undefined,
    direccion:       data.direccion || undefined,
    obraSocial:      data.obraSocial || undefined,
    numeroAfiliado:  data.numeroAfiliado || undefined,
    contactoEmergenciaNombre:   data.contactoEmergenciaNombre || undefined,
    contactoEmergenciaTelefono: data.contactoEmergenciaTelefono || undefined,
    observaciones:   data.observaciones || undefined,
    restricciones:   data.restricciones || undefined,
    isActivo:        data.isActivo ?? true,
  })

  const mutation = useMutation({
    mutationFn: async (data: FormData) => {
      if (isEdit) {
        await pacientesApi.actualizar(pacienteId!, buildPayload(data))
      } else {
        await pacientesApi.crear(buildPayload(data))
      }
    },
    onSuccess: () => {
      toast.success(isEdit ? 'Paciente actualizado' : 'Paciente creado')
      qc.invalidateQueries({ queryKey: ['pacientes'] })
      if (isEdit) qc.invalidateQueries({ queryKey: ['paciente', pacienteId] })
      onClose()
    },
    onError: (e: { response?: { data?: { error?: string } } }) =>
      toast.error(e.response?.data?.error ?? 'Error al guardar el paciente'),
  })

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/50" onClick={onClose} />
      <div className="relative bg-white dark:bg-zinc-900 rounded-2xl shadow-2xl w-full max-w-2xl mx-4 max-h-[90vh] flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 dark:border-zinc-800 shrink-0">
          <h2 className="text-base font-semibold text-gray-900 dark:text-white">
            {isEdit ? 'Editar paciente' : 'Nuevo paciente'}
          </h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 dark:hover:text-white">
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit(d => mutation.mutate(d))} className="overflow-y-auto">
          <div className="px-6 py-5 space-y-5">

            {/* Datos personales */}
            <Section title="Datos personales">
              <div className="grid grid-cols-2 gap-4">
                <Field label="Apellido *" error={errors.apellido?.message}>
                  <input {...register('apellido')} className={input} placeholder="García" />
                </Field>
                <Field label="Nombre *" error={errors.nombre?.message}>
                  <input {...register('nombre')} className={input} placeholder="Juan" />
                </Field>
                <Field label="DNI" error={errors.dni?.message}>
                  <input {...register('dni')} className={input} placeholder="30000000" />
                </Field>
                <Field label="Fecha de nacimiento" error={errors.fechaNacimiento?.message}>
                  <input {...register('fechaNacimiento')} type="date" className={input} />
                </Field>
              </div>
            </Section>

            {/* Contacto */}
            <Section title="Contacto">
              <div className="grid grid-cols-2 gap-4">
                <Field label="Teléfono" error={errors.telefono?.message}>
                  <input {...register('telefono')} className={input} placeholder="11-1234-5678" />
                </Field>
                <Field label="Email" error={errors.email?.message}>
                  <input {...register('email')} type="email" className={input} placeholder="paciente@email.com" />
                </Field>
                <Field label="Dirección" error={errors.direccion?.message} className="col-span-2">
                  <input {...register('direccion')} className={input} placeholder="Av. Corrientes 1234, CABA" />
                </Field>
              </div>
            </Section>

            {/* Obra social */}
            <Section title="Cobertura médica">
              <div className="grid grid-cols-2 gap-4">
                <Field label="Obra social" error={errors.obraSocial?.message}>
                  <input {...register('obraSocial')} className={input} placeholder="OSDE" />
                </Field>
                <Field label="Número de afiliado" error={errors.numeroAfiliado?.message}>
                  <input {...register('numeroAfiliado')} className={input} placeholder="12345678" />
                </Field>
              </div>
            </Section>

            {/* Emergencias */}
            <Section title="Contacto de emergencia">
              <div className="grid grid-cols-2 gap-4">
                <Field label="Nombre" error={errors.contactoEmergenciaNombre?.message}>
                  <input {...register('contactoEmergenciaNombre')} className={input} placeholder="María García" />
                </Field>
                <Field label="Teléfono" error={errors.contactoEmergenciaTelefono?.message}>
                  <input {...register('contactoEmergenciaTelefono')} className={input} placeholder="11-8765-4321" />
                </Field>
              </div>
            </Section>

            {/* Notas clínicas */}
            <Section title="Notas clínicas">
              <div className="space-y-3">
                <Field label="Restricciones / Alergias" error={errors.restricciones?.message}>
                  <textarea
                    {...register('restricciones')}
                    rows={2}
                    className={`${input} resize-none`}
                    placeholder="Alergia a la penicilina, hipertensión..."
                  />
                </Field>
                <Field label="Observaciones" error={errors.observaciones?.message}>
                  <textarea
                    {...register('observaciones')}
                    rows={3}
                    className={`${input} resize-none`}
                    placeholder="Notas generales sobre el paciente..."
                  />
                </Field>
              </div>
            </Section>

            {/* Estado (solo al editar) */}
            {isEdit && (
              <label className="flex items-center gap-3 cursor-pointer">
                <input {...register('isActivo')} type="checkbox" className="rounded border-gray-300 text-indigo-600 w-4 h-4" />
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Paciente activo</span>
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
              {mutation.isPending ? 'Guardando...' : isEdit ? 'Guardar cambios' : 'Crear paciente'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

const input = 'w-full px-3 py-2 border border-gray-300 dark:border-zinc-700 rounded-lg text-sm bg-white dark:bg-zinc-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500'

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
