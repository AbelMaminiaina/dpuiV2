-- Script d'optimisation : Table de noeuds avec clé primaire
USE LignageDb2;
GO

-- 1. Supprimer la table noeuds si elle existe
IF OBJECT_ID('noeuds', 'U') IS NOT NULL
    DROP TABLE noeuds;
GO

-- 2. Créer la table des noeuds avec ID permanent (clé primaire)
CREATE TABLE noeuds (
    id INT IDENTITY(1,1) PRIMARY KEY,
    noeud NVARCHAR(500) NOT NULL UNIQUE
);
GO

-- 3. Index pour recherche rapide par nom
CREATE INDEX IX_noeuds_noeud ON noeuds (noeud);
GO

-- 4. Peupler la table avec les noeuds distincts
INSERT INTO noeuds (noeud)
SELECT DISTINCT noeud
FROM (
    SELECT noeuds1 AS noeud FROM table1
    UNION
    SELECT noeuds1lie AS noeud FROM table1
) AS tous_noeuds
ORDER BY noeud;
GO

-- 5. Créer les index sur table1 pour les jointures
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_table1_noeuds1')
    CREATE INDEX IX_table1_noeuds1 ON table1 (noeuds1);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_table1_noeuds1lie')
    CREATE INDEX IX_table1_noeuds1lie ON table1 (noeuds1lie);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_table1_edg_dir')
    CREATE INDEX IX_table1_edg_dir ON table1 (edg_dir);
GO

-- Vérification
SELECT 'Noeuds créés: ' + CAST(COUNT(*) AS VARCHAR) AS Info FROM noeuds;
GO
