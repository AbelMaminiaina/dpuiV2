using Dapper;
using Microsoft.Data.SqlClient;
using LignageApp.Models;

namespace LignageApp.Data;

public class LignageRepository : ILignageRepository
{
    private readonly string _connectionString;

    public LignageRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("LignageDb2") ?? string.Empty;
    }

    public async Task<List<NodeDto>> GetDistinctNodesAsync()
    {
        string sql = @"
            SELECT
                sub.LanUid + '|' + sub.LinUid + '|' + sub.EdgDir + '|D' AS Id,
                sub.Node
            FROM (
                SELECT
                    STUFF(
                        CASE WHEN DTA_1 IS NOT NULL AND DTA_1 <> '' THEN '.' + DTA_1 ELSE '' END +
                        CASE WHEN DTA_2 IS NOT NULL AND DTA_2 <> '' THEN '.' + DTA_2 ELSE '' END +
                        CASE WHEN DTA_3 IS NOT NULL AND DTA_3 <> '' THEN '.' + DTA_3 ELSE '' END +
                        CASE WHEN DTA_4 IS NOT NULL AND DTA_4 <> '' THEN '.' + DTA_4 ELSE '' END,
                        1, 1, '') AS Node,
                    LAN_UID AS LanUid,
                    LIN_UID AS LinUid,
                    EDG_DIR AS EdgDir,
                    ROW_NUMBER() OVER (PARTITION BY
                        STUFF(
                            CASE WHEN DTA_1 IS NOT NULL AND DTA_1 <> '' THEN '.' + DTA_1 ELSE '' END +
                            CASE WHEN DTA_2 IS NOT NULL AND DTA_2 <> '' THEN '.' + DTA_2 ELSE '' END +
                            CASE WHEN DTA_3 IS NOT NULL AND DTA_3 <> '' THEN '.' + DTA_3 ELSE '' END +
                            CASE WHEN DTA_4 IS NOT NULL AND DTA_4 <> '' THEN '.' + DTA_4 ELSE '' END,
                            1, 1, '')
                        ORDER BY LAN_UID, LIN_UID) AS rn
                FROM LINE_VIS_EDG
            ) sub
            WHERE sub.rn = 1
            ORDER BY sub.Node;";

        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<NodeDto>(sql);
        return result.ToList();
    }

    public async Task<LignageRow?> GetByPKAsync(string lanUid, string linUid, string edgDir)
    {
        const string sql = @"
            SELECT
                LAN_UID AS LanUid,
                LIN_UID AS LinUid,
                EDG_DIR AS EdgDir,
                DTA_1 AS Dta1,
                DTA_2 AS Dta2,
                DTA_3 AS Dta3,
                DTA_4 AS Dta4,
                EDG_1 AS Edg1,
                EDG_2 AS Edg2,
                EDG_3 AS Edg3,
                EDG_4 AS Edg4,
                TXN_TRI AS TxnTri
            FROM LINE_VIS_EDG
            WHERE LAN_UID = @LanUid AND LIN_UID = @LinUid AND EDG_DIR = @EdgDir;";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<LignageRow>(sql,
            new { LanUid = lanUid, LinUid = linUid, EdgDir = edgDir });
    }

    public async Task<List<LineageDto>> GetSuccessorsAsync(string lanUid, string linUid, string edgDir, bool useEdg = false, int maxRes = 500)
    {
        var nodeCol1 = useEdg ? "EDG_1" : "DTA_1";
        var nodeCol2 = useEdg ? "EDG_2" : "DTA_2";
        var nodeCol3 = useEdg ? "EDG_3" : "DTA_3";
        var nodeCol4 = useEdg ? "EDG_4" : "DTA_4";

        string sql = $@"
            SET NOCOUNT ON;

            CREATE TABLE #CurrentNode (N1 VARCHAR(MAX), N2 VARCHAR(MAX), N3 VARCHAR(MAX), N4 VARCHAR(MAX));
            INSERT INTO #CurrentNode SELECT {nodeCol1}, {nodeCol2}, {nodeCol3}, {nodeCol4}
            FROM LINE_VIS_EDG WHERE LAN_UID = @LanUid AND LIN_UID = @LinUid AND EDG_DIR = @EdgDir;

            CREATE TABLE #Successors (S1 VARCHAR(MAX), S2 VARCHAR(MAX), S3 VARCHAR(MAX), S4 VARCHAR(MAX), TXN_TRI VARCHAR(1000));

            INSERT INTO #Successors
            SELECT TOP (@MaxRes) t.EDG_1, t.EDG_2, t.EDG_3, t.EDG_4, t.TXN_TRI
            FROM LINE_VIS_EDG t
            INNER JOIN #CurrentNode n ON ISNULL(t.DTA_1,'') = ISNULL(n.N1,'')
                AND ISNULL(t.DTA_2,'') = ISNULL(n.N2,'')
                AND ISNULL(t.DTA_3,'') = ISNULL(n.N3,'')
                AND ISNULL(t.DTA_4,'') = ISNULL(n.N4,'')
            WHERE t.EDG_DIR = 'I';

            INSERT INTO #Successors
            SELECT TOP (@MaxRes) t.DTA_1, t.DTA_2, t.DTA_3, t.DTA_4, t.TXN_TRI
            FROM LINE_VIS_EDG t
            INNER JOIN #CurrentNode n ON ISNULL(t.EDG_1,'') = ISNULL(n.N1,'')
                AND ISNULL(t.EDG_2,'') = ISNULL(n.N2,'')
                AND ISNULL(t.EDG_3,'') = ISNULL(n.N3,'')
                AND ISNULL(t.EDG_4,'') = ISNULL(n.N4,'')
            WHERE t.EDG_DIR = 'O';

            SET NOCOUNT OFF;

            SELECT TOP (@MaxRes)
                COALESCE(d.Id, e.Id) AS Id,
                STUFF(
                    CASE WHEN s.S1 IS NOT NULL AND s.S1 <> '' THEN '.' + s.S1 ELSE '' END +
                    CASE WHEN s.S2 IS NOT NULL AND s.S2 <> '' THEN '.' + s.S2 ELSE '' END +
                    CASE WHEN s.S3 IS NOT NULL AND s.S3 <> '' THEN '.' + s.S3 ELSE '' END +
                    CASE WHEN s.S4 IS NOT NULL AND s.S4 <> '' THEN '.' + s.S4 ELSE '' END,
                    1, 1, '') AS Node,
                s.TXN_TRI AS Transformation
            FROM #Successors s
            LEFT JOIN (
                SELECT ISNULL(DTA_1,'') AS D1, ISNULL(DTA_2,'') AS D2, ISNULL(DTA_3,'') AS D3, ISNULL(DTA_4,'') AS D4,
                       MIN(LAN_UID + '|' + LIN_UID + '|' + EDG_DIR + '|D') AS Id
                FROM LINE_VIS_EDG GROUP BY ISNULL(DTA_1,''), ISNULL(DTA_2,''), ISNULL(DTA_3,''), ISNULL(DTA_4,'')
            ) d ON d.D1 = ISNULL(s.S1,'') AND d.D2 = ISNULL(s.S2,'') AND d.D3 = ISNULL(s.S3,'') AND d.D4 = ISNULL(s.S4,'')
            LEFT JOIN (
                SELECT ISNULL(EDG_1,'') AS E1, ISNULL(EDG_2,'') AS E2, ISNULL(EDG_3,'') AS E3, ISNULL(EDG_4,'') AS E4,
                       MIN(LAN_UID + '|' + LIN_UID + '|' + EDG_DIR + '|E') AS Id
                FROM LINE_VIS_EDG GROUP BY ISNULL(EDG_1,''), ISNULL(EDG_2,''), ISNULL(EDG_3,''), ISNULL(EDG_4,'')
            ) e ON e.E1 = ISNULL(s.S1,'') AND e.E2 = ISNULL(s.S2,'') AND e.E3 = ISNULL(s.S3,'') AND e.E4 = ISNULL(s.S4,'');

            DROP TABLE #CurrentNode;
            DROP TABLE #Successors;";

        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<LineageDto>(sql,
            new { LanUid = lanUid, LinUid = linUid, EdgDir = edgDir, MaxRes = maxRes });
        return result.ToList();
    }

    public async Task<List<LineageDto>> GetPredecessorsAsync(string lanUid, string linUid, string edgDir, bool useEdg = false, int maxRes = 500)
    {
        var nodeCol1 = useEdg ? "EDG_1" : "DTA_1";
        var nodeCol2 = useEdg ? "EDG_2" : "DTA_2";
        var nodeCol3 = useEdg ? "EDG_3" : "DTA_3";
        var nodeCol4 = useEdg ? "EDG_4" : "DTA_4";

        string sql = $@"
            SET NOCOUNT ON;

            CREATE TABLE #CurrentNode (N1 VARCHAR(MAX), N2 VARCHAR(MAX), N3 VARCHAR(MAX), N4 VARCHAR(MAX));
            INSERT INTO #CurrentNode SELECT {nodeCol1}, {nodeCol2}, {nodeCol3}, {nodeCol4}
            FROM LINE_VIS_EDG WHERE LAN_UID = @LanUid AND LIN_UID = @LinUid AND EDG_DIR = @EdgDir;

            CREATE TABLE #Predecessors (P1 VARCHAR(MAX), P2 VARCHAR(MAX), P3 VARCHAR(MAX), P4 VARCHAR(MAX), TXN_TRI VARCHAR(1000));

            INSERT INTO #Predecessors
            SELECT TOP (@MaxRes) t.EDG_1, t.EDG_2, t.EDG_3, t.EDG_4, t.TXN_TRI
            FROM LINE_VIS_EDG t
            INNER JOIN #CurrentNode n ON ISNULL(t.DTA_1,'') = ISNULL(n.N1,'')
                AND ISNULL(t.DTA_2,'') = ISNULL(n.N2,'')
                AND ISNULL(t.DTA_3,'') = ISNULL(n.N3,'')
                AND ISNULL(t.DTA_4,'') = ISNULL(n.N4,'')
            WHERE t.EDG_DIR = 'O';

            INSERT INTO #Predecessors
            SELECT TOP (@MaxRes) t.DTA_1, t.DTA_2, t.DTA_3, t.DTA_4, t.TXN_TRI
            FROM LINE_VIS_EDG t
            INNER JOIN #CurrentNode n ON ISNULL(t.EDG_1,'') = ISNULL(n.N1,'')
                AND ISNULL(t.EDG_2,'') = ISNULL(n.N2,'')
                AND ISNULL(t.EDG_3,'') = ISNULL(n.N3,'')
                AND ISNULL(t.EDG_4,'') = ISNULL(n.N4,'')
            WHERE t.EDG_DIR = 'I';

            SET NOCOUNT OFF;

            SELECT TOP (@MaxRes)
                COALESCE(d.Id, e.Id) AS Id,
                STUFF(
                    CASE WHEN p.P1 IS NOT NULL AND p.P1 <> '' THEN '.' + p.P1 ELSE '' END +
                    CASE WHEN p.P2 IS NOT NULL AND p.P2 <> '' THEN '.' + p.P2 ELSE '' END +
                    CASE WHEN p.P3 IS NOT NULL AND p.P3 <> '' THEN '.' + p.P3 ELSE '' END +
                    CASE WHEN p.P4 IS NOT NULL AND p.P4 <> '' THEN '.' + p.P4 ELSE '' END,
                    1, 1, '') AS Node,
                p.TXN_TRI AS Transformation
            FROM #Predecessors p
            LEFT JOIN (
                SELECT ISNULL(DTA_1,'') AS D1, ISNULL(DTA_2,'') AS D2, ISNULL(DTA_3,'') AS D3, ISNULL(DTA_4,'') AS D4,
                       MIN(LAN_UID + '|' + LIN_UID + '|' + EDG_DIR + '|D') AS Id
                FROM LINE_VIS_EDG GROUP BY ISNULL(DTA_1,''), ISNULL(DTA_2,''), ISNULL(DTA_3,''), ISNULL(DTA_4,'')
            ) d ON d.D1 = ISNULL(p.P1,'') AND d.D2 = ISNULL(p.P2,'') AND d.D3 = ISNULL(p.P3,'') AND d.D4 = ISNULL(p.P4,'')
            LEFT JOIN (
                SELECT ISNULL(EDG_1,'') AS E1, ISNULL(EDG_2,'') AS E2, ISNULL(EDG_3,'') AS E3, ISNULL(EDG_4,'') AS E4,
                       MIN(LAN_UID + '|' + LIN_UID + '|' + EDG_DIR + '|E') AS Id
                FROM LINE_VIS_EDG GROUP BY ISNULL(EDG_1,''), ISNULL(EDG_2,''), ISNULL(EDG_3,''), ISNULL(EDG_4,'')
            ) e ON e.E1 = ISNULL(p.P1,'') AND e.E2 = ISNULL(p.P2,'') AND e.E3 = ISNULL(p.P3,'') AND e.E4 = ISNULL(p.P4,'');

            DROP TABLE #CurrentNode;
            DROP TABLE #Predecessors;";

        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<LineageDto>(sql,
            new { LanUid = lanUid, LinUid = linUid, EdgDir = edgDir, MaxRes = maxRes });
        return result.ToList();
    }

    public async Task<ProgramDto?> GetProgramAsync(string lanUid)
    {
        const string sql = @"
            SELECT programme AS Program, version AS Version
            FROM table2
            WHERE lna_uid = @LanUid;";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<ProgramDto>(sql, new { LanUid = lanUid });
    }

    public async Task<List<NodeDto>> SearchNodesAsync(string searchTerm, int maxResults = 50)
    {
        string sql = @"
            SELECT TOP (@MaxResults)
                sub.LanUid + '|' + sub.LinUid + '|' + sub.EdgDir + '|D' AS Id,
                sub.Node
            FROM (
                SELECT
                    STUFF(
                        CASE WHEN DTA_1 IS NOT NULL AND DTA_1 <> '' THEN '.' + DTA_1 ELSE '' END +
                        CASE WHEN DTA_2 IS NOT NULL AND DTA_2 <> '' THEN '.' + DTA_2 ELSE '' END +
                        CASE WHEN DTA_3 IS NOT NULL AND DTA_3 <> '' THEN '.' + DTA_3 ELSE '' END +
                        CASE WHEN DTA_4 IS NOT NULL AND DTA_4 <> '' THEN '.' + DTA_4 ELSE '' END,
                        1, 1, '') AS Node,
                    LAN_UID AS LanUid,
                    LIN_UID AS LinUid,
                    EDG_DIR AS EdgDir,
                    ROW_NUMBER() OVER (PARTITION BY
                        STUFF(
                            CASE WHEN DTA_1 IS NOT NULL AND DTA_1 <> '' THEN '.' + DTA_1 ELSE '' END +
                            CASE WHEN DTA_2 IS NOT NULL AND DTA_2 <> '' THEN '.' + DTA_2 ELSE '' END +
                            CASE WHEN DTA_3 IS NOT NULL AND DTA_3 <> '' THEN '.' + DTA_3 ELSE '' END +
                            CASE WHEN DTA_4 IS NOT NULL AND DTA_4 <> '' THEN '.' + DTA_4 ELSE '' END,
                            1, 1, '')
                        ORDER BY LAN_UID, LIN_UID) AS rn
                FROM LINE_VIS_EDG
            ) sub
            WHERE sub.rn = 1 AND sub.Node = @SearchTerm
            ORDER BY sub.Node;";

        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<NodeDto>(sql,
            new { SearchTerm = searchTerm, MaxResults = maxResults });
        return result.ToList();
    }

    public async Task<PathResult> FindPathAsync(string startNodeId, string endNodeId, int maxDepth = 20)
    {
        var result = new PathResult();

        // Parse the node IDs to get node names
        var startParts = startNodeId.Split('|');
        var endParts = endNodeId.Split('|');

        if (startParts.Length < 3 || endParts.Length < 3)
        {
            return result;
        }

        var startLanUid = Uri.UnescapeDataString(startParts[0]);
        var startLinUid = Uri.UnescapeDataString(startParts[1]);
        var startEdgDir = startParts[2];
        var startUseEdg = startParts.Length > 3 && startParts[3] == "E";

        var endLanUid = Uri.UnescapeDataString(endParts[0]);
        var endLinUid = Uri.UnescapeDataString(endParts[1]);
        var endEdgDir = endParts[2];
        var endUseEdg = endParts.Length > 3 && endParts[3] == "E";

        // Get start and end node names
        var startRow = await GetByPKAsync(startLanUid, startLinUid, startEdgDir);
        var endRow = await GetByPKAsync(endLanUid, endLinUid, endEdgDir);

        if (startRow == null || endRow == null)
        {
            return result;
        }

        result.StartNode = startUseEdg ? startRow.LinkedNode : startRow.SourceNode;
        result.EndNode = endUseEdg ? endRow.LinkedNode : endRow.SourceNode;

        // Use optimized CTE stored procedure
        using var connection = new SqlConnection(_connectionString);
        var pathResults = await connection.QueryAsync<PathStepDto>(
            "sp_FindPath",
            new { StartNode = result.StartNode, EndNode = result.EndNode, MaxDepth = maxDepth },
            commandType: System.Data.CommandType.StoredProcedure,
            commandTimeout: 120
        );

        var steps = pathResults.ToList();

        if (steps.Count > 0 && steps[0].PathExists == 1)
        {
            result.PathExists = true;
            result.Steps = steps.Select(s => new PathStep
            {
                Order = s.StepOrder,
                Node = CleanNodeName(s.NodeName),
                Transformation = s.Transformation ?? "",
                Id = s.NodeId ?? ""
            }).ToList();
        }

        return result;
    }

    private string CleanNodeName(string nodeName)
    {
        if (string.IsNullOrEmpty(nodeName)) return nodeName;
        // Remove trailing dots from node name (e.g., "DB.Schema.Table." -> "DB.Schema.Table")
        return nodeName.TrimEnd('.');
    }

    // DTO for stored procedure result
    private class PathStepDto
    {
        public int PathExists { get; set; }
        public int StepOrder { get; set; }
        public string NodeName { get; set; } = string.Empty;
        public string? Transformation { get; set; }
        public string? NodeId { get; set; }
    }
}
