-- ============================================================
-- TURNOSYS — Script 05: Datos iniciales / seed (MySQL 8.0+)
-- ============================================================
-- Idempotente: se puede ejecutar múltiples veces sin duplicar.
-- (El seeder de la app también inserta roles/parámetros/admin al arrancar.)
-- ============================================================

USE turnosys;

-- ============================================================
-- Roles del sistema (IDs fijos)
-- ============================================================
INSERT IGNORE INTO Roles (Id, Nombre, Descripcion) VALUES
    (1, 'SuperAdmin',    'Administrador global del sistema. Acceso total sin restricción de empresa.'),
    (2, 'Administrador', 'Administrador de empresa. CRUD completo dentro de su empresa.'),
    (3, 'Recepcionista', 'Gestión de turnos, pacientes y agenda. Sin acceso a parámetros.'),
    (4, 'Profesional',   'Acceso exclusivo a su propia agenda y sus pacientes.');

-- ============================================================
-- Empresa de demostración
-- ============================================================
INSERT INTO Empresas (Id, RazonSocial, NombreFantasia, Email, Telefono, TimeZone, IsActivo)
SELECT '00000000-0000-0000-0000-000000000001',
       'Empresa Demo S.R.L.', 'Clínica Demo TurnoSys',
       'demo@turnosys.com', '(011) 4000-0000',
       'America/Argentina/Buenos_Aires', 1
WHERE NOT EXISTS (
    SELECT 1 FROM Empresas WHERE Id = '00000000-0000-0000-0000-000000000001'
);

-- ============================================================
-- Usuario SuperAdmin
-- NO se inserta acá: el seeder de la app (DatabaseSeeder) lo crea
-- con un hash BCrypt válido de 'Admin1234!' al arrancar.
-- Insertarlo desde SQL con un hash placeholder rompe el login.
-- ============================================================

-- ============================================================
-- Parámetros del sistema (globales) — insert idempotente por Clave
-- ============================================================
INSERT INTO ParametrosSistema (Id, EmpresaId, Clave, Valor, TipoDato, Descripcion, IsGlobal)
SELECT UUID(), NULL, d.Clave, d.Valor, d.TipoDato, d.Descripcion, 1
FROM (
    SELECT 'TURNO_DURACION_MINUTOS'         AS Clave, '30'       AS Valor, 'int'    AS TipoDato, 'Duración default de un turno en minutos'              AS Descripcion
    UNION ALL SELECT 'TURNO_HORA_INICIO',              '08:00',  'string', 'Hora de inicio de atención por defecto'
    UNION ALL SELECT 'TURNO_HORA_FIN',                 '20:00',  'string', 'Hora de fin de atención por defecto'
    UNION ALL SELECT 'TURNO_DIAS_ATENCION',            '1,2,3,4,5','string','Días hábiles: 0=Dom,1=Lun,...,6=Sab'
    UNION ALL SELECT 'RECORDATORIO_DIAS_ANTICIPACION', '1',      'int',    'Días de anticipación para recordatorio de turno'
    UNION ALL SELECT 'RECORDATORIO_HORA_EJECUCION',    '08:00',  'string', 'Hora de ejecución del job de recordatorios (HH:mm)'
    UNION ALL SELECT 'EMAIL_PROVEEDOR',                'auto',   'string', 'Proveedor de email: resend | smtp | auto'
    UNION ALL SELECT 'EMAIL_FROM',                     '',       'string', 'Dirección de correo remitente'
    UNION ALL SELECT 'EMAIL_FROM_NAME',                'TurnoSys','string', 'Nombre del remitente en emails'
    UNION ALL SELECT 'EMAIL_TIMEOUT_SEGUNDOS',         '30',     'int',    'Timeout de envío de email en segundos'
    UNION ALL SELECT 'EMAIL_RETRY_INTENTOS',           '3',      'int',    'Cantidad de reintentos al fallar el envío'
    UNION ALL SELECT 'LOGIN_MAX_INTENTOS_FALLIDOS',    '5',      'int',    'Intentos fallidos de login antes de bloqueo temporal'
    UNION ALL SELECT 'LOGIN_BLOQUEO_MINUTOS_BASE',     '15',     'int',    'Minutos base de bloqueo por exceso de intentos fallidos'
) d
WHERE NOT EXISTS (
    SELECT 1 FROM ParametrosSistema p WHERE p.Clave = d.Clave AND p.EmpresaId IS NULL
);

-- ============================================================
-- DATOS DEMO (opcionales — comentar en producción)
-- ============================================================

-- Categorías de prácticas
INSERT INTO CategoriasPracticas (Id, EmpresaId, Nombre, Color, Orden)
SELECT * FROM (
    SELECT '00000000-0000-0000-0001-000000000001' AS Id, '00000000-0000-0000-0000-000000000001' AS EmpresaId, 'Clínica General' AS Nombre, '#3B82F6' AS Color, 1 AS Orden
    UNION ALL SELECT '00000000-0000-0000-0001-000000000002', '00000000-0000-0000-0000-000000000001', 'Estética', '#EC4899', 2
) d
WHERE NOT EXISTS (
    SELECT 1 FROM CategoriasPracticas WHERE EmpresaId = '00000000-0000-0000-0000-000000000001'
);

-- Prácticas
INSERT INTO Practicas (Id, EmpresaId, CategoriaId, Nombre, Precio, DuracionMinutos, Color, IsActivo)
SELECT UUID(), '00000000-0000-0000-0000-000000000001', d.CategoriaId, d.Nombre, d.Precio, d.Duracion, d.Color, 1
FROM (
    SELECT '00000000-0000-0000-0001-000000000001' AS CategoriaId, 'Consulta General'          AS Nombre, 3500.00  AS Precio, 30 AS Duracion, '#3B82F6' AS Color
    UNION ALL SELECT '00000000-0000-0000-0001-000000000001', 'Control Periódico',          2500.00,  20, '#60A5FA'
    UNION ALL SELECT '00000000-0000-0000-0001-000000000001', 'Urgencia',                   4000.00,  45, '#EF4444'
    UNION ALL SELECT '00000000-0000-0000-0001-000000000002', 'Limpieza Facial',            5000.00,  60, '#EC4899'
    UNION ALL SELECT '00000000-0000-0000-0001-000000000002', 'Tratamiento Antiedad',       8000.00,  90, '#F472B6'
    UNION ALL SELECT '00000000-0000-0000-0001-000000000002', 'Depilación Láser (pierna)', 12000.00,  60, '#A855F7'
) d
WHERE NOT EXISTS (
    SELECT 1 FROM Practicas WHERE EmpresaId = '00000000-0000-0000-0000-000000000001'
);

-- Profesionales
INSERT INTO Profesionales (Id, EmpresaId, Nombre, Apellido, Email, Especialidad, Matricula, ColorAgenda, IsActivo)
SELECT * FROM (
    SELECT '00000000-0000-0000-0002-000000000001' AS Id, '00000000-0000-0000-0000-000000000001' AS EmpresaId, 'Martín' AS Nombre, 'García' AS Apellido, 'garcia@clinicademo.com' AS Email, 'Médico Clínico' AS Especialidad, 'MN 12345' AS Matricula, '#3B82F6' AS ColorAgenda, 1 AS IsActivo
    UNION ALL SELECT '00000000-0000-0000-0002-000000000002', '00000000-0000-0000-0000-000000000001', 'Valentina', 'López', 'lopez@clinicademo.com', 'Cosmetóloga', NULL, '#EC4899', 1
) d
WHERE NOT EXISTS (
    SELECT 1 FROM Profesionales WHERE EmpresaId = '00000000-0000-0000-0000-000000000001'
);

-- Horarios de profesionales (Lun-Vie / con sábado para estética)
INSERT INTO HorariosProfesionales (Id, ProfesionalId, DiaSemana, HoraInicio, HoraFin, IsActivo)
SELECT UUID(), d.ProfesionalId, d.DiaSemana, d.HoraInicio, d.HoraFin, 1
FROM (
    SELECT '00000000-0000-0000-0002-000000000001' AS ProfesionalId, 1 AS DiaSemana, '08:00:00' AS HoraInicio, '18:00:00' AS HoraFin
    UNION ALL SELECT '00000000-0000-0000-0002-000000000001', 2, '08:00:00', '18:00:00'
    UNION ALL SELECT '00000000-0000-0000-0002-000000000001', 3, '08:00:00', '18:00:00'
    UNION ALL SELECT '00000000-0000-0000-0002-000000000001', 4, '08:00:00', '18:00:00'
    UNION ALL SELECT '00000000-0000-0000-0002-000000000001', 5, '08:00:00', '14:00:00'
    UNION ALL SELECT '00000000-0000-0000-0002-000000000002', 1, '09:00:00', '19:00:00'
    UNION ALL SELECT '00000000-0000-0000-0002-000000000002', 2, '09:00:00', '19:00:00'
    UNION ALL SELECT '00000000-0000-0000-0002-000000000002', 3, '09:00:00', '19:00:00'
    UNION ALL SELECT '00000000-0000-0000-0002-000000000002', 4, '09:00:00', '19:00:00'
    UNION ALL SELECT '00000000-0000-0000-0002-000000000002', 5, '09:00:00', '19:00:00'
    UNION ALL SELECT '00000000-0000-0000-0002-000000000002', 6, '09:00:00', '14:00:00'
) d
WHERE NOT EXISTS (
    SELECT 1 FROM HorariosProfesionales
    WHERE ProfesionalId IN ('00000000-0000-0000-0002-000000000001','00000000-0000-0000-0002-000000000002')
);

-- Pacientes
INSERT INTO Pacientes (Id, EmpresaId, Nombre, Apellido, DNI, Telefono, Email, FechaNacimiento, IsActivo)
SELECT UUID(), '00000000-0000-0000-0000-000000000001', d.Nombre, d.Apellido, d.DNI, d.Telefono, d.Email, d.FechaNac, 1
FROM (
    SELECT 'Juan'    AS Nombre, 'Pérez'     AS Apellido, '30000001' AS DNI, '11-1111-1111' AS Telefono, 'juan.perez@email.com'     AS Email, '1988-03-15' AS FechaNac
    UNION ALL SELECT 'María',   'González',  '32000002', '11-2222-2222', 'maria.gonzalez@email.com', '1990-07-22'
    UNION ALL SELECT 'Carlos',  'Rodríguez', '28000003', '11-3333-3333', NULL,                       '1982-11-08'
    UNION ALL SELECT 'Ana',     'Martínez',  '35000004', '11-4444-4444', 'ana.martinez@email.com',   '1995-01-30'
    UNION ALL SELECT 'Roberto', 'Sánchez',   '26000005', '11-5555-5555', NULL,                       '1975-05-12'
) d
WHERE NOT EXISTS (
    SELECT 1 FROM Pacientes WHERE EmpresaId = '00000000-0000-0000-0000-000000000001'
);
