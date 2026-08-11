-- ============================================
-- Script de vérification des données LINE_VIS_EDG
-- ============================================

-- 1. Voir toutes les données brutes
PRINT '=== 1. DONNÉES BRUTES ==='
SELECT
    LAN_UID,
    LIN_UID,
    EDG_DIR,
    DTA_1, DTA_2, DTA_3, DTA_4,
    EDG_1, EDG_2, EDG_3, EDG_4,
    TXN_TRI
FROM LINE_VIS_EDG
ORDER BY LAN_UID, LIN_UID;

-- 2. Liste des noeuds distincts (ce qui s'affiche sur la page d'accueil)
PRINT '=== 2. NOEUDS DISTINCTS (Page Nodes) ==='
SELECT DISTINCT
    STUFF(
        CASE WHEN DTA_1 IS NOT NULL AND DTA_1 <> '' THEN '.' + DTA_1 ELSE '' END +
        CASE WHEN DTA_2 IS NOT NULL AND DTA_2 <> '' THEN '.' + DTA_2 ELSE '' END +
        CASE WHEN DTA_3 IS NOT NULL AND DTA_3 <> '' THEN '.' + DTA_3 ELSE '' END +
        CASE WHEN DTA_4 IS NOT NULL AND DTA_4 <> '' THEN '.' + DTA_4 ELSE '' END,
        1, 1, '') AS Node
FROM LINE_VIS_EDG
ORDER BY Node;

-- 3. Pour un noeud donné, voir ses successeurs et prédécesseurs
-- MODIFIER les valeurs ci-dessous selon vos données
DECLARE @LanUid VARCHAR(200) = 'LAN001';  -- Remplacer par une vraie valeur
DECLARE @LinUid VARCHAR(500) = 'LIN001';  -- Remplacer par une vraie valeur
DECLARE @EdgDir CHAR(1) = 'I';            -- 'I' ou 'O'

PRINT '=== 3. NOEUD ACTUEL ==='
SELECT
    LAN_UID, LIN_UID, EDG_DIR,
    STUFF(
        CASE WHEN DTA_1 IS NOT NULL AND DTA_1 <> '' THEN '.' + DTA_1 ELSE '' END +
        CASE WHEN DTA_2 IS NOT NULL AND DTA_2 <> '' THEN '.' + DTA_2 ELSE '' END +
        CASE WHEN DTA_3 IS NOT NULL AND DTA_3 <> '' THEN '.' + DTA_3 ELSE '' END +
        CASE WHEN DTA_4 IS NOT NULL AND DTA_4 <> '' THEN '.' + DTA_4 ELSE '' END,
        1, 1, '') AS CurrentNode
FROM LINE_VIS_EDG
WHERE LAN_UID = @LanUid AND LIN_UID = @LinUid AND EDG_DIR = @EdgDir;

-- 4. Logique des relations:
-- EDG_DIR = 'I' : DTA est prédécesseur de EDG (DTA --> EDG)
-- EDG_DIR = 'O' : EDG est prédécesseur de DTA (EDG --> DTA)

PRINT '=== 4. RELATIONS AVEC DIRECTION ==='
SELECT
    LAN_UID, LIN_UID, EDG_DIR,
    STUFF(
        CASE WHEN DTA_1 IS NOT NULL AND DTA_1 <> '' THEN '.' + DTA_1 ELSE '' END +
        CASE WHEN DTA_2 IS NOT NULL AND DTA_2 <> '' THEN '.' + DTA_2 ELSE '' END +
        CASE WHEN DTA_3 IS NOT NULL AND DTA_3 <> '' THEN '.' + DTA_3 ELSE '' END +
        CASE WHEN DTA_4 IS NOT NULL AND DTA_4 <> '' THEN '.' + DTA_4 ELSE '' END,
        1, 1, '') AS SourceNode,
    CASE EDG_DIR
        WHEN 'I' THEN ' --> '
        WHEN 'O' THEN ' <-- '
    END AS Direction,
    STUFF(
        CASE WHEN EDG_1 IS NOT NULL AND EDG_1 <> '' THEN '.' + EDG_1 ELSE '' END +
        CASE WHEN EDG_2 IS NOT NULL AND EDG_2 <> '' THEN '.' + EDG_2 ELSE '' END +
        CASE WHEN EDG_3 IS NOT NULL AND EDG_3 <> '' THEN '.' + EDG_3 ELSE '' END +
        CASE WHEN EDG_4 IS NOT NULL AND EDG_4 <> '' THEN '.' + EDG_4 ELSE '' END,
        1, 1, '') AS LinkedNode,
    TXN_TRI AS Transformation
FROM LINE_VIS_EDG
ORDER BY SourceNode, EDG_DIR;

-- 5. Vérifier les successeurs d'un noeud spécifique
PRINT '=== 5. SUCCESSEURS ==='
PRINT 'Pour le noeud DTA actuel, les successeurs sont:'
PRINT '- Si EDG_DIR=I et DTA match: EDG est le successeur'
PRINT '- Si EDG_DIR=O et EDG match: DTA est le successeur'

;WITH CurrentNode AS (
    SELECT DTA_1, DTA_2, DTA_3, DTA_4
    FROM LINE_VIS_EDG
    WHERE LAN_UID = @LanUid AND LIN_UID = @LinUid AND EDG_DIR = @EdgDir
)
-- EDG_DIR='I': DTA --> EDG, donc EDG est successeur
SELECT 'Successeur (via I)' AS Type,
    STUFF(
        CASE WHEN t.EDG_1 IS NOT NULL AND t.EDG_1 <> '' THEN '.' + t.EDG_1 ELSE '' END +
        CASE WHEN t.EDG_2 IS NOT NULL AND t.EDG_2 <> '' THEN '.' + t.EDG_2 ELSE '' END +
        CASE WHEN t.EDG_3 IS NOT NULL AND t.EDG_3 <> '' THEN '.' + t.EDG_3 ELSE '' END +
        CASE WHEN t.EDG_4 IS NOT NULL AND t.EDG_4 <> '' THEN '.' + t.EDG_4 ELSE '' END,
        1, 1, '') AS SuccessorNode,
    t.TXN_TRI AS Transformation
FROM LINE_VIS_EDG t
INNER JOIN CurrentNode n ON ISNULL(t.DTA_1,'') = ISNULL(n.DTA_1,'')
    AND ISNULL(t.DTA_2,'') = ISNULL(n.DTA_2,'')
    AND ISNULL(t.DTA_3,'') = ISNULL(n.DTA_3,'')
    AND ISNULL(t.DTA_4,'') = ISNULL(n.DTA_4,'')
WHERE t.EDG_DIR = 'I'
UNION ALL
-- EDG_DIR='O': EDG --> DTA, donc DTA est successeur
SELECT 'Successeur (via O)' AS Type,
    STUFF(
        CASE WHEN t.DTA_1 IS NOT NULL AND t.DTA_1 <> '' THEN '.' + t.DTA_1 ELSE '' END +
        CASE WHEN t.DTA_2 IS NOT NULL AND t.DTA_2 <> '' THEN '.' + t.DTA_2 ELSE '' END +
        CASE WHEN t.DTA_3 IS NOT NULL AND t.DTA_3 <> '' THEN '.' + t.DTA_3 ELSE '' END +
        CASE WHEN t.DTA_4 IS NOT NULL AND t.DTA_4 <> '' THEN '.' + t.DTA_4 ELSE '' END,
        1, 1, '') AS SuccessorNode,
    t.TXN_TRI AS Transformation
FROM LINE_VIS_EDG t
INNER JOIN CurrentNode n ON ISNULL(t.EDG_1,'') = ISNULL(n.DTA_1,'')
    AND ISNULL(t.EDG_2,'') = ISNULL(n.DTA_2,'')
    AND ISNULL(t.EDG_3,'') = ISNULL(n.DTA_3,'')
    AND ISNULL(t.EDG_4,'') = ISNULL(n.DTA_4,'')
WHERE t.EDG_DIR = 'O';

-- 6. Vérifier les prédécesseurs d'un noeud spécifique
PRINT '=== 6. PRÉDÉCESSEURS ==='
PRINT 'Pour le noeud DTA actuel, les prédécesseurs sont:'
PRINT '- Si EDG_DIR=O et DTA match: EDG est le prédécesseur'
PRINT '- Si EDG_DIR=I et EDG match: DTA est le prédécesseur'

;WITH CurrentNode AS (
    SELECT DTA_1, DTA_2, DTA_3, DTA_4
    FROM LINE_VIS_EDG
    WHERE LAN_UID = @LanUid AND LIN_UID = @LinUid AND EDG_DIR = @EdgDir
)
-- EDG_DIR='O': EDG --> DTA, donc EDG est prédécesseur
SELECT 'Prédécesseur (via O)' AS Type,
    STUFF(
        CASE WHEN t.EDG_1 IS NOT NULL AND t.EDG_1 <> '' THEN '.' + t.EDG_1 ELSE '' END +
        CASE WHEN t.EDG_2 IS NOT NULL AND t.EDG_2 <> '' THEN '.' + t.EDG_2 ELSE '' END +
        CASE WHEN t.EDG_3 IS NOT NULL AND t.EDG_3 <> '' THEN '.' + t.EDG_3 ELSE '' END +
        CASE WHEN t.EDG_4 IS NOT NULL AND t.EDG_4 <> '' THEN '.' + t.EDG_4 ELSE '' END,
        1, 1, '') AS PredecessorNode,
    t.TXN_TRI AS Transformation
FROM LINE_VIS_EDG t
INNER JOIN CurrentNode n ON ISNULL(t.DTA_1,'') = ISNULL(n.DTA_1,'')
    AND ISNULL(t.DTA_2,'') = ISNULL(n.DTA_2,'')
    AND ISNULL(t.DTA_3,'') = ISNULL(n.DTA_3,'')
    AND ISNULL(t.DTA_4,'') = ISNULL(n.DTA_4,'')
WHERE t.EDG_DIR = 'O'
UNION ALL
-- EDG_DIR='I': DTA --> EDG, donc DTA est prédécesseur si EDG match
SELECT 'Prédécesseur (via I)' AS Type,
    STUFF(
        CASE WHEN t.DTA_1 IS NOT NULL AND t.DTA_1 <> '' THEN '.' + t.DTA_1 ELSE '' END +
        CASE WHEN t.DTA_2 IS NOT NULL AND t.DTA_2 <> '' THEN '.' + t.DTA_2 ELSE '' END +
        CASE WHEN t.DTA_3 IS NOT NULL AND t.DTA_3 <> '' THEN '.' + t.DTA_3 ELSE '' END +
        CASE WHEN t.DTA_4 IS NOT NULL AND t.DTA_4 <> '' THEN '.' + t.DTA_4 ELSE '' END,
        1, 1, '') AS PredecessorNode,
    t.TXN_TRI AS Transformation
FROM LINE_VIS_EDG t
INNER JOIN CurrentNode n ON ISNULL(t.EDG_1,'') = ISNULL(n.DTA_1,'')
    AND ISNULL(t.EDG_2,'') = ISNULL(n.DTA_2,'')
    AND ISNULL(t.EDG_3,'') = ISNULL(n.DTA_3,'')
    AND ISNULL(t.EDG_4,'') = ISNULL(n.DTA_4,'')
WHERE t.EDG_DIR = 'I';

-- 7. Statistiques
PRINT '=== 7. STATISTIQUES ==='
SELECT
    (SELECT COUNT(*) FROM LINE_VIS_EDG) AS TotalRows,
    (SELECT COUNT(*) FROM LINE_VIS_EDG WHERE EDG_DIR = 'I') AS RowsWithI,
    (SELECT COUNT(*) FROM LINE_VIS_EDG WHERE EDG_DIR = 'O') AS RowsWithO,
    (SELECT COUNT(DISTINCT STUFF(
        CASE WHEN DTA_1 IS NOT NULL AND DTA_1 <> '' THEN '.' + DTA_1 ELSE '' END +
        CASE WHEN DTA_2 IS NOT NULL AND DTA_2 <> '' THEN '.' + DTA_2 ELSE '' END +
        CASE WHEN DTA_3 IS NOT NULL AND DTA_3 <> '' THEN '.' + DTA_3 ELSE '' END +
        CASE WHEN DTA_4 IS NOT NULL AND DTA_4 <> '' THEN '.' + DTA_4 ELSE '' END,
        1, 1, '')) FROM LINE_VIS_EDG) AS DistinctNodes;
