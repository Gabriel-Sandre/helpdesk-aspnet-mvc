using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models;

public class Categoria
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome da categoria.")]
    [StringLength(80)]
    [Display(Name = "Categoria")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    public ICollection<Chamado> Chamados { get; set; } = new List<Chamado>();
}
