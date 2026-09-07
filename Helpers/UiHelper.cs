using System.ComponentModel.DataAnnotations;
using System.Reflection;
using HelpDesk.Models;

namespace HelpDesk.Helpers;

public static class UiHelper
{
    /// <summary>Devolve o texto do atributo [Display] do enum, ou o próprio nome.</summary>
    public static string Texto(this Enum valor)
    {
        MemberInfo? membro = valor.GetType().GetMember(valor.ToString()).FirstOrDefault();
        var display = membro?.GetCustomAttribute<DisplayAttribute>();
        return display?.Name ?? valor.ToString();
    }

    public static string CorStatus(StatusChamado status) => status switch
    {
        StatusChamado.Aberto => "primary",
        StatusChamado.EmAndamento => "info",
        StatusChamado.AguardandoUsuario => "warning",
        StatusChamado.Resolvido => "success",
        StatusChamado.Fechado => "secondary",
        _ => "secondary"
    };

    public static string CorPrioridade(Prioridade prioridade) => prioridade switch
    {
        Prioridade.Baixa => "secondary",
        Prioridade.Media => "primary",
        Prioridade.Alta => "warning",
        Prioridade.Critica => "danger",
        _ => "secondary"
    };

    public static string CorPerfil(PerfilUsuario perfil) => perfil switch
    {
        PerfilUsuario.Admin => "danger",
        PerfilUsuario.Tecnico => "info",
        _ => "secondary"
    };
}
