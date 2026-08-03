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

    // SQL helper to concatenate fields with '.' (compatible SQL Server 2014+)
    private const string ConcatDta = @"STUFF(
        CASE WHEN DTA_1 IS NOT NULL AND DTA_1 <> '' THEN '.' + DTA_1 ELSE '' END +
        CASE WHEN DTA_2 IS NOT NULL AND DTA_2 <> '' THEN '.' + DTA_2 ELSE '' END +
        CASE WHEN DTA_3 IS NOT NULL AND DTA_3 <> '' THEN '.' + DTA_3 ELSE '' END +
        CASE WHEN DTA_4 IS NOT NULL AND DTA_4 <> '' THEN '.' + DTA_4 ELSE '' END,
        1, 1, '')";

    private const string ConcatEdg = @"STUFF(
        CASE WHEN EDG_1 IS NOT NULL AND EDG_1 <> '' THEN '.' + EDG_1 ELSE '' END +
        CASE WHEN EDG_2 IS NOT NULL AND EDG_2 <> '' THEN '.' + EDG_2 ELSE '' END +
        CASE WHEN EDG_3 IS NOT NULL AND EDG_3 <> '' THEN '.' + EDG_3 ELSE '' END +
        CASE WHEN EDG_4 IS NOT NULL AND EDG_4 <> '' THEN '.' + EDG_4 ELSE '' END,
        1, 1, '')";

    public async Task<List<NodeDto>> GetDistinctNodesAsync()
    {
        // Concatenate DTA_1-4 with '.' to create the full node
        string sql = $@"
            WITH NodesWithPK AS (
                SELECT
                    {ConcatDta} AS Node,
                    LAN_UID AS LanUid,
                    LIN_UID AS LinUid,
                    EDG_DIR AS EdgDir,
                    ROW_NUMBER() OVER (PARTITION BY {ConcatDta} ORDER BY LAN_UID, LIN_UID) AS rn
                FROM LINE_VIS_EDG
            )
            SELECT
                LanUid + '|' + LinUid + '|' + EdgDir AS Id,
                Node
            FROM NodesWithPK
            WHERE rn = 1
            ORDER BY Node;";

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

    public async Task<List<LineageDto>> GetSuccessorsAsync(string lanUid, string linUid, string edgDir)
    {
        // Compare 4 columns DTA_1-4 with EDG_1-4 to find successors
        string sql = $@"
            WITH CurrentNode AS (
                SELECT DTA_1, DTA_2, DTA_3, DTA_4
                FROM LINE_VIS_EDG
                WHERE LAN_UID = @LanUid AND LIN_UID = @LinUid AND EDG_DIR = @EdgDir
            )
            -- EDG_DIR='I': DTA is predecessor of EDG, so EDG is successor
            SELECT
                t.LAN_UID + '|' + t.LIN_UID + '|' + t.EDG_DIR AS Id,
                {ConcatEdg.Replace("EDG_", "t.EDG_")} AS Node,
                t.TXN_TRI AS Transformation
            FROM LINE_VIS_EDG t
            INNER JOIN CurrentNode n ON ISNULL(t.DTA_1,'') = ISNULL(n.DTA_1,'') AND ISNULL(t.DTA_2,'') = ISNULL(n.DTA_2,'') AND ISNULL(t.DTA_3,'') = ISNULL(n.DTA_3,'') AND ISNULL(t.DTA_4,'') = ISNULL(n.DTA_4,'')
            WHERE t.EDG_DIR = 'I'
            UNION
            -- EDG_DIR='O': EDG is predecessor of DTA, so DTA is successor
            SELECT
                t.LAN_UID + '|' + t.LIN_UID + '|' + t.EDG_DIR AS Id,
                {ConcatDta.Replace("DTA_", "t.DTA_")} AS Node,
                t.TXN_TRI AS Transformation
            FROM LINE_VIS_EDG t
            INNER JOIN CurrentNode n ON ISNULL(t.EDG_1,'') = ISNULL(n.DTA_1,'') AND ISNULL(t.EDG_2,'') = ISNULL(n.DTA_2,'') AND ISNULL(t.EDG_3,'') = ISNULL(n.DTA_3,'') AND ISNULL(t.EDG_4,'') = ISNULL(n.DTA_4,'')
            WHERE t.EDG_DIR = 'O';";

        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<LineageDto>(sql,
            new { LanUid = lanUid, LinUid = linUid, EdgDir = edgDir });
        return result.ToList();
    }

    public async Task<List<LineageDto>> GetPredecessorsAsync(string lanUid, string linUid, string edgDir)
    {
        // Compare 4 columns DTA_1-4 with EDG_1-4 to find predecessors
        string sql = $@"
            WITH CurrentNode AS (
                SELECT DTA_1, DTA_2, DTA_3, DTA_4
                FROM LINE_VIS_EDG
                WHERE LAN_UID = @LanUid AND LIN_UID = @LinUid AND EDG_DIR = @EdgDir
            )
            -- EDG_DIR='O': EDG is predecessor of DTA, so if DTA=node, EDG is predecessor
            SELECT
                t.LAN_UID + '|' + t.LIN_UID + '|' + t.EDG_DIR AS Id,
                {ConcatEdg.Replace("EDG_", "t.EDG_")} AS Node,
                t.TXN_TRI AS Transformation
            FROM LINE_VIS_EDG t
            INNER JOIN CurrentNode n ON ISNULL(t.DTA_1,'') = ISNULL(n.DTA_1,'') AND ISNULL(t.DTA_2,'') = ISNULL(n.DTA_2,'') AND ISNULL(t.DTA_3,'') = ISNULL(n.DTA_3,'') AND ISNULL(t.DTA_4,'') = ISNULL(n.DTA_4,'')
            WHERE t.EDG_DIR = 'O'
            UNION
            -- EDG_DIR='I': DTA is predecessor of EDG, so if EDG=node, DTA is predecessor
            SELECT
                t.LAN_UID + '|' + t.LIN_UID + '|' + t.EDG_DIR AS Id,
                {ConcatDta.Replace("DTA_", "t.DTA_")} AS Node,
                t.TXN_TRI AS Transformation
            FROM LINE_VIS_EDG t
            INNER JOIN CurrentNode n ON ISNULL(t.EDG_1,'') = ISNULL(n.DTA_1,'') AND ISNULL(t.EDG_2,'') = ISNULL(n.DTA_2,'') AND ISNULL(t.EDG_3,'') = ISNULL(n.DTA_3,'') AND ISNULL(t.EDG_4,'') = ISNULL(n.DTA_4,'')
            WHERE t.EDG_DIR = 'I';";

        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<LineageDto>(sql,
            new { LanUid = lanUid, LinUid = linUid, EdgDir = edgDir });
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
}
