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

    public async Task<List<NoeudDto>> GetNoeudsDistinctsAsync()
    {
        const string sql = @"
            WITH NoeudsAvecPK AS (
                SELECT
                    noeuds1 AS Noeud,
                    lna_uid AS LnaUid,
                    lin_uid AS LinUid,
                    edg_dir AS EdgDir,
                    ROW_NUMBER() OVER (PARTITION BY noeuds1 ORDER BY lna_uid, lin_uid) AS rn
                FROM table1
            )
            SELECT
                LnaUid + '|' + LinUid + '|' + EdgDir AS Id,
                Noeud
            FROM NoeudsAvecPK
            WHERE rn = 1
            ORDER BY Noeud;";

        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<NoeudDto>(sql);
        return result.ToList();
    }

    public async Task<LignageRow?> GetByPKAsync(string lnaUid, string linUid, string edgDir)
    {
        const string sql = @"
            SELECT
                lna_uid AS LnaUid,
                lin_uid AS LinUid,
                edg_dir AS EdgDir,
                noeuds1 AS Noeuds1,
                noeuds1lie AS Noeuds1Lie,
                transformation AS Transformation
            FROM table1
            WHERE lna_uid = @LnaUid AND lin_uid = @LinUid AND edg_dir = @EdgDir;";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<LignageRow>(sql,
            new { LnaUid = lnaUid, LinUid = linUid, EdgDir = edgDir });
    }

    public async Task<List<LignageDto>> GetSuccesseursAsync(string lnaUid, string linUid, string edgDir)
    {
        // Optimisé: une seule requête avec JOIN
        const string sql = @"
            WITH NoeudActuel AS (
                SELECT noeuds1 FROM table1
                WHERE lna_uid = @LnaUid AND lin_uid = @LinUid AND edg_dir = @EdgDir
            )
            SELECT
                t.lna_uid + '|' + t.lin_uid + '|' + t.edg_dir AS Id,
                t.noeuds1lie AS Noeud,
                t.transformation AS Transformation
            FROM table1 t
            INNER JOIN NoeudActuel n ON t.noeuds1 = n.noeuds1
            WHERE t.edg_dir = 'I'
            UNION
            SELECT
                t.lna_uid + '|' + t.lin_uid + '|' + t.edg_dir AS Id,
                t.noeuds1 AS Noeud,
                t.transformation AS Transformation
            FROM table1 t
            INNER JOIN NoeudActuel n ON t.noeuds1lie = n.noeuds1
            WHERE t.edg_dir = 'O';";

        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<LignageDto>(sql,
            new { LnaUid = lnaUid, LinUid = linUid, EdgDir = edgDir });
        return result.ToList();
    }

    public async Task<List<LignageDto>> GetPredecesseursAsync(string lnaUid, string linUid, string edgDir)
    {
        // Optimisé: une seule requête avec JOIN
        const string sql = @"
            WITH NoeudActuel AS (
                SELECT noeuds1 FROM table1
                WHERE lna_uid = @LnaUid AND lin_uid = @LinUid AND edg_dir = @EdgDir
            )
            SELECT
                t.lna_uid + '|' + t.lin_uid + '|' + t.edg_dir AS Id,
                t.noeuds1lie AS Noeud,
                t.transformation AS Transformation
            FROM table1 t
            INNER JOIN NoeudActuel n ON t.noeuds1 = n.noeuds1
            WHERE t.edg_dir = 'O'
            UNION
            SELECT
                t.lna_uid + '|' + t.lin_uid + '|' + t.edg_dir AS Id,
                t.noeuds1 AS Noeud,
                t.transformation AS Transformation
            FROM table1 t
            INNER JOIN NoeudActuel n ON t.noeuds1lie = n.noeuds1
            WHERE t.edg_dir = 'I';";

        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<LignageDto>(sql,
            new { LnaUid = lnaUid, LinUid = linUid, EdgDir = edgDir });
        return result.ToList();
    }

    public async Task<ProgrammeDto?> GetProgrammeAsync(string lnaUid)
    {
        const string sql = @"
            SELECT programme AS Programme, version AS Version
            FROM table2
            WHERE lna_uid = @LnaUid;";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<ProgrammeDto>(sql, new { LnaUid = lnaUid });
    }
}
