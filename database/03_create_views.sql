-- ============================================================
-- TURNOSYS — Script 03: Vistas para estadísticas (MySQL 8.0+)
-- ============================================================
-- Conversiones desde SQL Server:
--   CAST(x AS DATE)         → DATE(x)
--   DATEPART(HOUR, x)       → HOUR(x)
--   DATEPART(WEEKDAY, x)    → DAYOFWEEK(x)   (1=Dom ... 7=Sab, igual que SQL Server US)
--   GETUTCDATE()            → UTC_TIMESTAMP()
--   DATEDIFF(DAY, a, b)     → DATEDIFF(b, a) (MySQL invierte argumentos, unidad días)
-- ============================================================

USE turnosys;

-- ============================================================
-- VISTA: Resumen diario de turnos
-- ============================================================
CREATE OR REPLACE VIEW vw_ResumenDiarioTurnos AS
SELECT
    t.EmpresaId,
    t.ProfesionalId,
    t.PracticaId,
    DATE(t.FechaHoraInicio)              AS Fecha,
    HOUR(t.FechaHoraInicio)              AS Hora,
    DAYOFWEEK(t.FechaHoraInicio)         AS DiaSemana,
    t.EstadoId,
    COUNT(*)                             AS CantidadTurnos,
    SUM(p.Precio)                        AS IngresoEstimado
FROM Turnos t
INNER JOIN Practicas p ON p.Id = t.PracticaId
WHERE t.IsDeleted = 0
GROUP BY
    t.EmpresaId,
    t.ProfesionalId,
    t.PracticaId,
    DATE(t.FechaHoraInicio),
    HOUR(t.FechaHoraInicio),
    DAYOFWEEK(t.FechaHoraInicio),
    t.EstadoId;

-- ============================================================
-- VISTA: Métricas por paciente (base para segmentación)
-- ============================================================
CREATE OR REPLACE VIEW vw_MetricasPacientes AS
SELECT
    t.EmpresaId,
    t.PacienteId,
    COUNT(*)                                             AS TotalTurnos,
    SUM(CASE WHEN t.EstadoId = 4 THEN 1 ELSE 0 END)     AS TurnosAtendidos,
    SUM(CASE WHEN t.EstadoId = 3 THEN 1 ELSE 0 END)     AS TurnosCancelados,
    MIN(t.FechaHoraInicio)                              AS PrimerTurno,
    MAX(t.FechaHoraInicio)                              AS UltimoTurno,
    DATEDIFF(UTC_TIMESTAMP(), MAX(t.FechaHoraInicio))   AS DiasDesdeUltimoTurno
FROM Turnos t
WHERE t.IsDeleted = 0
GROUP BY t.EmpresaId, t.PacienteId;
