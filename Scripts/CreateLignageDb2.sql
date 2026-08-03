-- Recréation de LignageDb2 avec la nouvelle structure LINE_VIS_EDG
-- PK: (LAN_UID, LIN_UID, EDG_DIR)
USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'LignageDb2')
BEGIN
    ALTER DATABASE LignageDb2 SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE LignageDb2;
END
GO

CREATE DATABASE LignageDb2;
GO

USE LignageDb2;
GO

-- LINE_VIS_EDG avec PK composite (LAN_UID, LIN_UID, EDG_DIR)
-- DTA_1-4 = données du noeud source (noeuds1)
-- EDG_1-4 = données du noeud lié (noeuds1lie)
-- TXN_TRI = transformation
CREATE TABLE LINE_VIS_EDG (
    LAN_UID VARCHAR(200) NOT NULL,
    LIN_UID VARCHAR(500) NOT NULL,
    EDG_DIR CHAR(1) NOT NULL CHECK (EDG_DIR IN ('I', 'O')),
    DTA_1 VARCHAR(1000) NULL,
    DTA_2 VARCHAR(1000) NULL,
    DTA_3 VARCHAR(MAX) NULL,
    DTA_4 VARCHAR(1000) NULL,
    EDG_1 VARCHAR(1000) NULL,
    EDG_2 VARCHAR(1000) NULL,
    EDG_3 VARCHAR(MAX) NULL,
    EDG_4 VARCHAR(1000) NULL,
    TXN_TRI VARCHAR(1000) NULL,
    CONSTRAINT PK_LINE_VIS_EDG PRIMARY KEY (LAN_UID, LIN_UID, EDG_DIR)
);
GO

-- Table2 pour les programmes (inchangée)
CREATE TABLE table2 (
    lna_uid VARCHAR(200) NOT NULL PRIMARY KEY,
    programme VARCHAR(255) NULL,
    version VARCHAR(100) NULL
);
GO

-- Index pour optimiser les recherches sur noeuds
-- Note: DTA_3 et EDG_3 sont VARCHAR(MAX), non indexables directement
CREATE INDEX IX_LINE_VIS_EDG_DTA ON LINE_VIS_EDG (DTA_1, DTA_2, DTA_4);
CREATE INDEX IX_LINE_VIS_EDG_EDG ON LINE_VIS_EDG (EDG_1, EDG_2, EDG_4);
CREATE INDEX IX_LINE_VIS_EDG_DIR ON LINE_VIS_EDG (EDG_DIR);
GO

-- Données de test
INSERT INTO table2 (lna_uid, programme, version) VALUES
('LNA001', 'ETL_Extract', '1.0.0'),
('LNA002', 'ETL_Transform', '2.1.0'),
('LNA003', 'ETL_Load', '1.5.2'),
('LNA004', 'Report_Gen', '3.0.0');
GO

-- DTA_1-4 représente le chemin complet du noeud source (ex: SERVER.DATABASE.SCHEMA.TABLE)
-- EDG_1-4 représente le chemin complet du noeud lié
INSERT INTO LINE_VIS_EDG (LAN_UID, LIN_UID, EDG_DIR, DTA_1, DTA_2, DTA_3, DTA_4, EDG_1, EDG_2, EDG_3, EDG_4, TXN_TRI) VALUES
-- Flux 1: SOURCE_A -> STAGING_A -> DIM_A -> FACT_A
('LNA001', 'LIN001', 'I', 'SRV1', 'DB_SOURCE', 'dbo', 'TABLE_A', 'SRV1', 'DB_STAGING', 'dbo', 'TABLE_A', 'Extract'),
('LNA002', 'LIN002', 'I', 'SRV1', 'DB_STAGING', 'dbo', 'TABLE_A', 'SRV1', 'DB_DW', 'dim', 'TABLE_A', 'Transform'),
('LNA003', 'LIN003', 'I', 'SRV1', 'DB_DW', 'dim', 'TABLE_A', 'SRV1', 'DB_DW', 'fact', 'TABLE_A', 'Load'),

-- Flux 2: SOURCE_B -> STAGING_B -> FACT_A
('LNA001', 'LIN004', 'I', 'SRV2', 'DB_SOURCE', 'dbo', 'TABLE_B', 'SRV1', 'DB_STAGING', 'dbo', 'TABLE_B', 'Extract'),
('LNA002', 'LIN005', 'I', 'SRV1', 'DB_STAGING', 'dbo', 'TABLE_B', 'SRV1', 'DB_DW', 'fact', 'TABLE_A', 'Join'),

-- Flux 3: FACT_A -> REPORT
('LNA004', 'LIN006', 'I', 'SRV1', 'DB_DW', 'fact', 'TABLE_A', 'SRV1', 'DB_REPORT', 'rpt', 'SALES', 'Aggregate'),

-- Quelques relations inverses (O = Output)
('LNA002', 'LIN007', 'O', 'SRV1', 'DB_DW', 'dim', 'TABLE_A', 'SRV1', 'DB_STAGING', 'dbo', 'TABLE_A', 'Reverse_Lookup'),
('LNA003', 'LIN008', 'O', 'SRV1', 'DB_DW', 'fact', 'TABLE_A', 'SRV1', 'DB_DW', 'dim', 'TABLE_A', 'Dimension_Check');
GO

PRINT 'Base de données LignageDb2 créée avec succès!';
PRINT 'Structure: PK = (LAN_UID VARCHAR(200), LIN_UID VARCHAR(500), EDG_DIR CHAR(1))';
PRINT 'DTA_1-4 = Noeud source, EDG_1-4 = Noeud lié, TXN_TRI = Transformation';
SELECT COUNT(*) AS 'Nombre de lignes dans LINE_VIS_EDG' FROM LINE_VIS_EDG;
SELECT COUNT(*) AS 'Nombre de lignes dans table2' FROM table2;
GO
