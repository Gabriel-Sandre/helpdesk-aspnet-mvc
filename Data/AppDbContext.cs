using HelpDesk.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Chamado> Chamados => Set<Chamado>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Perfil).HasConversion<int>();
        });

        modelBuilder.Entity<Categoria>(e =>
        {
            e.HasIndex(c => c.Nome).IsUnique();
        });

        modelBuilder.Entity<Chamado>(e =>
        {
            e.Property(c => c.Status).HasConversion<int>();
            e.Property(c => c.Prioridade).HasConversion<int>();

            e.HasOne(c => c.Categoria)
             .WithMany(cat => cat.Chamados)
             .HasForeignKey(c => c.CategoriaId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.Solicitante)
             .WithMany(u => u.ChamadosAbertos)
             .HasForeignKey(c => c.SolicitanteId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.Tecnico)
             .WithMany(u => u.ChamadosAtribuidos)
             .HasForeignKey(c => c.TecnicoId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(c => c.Status);
            e.HasIndex(c => c.DataAbertura);
        });

        modelBuilder.Entity<Comentario>(e =>
        {
            e.HasOne(c => c.Chamado)
             .WithMany(ch => ch.Comentarios)
             .HasForeignKey(c => c.ChamadoId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.Autor)
             .WithMany(u => u.Comentarios)
             .HasForeignKey(c => c.AutorId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
