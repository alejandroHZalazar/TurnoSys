-- ============================================================
-- TURNOSYS — Script 04: Stored Procedures (MySQL 8.0+)
-- ============================================================
-- SPs MANTENIDOS:
--   sp_GetDashboardKPIs            → dos resultsets en un roundtrip
--   sp_ConsolidarMetricasMensuales → consolidado mensual idempotente (DELETE+INSERT)
--
-- Nota: en MySQL el MERGE de SQL Server no existe; se reemplaza por
-- DELETE de las filas del mes + INSERT fresco (idempotente y sin
-- problemas con el ProfesionalId NULL en índices UNIQUE).
-- ============================================================

USE turnosys;

DROP PROCEDURE IF EXISTS sp_GetDashboardKPIs;
DROP PROCEDURE IF EXISTS sp_ConsolidarMetricasMensuales;

DELIMITER $$

-- ============================================================
-- SP: Dashboard KPIs consolidados
--
-- Llamar desde C# con Dapper (QueryMultipleAsync):
--   using var multi = await conn.QueryMultipleAsync(
--       "CALL sp_GetDashboardKPIs(@EmpresaId, @Desde, @Hasta)",
--       new { EmpresaId, Desde, Hasta });
--   var kpis   = await multi.ReadSingleAsync<KpisDto>();
--   var nuevos = await multi.ReadSingleAsync<PacientesNuevosDto>();
-- ============================================================
CREATE PROCEDURE sp_GetDashboardKPIs(
    IN p_EmpresaId CHAR(36),
    IN p_Desde     DATE,
    IN p_Hasta     DATE
)
BEGIN
    -- Resultset 1: KPIs de turnos del período
    SELECT
        COUNT(*)                                                       AS TotalTurnos,
        SUM(CASE WHEN t.EstadoId = 4 THEN 1 ELSE 0 END)              AS Atendidos,
        SUM(CASE WHEN t.EstadoId = 3 THEN 1 ELSE 0 END)              AS Cancelados,
        SUM(CASE WHEN t.EstadoId = 2 THEN 1 ELSE 0 END)              AS Reservados,
        SUM(CASE WHEN t.EstadoId IN (2,4) THEN p.Precio ELSE 0 END)  AS IngresoEstimado,
        CASE
            WHEN COUNT(*) = 0 THEN 0.0
            ELSE CAST(SUM(CASE WHEN t.EstadoId IN (2,4) THEN 1 ELSE 0 END) AS DECIMAL(10,2))
                 / COUNT(*) * 100
        END                                                           AS PorcentajeOcupacion
    FROM Turnos t
    INNER JOIN Practicas p ON p.Id = t.PracticaId
    WHERE
        t.EmpresaId = p_EmpresaId
        AND DATE(t.FechaHoraInicio) BETWEEN p_Desde AND p_Hasta
        AND t.IsDeleted = 0;

    -- Resultset 2: Pacientes nuevos (alta dentro del período)
    SELECT COUNT(DISTINCT t.PacienteId) AS PacientesNuevos
    FROM Turnos t
    INNER JOIN Pacientes pa ON pa.Id = t.PacienteId
    WHERE
        t.EmpresaId = p_EmpresaId
        AND DATE(t.FechaHoraInicio) BETWEEN p_Desde AND p_Hasta
        AND t.IsDeleted = 0
        AND pa.CreatedAt >= p_Desde;
END$$

-- ============================================================
-- SP: Consolidar métricas mensuales (job nocturno)
--
-- Llamar desde C# con ExecuteSqlRawAsync:
--   await db.Database.ExecuteSqlRawAsync(
--       "CALL sp_ConsolidarMetricasMensuales({0}, {1})", anio, mes);
--
-- Pasar NULL en ambos parámetros para consolidar el mes anterior.
-- ============================================================
CREATE PROCEDURE sp_ConsolidarMetricasMensuales(
    IN p_Anio INT,
    IN p_Mes  INT
)
BEGIN
    DECLARE v_Desde DATE;
    DECLARE v_Hasta DATE;

    IF p_Anio IS NULL THEN SET p_Anio = YEAR(DATE_SUB(UTC_TIMESTAMP(), INTERVAL 1 MONTH)); END IF;
    IF p_Mes  IS NULL THEN SET p_Mes  = MONTH(DATE_SUB(UTC_TIMESTAMP(), INTERVAL 1 MONTH)); END IF;

    SET v_Desde = STR_TO_DATE(CONCAT(p_Anio, '-', p_Mes, '-01'), '%Y-%m-%d');
    SET v_Hasta = LAST_DAY(v_Desde);

    -- Idempotencia: borrar lo previo del mes y reinsertar
    DELETE FROM MetricasMensuales WHERE Anio = p_Anio AND Mes = p_Mes;

    -- Consolidado total por empresa (ProfesionalId NULL)
    INSERT INTO MetricasMensuales
        (Id, EmpresaId, Anio, Mes, ProfesionalId, TotalTurnos, TurnosAtendidos, TurnosCancelados, IngresoEstimado)
    SELECT
        UUID(),
        t.EmpresaId, p_Anio, p_Mes, NULL,
        COUNT(*),
        SUM(CASE WHEN t.EstadoId = 4 THEN 1 ELSE 0 END),
        SUM(CASE WHEN t.EstadoId = 3 THEN 1 ELSE 0 END),
        SUM(CASE WHEN t.EstadoId IN (2,4) THEN p.Precio ELSE 0 END)
    FROM Turnos t
    INNER JOIN Practicas p ON p.Id = t.PracticaId
    WHERE t.IsDeleted = 0
      AND DATE(t.FechaHoraInicio) BETWEEN v_Desde AND v_Hasta
    GROUP BY t.EmpresaId;

    -- Consolidado por profesional
    INSERT INTO MetricasMensuales
        (Id, EmpresaId, Anio, Mes, ProfesionalId, TotalTurnos, TurnosAtendidos, TurnosCancelados, IngresoEstimado)
    SELECT
        UUID(),
        t.EmpresaId, p_Anio, p_Mes, t.ProfesionalId,
        COUNT(*),
        SUM(CASE WHEN t.EstadoId = 4 THEN 1 ELSE 0 END),
        SUM(CASE WHEN t.EstadoId = 3 THEN 1 ELSE 0 END),
        SUM(CASE WHEN t.EstadoId IN (2,4) THEN p.Precio ELSE 0 END)
    FROM Turnos t
    INNER JOIN Practicas p ON p.Id = t.PracticaId
    WHERE t.IsDeleted = 0
      AND DATE(t.FechaHoraInicio) BETWEEN v_Desde AND v_Hasta
    GROUP BY t.EmpresaId, t.ProfesionalId;
END$$

DELIMITER ;
