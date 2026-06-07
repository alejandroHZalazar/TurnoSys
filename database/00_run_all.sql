-- ============================================================
-- TURNOSYS — Script maestro (MySQL 8.0+)
-- ============================================================
-- Ejecutar desde el cliente mysql, parado en la carpeta database/:
--
--   mysql -h 72.61.47.240 -P 3306 -u remoto -p < 00_run_all.sql
--
-- o dentro de una sesión mysql:
--   SOURCE 01_create_database.sql;
--   ... etc
--
-- Las rutas de SOURCE son relativas al directorio donde se inició el cliente.
-- ============================================================

SOURCE 01_create_database.sql;
SOURCE 02_create_tables.sql;
SOURCE 03_create_views.sql;
SOURCE 04_stored_procedures.sql;
SOURCE 05_seed_data.sql;

SELECT '=== TurnoSys: Setup completado ===' AS Resultado;
