-- ============================================================
-- TURNOSYS — Script 07: Gestión de Usuarios / Roles / Permisos
-- ============================================================
-- Ejecutar sobre la BD existente (no destructivo).
-- Agrega columnas nuevas y actualiza datos de roles.
-- ============================================================

USE turnosys;

-- ── 1. Roles: agregar columna Permisos ──────────────────────
ALTER TABLE Roles
    ADD COLUMN IF NOT EXISTS Permisos LONGTEXT NULL
        COMMENT 'JSON array de permisos. NULL = acceso total (SuperAdmin).';

-- ── 2. Usuarios: agregar columna ProfesionalId ──────────────
ALTER TABLE Usuarios
    ADD COLUMN IF NOT EXISTS ProfesionalId CHAR(36) NULL
        COMMENT 'Profesional asociado al usuario (opcional)',
    ADD CONSTRAINT IF NOT EXISTS FK_Usuarios_Profesionales
        FOREIGN KEY (ProfesionalId) REFERENCES Profesionales(Id) ON DELETE SET NULL;

-- ── 3. Actualizar permisos de roles existentes ───────────────

-- SuperAdmin: acceso total (NULL)
UPDATE Roles SET Permisos = NULL WHERE Id = 1;

-- Administrador
UPDATE Roles SET Permisos = '["agenda.ver","agenda.crear","agenda.editar","agenda.cancelar","pacientes.ver","pacientes.crear","pacientes.editar","pacientes.eliminar","profesionales.ver","profesionales.crear","profesionales.editar","profesionales.eliminar","practicas.ver","practicas.crear","practicas.editar","practicas.eliminar","dashboard.ver","empresa.ver","empresa.editar","configuracion.ver","configuracion.editar","usuarios.ver","usuarios.crear","usuarios.editar","usuarios.desactivar"]'
WHERE Id = 2;

-- Recepcionista
UPDATE Roles SET Permisos = '["agenda.ver","agenda.crear","agenda.editar","agenda.cancelar","pacientes.ver","pacientes.crear","pacientes.editar","profesionales.ver","practicas.ver","dashboard.ver"]'
WHERE Id = 3;

-- Profesional
UPDATE Roles SET Permisos = '["agenda.ver","pacientes.ver","dashboard.ver"]'
WHERE Id = 4;

-- ── 4. Verificar ─────────────────────────────────────────────
SELECT Id, Nombre, LEFT(Permisos, 60) AS PermisosPreview FROM Roles;
