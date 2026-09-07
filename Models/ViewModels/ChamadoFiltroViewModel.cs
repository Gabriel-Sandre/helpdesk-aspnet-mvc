using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HelpDesk.Models.ViewModels;

public class ChamadoFiltroViewModel
{
    [Display(Name = "Busca")]
    public string? Busca { get; set; }

    [Display(Name = "Status")]
    public StatusChamado? Status { get; set; }

    [Display(Name = "Prioridade")]
    public Prioridade? Prioridade { get; set; }

    [Display(Name = "Categoria")]
    public int? CategoriaId { get; set; }

    [Display(Name = "Somente os meus")]
    public bool SomenteMeus { get; set; }

    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalRegistros { get; set; }

    public IEnumerable<Chamado> Chamados { get; set; } = new List<Chamado>();
    public SelectList? Categorias { get; set; }
}
