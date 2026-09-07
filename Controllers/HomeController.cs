using System.Diagnostics;
using HelpDesk.Data;
using HelpDesk.Models;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Controllers;

[Authorize]
public class HomeController : BaseController
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        IQueryable<Chamado> consulta = _db.Chamados
            .Include(c => c.Categoria)
            .Include(c => c.Solicitante)
            .Include(c => c.Tecnico);

        if (!EhEquipe)
            consulta = consulta.Where(c => c.SolicitanteId == UsuarioId);

        var lista = await consulta.ToListAsync();

        var resolvidos = lista
            .Where(c => c.DataFechamento != null)
            .ToList();

        var vm = new DashboardViewModel
        {
            TotalChamados = lista.Count,
            Abertos = lista.Count(c => c.Status == StatusChamado.Aberto),
            EmAndamento = lista.Count(c => c.Status == StatusChamado.EmAndamento
                                        || c.Status == StatusChamado.AguardandoUsuario),
            Resolvidos = lista.Count(c => c.Status == StatusChamado.Resolvido),
            Fechados = lista.Count(c => c.Status == StatusChamado.Fechado),
            SemTecnico = lista.Count(c => c.TecnicoId == null && !c.EstaEncerrado),
            MediaDiasResolucao = resolvidos.Count == 0
                ? 0
                : Math.Round(resolvidos.Average(c => (c.DataFechamento!.Value - c.DataAbertura).TotalDays), 1),
            PorCategoria = lista
                .GroupBy(c => c.Categoria?.Nome ?? "Sem categoria")
                .OrderByDescending(g => g.Count())
                .ToDictionary(g => g.Key, g => g.Count()),
            PorPrioridade = lista
                .GroupBy(c => c.Prioridade)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),
            UltimosChamados = lista
                .OrderByDescending(c => c.DataAbertura)
                .Take(5)
                .ToList()
        };

        return View(vm);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
