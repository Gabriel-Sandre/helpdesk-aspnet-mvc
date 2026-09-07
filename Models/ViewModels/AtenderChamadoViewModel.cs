using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HelpDesk.Models.ViewModels;

public class AtenderChamadoViewModel
{
    public int ChamadoId { get; set; }

    [Display(Name = "Status")]
    public StatusChamado Status { get; set; }

    [Display(Name = "Prioridade")]
    public Prioridade Prioridade { get; set; }

    [Display(Name = "Técnico responsável")]
    public int? TecnicoId { get; set; }

    [StringLength(4000)]
    [Display(Name = "Solução aplicada")]
    public string? Solucao { get; set; }

    public SelectList? Tecnicos { get; set; }
}
