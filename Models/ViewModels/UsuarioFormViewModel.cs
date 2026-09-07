using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models.ViewModels;

public class UsuarioFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(120)]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [StringLength(60, MinimumLength = 4, ErrorMessage = "A senha deve ter pelo menos 4 caracteres.")]
    [Display(Name = "Senha")]
    public string? Senha { get; set; }

    [Display(Name = "Perfil")]
    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Usuario;

    [StringLength(80)]
    [Display(Name = "Setor")]
    public string? Setor { get; set; }

    [Display(Name = "Ativo")]
    public bool Ativo { get; set; } = true;
}
