namespace LignageApp.Models;

/// <summary>
/// Composite primary key for LINE_VIS_EDG
/// LAN_UID: VARCHAR(200), LIN_UID: VARCHAR(500), EDG_DIR: CHAR(1)
/// </summary>
public class LignagePK
{
    public string LanUid { get; set; } = string.Empty;
    public string LinUid { get; set; } = string.Empty;
    public string EdgDir { get; set; } = string.Empty;

    // URL encoding (uses | as separator because _ can be in values)
    public string ToUrlParam() => $"{Uri.EscapeDataString(LanUid)}|{Uri.EscapeDataString(LinUid)}|{EdgDir}";

    public static LignagePK? Parse(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        var parts = value.Split('|');
        if (parts.Length != 3) return null;
        return new LignagePK
        {
            LanUid = Uri.UnescapeDataString(parts[0]),
            LinUid = Uri.UnescapeDataString(parts[1]),
            EdgDir = parts[2]
        };
    }
}

/// <summary>
/// LINE_VIS_EDG row with primary key
/// DTA_1-4 = source node data
/// EDG_1-4 = linked node data
/// TXN_TRI = transformation
/// </summary>
public class LignageRow
{
    public string LanUid { get; set; } = string.Empty;
    public string LinUid { get; set; } = string.Empty;
    public string EdgDir { get; set; } = string.Empty;

    // Source node data (DTA_1-4)
    public string Dta1 { get; set; } = string.Empty;
    public string Dta2 { get; set; } = string.Empty;
    public string Dta3 { get; set; } = string.Empty;
    public string Dta4 { get; set; } = string.Empty;

    // Linked node data (EDG_1-4)
    public string Edg1 { get; set; } = string.Empty;
    public string Edg2 { get; set; } = string.Empty;
    public string Edg3 { get; set; } = string.Empty;
    public string Edg4 { get; set; } = string.Empty;

    // Transformation
    public string TxnTri { get; set; } = string.Empty;

    // Computed properties for display
    public string SourceNode => ConcatParts(Dta1, Dta2, Dta3, Dta4);
    public string LinkedNode => ConcatParts(Edg1, Edg2, Edg3, Edg4);
    public string Transformation => TxnTri;

    public LignagePK PK => new() { LanUid = LanUid, LinUid = LinUid, EdgDir = EdgDir };

    private static string ConcatParts(string p1, string p2, string p3, string p4)
    {
        var parts = new[] { p1, p2, p3, p4 }.Where(p => !string.IsNullOrEmpty(p));
        return string.Join(".", parts);
    }
}

/// <summary>
/// DTO for displaying a node with its identifier
/// </summary>
public class NodeDto
{
    public string Id { get; set; } = string.Empty;
    public string Node { get; set; } = string.Empty;
}

/// <summary>
/// DTO for successor/predecessor
/// </summary>
public class LineageDto
{
    public string Id { get; set; } = string.Empty;
    public string Node { get; set; } = string.Empty;
    public string Transformation { get; set; } = string.Empty;
}

/// <summary>
/// Navigation history entry
/// </summary>
public class HistoryEntry
{
    public string Id { get; set; } = string.Empty;
    public string Node { get; set; } = string.Empty;
    public List<LineageDto> Successors { get; set; } = new();
    public List<LineageDto> Predecessors { get; set; } = new();
    public ProgramDto? Program { get; set; }
}

/// <summary>
/// Program information
/// </summary>
public class ProgramDto
{
    public string Program { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}
