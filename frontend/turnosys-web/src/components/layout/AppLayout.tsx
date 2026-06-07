import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuthStore } from '@/store/authStore'
import {
  CalendarDays, Users, UserRound, Stethoscope,
  Settings, Building2, BarChart3, LogOut
} from 'lucide-react'
import { cn } from '@/lib/utils'

const navItems = [
  { to: '/agenda', icon: CalendarDays, label: 'Agenda' },
  { to: '/pacientes', icon: Users, label: 'Pacientes' },
  { to: '/profesionales', icon: UserRound, label: 'Profesionales' },
  { to: '/practicas', icon: Stethoscope, label: 'Prácticas' },
  { to: '/dashboard', icon: BarChart3, label: 'Dashboard' },
  { to: '/empresa', icon: Building2, label: 'Empresa' },
  { to: '/configuracion', icon: Settings, label: 'Configuración' },
]

export default function AppLayout() {
  const { nombreCompleto, logout } = useAuthStore()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="flex h-screen bg-gray-50 dark:bg-zinc-950 overflow-hidden">
      {/* Sidebar — oculto en mobile, visible desde md */}
      <aside className="hidden md:flex w-56 flex-col bg-white dark:bg-zinc-900 border-r border-gray-200 dark:border-zinc-800 shrink-0">
        {/* Logo */}
        <div className="h-14 flex items-center px-4 border-b border-gray-200 dark:border-zinc-800">
          <div className="w-8 h-8 bg-indigo-600 rounded-lg flex items-center justify-center mr-3">
            <CalendarDays className="w-4 h-4 text-white" />
          </div>
          <span className="font-bold text-gray-900 dark:text-white text-sm">TurnoSys</span>
        </div>

        {/* Nav */}
        <nav className="flex-1 p-2 space-y-0.5 overflow-y-auto">
          {navItems.map(({ to, icon: Icon, label }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors',
                  isActive
                    ? 'bg-indigo-50 dark:bg-indigo-950 text-indigo-700 dark:text-indigo-300'
                    : 'text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-zinc-800 hover:text-gray-900 dark:hover:text-white'
                )
              }
            >
              <Icon className="w-4 h-4 shrink-0" />
              {label}
            </NavLink>
          ))}
        </nav>

        {/* User */}
        <div className="p-3 border-t border-gray-200 dark:border-zinc-800">
          <div className="flex items-center gap-2 mb-2">
            <div className="w-7 h-7 rounded-full bg-indigo-100 dark:bg-indigo-900 flex items-center justify-center text-xs font-bold text-indigo-700 dark:text-indigo-300 shrink-0">
              {nombreCompleto?.[0] ?? 'U'}
            </div>
            <span className="text-xs text-gray-700 dark:text-gray-300 truncate">{nombreCompleto}</span>
          </div>
          <button
            onClick={handleLogout}
            className="flex items-center gap-2 w-full text-xs text-gray-500 hover:text-gray-700 dark:hover:text-gray-300 px-1 py-1 rounded"
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
      <nav className="md:hidden fixed bottom-0 left-0 right-0 z-40 bg-white dark:bg-zinc-900 border-t border-gray-200 dark:border-zinc-800 flex items-center h-16 px-1 overflow-x-auto gap-1">
        {navItems.map(({ to, icon: Icon, label }) => (
          <NavLink
            key={to}
            to={to}
            className={({ isActive }) =>
              cn(
                'flex flex-col items-center gap-0.5 px-2 py-1 rounded-lg font-medium transition-colors shrink-0',
                isActive
                  ? 'text-indigo-600 dark:text-indigo-400'
                  : 'text-gray-400 dark:text-gray-500'
              )
            }
          >
            <Icon className="w-5 h-5 shrink-0" />
            <span className="truncate text-[10px]">{label}</span>
          </NavLink>
        ))}
      </nav>
    </div>
  )
}
