-- ============================================================
-- TURNOSYS — Script 06: Logotipo de empresa como BLOB
-- ============================================================
-- Agrega almacenamiento del logo dentro de la BD (LONGBLOB)
-- en lugar de (o además de) la URL externa.
-- ============================================================

USE turnosys;

ALTER TABLE Empresas
    ADD COLUMN IF NOT EXISTS LogotipoBlob        LONGBLOB     NULL,
    ADD COLUMN IF NOT EXISTS LogotipoContentType VARCHAR(100) NULL;
