using HelpDesk.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HelpDesk.Tests.Integration;

/// <summary>
/// Sobe a aplicação real (Program.cs) em memória para os testes de integração,
/// trocando apenas o banco: SQLite em memória, isolado do helpdesk.db e do SQL Server.
/// </summary>
public class HelpDeskWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove o banco configurado no Program.cs.
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();

            // Um banco SQLite em memória só existe enquanto a conexão estiver aberta.
            // Uma conexão única (singleton) mantém os dados durante toda a vida da factory
            // e é descartada junto com ela.
            services.AddSingleton(_ =>
            {
                var conexao = new SqliteConnection("Data Source=:memory:");
                conexao.Open();
                return conexao;
            });

            services.AddDbContext<AppDbContext>((provider, options) =>
                options.UseSqlite(provider.GetRequiredService<SqliteConnection>()));
        });
    }
}
