using HelpDesk.Data;
using HelpDesk.Models;
using HelpDesk.Models.ViewModels;
using HelpDesk.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Controllers;

[Authorize(Policy = "SomenteAdmin")]
public class UsuariosController : BaseController
{
    private readonly AppDbContext _db;

    public UsuariosController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var usuarios = await _db.Usuarios
            .OrderBy(u => u.Nome)
            .AsNoTracking()
            .ToListAsync();

        return View(usuarios);
    }

    public IActionResult Create() => View(new UsuarioFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Senha))
            ModelState.AddModelError(nameof(model.Senha), "Informe a senha inicial.");

        var email = model.Email.Trim().ToLower();
        if (await _db.Usuarios.AnyAsync(u => u.Email.ToLower() == email))
            ModelState.AddModelError(nameof(model.Email), "Já existe um usuário com esse e-mail.");

        if (!ModelState.IsValid)
            return View(model);

        _db.Usuarios.Add(new Usuario
        {
            Nome = model.Nome.Trim(),
            Email = email,
            SenhaHash = SenhaHasher.Gerar(model.Senha!),
            Perfil = model.Perfil,
            Setor = model.Setor?.Trim(),
            Ativo = model.Ativo,
            CriadoEm = DateTime.Now
        });

        await _db.SaveChangesAsync();

        Aviso("Usuário cadastrado com sucesso.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        return View(new UsuarioFormViewModel
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Perfil = usuario.Perfil,
            Setor = usuario.Setor,
            Ativo = usuario.Ativo
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UsuarioFormViewModel model)
    {
        var usuario = await _db.Usuarios.FindAsync(model.Id);
        if (usuario is null) return NotFound();

        var email = model.Email.Trim().ToLower();
        if (await _db.Usuarios.AnyAsync(u => u.Id != model.Id && u.Email.ToLower() == email))
            ModelState.AddModelError(nameof(model.Email), "Já existe um usuário com esse e-mail.");

        if (!ModelState.IsValid)
            return View(model);

        usuario.Nome = model.Nome.Trim();
        usuario.Email = email;
        usuario.Perfil = model.Perfil;
        usuario.Setor = model.Setor?.Trim();
        usuario.Ativo = model.Ativo;

        if (!string.IsNullOrWhiteSpace(model.Senha))
            usuario.SenhaHash = SenhaHasher.Gerar(model.Senha);

        await _db.SaveChangesAsync();

        Aviso("Usuário atualizado.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarAtivo(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        if (usuario.Id == UsuarioId)
        {
            Aviso("Você não pode desativar a própria conta.", "warning");
            return RedirectToAction(nameof(Index));
        }

        usuario.Ativo = !usuario.Ativo;
        await _db.SaveChangesAsync();

        Aviso($"Usuário {(usuario.Ativo ? "ativado" : "desativado")}.");
        return RedirectToAction(nameof(Index));
    }
}
