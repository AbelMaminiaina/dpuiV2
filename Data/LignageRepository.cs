using Dapper;
using Microsoft.Data.SqlClient;
using LignageApp.Models;

namespace LignageApp.Data;

public class LignageRepository : ILignageRepository
{
    private readonly string _connectionString;

    public LignageRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("LignageDb");
    }

    public async Task<List<NoeudDto>> GetNoeudsDistinctsAsync()
    {
        const string sql = @"
            SELECT ROW_NUMBER() OVER (ORDER BY noeud_hash) AS Id, noeud AS Noeud
            FROM (
                SELECT DISTINCT noeud, HASHBYTES('SHA2_256', noeud) AS noeud_hash
                FROM (
                    SELECT noeuds1 AS noeud FROM table1
                    UNION
                    SELECT noeuds1lie AS noeud FROM table1
                ) AS tous_noeuds
            ) AS noeuds_uniques
            ORDER BY noeud_hash;";

        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<NoeudDto>(sql);
        return result.ToList();
    }

    public async Task<NoeudDto?> GetNoeudByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Noeud FROM (
                SELECT ROW_NUMBER() OVER (ORDER BY HASHBYTES('SHA2_256', noeud)) AS Id, noeud AS Noeud
                FROM (
                    SELECT DISTINCT noeud
                    FROM (
                        SELECT noeuds1 AS noeud FROM table1
                        UNION
                        SELECT noeuds1lie AS noeud FROM table1
                    ) AS tous_noeuds
                ) AS noeuds_uniques
            ) AS numbered
            WHERE Id = @Id;";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<NoeudDto>(sql, new { Id = id });
    }

    public async Task<List<LignageDto>> GetSuccesseursAsync(int id)
    {
        const string sql = @"
            WITH NoeudsAvecId AS (
                SELECT ROW_NUMBER() OVER (ORDER BY HASHBYTES('SHA2_256', noeud)) AS Id,
                       noeud,
                       HASHBYTES('SHA2_256', noeud) AS noeud_hash
                FROM (
                    SELECT DISTINCT noeud
                    FROM (
                        SELECT noeuds1 AS noeud FROM table1
                        UNION
                        SELECT noeuds1lie AS noeud FROM table1
                    ) AS tous_noeuds
                ) AS noeuds_uniques
            ),
            NoeudCourant AS (
                SELECT noeud_hash FROM NoeudsAvecId WHERE Id = @Id
            )
            SELECT n.Id, s.Noeud, s.Transformation
            FROM (
                SELECT noeuds1lie AS Noeud, transformation AS Transformation
                FROM table1
                WHERE HASHBYTES('SHA2_256', noeuds1) = (SELECT noeud_hash FROM NoeudCourant) AND edg_dir = 'I'
                UNION
                SELECT noeuds1 AS Noeud, transformation AS Transformation
                FROM table1
                WHERE HASHBYTES('SHA2_256', noeuds1lie) = (SELECT noeud_hash FROM NoeudCourant) AND edg_dir = 'O'
            ) AS s
            JOIN NoeudsAvecId n ON n.noeud_hash = HASHBYTES('SHA2_256', s.Noeud);";

        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<LignageDto>(sql, new { Id = id });
        return result.ToList();
    }

    public async Task<List<LignageDto>> GetPredecesseursAsync(int id)
    {
        const string sql = @"
            WITH NoeudsAvecId AS (
                SELECT ROW_NUMBER() OVER (ORDER BY HASHBYTES('SHA2_256', noeud)) AS Id,
                       noeud,
                       HASHBYTES('SHA2_256', noeud) AS noeud_hash
                FROM (
                    SELECT DISTINCT noeud
                    FROM (
                        SELECT noeuds1 AS noeud FROM table1
                        UNION
                        SELECT noeuds1lie AS noeud FROM table1
                    ) AS tous_noeuds
                ) AS noeuds_uniques
            ),
            NoeudCourant AS (
                SELECT noeud_hash FROM NoeudsAvecId WHERE Id = @Id
            )
            SELECT n.Id, p.Noeud, p.Transformation
            FROM (
                SELECT noeuds1lie AS Noeud, transformation AS Transformation
                FROM table1
                WHERE HASHBYTES('SHA2_256', noeuds1) = (SELECT noeud_hash FROM NoeudCourant) AND edg_dir = 'O'
                UNION
                SELECT noeuds1 AS Noeud, transformation AS Transformation
                FROM table1
                WHERE HASHBYTES('SHA2_256', noeuds1lie) = (SELECT noeud_hash FROM NoeudCourant) AND edg_dir = 'I'
            ) AS p
            JOIN NoeudsAvecId n ON n.noeud_hash = HASHBYTES('SHA2_256', p.Noeud);";

        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<LignageDto>(sql, new { Id = id });
        return result.ToList();
    }

    public async Task<ProgrammeDto?> GetProgrammeAsync(int id)
    {
        const string sql = @"
            WITH NoeudsAvecId AS (
                SELECT ROW_NUMBER() OVER (ORDER BY HASHBYTES('SHA2_256', noeud)) AS Id,
                       noeud
                FROM (
                    SELECT DISTINCT noeud
                    FROM (
                        SELECT noeuds1 AS noeud FROM table1
                        UNION
                        SELECT noeuds1lie AS noeud FROM table1
                    ) AS tous_noeuds
                ) AS noeuds_uniques
            )
            SELECT TOP 1 t2.programme AS Programme, t2.version AS Version
            FROM NoeudsAvecId n
            JOIN table1 t1 ON t1.noeuds1 = n.noeud
            JOIN table2 t2 ON t2.lna_uid = t1.lna_uid
            WHERE n.Id = @Id;";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<ProgrammeDto>(sql, new { Id = id });
    }
}
