namespace LignageApp.Models;

/// <summary>
/// Clé primaire composite de table1
/// LNA_UID: VARCHAR(200), LIN_UID: VARCHAR(500), EDG_DIR: CHAR(1)
/// </summary>
public class LignagePK
{
    public string LnaUid { get; set; } = string.Empty;
    public string LinUid { get; set; } = string.Empty;
    public string EdgDir { get; set; } = string.Empty;

    // Encodage pour URL (utilise | comme séparateur car _ peut être dans les valeurs)
    public string ToUrlParam() => $"{Uri.EscapeDataString(LnaUid)}|{Uri.EscapeDataString(LinUid)}|{EdgDir}";

    public static LignagePK? Parse(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        var parts = value.Split('|');
        if (parts.Length != 3) return null;
        return new LignagePK
        {
            LnaUid = Uri.UnescapeDataString(parts[0]),
            LinUid = Uri.UnescapeDataString(parts[1]),
            EdgDir = parts[2]
        };
    }
}

/// <summary>
/// Ligne de table1 avec sa clé primaire
/// </summary>
public class LignageRow
{
    public string LnaUid { get; set; } = string.Empty;
    public string LinUid { get; set; } = string.Empty;
    public string EdgDir { get; set; } = string.Empty;
    public string Noeuds1 { get; set; } = string.Empty;
    public string Noeuds1Lie { get; set; } = string.Empty;
    public string Transformation { get; set; } = string.Empty;

    public LignagePK PK => new() { LnaUid = LnaUid, LinUid = LinUid, EdgDir = EdgDir };
}

/// <summary>
/// DTO pour afficher un noeud avec son identifiant
/// </summary>
public class NoeudDto
{
    public string Id { get; set; } = string.Empty;  // Format encodé pour URL
    public string Noeud { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour successeur/prédécesseur
/// </summary>
public class LignageDto
{
    public string Id { get; set; } = string.Empty;  // PK encodée
    public string Noeud { get; set; } = string.Empty;
    public string Transformation { get; set; } = string.Empty;
}

/// <summary>
/// Entrée dans l'historique de navigation
/// </summary>
public class HistoriqueEntry
{
    public string Id { get; set; } = string.Empty;
    public string Noeud { get; set; } = string.Empty;
    public List<LignageDto> Successeurs { get; set; } = new();
    public List<LignageDto> Predecesseurs { get; set; } = new();
    public ProgrammeDto? Programme { get; set; }
}

/// <summary>
/// Informations sur le programme
/// </summary>
public class ProgrammeDto
{
    public string Programme { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}
