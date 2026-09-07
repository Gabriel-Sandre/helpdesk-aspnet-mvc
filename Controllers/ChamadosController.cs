using HelpDesk.Data;
using HelpDesk.Models;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Controllers;

[Authorize]
public class ChamadosController : BaseController
{
    private const int TamanhoPagina = 10;
    private readonly AppDbContext _db;

    public ChamadosController(AppDbContext db) => _db = db;

    // GET: /Chamados
    public async Task<IActionResult> Index(ChamadoFiltroViewModel filtro)
    {
        IQueryable<Chamado> consulta = _db.Chamados
            .Include(c => c.Categoria)
            .Include(c => c.Solicitante)
            .Include(c => c.Tecnico)
            .AsNoTracking();

        // Usuário comum só enxerga os próprios chamados
        if (!EhEquipe)
            consulta = consulta.Where(c => c.SolicitanteId == UsuarioId);
        else if (filtro.SomenteMeus)
            consulta = consulta.Where(c => c.TecnicoId == UsuarioId);

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var busca = filtro.Busca.Trim();
            consulta = consulta.Where(c => c.Titulo.Contains(busca) || c.Descricao.Contains(busca));
        }

        if (filtro.Status.HasValue)
            consulta = consulta.Where(c => c.Status == filtro.Status.Value);

        if (filtro.Prioridade.HasValue)
            consulta = consulta.Where(c => c.Prioridade == filtro.Prioridade.Value);

        if (filtro.CategoriaId.HasValue && filtro.CategoriaId > 0)
            consulta = consulta.Where(c => c.CategoriaId == filtro.CategoriaId.Value);

        int total = await consulta.CountAsync();
        int pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
        int totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanhoPagina));
        if (pagina > totalPaginas) pagina = totalPaginas;

        filtro.TotalRegistros = total;
        filtro.TotalPaginas = totalPaginas;
        filtro.Pagina = pagina;
        filtro.Chamados = await consulta
            .OrderByDescending(c => c.Prioridade)
            .ThenByDescending(c => c.DataAbertura)
            .Skip((pagina - 1) * TamanhoPagina)
            .Take(TamanhoPagina)
            .ToListAsync();

        filtro.Categorias = new SelectList(
            await _db.Categorias.OrderBy(c => c.Nome).ToListAsync(),
            nameof(Categoria.Id), nameof(Categoria.Nome), filtro.CategoriaId);

        return View(filtro);
    }

    // GET: /Chamados/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var chamado = await CarregarChamadoAsync(id);
        if (chamado is null) return NotFound();
        if (!PodeVer(chamado)) return Forbid();

        return View(chamado);
    }

    // GET: /Chamados/Create
    public async Task<IActionResult> Create()
    {
        await PrepararCombosAsync();
        return View(new Chamado());
    }

    // POST: /Chamados/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Chamado chamado)
    {
        ModelState.Remove(nameof(Chamado.Solicitante));
        ModelState.Remove(nameof(Chamado.Categoria));
        ModelState.Remove(nameof(Chamado.Tecnico));
        ModelState.Remove(nameof(Chamado.Comentarios));

        if (!await _db.Categorias.AnyAsync(c => c.Id == chamado.CategoriaId))
            ModelState.AddModelError(nameof(Chamado.CategoriaId), "Selecione uma categoria válida.");

        if (!ModelState.IsValid)
        {
            await PrepararCombosAsync();
            return View(chamado);
        }

        chamado.Id = 0;
        chamado.SolicitanteId = UsuarioId;
        chamado.Status = StatusChamado.Aberto;
        chamado.TecnicoId = null;
        chamado.DataAbertura = DateTime.Now;
        chamado.DataAtualizacao = DateTime.Now;
        chamado.DataFechamento = null;
        chamado.Solucao = null;

        _db.Chamados.Add(chamado);
        await _db.SaveChangesAsync();

        Aviso($"Chamado #{chamado.Id} aberto com sucesso.");
        return RedirectToAction(nameof(Details), new { id = chamado.Id });
    }

    // POST: /Chamados/Comentar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Comentar(int id, string texto, bool interno = false)
    {
        var chamado = await _db.Chamados.FindAsync(id);
        if (chamado is null) return NotFound();
        if (!PodeVer(chamado)) return Forbid();

        if (string.IsNullOrWhiteSpace(texto))
        {
            Aviso("Escreva um comentário antes de enviar.", "warning");
            return RedirectToAction(nameof(Details), new { id });
        }

        _db.Comentarios.Add(new Comentario
        {
            ChamadoId = id,
            AutorId = UsuarioId,
            Texto = texto.Trim(),
            Interno = interno && EhEquipe,
            CriadoEm = DateTime.Now
        });

        chamado.DataAtualizacao = DateTime.Now;
        await _db.SaveChangesAsync();

        Aviso("Comentário registrado.");
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: /Chamados/Atender/5
    [Authorize(Policy = "Equipe")]
    public async Task<IActionResult> Atender(int id)
    {
        var chamado = await CarregarChamadoAsync(id);
        if (chamado is null) return NotFound();

        var vm = new AtenderChamadoViewModel
        {
            ChamadoId = chamado.Id,
            Status = chamado.Status,
            Prioridade = chamado.Prioridade,
            TecnicoId = chamado.TecnicoId,
            Solucao = chamado.Solucao,
            Tecnicos = await MontarComboTecnicosAsync(chamado.TecnicoId)
        };

        ViewBag.Chamado = chamado;
        return View(vm);
    }

    // POST: /Chamados/Atender/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Equipe")]
    public async Task<IActionResult> Atender(AtenderChamadoViewModel model)
    {
        ModelState.Remove(nameof(model.Tecnicos));

        var chamado = await CarregarChamadoAsync(model.ChamadoId);
        if (chamado is null) return NotFound();

        bool exigeSolucao = model.Status is StatusChamado.Resolvido or StatusChamado.Fechado;
        if (exigeSolucao && string.IsNullOrWhiteSpace(model.Solucao))
            ModelState.AddModelError(nameof(model.Solucao), "Descreva a solução para encerrar o chamado.");

        if (!ModelState.IsValid)
        {
            model.Tecnicos = await MontarComboTecnicosAsync(model.TecnicoId);
            ViewBag.Chamado = chamado;
            return View(model);
        }

        chamado.Status = model.Status;
        chamado.Prioridade = model.Prioridade;
        chamado.TecnicoId = model.TecnicoId;
        chamado.Solucao = model.Solucao?.Trim();
        chamado.DataAtualizacao = DateTime.Now;
        chamado.DataFechamento = exigeSolucao ? (chamado.DataFechamento ?? DateTime.Now) : null;

        await _db.SaveChangesAsync();

        Aviso($"Chamado #{chamado.Id} atualizado.");
        return RedirectToAction(nameof(Details), new { id = chamado.Id });
    }

    // POST: /Chamados/Assumir/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Equipe")]
    public async Task<IActionResult> Assumir(int id)
    {
        var chamado = await _db.Chamados.FindAsync(id);
        if (chamado is null) return NotFound();

        chamado.TecnicoId = UsuarioId;
        if (chamado.Status == StatusChamado.Aberto)
            chamado.Status = StatusChamado.EmAndamento;
        chamado.DataAtualizacao = DateTime.Now;

        await _db.SaveChangesAsync();

        Aviso($"Você assumiu o chamado #{chamado.Id}.");
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Chamados/Reabrir/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reabrir(int id)
    {
        var chamado = await _db.Chamados.FindAsync(id);
        if (chamado is null) return NotFound();
        if (!PodeVer(chamado)) return Forbid();

        chamado.Status = StatusChamado.Aberto;
        chamado.DataFechamento = null;
        chamado.DataAtualizacao = DateTime.Now;

        await _db.SaveChangesAsync();

        Aviso($"Chamado #{chamado.Id} reaberto.", "warning");
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Chamados/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "SomenteAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var chamado = await _db.Chamados.FindAsync(id);
        if (chamado is null) return NotFound();

        _db.Chamados.Remove(chamado);
        await _db.SaveChangesAsync();

        Aviso($"Chamado #{id} excluído.", "danger");
        return RedirectToAction(nameof(Index));
    }

    private async Task<Chamado?> CarregarChamadoAsync(int id) =>
        await _db.Chamados
            .Include(c => c.Categoria)
            .Include(c => c.Solicitante)
            .Include(c => c.Tecnico)
            .Include(c => c.Comentarios).ThenInclude(c => c.Autor)
            .FirstOrDefaultAsync(c => c.Id == id);

    private bool PodeVer(Chamado chamado) => EhEquipe || chamado.SolicitanteId == UsuarioId;

    private async Task PrepararCombosAsync()
    {
        ViewBag.Categorias = new SelectList(
            await _db.Categorias.OrderBy(c => c.Nome).ToListAsync(),
            nameof(Categoria.Id), nameof(Categoria.Nome));
    }

    private async Task<SelectList> MontarComboTecnicosAsync(int? selecionado)
    {
        var tecnicos = await _db.Usuarios
            .Where(u => u.Ativo && (u.Perfil == PerfilUsuario.Tecnico || u.Perfil == PerfilUsuario.Admin))
            .OrderBy(u => u.Nome)
            .ToListAsync();

        return new SelectList(tecnicos, nameof(Usuario.Id), nameof(Usuario.Nome), selecionado);
    }
}
