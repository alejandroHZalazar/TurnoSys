import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuthStore } from '@/store/authStore'
import {
  CalendarDays, Users, UserRound, Stethoscope,
  Settings, Building2, BarChart3, LogOut, ShieldCheck, UsersRound
} from 'lucide-react'
import { cn } from '@/lib/utils'
import { usePermiso } from '@/hooks/usePermiso'

const NAV_ITEMS = [
  { to: '/agenda',          icon: CalendarDays, label: 'Agenda',         permiso: 'agenda.ver' },
  { to: '/pacientes',       icon: Users,        label: 'Pacientes',      permiso: 'pacientes.ver' },
  { to: '/profesionales',   icon: UserRound,    label: 'Profesionales',  permiso: 'profesionales.ver' },
  { to: '/practicas',       icon: Stethoscope,  label: 'Prácticas',      permiso: 'practicas.ver' },
  { to: '/dashboard',       icon: BarChart3,    label: 'Dashboard',      permiso: 'dashboard.ver' },
  { to: '/empresa',         icon: Building2,    label: 'Empresa',        permiso: 'empresa.ver' },
  { to: '/configuracion',   icon: Settings,     label: 'Configuración',  permiso: 'configuracion.ver' },
  { to: '/usuarios',        icon: UsersRound,   label: 'Usuarios',       permiso: 'usuarios.ver' },
  { to: '/roles',           icon: ShieldCheck,  label: 'Roles',          permiso: 'usuarios.ver' },
]

function NavItem({ to, icon: Icon, label, permiso }: typeof NAV_ITEMS[0]) {
  const tiene = usePermiso(permiso)
  if (!tiene) return null

  return (
    <NavLink
      to={to}
      className={({ isActive }) =>
        cn(
          'flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors',
          isActive
            ? 'bg-indigo-50 text-indigo-700'
            : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
        )
      }
    >
      <Icon className="w-4 h-4 shrink-0" />
      {label}
    </NavLink>
  )
}

function NavItemMobile({ to, icon: Icon, label, permiso }: typeof NAV_ITEMS[0]) {
  const tiene = usePermiso(permiso)
  if (!tiene) return null

  return (
    <NavLink
      to={to}
      className={({ isActive }) =>
        cn(
          'flex flex-col items-center gap-0.5 px-2 py-1 rounded-lg font-medium transition-colors shrink-0',
          isActive ? 'text-indigo-600' : 'text-gray-400'
        )
      }
    >
      <Icon className="w-5 h-5 shrink-0" />
      <span className="truncate text-[10px]">{label}</span>
    </NavLink>
  )
}

export default function AppLayout() {
  const { nombreCompleto, rol, logout } = useAuthStore()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="flex h-screen bg-gray-50 overflow-hidden">
      {/* Sidebar — oculto en mobile, visible desde md */}
      <aside className="hidden md:flex w-56 flex-col bg-white border-r border-gray-200 shrink-0">
        {/* Logo */}
        <div className="h-14 flex items-center px-4 border-b border-gray-200">
          <div className="w-8 h-8 bg-indigo-600 rounded-lg flex items-center justify-center mr-3">
            <CalendarDays className="w-4 h-4 text-white" />
          </div>
          <span className="font-bold text-gray-900 text-sm">TurnoSys</span>
        </div>

        {/* Nav */}
        <nav className="flex-1 p-2 space-y-0.5 overflow-y-auto">
          {NAV_ITEMS.map((item) => (
            <NavItem key={item.to} {...item} />
          ))}
        </nav>

        {/* User */}
        <div className="p-3 border-t border-gray-200">
          <div className="flex items-center gap-2 mb-1">
            <div className="w-7 h-7 rounded-full bg-indigo-100 flex items-center justify-center text-xs font-bold text-indigo-700 shrink-0">
              {nombreCompleto?.[0] ?? 'U'}
            </div>
            <div className="min-w-0">
              <span className="text-xs text-gray-700 truncate block">{nombreCompleto}</span>
              <span className="text-[10px] text-gray-400">{rol}</span>
            </div>
          </div>
          <button
            onClick={handleLogout}
            className="flex items-center gap-2 w-full text-xs text-gray-500 hover:text-gray-700 px-1 py-1 rounded"
          >
            <LogOut className="w-3.5 h-3.5" />
            Cerrar sesión
          </button>
        </div>
      </aside>

      {/* Main content */}
      <main className="flex-1 overflow-auto pb-16 md:pb-0">
        <Outlet />
      </main>

      {/* Nav inferior — solo mobile */}
      <nav className="md:hidden fixed bottom-0 left-0 right-0 z-40 bg-white border-t border-gray-200 flex items-center h-16 px-1 overflow-x-auto gap-1">
        {NAV_ITEMS.map((item) => (
          <NavItemMobile key={item.to} {...item} />
        ))}
      </nav>
    </div>
  )
}
