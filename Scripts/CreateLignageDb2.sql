-- Recréation de LignageDb2 avec la structure correcte
-- PK: (LNA_UID, LIN_UID, EDG_DIR)
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

-- Table1 avec PK composite (LNA_UID, LIN_UID, EDG_DIR)
CREATE TABLE table1 (
    lna_uid VARCHAR(200) NOT NULL,
    lin_uid VARCHAR(500) NOT NULL,
    edg_dir CHAR(1) NOT NULL CHECK (edg_dir IN ('I', 'O')),
    noeuds1 VARCHAR(MAX) NOT NULL,
    noeuds1lie VARCHAR(MAX) NOT NULL,
    transformation VARCHAR(500) NULL,
    CONSTRAINT PK_table1 PRIMARY KEY (lna_uid, lin_uid, edg_dir)
);
GO

-- Table2 pour les programmes
CREATE TABLE table2 (
    lna_uid VARCHAR(200) NOT NULL PRIMARY KEY,
    programme VARCHAR(255) NULL,
    version VARCHAR(100) NULL
);
GO

-- Index pour optimiser les recherches sur noeuds
CREATE INDEX IX_table1_noeuds1 ON table1 (noeuds1);
CREATE INDEX IX_table1_noeuds1lie ON table1 (noeuds1lie);
CREATE INDEX IX_table1_edg_dir ON table1 (edg_dir);
GO

-- Données de test
INSERT INTO table2 (lna_uid, programme, version) VALUES
('LNA001', 'ETL_Extract', '1.0.0'),
('LNA002', 'ETL_Transform', '2.1.0'),
('LNA003', 'ETL_Load', '1.5.2'),
('LNA004', 'Report_Gen', '3.0.0');
GO

INSERT INTO table1 (lna_uid, lin_uid, edg_dir, noeuds1, noeuds1lie, transformation) VALUES
-- Flux 1: SOURCE_A -> STAGING_A -> DIM_A -> FACT_A
('LNA001', 'LIN001', 'I', 'SOURCE_TABLE_A', 'STAGING_TABLE_A', 'Extract'),
('LNA002', 'LIN002', 'I', 'STAGING_TABLE_A', 'DIM_TABLE_A', 'Transform'),
('LNA003', 'LIN003', 'I', 'DIM_TABLE_A', 'FACT_TABLE_A', 'Load'),

-- Flux 2: SOURCE_B -> STAGING_B -> FACT_A
('LNA001', 'LIN004', 'I', 'SOURCE_TABLE_B', 'STAGING_TABLE_B', 'Extract'),
('LNA002', 'LIN005', 'I', 'STAGING_TABLE_B', 'FACT_TABLE_A', 'Join'),

-- Flux 3: FACT_A -> REPORT
('LNA004', 'LIN006', 'I', 'FACT_TABLE_A', 'REPORT_SALES', 'Aggregate'),

-- Quelques relations inverses (O = Output)
('LNA002', 'LIN007', 'O', 'DIM_TABLE_A', 'STAGING_TABLE_A', 'Reverse_Lookup'),
('LNA003', 'LIN008', 'O', 'FACT_TABLE_A', 'DIM_TABLE_A', 'Dimension_Check');
GO

PRINT 'Base de données LignageDb2 créée avec succès!';
PRINT 'Structure: PK = (lna_uid VARCHAR(200), lin_uid VARCHAR(500), edg_dir CHAR(1))';
SELECT COUNT(*) AS 'Nombre de lignes dans table1' FROM table1;
SELECT COUNT(*) AS 'Nombre de lignes dans table2' FROM table2;
GO
