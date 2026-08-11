-- ============================================
-- Procédure stockée: Recherche de chemin CTE
-- Algorithme: BFS avec CTE récursif optimisé
-- ============================================

USE LignageDb2;
GO

IF OBJECT_ID('dbo.sp_FindPath', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_FindPath;
GO

CREATE PROCEDURE dbo.sp_FindPath
    @StartNode NVARCHAR(500),
    @EndNode NVARCHAR(500),
    @MaxDepth INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    -- Table pour stocker le chemin
    CREATE TABLE #PathSteps (
        StepOrder INT,
        NodeName NVARCHAR(500),
        Transformation NVARCHAR(500)
    );

    -- CTE Récursif: Recherche depuis le départ
    ;WITH ForwardPath AS (
        -- Point de départ: EDG_DIR = 'I' (DTA -> EDG)
        SELECT
            ISNULL(DTA_1,'') + '.' + ISNULL(DTA_2,'') + '.' + ISNULL(DTA_3,'') + '.' + ISNULL(DTA_4,'') AS CurrentNode,
            ISNULL(EDG_1,'') + '.' + ISNULL(EDG_2,'') + '.' + ISNULL(EDG_3,'') + '.' + ISNULL(EDG_4,'') AS NextNode,
            TXN_TRI AS Transformation,
            1 AS Depth,
            CAST(ISNULL(DTA_1,'') + '.' + ISNULL(DTA_2,'') + '.' + ISNULL(DTA_3,'') + '.' + ISNULL(DTA_4,'') AS NVARCHAR(MAX)) AS VisitedNodes,
            CAST(
                ISNULL(DTA_1,'') + '.' + ISNULL(DTA_2,'') + '.' + ISNULL(DTA_3,'') + '.' + ISNULL(DTA_4,'') + '|' + ISNULL(TXN_TRI,'') + CHAR(10) +
                ISNULL(EDG_1,'') + '.' + ISNULL(EDG_2,'') + '.' + ISNULL(EDG_3,'') + '.' + ISNULL(EDG_4,'') + '|'
            AS NVARCHAR(MAX)) AS PathData
        FROM LINE_VIS_EDG
        WHERE ISNULL(DTA_1,'') + '.' + ISNULL(DTA_2,'') + '.' + ISNULL(DTA_3,'') + '.' + ISNULL(DTA_4,'') = @StartNode
          AND EDG_DIR = 'I'

        UNION ALL

        -- Point de départ: EDG_DIR = 'O' (EDG -> DTA)
        SELECT
            ISNULL(EDG_1,'') + '.' + ISNULL(EDG_2,'') + '.' + ISNULL(EDG_3,'') + '.' + ISNULL(EDG_4,'') AS CurrentNode,
            ISNULL(DTA_1,'') + '.' + ISNULL(DTA_2,'') + '.' + ISNULL(DTA_3,'') + '.' + ISNULL(DTA_4,'') AS NextNode,
            TXN_TRI AS Transformation,
            1 AS Depth,
            CAST(ISNULL(EDG_1,'') + '.' + ISNULL(EDG_2,'') + '.' + ISNULL(EDG_3,'') + '.' + ISNULL(EDG_4,'') AS NVARCHAR(MAX)) AS VisitedNodes,
            CAST(
                ISNULL(EDG_1,'') + '.' + ISNULL(EDG_2,'') + '.' + ISNULL(EDG_3,'') + '.' + ISNULL(EDG_4,'') + '|' + ISNULL(TXN_TRI,'') + CHAR(10) +
                ISNULL(DTA_1,'') + '.' + ISNULL(DTA_2,'') + '.' + ISNULL(DTA_3,'') + '.' + ISNULL(DTA_4,'') + '|'
            AS NVARCHAR(MAX)) AS PathData
        FROM LINE_VIS_EDG
        WHERE ISNULL(EDG_1,'') + '.' + ISNULL(EDG_2,'') + '.' + ISNULL(EDG_3,'') + '.' + ISNULL(EDG_4,'') = @StartNode
          AND EDG_DIR = 'O'

        UNION ALL

        -- Récursion: EDG_DIR = 'I'
        SELECT
            fp.NextNode AS CurrentNode,
            ISNULL(e.EDG_1,'') + '.' + ISNULL(e.EDG_2,'') + '.' + ISNULL(e.EDG_3,'') + '.' + ISNULL(e.EDG_4,'') AS NextNode,
            e.TXN_TRI AS Transformation,
            fp.Depth + 1 AS Depth,
            fp.VisitedNodes + ',' + ISNULL(e.EDG_1,'') + '.' + ISNULL(e.EDG_2,'') + '.' + ISNULL(e.EDG_3,'') + '.' + ISNULL(e.EDG_4,'') AS VisitedNodes,
            fp.PathData + ISNULL(e.TXN_TRI,'') + CHAR(10) +
                ISNULL(e.EDG_1,'') + '.' + ISNULL(e.EDG_2,'') + '.' + ISNULL(e.EDG_3,'') + '.' + ISNULL(e.EDG_4,'') + '|' AS PathData
        FROM ForwardPath fp
        INNER JOIN LINE_VIS_EDG e ON
            ISNULL(e.DTA_1,'') + '.' + ISNULL(e.DTA_2,'') + '.' + ISNULL(e.DTA_3,'') + '.' + ISNULL(e.DTA_4,'') = fp.NextNode
            AND e.EDG_DIR = 'I'
        WHERE fp.Depth < @MaxDepth
          AND CHARINDEX(ISNULL(e.EDG_1,'') + '.' + ISNULL(e.EDG_2,'') + '.' + ISNULL(e.EDG_3,'') + '.' + ISNULL(e.EDG_4,''), fp.VisitedNodes) = 0

        UNION ALL

        -- Récursion: EDG_DIR = 'O'
        SELECT
            fp.NextNode AS CurrentNode,
            ISNULL(e.DTA_1,'') + '.' + ISNULL(e.DTA_2,'') + '.' + ISNULL(e.DTA_3,'') + '.' + ISNULL(e.DTA_4,'') AS NextNode,
            e.TXN_TRI AS Transformation,
            fp.Depth + 1 AS Depth,
            fp.VisitedNodes + ',' + ISNULL(e.DTA_1,'') + '.' + ISNULL(e.DTA_2,'') + '.' + ISNULL(e.DTA_3,'') + '.' + ISNULL(e.DTA_4,'') AS VisitedNodes,
            fp.PathData + ISNULL(e.TXN_TRI,'') + CHAR(10) +
                ISNULL(e.DTA_1,'') + '.' + ISNULL(e.DTA_2,'') + '.' + ISNULL(e.DTA_3,'') + '.' + ISNULL(e.DTA_4,'') + '|' AS PathData
        FROM ForwardPath fp
        INNER JOIN LINE_VIS_EDG e ON
            ISNULL(e.EDG_1,'') + '.' + ISNULL(e.EDG_2,'') + '.' + ISNULL(e.EDG_3,'') + '.' + ISNULL(e.EDG_4,'') = fp.NextNode
            AND e.EDG_DIR = 'O'
        WHERE fp.Depth < @MaxDepth
          AND CHARINDEX(ISNULL(e.DTA_1,'') + '.' + ISNULL(e.DTA_2,'') + '.' + ISNULL(e.DTA_3,'') + '.' + ISNULL(e.DTA_4,''), fp.VisitedNodes) = 0
    )
    -- Trouver le chemin le plus court vers la destination
    SELECT TOP 1 PathData, Depth
    INTO #Result
    FROM ForwardPath
    WHERE NextNode = @EndNode
    ORDER BY Depth
    OPTION (MAXRECURSION 1000);

    -- Si chemin trouvé
    IF EXISTS (SELECT 1 FROM #Result)
    BEGIN
        DECLARE @PathData NVARCHAR(MAX);
        SELECT @PathData = PathData FROM #Result;

        -- Parser le chemin (format: NodeName|Transform\nNodeName|Transform\n...)
        INSERT INTO #PathSteps (StepOrder, NodeName, Transformation)
        SELECT
            ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS StepOrder,
            LTRIM(RTRIM(LEFT(value, CHARINDEX('|', value) - 1))) AS NodeName,
            LTRIM(RTRIM(SUBSTRING(value, CHARINDEX('|', value) + 1, LEN(value)))) AS Transformation
        FROM STRING_SPLIT(@PathData, CHAR(10))
        WHERE LTRIM(RTRIM(value)) <> '' AND CHARINDEX('|', value) > 0;

        SELECT
            1 AS PathExists,
            StepOrder,
            NodeName,
            Transformation,
            '' AS NodeId
        FROM #PathSteps
        ORDER BY StepOrder;
    END
    ELSE
    BEGIN
        SELECT
            0 AS PathExists,
            0 AS StepOrder,
            @StartNode AS NodeName,
            '' AS Transformation,
            '' AS NodeId;
    END

    DROP TABLE IF EXISTS #Result;
    DROP TABLE IF EXISTS #PathSteps;
END
GO

PRINT 'Stored procedure sp_FindPath created successfully!';
GO
