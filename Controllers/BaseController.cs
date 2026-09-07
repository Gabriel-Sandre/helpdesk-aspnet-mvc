using System.Security.Claims;
using HelpDesk.Models;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers;

public abstract class BaseController : Controller
{
    protected int UsuarioId
    {
        get
        {
            var valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(valor, out int id) ? id : 0;
        }
    }

    protected string UsuarioNome => User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    protected PerfilUsuario PerfilAtual
    {
        get
        {
            var valor = User.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<PerfilUsuario>(valor, out var perfil) ? perfil : PerfilUsuario.Usuario;
        }
    }

    protected bool EhEquipe => PerfilAtual is PerfilUsuario.Admin or PerfilUsuario.Tecnico;

    protected bool EhAdmin => PerfilAtual == PerfilUsuario.Admin;

    protected void Aviso(string mensagem, string tipo = "success")
    {
        TempData["Mensagem"] = mensagem;
        TempData["MensagemTipo"] = tipo;
    }
}
