using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models;

public enum PerfilUsuario
{
    [Display(Name = "Usuário")]
    Usuario = 0,

    [Display(Name = "Técnico")]
    Tecnico = 1,

    [Display(Name = "Administrador")]
    Admin = 2
}

public enum StatusChamado
{
    [Display(Name = "Aberto")]
    Aberto = 0,

    [Display(Name = "Em andamento")]
    EmAndamento = 1,

    [Display(Name = "Aguardando usuário")]
    AguardandoUsuario = 2,

    [Display(Name = "Resolvido")]
    Resolvido = 3,

    [Display(Name = "Fechado")]
    Fechado = 4
}

public enum Prioridade
{
    [Display(Name = "Baixa")]
    Baixa = 0,

    [Display(Name = "Média")]
    Media = 1,

    [Display(Name = "Alta")]
    Alta = 2,

    [Display(Name = "Crítica")]
    Critica = 3
}
