-- Script to generate 1000 linked nodes for testing
-- Table: LINE_VIS_EDG
-- Columns: LAN_UID, LIN_UID, EDG_DIR, DTA_1-4 (source), EDG_1-4 (linked), TXN_TRI (transformation)

-- Clear existing test data (optional - uncomment if needed)
-- DELETE FROM LINE_VIS_EDG WHERE LAN_UID LIKE 'TEST_%';

SET NOCOUNT ON;

DECLARE @i INT = 1;
DECLARE @lanUid VARCHAR(200);
DECLARE @linUid VARCHAR(500);
DECLARE @edgDir CHAR(1);
DECLARE @srcDb VARCHAR(100);
DECLARE @srcSchema VARCHAR(100);
DECLARE @srcTable VARCHAR(100);
DECLARE @srcColumn VARCHAR(100);
DECLARE @tgtDb VARCHAR(100);
DECLARE @tgtSchema VARCHAR(100);
DECLARE @tgtTable VARCHAR(100);
DECLARE @tgtColumn VARCHAR(100);
DECLARE @transformation VARCHAR(100);

-- Create a chain of 500 nodes (each row links 2 nodes)
-- This creates paths like: Node1 -> Node2 -> Node3 -> ... -> Node500
WHILE @i <= 500
BEGIN
    SET @lanUid = 'TEST_CHAIN_' + RIGHT('000' + CAST(@i AS VARCHAR), 3);
    SET @linUid = 'LINK_' + RIGHT('0000' + CAST(@i AS VARCHAR), 4);
    SET @edgDir = 'I'; -- Input direction

    -- Source node
    SET @srcDb = 'DB_SOURCE';
    SET @srcSchema = 'SCH_' + RIGHT('00' + CAST((@i % 10) + 1 AS VARCHAR), 2);
    SET @srcTable = 'TABLE_' + RIGHT('000' + CAST(@i AS VARCHAR), 3);
    SET @srcColumn = 'COL_' + CAST((@i % 5) + 1 AS VARCHAR);

    -- Target node (next in chain)
    SET @tgtDb = 'DB_SOURCE';
    SET @tgtSchema = 'SCH_' + RIGHT('00' + CAST(((@i + 1) % 10) + 1 AS VARCHAR), 2);
    SET @tgtTable = 'TABLE_' + RIGHT('000' + CAST(@i + 1 AS VARCHAR), 3);
    SET @tgtColumn = 'COL_' + CAST(((@i + 1) % 5) + 1 AS VARCHAR);

    -- Transformation type
    SET @transformation = CASE (@i % 5)
        WHEN 0 THEN 'MAPPING'
        WHEN 1 THEN 'FILTER'
        WHEN 2 THEN 'AGGREGATE'
        WHEN 3 THEN 'JOIN'
        WHEN 4 THEN 'TRANSFORM'
    END;

    INSERT INTO LINE_VIS_EDG (LAN_UID, LIN_UID, EDG_DIR, DTA_1, DTA_2, DTA_3, DTA_4, EDG_1, EDG_2, EDG_3, EDG_4, TXN_TRI)
    VALUES (@lanUid, @linUid, @edgDir, @srcDb, @srcSchema, @srcTable, @srcColumn, @tgtDb, @tgtSchema, @tgtTable, @tgtColumn, @transformation);

    SET @i = @i + 1;
END

-- Create branching paths (multiple successors from some nodes)
-- This creates a tree structure with branches
SET @i = 1;
WHILE @i <= 200
BEGIN
    SET @lanUid = 'TEST_BRANCH_' + RIGHT('000' + CAST(@i AS VARCHAR), 3);
    SET @linUid = 'BRANCH_' + RIGHT('0000' + CAST(@i AS VARCHAR), 4);
    SET @edgDir = 'I';

    -- Source: pick from chain nodes (every 5th node has branches)
    SET @srcDb = 'DB_SOURCE';
    SET @srcSchema = 'SCH_' + RIGHT('00' + CAST(((@i * 5) % 10) + 1 AS VARCHAR), 2);
    SET @srcTable = 'TABLE_' + RIGHT('000' + CAST((@i * 5) % 500 + 1 AS VARCHAR), 3);
    SET @srcColumn = 'COL_' + CAST(((@i * 5) % 5) + 1 AS VARCHAR);

    -- Target: new branch node
    SET @tgtDb = 'DB_BRANCH';
    SET @tgtSchema = 'SCH_BR_' + RIGHT('00' + CAST((@i % 5) + 1 AS VARCHAR), 2);
    SET @tgtTable = 'BRANCH_TBL_' + RIGHT('000' + CAST(@i AS VARCHAR), 3);
    SET @tgtColumn = 'BR_COL_' + CAST((@i % 3) + 1 AS VARCHAR);

    SET @transformation = CASE (@i % 4)
        WHEN 0 THEN 'COPY'
        WHEN 1 THEN 'DERIVE'
        WHEN 2 THEN 'LOOKUP'
        WHEN 3 THEN 'MERGE'
    END;

    INSERT INTO LINE_VIS_EDG (LAN_UID, LIN_UID, EDG_DIR, DTA_1, DTA_2, DTA_3, DTA_4, EDG_1, EDG_2, EDG_3, EDG_4, TXN_TRI)
    VALUES (@lanUid, @linUid, @edgDir, @srcDb, @srcSchema, @srcTable, @srcColumn, @tgtDb, @tgtSchema, @tgtTable, @tgtColumn, @transformation);

    SET @i = @i + 1;
END

-- Create convergence paths (multiple sources to same target)
-- Some nodes receive data from multiple sources
SET @i = 1;
WHILE @i <= 150
BEGIN
    SET @lanUid = 'TEST_CONVERGE_' + RIGHT('000' + CAST(@i AS VARCHAR), 3);
    SET @linUid = 'CONV_' + RIGHT('0000' + CAST(@i AS VARCHAR), 4);
    SET @edgDir = 'I';

    -- Source: different source nodes
    SET @srcDb = 'DB_EXT_' + CAST((@i % 3) + 1 AS VARCHAR);
    SET @srcSchema = 'EXT_SCH_' + RIGHT('00' + CAST((@i % 8) + 1 AS VARCHAR), 2);
    SET @srcTable = 'EXT_TABLE_' + RIGHT('000' + CAST(@i AS VARCHAR), 3);
    SET @srcColumn = 'EXT_COL_' + CAST((@i % 4) + 1 AS VARCHAR);

    -- Target: converge to specific nodes in chain
    SET @tgtDb = 'DB_SOURCE';
    SET @tgtSchema = 'SCH_' + RIGHT('00' + CAST(((@i * 3) % 10) + 1 AS VARCHAR), 2);
    SET @tgtTable = 'TABLE_' + RIGHT('000' + CAST((@i * 3) % 500 + 1 AS VARCHAR), 3);
    SET @tgtColumn = 'COL_' + CAST(((@i * 3) % 5) + 1 AS VARCHAR);

    SET @transformation = CASE (@i % 3)
        WHEN 0 THEN 'UNION'
        WHEN 1 THEN 'APPEND'
        WHEN 2 THEN 'COALESCE'
    END;

    INSERT INTO LINE_VIS_EDG (LAN_UID, LIN_UID, EDG_DIR, DTA_1, DTA_2, DTA_3, DTA_4, EDG_1, EDG_2, EDG_3, EDG_4, TXN_TRI)
    VALUES (@lanUid, @linUid, @edgDir, @srcDb, @srcSchema, @srcTable, @srcColumn, @tgtDb, @tgtSchema, @tgtTable, @tgtColumn, @transformation);

    SET @i = @i + 1;
END

-- Create final destination nodes (data warehouse targets)
SET @i = 1;
WHILE @i <= 100
BEGIN
    SET @lanUid = 'TEST_DWH_' + RIGHT('000' + CAST(@i AS VARCHAR), 3);
    SET @linUid = 'DWH_' + RIGHT('0000' + CAST(@i AS VARCHAR), 4);
    SET @edgDir = 'I';

    -- Source: branch nodes
    SET @srcDb = 'DB_BRANCH';
    SET @srcSchema = 'SCH_BR_' + RIGHT('00' + CAST((@i % 5) + 1 AS VARCHAR), 2);
    SET @srcTable = 'BRANCH_TBL_' + RIGHT('000' + CAST((@i * 2) % 200 + 1 AS VARCHAR), 3);
    SET @srcColumn = 'BR_COL_' + CAST((@i % 3) + 1 AS VARCHAR);

    -- Target: data warehouse
    SET @tgtDb = 'DWH';
    SET @tgtSchema = 'FACT';
    SET @tgtTable = 'FACT_' + CASE (@i % 5)
        WHEN 0 THEN 'SALES'
        WHEN 1 THEN 'INVENTORY'
        WHEN 2 THEN 'CUSTOMERS'
        WHEN 3 THEN 'ORDERS'
        WHEN 4 THEN 'PRODUCTS'
    END;
    SET @tgtColumn = 'MEASURE_' + CAST((@i % 10) + 1 AS VARCHAR);

    SET @transformation = 'LOAD_' + CASE (@i % 3)
        WHEN 0 THEN 'FULL'
        WHEN 1 THEN 'INCREMENTAL'
        WHEN 2 THEN 'MERGE'
    END;

    INSERT INTO LINE_VIS_EDG (LAN_UID, LIN_UID, EDG_DIR, DTA_1, DTA_2, DTA_3, DTA_4, EDG_1, EDG_2, EDG_3, EDG_4, TXN_TRI)
    VALUES (@lanUid, @linUid, @edgDir, @srcDb, @srcSchema, @srcTable, @srcColumn, @tgtDb, @tgtSchema, @tgtTable, @tgtColumn, @transformation);

    SET @i = @i + 1;
END

-- Create some circular paths for testing (optional - be careful with cycles)
-- These create short loops: A -> B -> C -> A
SET @i = 1;
WHILE @i <= 50
BEGIN
    -- First link: LOOP_A -> LOOP_B
    SET @lanUid = 'TEST_LOOP_A_' + RIGHT('00' + CAST(@i AS VARCHAR), 2);
    SET @linUid = 'LOOP_AB_' + RIGHT('000' + CAST(@i AS VARCHAR), 3);
    SET @edgDir = 'I';

    INSERT INTO LINE_VIS_EDG (LAN_UID, LIN_UID, EDG_DIR, DTA_1, DTA_2, DTA_3, DTA_4, EDG_1, EDG_2, EDG_3, EDG_4, TXN_TRI)
    VALUES (@lanUid, @linUid, @edgDir,
            'LOOP', 'GRP_' + CAST(@i AS VARCHAR), 'NODE_A', '',
            'LOOP', 'GRP_' + CAST(@i AS VARCHAR), 'NODE_B', '',
            'CYCLE_STEP');

    -- Second link: LOOP_B -> LOOP_C
    SET @lanUid = 'TEST_LOOP_B_' + RIGHT('00' + CAST(@i AS VARCHAR), 2);
    SET @linUid = 'LOOP_BC_' + RIGHT('000' + CAST(@i AS VARCHAR), 3);

    INSERT INTO LINE_VIS_EDG (LAN_UID, LIN_UID, EDG_DIR, DTA_1, DTA_2, DTA_3, DTA_4, EDG_1, EDG_2, EDG_3, EDG_4, TXN_TRI)
    VALUES (@lanUid, @linUid, @edgDir,
            'LOOP', 'GRP_' + CAST(@i AS VARCHAR), 'NODE_B', '',
            'LOOP', 'GRP_' + CAST(@i AS VARCHAR), 'NODE_C', '',
            'CYCLE_STEP');

    SET @i = @i + 1;
END

SET NOCOUNT OFF;

-- Summary
SELECT 'Test data generation complete' AS Status;
SELECT COUNT(*) AS TotalRows FROM LINE_VIS_EDG WHERE LAN_UID LIKE 'TEST_%';
SELECT
    CASE
        WHEN LAN_UID LIKE 'TEST_CHAIN_%' THEN 'Chain'
        WHEN LAN_UID LIKE 'TEST_BRANCH_%' THEN 'Branch'
        WHEN LAN_UID LIKE 'TEST_CONVERGE_%' THEN 'Converge'
        WHEN LAN_UID LIKE 'TEST_DWH_%' THEN 'DWH'
        WHEN LAN_UID LIKE 'TEST_LOOP_%' THEN 'Loop'
    END AS Category,
    COUNT(*) AS Count
FROM LINE_VIS_EDG
WHERE LAN_UID LIKE 'TEST_%'
GROUP BY
    CASE
        WHEN LAN_UID LIKE 'TEST_CHAIN_%' THEN 'Chain'
        WHEN LAN_UID LIKE 'TEST_BRANCH_%' THEN 'Branch'
        WHEN LAN_UID LIKE 'TEST_CONVERGE_%' THEN 'Converge'
        WHEN LAN_UID LIKE 'TEST_DWH_%' THEN 'DWH'
        WHEN LAN_UID LIKE 'TEST_LOOP_%' THEN 'Loop'
    END;

-- Sample paths to test:
-- 1. Chain path: DB_SOURCE.SCH_01.TABLE_001.COL_1 -> DB_SOURCE.SCH_02.TABLE_501.COL_1 (long path)
-- 2. Branch path: Any TABLE_XXX -> BRANCH_TBL_XXX -> DWH.FACT.FACT_SALES
-- 3. Converge path: EXT_TABLE_XXX -> TABLE_XXX
