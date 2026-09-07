using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.Models;

public class Chamado
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o título do chamado.")]
    [StringLength(140, MinimumLength = 5, ErrorMessage = "O título deve ter entre 5 e 140 caracteres.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descreva o problema.")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "Descreva o problema com pelo menos 10 caracteres.")]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = string.Empty;

    [Display(Name = "Categoria")]
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    [Display(Name = "Prioridade")]
    public Prioridade Prioridade { get; set; } = Prioridade.Media;

    [Display(Name = "Status")]
    public StatusChamado Status { get; set; } = StatusChamado.Aberto;

    [Display(Name = "Solicitante")]
    public int SolicitanteId { get; set; }
    public Usuario? Solicitante { get; set; }

    [Display(Name = "Técnico responsável")]
    public int? TecnicoId { get; set; }
    public Usuario? Tecnico { get; set; }

    [Display(Name = "Aberto em")]
    public DateTime DataAbertura { get; set; } = DateTime.Now;

    [Display(Name = "Última atualização")]
    public DateTime DataAtualizacao { get; set; } = DateTime.Now;

    [Display(Name = "Fechado em")]
    public DateTime? DataFechamento { get; set; }

    [StringLength(4000)]
    [Display(Name = "Solução aplicada")]
    public string? Solucao { get; set; }

    public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    [NotMapped]
    public bool EstaEncerrado => Status == StatusChamado.Resolvido || Status == StatusChamado.Fechado;

    [NotMapped]
    public int DiasEmAberto =>
        (int)((DataFechamento ?? DateTime.Now) - DataAbertura).TotalDays;
}
