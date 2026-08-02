namespace LignageApp.Models;

public class NoeudDto
{
    public int Id { get; set; }
    public string Noeud { get; set; }
}

public class LignageDto
{
    public int Id { get; set; }
    public string Noeud { get; set; }
    public string Transformation { get; set; }
}

public class HistoriqueEntry
{
    public int Id { get; set; }
    public string Noeud { get; set; }
    public List<LignageDto> Successeurs { get; set; } = new();
    public List<LignageDto> Predecesseurs { get; set; } = new();
    public ProgrammeDto? Programme { get; set; }
}

public class ProgrammeDto
{
    public string Programme { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}
