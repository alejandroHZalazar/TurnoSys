-- ============================================================
-- TURNOSYS — Script 07: Gestión de Usuarios / Roles / Permisos
-- MySQL 8.x
-- ============================================================
-- Ejecutar sobre la BD existente (no destructivo).
-- Agrega columnas nuevas y actualiza datos de roles.
-- ============================================================

USE turnosys;

-- ============================================================
-- 1. Roles: agregar columna Permisos
-- ============================================================

SET @sql = (
    SELECT IF(
        EXISTS(
            SELECT 1
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'Roles'
              AND COLUMN_NAME = 'Permisos'
        ),
        'SELECT "Columna Permisos ya existe";',
        '
        ALTER TABLE Roles
        ADD COLUMN Permisos LONGTEXT NULL
        COMMENT ''JSON array de permisos. NULL = acceso total (SuperAdmin).'';
        '
    )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;


-- ============================================================
-- 2. Usuarios: agregar columna ProfesionalId
-- ============================================================

SET @sql = (
    SELECT IF(
        EXISTS(
            SELECT 1
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'Usuarios'
              AND COLUMN_NAME = 'ProfesionalId'
        ),
        'SELECT "Columna ProfesionalId ya existe";',
        '
        ALTER TABLE Usuarios
        ADD COLUMN ProfesionalId CHAR(36) NULL
        COMMENT ''Profesional asociado al usuario (opcional)'';
        '
    )
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;


-- ============================================================
-- 3. Agregar FK si no existe
-- ============================================================

SET @fk_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Usuarios'
      AND CONSTRAINT_NAME = 'FK_Usuarios_Profesionales'
);

SET @sql = IF(
    @fk_exists > 0,
    'SELECT "FK_Usuarios_Profesionales ya existe";',
    '
    ALTER TABLE Usuarios
    ADD CONSTRAINT FK_Usuarios_Profesionales
    FOREIGN KEY (ProfesionalId)
    REFERENCES Profesionales(Id)
    ON DELETE SET NULL;
    '
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;


-- ============================================================
-- 4. Actualizar permisos de roles existentes
-- ============================================================

-- SuperAdmin: acceso total (NULL)
UPDATE Roles
SET Permisos = NULL
WHERE Id = 1;


-- Administrador
UPDATE Roles
SET Permisos =
'[
"agenda.ver",
"agenda.crear",
"agenda.editar",
"agenda.cancelar",
"pacientes.ver",
"pacientes.crear",
"pacientes.editar",
"pacientes.eliminar",
"profesionales.ver",
"profesionales.crear",
"profesionales.editar",
"profesionales.eliminar",
"practicas.ver",
"practicas.crear",
"practicas.editar",
"practicas.eliminar",
"dashboard.ver",
"empresa.ver",
"empresa.editar",
"configuracion.ver",
"configuracion.editar",
"usuarios.ver",
"usuarios.crear",
"usuarios.editar",
"usuarios.desactivar"
]'
WHERE Id = 2;


-- Recepcionista
UPDATE Roles
SET Permisos =
'[
"agenda.ver",
"agenda.crear",
"agenda.editar",
"agenda.cancelar",
"pacientes.ver",
"pacientes.crear",
"pacientes.editar",
"profesionales.ver",
"practicas.ver",
"dashboard.ver"
]'
WHERE Id = 3;


-- Profesional
UPDATE Roles
SET Permisos =
'[
"agenda.ver",
"pacientes.ver",
"dashboard.ver"
]'
WHERE Id = 4;


-- ============================================================
-- 5. Verificación
-- ============================================================

SELECT
    Id,
    Nombre,
    LEFT(Permisos, 60) AS PermisosPreview
FROM Roles;


SELECT 'Script 07 ejecutado correctamente.' AS Resultado;