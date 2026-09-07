using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models;

public class Comentario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Escreva o comentário.")]
    [StringLength(2000, MinimumLength = 2)]
    [Display(Name = "Comentário")]
    public string Texto { get; set; } = string.Empty;

    public int ChamadoId { get; set; }
    public Chamado? Chamado { get; set; }

    public int AutorId { get; set; }
    public Usuario? Autor { get; set; }

    [Display(Name = "Data")]
    public DateTime CriadoEm { get; set; } = DateTime.Now;

    [Display(Name = "Interno (somente equipe)")]
    public bool Interno { get; set; }
}
