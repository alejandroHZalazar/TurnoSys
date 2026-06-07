-- ============================================================
-- TURNOSYS — Script 02: Creación de tablas (MySQL 8.0+)
-- ============================================================
-- Conversiones desde SQL Server:
--   UNIQUEIDENTIFIER → CHAR(36)        (Pomelo mapea Guid a char(36))
--   NVARCHAR(n)      → VARCHAR(n)
--   NVARCHAR(MAX)    → LONGTEXT
--   BIT              → TINYINT(1)
--   DATETIME2        → DATETIME(6)
--   TIME             → TIME(6)
--   GETUTCDATE()     → UTC_TIMESTAMP(6)
--   IDENTITY(1,1)    → AUTO_INCREMENT
--   Índices filtrados (WHERE) e INCLUDE → no existen en MySQL, se omiten
-- ============================================================

USE turnosys;

-- ============================================================
-- TABLA: Empresas
-- ============================================================
CREATE TABLE IF NOT EXISTS Empresas (
    Id              CHAR(36)        NOT NULL DEFAULT (UUID()),
    RazonSocial     VARCHAR(200)    NOT NULL,
    NombreFantasia  VARCHAR(200)    NULL,
    CUIT            VARCHAR(20)     NULL,
    Direccion       VARCHAR(300)    NULL,
    Telefono        VARCHAR(50)     NULL,
    Email           VARCHAR(200)    NULL,
    LogotipoUrl     VARCHAR(500)    NULL,
    SitioWeb        VARCHAR(300)    NULL,
    Instagram       VARCHAR(200)    NULL,
    Facebook        VARCHAR(200)    NULL,
    WhatsApp        VARCHAR(50)     NULL,
    HorarioDesde    TIME(6)         NULL,
    HorarioHasta    TIME(6)         NULL,
    TimeZone        VARCHAR(100)    NOT NULL DEFAULT 'America/Argentina/Buenos_Aires',
    Observaciones   LONGTEXT        NULL,
    IsActivo        TINYINT(1)      NOT NULL DEFAULT 1,
    CreatedAt       DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    UpdatedAt       DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    IsDeleted       TINYINT(1)      NOT NULL DEFAULT 0,
    CONSTRAINT PK_Empresas PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABLA: Roles  (IDs fijos 1-4, no autoincremental)
-- ============================================================
CREATE TABLE IF NOT EXISTS Roles (
    Id          INT             NOT NULL,
    Nombre      VARCHAR(50)     NOT NULL,
    Descripcion VARCHAR(300)    NULL,
    CONSTRAINT PK_Roles PRIMARY KEY (Id),
    CONSTRAINT UQ_Roles_Nombre UNIQUE (Nombre)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABLA: Usuarios
-- ============================================================
CREATE TABLE IF NOT EXISTS Usuarios (
    Id               CHAR(36)       NOT NULL DEFAULT (UUID()),
    EmpresaId        CHAR(36)       NULL,
    RolId            INT            NOT NULL,
    Email            VARCHAR(200)   NOT NULL,
    PasswordHash     VARCHAR(500)   NOT NULL,
    NombreCompleto   VARCHAR(200)   NOT NULL,
    IsActivo         TINYINT(1)     NOT NULL DEFAULT 1,
    UltimoAcceso     DATETIME(6)    NULL,
    IntentosFallidos INT            NOT NULL DEFAULT 0,
    BloqueadoHasta   DATETIME(6)    NULL,
    CreatedAt        DATETIME(6)    NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    UpdatedAt        DATETIME(6)    NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    IsDeleted        TINYINT(1)     NOT NULL DEFAULT 0,
    CONSTRAINT PK_Usuarios PRIMARY KEY (Id),
    CONSTRAINT UQ_Usuarios_Email UNIQUE (Email),
    CONSTRAINT FK_Usuarios_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id),
    CONSTRAINT FK_Usuarios_Roles    FOREIGN KEY (RolId)     REFERENCES Roles(Id),
    INDEX IX_Usuarios_Email (Email),
    INDEX IX_Usuarios_EmpresaId (EmpresaId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABLA: RefreshTokens
-- ============================================================
CREATE TABLE IF NOT EXISTS RefreshTokens (
    Id              CHAR(36)        NOT NULL DEFAULT (UUID()),
    UsuarioId       CHAR(36)        NOT NULL,
    TokenHash       VARCHAR(500)    NOT NULL,
    Expiracion      DATETIME(6)     NOT NULL,
    IsRevocado      TINYINT(1)      NOT NULL DEFAULT 0,
    FechaRevocacion DATETIME(6)     NULL,
    IpOrigen        VARCHAR(50)     NULL,
    UserAgent       VARCHAR(500)    NULL,
    CreatedAt       DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    UpdatedAt       DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    IsDeleted       TINYINT(1)      NOT NULL DEFAULT 0,
    CONSTRAINT PK_RefreshTokens PRIMARY KEY (Id),
    CONSTRAINT FK_RefreshTokens_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE,
    INDEX IX_RefreshTokens_UsuarioId (UsuarioId),
    INDEX IX_RefreshTokens_TokenHash (TokenHash)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABLA: Profesionales
-- ============================================================
CREATE TABLE IF NOT EXISTS Profesionales (
    Id              CHAR(36)        NOT NULL DEFAULT (UUID()),
    EmpresaId       CHAR(36)        NOT NULL,
    Nombre          VARCHAR(100)    NOT NULL,
    Apellido        VARCHAR(100)    NOT NULL,
    Email           VARCHAR(200)    NULL,
    Telefono        VARCHAR(50)     NULL,
    Especialidad    VARCHAR(200)    NULL,
    Matricula       VARCHAR(100)    NULL,
    ColorAgenda     VARCHAR(7)      NOT NULL DEFAULT '#4F46E5',
    FotoUrl         VARCHAR(500)    NULL,
    IsActivo        TINYINT(1)      NOT NULL DEFAULT 1,
    Observaciones   LONGTEXT        NULL,
    CreatedAt       DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    UpdatedAt       DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    IsDeleted       TINYINT(1)      NOT NULL DEFAULT 0,
    CreatedBy       CHAR(36)        NULL,
    UpdatedBy       CHAR(36)        NULL,
    CONSTRAINT PK_Profesionales PRIMARY KEY (Id),
    CONSTRAINT FK_Profesionales_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id),
    INDEX IX_Profesionales_EmpresaId (EmpresaId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABLA: HorariosProfesionales
-- ============================================================
CREATE TABLE IF NOT EXISTS HorariosProfesionales (
    Id              CHAR(36)        NOT NULL DEFAULT (UUID()),
    ProfesionalId   CHAR(36)        NOT NULL,
    DiaSemana       TINYINT         NOT NULL,   -- 0=Dom, 1=Lun, ..., 6=Sab
    HoraInicio      TIME(6)         NOT NULL,
    HoraFin         TIME(6)         NOT NULL,
    IsActivo        TINYINT(1)      NOT NULL DEFAULT 1,
    VigenciaDesde   DATE            NULL,
    VigenciaHasta   DATE            NULL,
    CreatedAt       DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    UpdatedAt       DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    IsDeleted       TINYINT(1)      NOT NULL DEFAULT 0,
    CONSTRAINT PK_HorariosProfesionales PRIMARY KEY (Id),
    CONSTRAINT FK_HorariosProfesionales_Prof FOREIGN KEY (ProfesionalId) REFERENCES Profesionales(Id) ON DELETE CASCADE,
    CONSTRAINT CK_HorariosProfesionales_DiaSemana CHECK (DiaSemana BETWEEN 0 AND 6),
    CONSTRAINT CK_HorariosProfesionales_Horas CHECK (HoraFin > HoraInicio),
    INDEX IX_HorariosProfesionales_Prof (ProfesionalId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABLA: BloqueosHorarios
-- ============================================================
CREATE TABLE IF NOT EXISTS BloqueosHorarios (
    Id              CHAR(36)        NOT NULL DEFAULT (UUID()),
    ProfesionalId   CHAR(36)        NOT NULL,
    FechaDesde      DATETIME(6)     NOT NULL,
    FechaHasta      DATETIME(6)     NOT NULL,
    Motivo          VARCHAR(50)     NOT NULL DEFAULT 'Otro',
    Observaciones   VARCHAR(500)    NULL,
    CreatedAt       DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    UpdatedAt       DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    IsDeleted       TINYINT(1)      NOT NULL DEFAULT 0,
    CONSTRAINT PK_BloqueosHorarios PRIMARY KEY (Id),
    CONSTRAINT FK_BloqueosHorarios_Prof FOREIGN KEY (ProfesionalId) REFERENCES Profesionales(Id),
    CONSTRAINT CK_BloqueosHorarios_Motivo CHECK (Motivo IN ('Vacaciones','Licencia','Feriado','Capacitacion','Otro')),
    CONSTRAINT CK_BloqueosHorarios_Fechas CHECK (FechaHasta > FechaDesde),
    INDEX IX_BloqueosHorarios_Prof_Fecha (ProfesionalId, FechaDesde, FechaHasta)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABLA: CategoriasPracticas
-- ============================================================
CREATE TABLE IF NOT EXISTS CategoriasPracticas (
    Id          CHAR(36)        NOT NULL DEFAULT (UUID()),
    EmpresaId   CHAR(36)        NOT NULL,
    Nombre      VARCHAR(100)    NOT NULL,
    Color       VARCHAR(7)      NULL,
    Orden       INT             NOT NULL DEFAULT 0,
    CreatedAt   DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    UpdatedAt   DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    IsDeleted   TINYINT(1)      NOT NULL DEFAULT 0,
    CONSTRAINT PK_CategoriasPracticas PRIMARY KEY (Id),
    CONSTRAINT FK_CategoriasPracticas_Emp FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABLA: Practicas
-- ============================================================
CREATE TABLE IF NOT EXISTS Practicas (
    Id                    CHAR(36)       NOT NULL DEFAULT (UUID()),
    EmpresaId             CHAR(36)       NOT NULL,
    CategoriaId           CHAR(36)       NULL,
    Nombre                VARCHAR(200)   NOT NULL,
    Descripcion           LONGTEXT       NULL,
    Precio                DECIMAL(18,2)  NOT NULL DEFAULT 0,
    DuracionMinutos       INT            NOT NULL DEFAULT 30,
    Color                 VARCHAR(7)     NULL,
    RequiereObservaciones TINYINT(1)     NOT NULL DEFAULT 0,
    RecordatorioRecDias   INT            NULL,
    IsActivo              TINYINT(1)     NOT NULL DEFAULT 1,
    CreatedAt             DATETIME(6)    NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    UpdatedAt             DATETIME(6)    NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    IsDeleted             TINYINT(1)     NOT NULL DEFAULT 0,
    CONSTRAINT PK_Practicas PRIMARY KEY (Id),
    CONSTRAINT FK_Practicas_Empresas   FOREIGN KEY (EmpresaId)   REFERENCES Empresas(Id),
    CONSTRAINT FK_Practicas_Categorias FOREIGN KEY (CategoriaId) REFERENCES CategoriasPracticas(Id) ON DELETE SET NULL,
    CONSTRAINT CK_Practicas_Duracion CHECK (DuracionMinutos > 0),
    INDEX IX_Practicas_EmpresaId (EmpresaId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABLA: ProfesionalesPracticas  (M:N)
-- ============================================================
CREATE TABLE IF NOT EXISTS ProfesionalesPracticas (
    ProfesionalId   CHAR(36)    NOT NULL,
    PracticaId      CHAR(36)    NOT NULL,
    CONSTRAINT PK_ProfesionalesPracticas PRIMARY KEY (ProfesionalId, PracticaId),
    CONSTRAINT FK_PP_Profesional FOREIGN KEY (ProfesionalId) REFERENCES Profesionales(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PP_Practica    FOREIGN KEY (PracticaId)    REFERENCES Practicas(Id)      ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABLA: Pacientes
-- ============================================================
CREATE TABLE IF NOT EXISTS Pacientes (
    Id                    CHAR(36)        NOT NULL DEFAULT (UUID()),
    EmpresaId             CHAR(36)        NOT NULL,
    Nombre                VARCHAR(100)    NOT NULL,
    Apellido              VARCHAR(100)    NOT NULL,
    DNI                   VARCHAR(20)     NULL,
    FechaNacimiento       DATE            NULL,
    Telefono              VARCHAR(50)     NULL,
    Email                 VARCHAR(200)    NULL,
    Direccion             VARCHAR(300)    NULL,
    ObraSocial            VARCHAR(200)    NULL,
    NumeroAfiliado        VARCHAR(100)    NULL,
    ContactoEmergNombre   VARCHAR(200)    NULL,
    ContactoEmergTelefono VARCHAR(50)     NULL,
    Observaciones         LONGTEXT        NULL,
    Restricciones         LONGTEXT        NULL,
    ConsentimientoFirmado TINYINT(1)      NOT NULL DEFAULT 0,
    FechaConsentimiento   DATETIME(6)     NULL,
    IsActivo              TINYINT(1)      NOT NULL DEFAULT 1,
    CreatedAt             DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    UpdatedAt             DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    IsDeleted             TINYINT(1)      NOT NULL DEFAULT 0,
    CreatedBy             CHAR(36)        NULL,
    UpdatedBy             CHAR(36)        NULL,
    CONSTRAINT PK_Pacientes PRIMARY KEY (Id),
    CONSTRAINT FK_Pacientes_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id),
    INDEX IX_Pacientes_EmpresaId (EmpresaId),
    INDEX IX_Pacientes_DNI (EmpresaId, DNI),
    INDEX IX_Pacientes_Nombre (EmpresaId, Apellido, Nombre)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABLA: Turnos
-- ============================================================
CREATE TABLE IF NOT EXISTS Turnos (
    Id                         CHAR(36)     NOT NULL DEFAULT (UUID()),
    EmpresaId                  CHAR(36)     NOT NULL,
    ProfesionalId              CHAR(36)     NOT NULL,
    PacienteId                 CHAR(36)     NOT NULL,
    PracticaId                 CHAR(36)     NOT NULL,
    FechaHoraInicio            DATETIME(6)  NOT NULL,
    FechaHoraFin               DATETIME(6)  NOT NULL,
    EstadoId                   INT          NOT NULL DEFAULT 2,  -- 2 = Reservado
    Observaciones              LONGTEXT     NULL,
    MotivoCancelacion          VARCHAR(500) NULL,
    ProximoControlFecha        DATE         NULL,
    RecordatorioTurnoEnviado   TINYINT(1)   NOT NULL DEFAULT 0,
    RecordatorioControlEnviado TINYINT(1)   NOT NULL DEFAULT 0,
    CreatedAt                  DATETIME(6)  NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    UpdatedAt                  DATETIME(6)  NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    IsDeleted                  TINYINT(1)   NOT NULL DEFAULT 0,
    CreatedBy                  CHAR(36)     NULL,
    UpdatedBy                  CHAR(36)     NULL,
    CONSTRAINT PK_Turnos PRIMARY KEY (Id),
    CONSTRAINT FK_Turnos_Empresas      FOREIGN KEY (EmpresaId)     REFERENCES Empresas(Id),
    CONSTRAINT FK_Turnos_Profesionales FOREIGN KEY (ProfesionalId) REFERENCES Profesionales(Id),
    CONSTRAINT FK_Turnos_Pacientes     FOREIGN KEY (PacienteId)    REFERENCES Pacientes(Id),
    CONSTRAINT FK_Turnos_Practicas     FOREIGN KEY (PracticaId)    REFERENCES Practicas(Id),
    CONSTRAINT CK_Turnos_Fechas CHECK (FechaHoraFin > FechaHoraInicio),
    CONSTRAINT CK_Turnos_Estado CHECK (EstadoId IN (1, 2, 3, 4)),
    INDEX IX_Turnos_Profesional_Fecha (ProfesionalId, FechaHoraInicio, FechaHoraFin),
    INDEX IX_Turnos_Empresa_Fecha (EmpresaId, FechaHoraInicio),
    INDEX IX_Turnos_Paciente (PacienteId),
    INDEX IX_Turnos_RecordatorioPendiente (FechaHoraInicio, RecordatorioTurnoEnviado),
    INDEX IX_Turnos_Stats (EmpresaId, FechaHoraInicio, EstadoId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABLA: RecordatoriosControl
-- ============================================================
CREATE TABLE IF NOT EXISTS RecordatoriosControl (
    Id                      CHAR(36)     NOT NULL DEFAULT (UUID()),
    TurnoOrigenId           CHAR(36)     NOT NULL,
    ProfesionalId           CHAR(36)     NOT NULL,
    PacienteId              CHAR(36)     NOT NULL,
    FechaControlSugerida    DATE         NOT NULL,
    FechaRecordatorioEnviar DATE         NOT NULL,
    Estado                  VARCHAR(20)  NOT NULL DEFAULT 'Pendiente',
    FechaEnvioReal          DATETIME(6)  NULL,
    IntentoEnvio            INT          NOT NULL DEFAULT 0,
    CreatedAt               DATETIME(6)  NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    CONSTRAINT PK_RecordatoriosControl PRIMARY KEY (Id),
    CONSTRAINT FK_RC_Turno       FOREIGN KEY (TurnoOrigenId) REFERENCES Turnos(Id),
    CONSTRAINT FK_RC_Profesional FOREIGN KEY (ProfesionalId) REFERENCES Profesionales(Id),
    CONSTRAINT FK_RC_Paciente    FOREIGN KEY (PacienteId)    REFERENCES Pacientes(Id),
    CONSTRAINT CK_RC_Estado CHECK (Estado IN ('Pendiente','Enviado','Cancelado')),
    INDEX IX_RC_Pendientes (FechaRecordatorioEnviar, Estado)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABLA: ParametrosSistema
-- ============================================================
CREATE TABLE IF NOT EXISTS ParametrosSistema (
    Id           CHAR(36)       NOT NULL DEFAULT (UUID()),
    EmpresaId    CHAR(36)       NULL,   -- NULL = parámetro global del sistema
    Clave        VARCHAR(100)   NOT NULL,
    Valor        LONGTEXT       NULL,
    TipoDato     VARCHAR(20)    NOT NULL DEFAULT 'string',
    Descripcion  VARCHAR(500)   NULL,
    IsEncriptado TINYINT(1)     NOT NULL DEFAULT 0,
    IsGlobal     TINYINT(1)     NOT NULL DEFAULT 0,
    UpdatedAt    DATETIME(6)    NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    UpdatedBy    CHAR(36)       NULL,
    CONSTRAINT PK_ParametrosSistema PRIMARY KEY (Id),
    CONSTRAINT FK_Parametros_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id),
    CONSTRAINT UQ_Parametros_Clave UNIQUE (EmpresaId, Clave),
    CONSTRAINT CK_Parametros_TipoDato CHECK (TipoDato IN ('string','int','bool','json','decimal'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
-- NOTA: en MySQL los NULL son distintos en índices UNIQUE, por lo que la unicidad
-- de parámetros globales (EmpresaId IS NULL) la garantiza el seeder a nivel app.

-- ============================================================
-- TABLA: Notificaciones  (in-app)
-- ============================================================
CREATE TABLE IF NOT EXISTS Notificaciones (
    Id          CHAR(36)        NOT NULL DEFAULT (UUID()),
    UsuarioId   CHAR(36)        NOT NULL,
    Titulo      VARCHAR(200)    NOT NULL,
    Mensaje     LONGTEXT        NOT NULL,
    Tipo        VARCHAR(50)     NOT NULL DEFAULT 'info',
    IsLeida     TINYINT(1)      NOT NULL DEFAULT 0,
    Url         VARCHAR(500)    NULL,
    CreatedAt   DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    UpdatedAt   DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    IsDeleted   TINYINT(1)      NOT NULL DEFAULT 0,
    CONSTRAINT PK_Notificaciones PRIMARY KEY (Id),
    CONSTRAINT FK_Notificaciones_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE,
    CONSTRAINT CK_Notificaciones_Tipo CHECK (Tipo IN ('info','warning','error','recordatorio','success')),
    INDEX IX_Notificaciones_Usuario_NoLeidas (UsuarioId, IsLeida)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABLA: AuditLog
-- ============================================================
CREATE TABLE IF NOT EXISTS AuditLog (
    Id                BIGINT          NOT NULL AUTO_INCREMENT,
    EmpresaId         CHAR(36)        NULL,
    UsuarioId         CHAR(36)        NULL,
    Accion            VARCHAR(50)     NOT NULL,
    Entidad           VARCHAR(100)    NOT NULL,
    EntidadId         VARCHAR(100)    NULL,
    ValoresAnteriores LONGTEXT        NULL,
    ValoresNuevos     LONGTEXT        NULL,
    IpAddress         VARCHAR(50)     NULL,
    UserAgent         VARCHAR(500)    NULL,
    FechaHora         DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    CONSTRAINT PK_AuditLog PRIMARY KEY (Id),
    INDEX IX_AuditLog_Empresa_Fecha (EmpresaId, FechaHora),
    INDEX IX_AuditLog_Entidad (Entidad, EntidadId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABLA: SegmentosPacientes  (poblada por job semanal)
-- ============================================================
CREATE TABLE IF NOT EXISTS SegmentosPacientes (
    PacienteId      CHAR(36)        NOT NULL,
    EmpresaId       CHAR(36)        NOT NULL,
    Segmento        VARCHAR(50)     NOT NULL,
    FechaCalculo    DATETIME(6)     NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    CONSTRAINT PK_SegmentosPacientes PRIMARY KEY (PacienteId, EmpresaId),
    CONSTRAINT FK_SP_Paciente FOREIGN KEY (PacienteId) REFERENCES Pacientes(Id),
    CONSTRAINT FK_SP_Empresa  FOREIGN KEY (EmpresaId)  REFERENCES Empresas(Id),
    CONSTRAINT CK_SP_Segmento CHECK (Segmento IN ('Frecuente','Inactivo','EnRiesgo','Nuevo','VIP','Regular'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABLA: MetricasMensuales  (consolidado nocturno para estadísticas)
-- ============================================================
CREATE TABLE IF NOT EXISTS MetricasMensuales (
    Id               CHAR(36)       NOT NULL DEFAULT (UUID()),
    EmpresaId        CHAR(36)       NOT NULL,
    Anio             INT            NOT NULL,
    Mes              INT            NOT NULL,
    ProfesionalId    CHAR(36)       NULL,
    TotalTurnos      INT            NOT NULL DEFAULT 0,
    TurnosAtendidos  INT            NOT NULL DEFAULT 0,
    TurnosCancelados INT            NOT NULL DEFAULT 0,
    IngresoEstimado  DECIMAL(18,2)  NOT NULL DEFAULT 0,
    FechaCalculo     DATETIME(6)    NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    CONSTRAINT PK_MetricasMensuales PRIMARY KEY (Id),
    CONSTRAINT FK_MM_Empresas    FOREIGN KEY (EmpresaId)     REFERENCES Empresas(Id),
    CONSTRAINT FK_MM_Profesional FOREIGN KEY (ProfesionalId) REFERENCES Profesionales(Id),
    CONSTRAINT CK_MM_Mes CHECK (Mes BETWEEN 1 AND 12),
    INDEX IX_MetricasMensuales_Empresa (EmpresaId, Anio, Mes)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
