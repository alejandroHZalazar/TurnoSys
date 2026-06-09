import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { Toaster } from 'react-hot-toast'
import AppLayout from '@/components/layout/AppLayout'
import ProtectedRoute from '@/components/ui/ProtectedRoute'
import LoginPage from '@/pages/auth/LoginPage'
import AgendaPage from '@/pages/agenda/AgendaPage'
import PacientesPage from '@/pages/pacientes/PacientesPage'
import ProfesionalesPage from '@/pages/profesionales/ProfesionalesPage'
import PracticasPage from '@/pages/practicas/PracticasPage'
import DashboardPage from '@/pages/dashboard/DashboardPage'
import EmpresaPage from '@/pages/empresa/EmpresaPage'
import ConfiguracionPage from '@/pages/configuracion/ConfiguracionPage'
import UsuariosPage from '@/pages/usuarios/UsuariosPage'
import RolesPage from '@/pages/roles/RolesPage'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
})

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <AppLayout />
              </ProtectedRoute>
            }
          >
            <Route index element={<Navigate to="/agenda" replace />} />
            <Route path="agenda" element={<AgendaPage />} />
            <Route path="pacientes" element={<PacientesPage />} />
            <Route path="profesionales" element={<ProfesionalesPage />} />
            <Route path="practicas" element={<PracticasPage />} />
            <Route path="dashboard" element={<DashboardPage />} />
            <Route path="empresa" element={<EmpresaPage />} />
            <Route path="configuracion" element={<ConfiguracionPage />} />
            <Route path="usuarios" element={<UsuariosPage />} />
            <Route path="roles" element={<RolesPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/agenda" replace />} />
        </Routes>
      </BrowserRouter>
      <Toaster position="top-right" />
    </QueryClientProvider>
  )
}
