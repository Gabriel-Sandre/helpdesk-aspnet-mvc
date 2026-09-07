namespace HelpDesk.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalChamados { get; set; }
    public int Abertos { get; set; }
    public int EmAndamento { get; set; }
    public int Resolvidos { get; set; }
    public int Fechados { get; set; }
    public int SemTecnico { get; set; }
    public double MediaDiasResolucao { get; set; }

    public Dictionary<string, int> PorCategoria { get; set; } = new();
    public Dictionary<string, int> PorPrioridade { get; set; } = new();

    public IEnumerable<Chamado> UltimosChamados { get; set; } = new List<Chamado>();
}
