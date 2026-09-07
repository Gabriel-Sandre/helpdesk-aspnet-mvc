using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(120)]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(160)]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [StringLength(300)]
    public string SenhaHash { get; set; } = string.Empty;

    [Display(Name = "Perfil")]
    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Usuario;

    [StringLength(80)]
    [Display(Name = "Setor")]
    public string? Setor { get; set; }

    [Display(Name = "Ativo")]
    public bool Ativo { get; set; } = true;

    [Display(Name = "Criado em")]
    public DateTime CriadoEm { get; set; } = DateTime.Now;

    public ICollection<Chamado> ChamadosAbertos { get; set; } = new List<Chamado>();
    public ICollection<Chamado> ChamadosAtribuidos { get; set; } = new List<Chamado>();
    public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
}
