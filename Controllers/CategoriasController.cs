using HelpDesk.Data;
using HelpDesk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Controllers;

[Authorize(Policy = "SomenteAdmin")]
public class CategoriasController : BaseController
{
    private readonly AppDbContext _db;

    public CategoriasController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var categorias = await _db.Categorias
            .Include(c => c.Chamados)
            .OrderBy(c => c.Nome)
            .AsNoTracking()
            .ToListAsync();

        return View(categorias);
    }

    public IActionResult Create() => View(new Categoria());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Categoria categoria)
    {
        var nome = categoria.Nome.Trim();
        if (await _db.Categorias.AnyAsync(c => c.Nome == nome))
            ModelState.AddModelError(nameof(Categoria.Nome), "Já existe uma categoria com esse nome.");

        if (!ModelState.IsValid)
            return View(categoria);

        categoria.Nome = nome;
        _db.Categorias.Add(categoria);
        await _db.SaveChangesAsync();

        Aviso("Categoria criada.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var categoria = await _db.Categorias.FindAsync(id);
        if (categoria is null) return NotFound();

        return View(categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Categoria model)
    {
        var categoria = await _db.Categorias.FindAsync(model.Id);
        if (categoria is null) return NotFound();

        var nome = model.Nome.Trim();
        if (await _db.Categorias.AnyAsync(c => c.Id != model.Id && c.Nome == nome))
            ModelState.AddModelError(nameof(Categoria.Nome), "Já existe uma categoria com esse nome.");

        if (!ModelState.IsValid)
            return View(model);

        categoria.Nome = nome;
        categoria.Descricao = model.Descricao?.Trim();
        await _db.SaveChangesAsync();

        Aviso("Categoria atualizada.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _db.Categorias
            .Include(c => c.Chamados)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria is null) return NotFound();

        if (categoria.Chamados.Any())
        {
            Aviso("Não é possível excluir: existem chamados nessa categoria.", "warning");
            return RedirectToAction(nameof(Index));
        }

        _db.Categorias.Remove(categoria);
        await _db.SaveChangesAsync();

        Aviso("Categoria excluída.", "danger");
        return RedirectToAction(nameof(Index));
    }
}
