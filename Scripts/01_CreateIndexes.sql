-- ============================================
-- Index optimisés pour recherche de chemin
-- Table: LINE_VIS_EDG
-- ============================================

USE LignageDb2;
GO

-- Index sur les colonnes source (DTA_1-4)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LineVisEdg_Source')
BEGIN
    CREATE NONCLUSTERED INDEX IX_LineVisEdg_Source
    ON LINE_VIS_EDG (DTA_1, DTA_2, DTA_3, DTA_4)
    INCLUDE (EDG_1, EDG_2, EDG_3, EDG_4, TXN_TRI, LAN_UID, LIN_UID, EDG_DIR);
    PRINT 'Index IX_LineVisEdg_Source created';
END
GO

-- Index sur les colonnes cible (EDG_1-4)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LineVisEdg_Target')
BEGIN
    CREATE NONCLUSTERED INDEX IX_LineVisEdg_Target
    ON LINE_VIS_EDG (EDG_1, EDG_2, EDG_3, EDG_4)
    INCLUDE (DTA_1, DTA_2, DTA_3, DTA_4, TXN_TRI, LAN_UID, LIN_UID, EDG_DIR);
    PRINT 'Index IX_LineVisEdg_Target created';
END
GO

-- Index sur EDG_DIR pour filtrage rapide
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LineVisEdg_Dir')
BEGIN
    CREATE NONCLUSTERED INDEX IX_LineVisEdg_Dir
    ON LINE_VIS_EDG (EDG_DIR)
    INCLUDE (DTA_1, DTA_2, DTA_3, DTA_4, EDG_1, EDG_2, EDG_3, EDG_4);
    PRINT 'Index IX_LineVisEdg_Dir created';
END
GO

PRINT 'All indexes created successfully!';
GO
