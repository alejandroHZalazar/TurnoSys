import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import toast from 'react-hot-toast'
import { X } from 'lucide-react'
import { usuariosApi } from '@/api/usuarios'
import type { Rol, Usuario } from '@/types'

const schemaCrear = z.object({
  nombreCompleto: z.string().min(2, 'Requerido'),
  email: z.string().email('Email inválido'),
  password: z.string().min(8, 'Mínimo 8 caracteres'),
  rolId: z.coerce.number().min(1, 'Seleccione un rol'),
  isActivo: z.boolean(),
})

const schemaEditar = z.object({
  nombreCompleto: z.string().min(2, 'Requerido'),
  email: z.string().email('Email inválido'),
  password: z.string().optional(),
  rolId: z.coerce.number().min(1, 'Seleccione un rol'),
  isActivo: z.boolean(),
})

type FormData = z.infer<typeof schemaCrear>

interface Props {
  isOpen: boolean
  onClose: () => void
  usuario: Usuario | null
  roles: Rol[]
}

export default function UsuarioModal({ isOpen, onClose, usuario, roles }: Props) {
  const qc = useQueryClient()
  const isEdit = !!usuario

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(isEdit ? schemaEditar : schemaCrear) as ReturnType<typeof zodResolver>,
    defaultValues: {
      nombreCompleto: '',
      email: '',
      password: '',
      rolId: 2,
      isActivo: true,
    },
  })

  useEffect(() => {
    if (usuario) {
      reset({
        nombreCompleto: usuario.nombreCompleto,
        email: usuario.email,
        password: '',
        rolId: usuario.rolId,
        isActivo: usuario.isActivo,
      })
    } else {
      reset({ nombreCompleto: '', email: '', password: '', rolId: 2, isActivo: true })
    }
  }, [usuario, reset])

  const crear = useMutation({
    mutationFn: (d: FormData) => usuariosApi.crear({
      nombreCompleto: d.nombreCompleto,
      email: d.email,
      password: d.password!,
      rolId: d.rolId,
    }),
    onSuccess: () => {
      toast.success('Usuario creado')
      qc.invalidateQueries({ queryKey: ['usuarios'] })
      onClose()
    },
    onError: (e: unknown) => toast.error((e as { response?: { data?: { error?: string } } })?.response?.data?.error ?? 'Error al crear'),
  })

  const editar = useMutation({
    mutationFn: (d: FormData) => usuariosApi.editar(usuario!.id, {
      nombreCompleto: d.nombreCompleto,
      email: d.email,
      rolId: d.rolId,
      isActivo: d.isActivo,
    }),
    onSuccess: () => {
      toast.success('Usuario actualizado')
      qc.invalidateQueries({ queryKey: ['usuarios'] })
      onClose()
    },
    onError: (e: unknown) => toast.error((e as { response?: { data?: { error?: string } } })?.response?.data?.error ?? 'Error al editar'),
  })

  const onSubmit = (d: FormData) => isEdit ? editar.mutate(d) : crear.mutate(d)

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h2 className="text-base font-semibold text-gray-900">
            {isEdit ? 'Editar usuario' : 'Nuevo usuario'}
          </h2>
          <button onClick={onClose} className="p-1 rounded hover:bg-gray-100">
            <X className="w-5 h-5 text-gray-500" />
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Nombre completo</label>
            <input
              {...register('nombreCompleto')}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
            {errors.nombreCompleto && <p className="text-xs text-red-500 mt-1">{errors.nombreCompleto.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input
              {...register('email')}
              type="email"
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
            {errors.email && <p className="text-xs text-red-500 mt-1">{errors.email.message}</p>}
          </div>

          {!isEdit && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Contraseña</label>
              <input
                {...register('password')}
                type="password"
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
              {errors.password && <p className="text-xs text-red-500 mt-1">{errors.password.message}</p>}
            </div>
          )}

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Rol</label>
            <select
              {...register('rolId')}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            >
              {roles.map((r) => (
                <option key={r.id} value={r.id}>{r.nombre}</option>
              ))}
            </select>
            {errors.rolId && <p className="text-xs text-red-500 mt-1">{errors.rolId.message}</p>}
          </div>

          {isEdit && (
            <div className="flex items-center gap-2">
              <input type="checkbox" id="isActivo" {...register('isActivo')} className="rounded" />
              <label htmlFor="isActivo" className="text-sm text-gray-700">Activo</label>
            </div>
          )}

          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 px-4 py-2 text-sm font-medium border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={crear.isPending || editar.isPending}
              className="flex-1 px-4 py-2 text-sm font-medium bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg transition-colors disabled:opacity-50"
            >
              {crear.isPending || editar.isPending ? 'Guardando…' : isEdit ? 'Guardar cambios' : 'Crear usuario'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
