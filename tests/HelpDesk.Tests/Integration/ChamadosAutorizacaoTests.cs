using System.Net;
using System.Text.RegularExpressions;
using HelpDesk.Data;
using HelpDesk.Models;
using HelpDesk.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Tests.Integration;

public class ChamadosAutorizacaoTests : IClassFixture<HelpDeskWebApplicationFactory>
{
    private const string Senha = "senha-de-teste-123";

    private static readonly Regex TokenAntiforgery =
        new(@"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");

    private readonly HelpDeskWebApplicationFactory _factory;

    public ChamadosAutorizacaoTests(HelpDeskWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Details_SolicitanteAcessaProprioChamado_Retorna200()
    {
        Usuario dono = await CriarUsuarioAsync();
        Chamado chamado = await CriarChamadoAsync(dono.Id, "Impressora do RH sem toner");
        HttpClient client = CriarClient();
        await EntrarAsync(client, dono.Email);

        HttpResponseMessage resposta = await client.GetAsync($"/Chamados/Details/{chamado.Id}");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Contains(chamado.Titulo, await resposta.Content.ReadAsStringAsync());
    }

    // Numa aplicação MVC com autenticação por cookie, o Forbid() não devolve 403 ao navegador:
    // o middleware de cookie converte o "acesso negado" em um redirecionamento (302)
    // para a AccessDeniedPath configurada no Program.cs (/Conta/AcessoNegado).
    [Fact]
    public async Task Details_UsuarioComumAcessaChamadoDeOutroUsuario_RedirecionaParaAcessoNegado()
    {
        Usuario dono = await CriarUsuarioAsync();
        Usuario outroUsuario = await CriarUsuarioAsync();
        Chamado chamado = await CriarChamadoAsync(dono.Id, "Chamado que so o dono pode ver");
        HttpClient client = CriarClient();
        await EntrarAsync(client, outroUsuario.Email);

        HttpResponseMessage resposta = await client.GetAsync($"/Chamados/Details/{chamado.Id}");

        Assert.Equal(HttpStatusCode.Redirect, resposta.StatusCode);
        Assert.Contains("/Conta/AcessoNegado", resposta.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Details_SemLogin_RedirecionaParaLogin()
    {
        Usuario dono = await CriarUsuarioAsync();
        Chamado chamado = await CriarChamadoAsync(dono.Id, "Chamado acessado sem login");
        HttpClient client = CriarClient();

        HttpResponseMessage resposta = await client.GetAsync($"/Chamados/Details/{chamado.Id}");

        Assert.Equal(HttpStatusCode.Redirect, resposta.StatusCode);
        Assert.Contains("/Conta/Login", resposta.Headers.Location?.OriginalString);
    }

    // Sem seguir redirecionamentos: queremos ver o status que a aplicação devolveu,
    // e não o da página final para onde o navegador seria levado.
    private HttpClient CriarClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    // Faz login pelo formulário real, como o navegador: busca a página, lê o token
    // antiforgery e envia o POST. O cookie de autenticação fica guardado no client.
    private static async Task EntrarAsync(HttpClient client, string email)
    {
        string paginaDeLogin = await client.GetStringAsync("/Conta/Login");
        Match token = TokenAntiforgery.Match(paginaDeLogin);
        Assert.True(token.Success, "Token antiforgery não encontrado na página de login.");

        HttpResponseMessage resposta = await client.PostAsync("/Conta/Login", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["Email"] = email,
                ["Senha"] = Senha,
                ["__RequestVerificationToken"] = token.Groups[1].Value
            }));

        // Login válido redireciona para o painel; inválido devolve a própria tela (200).
        Assert.Equal(HttpStatusCode.Redirect, resposta.StatusCode);
    }

    // Cada teste cria os próprios dados, com e-mails únicos, sem depender do seed.
    private async Task<Usuario> CriarUsuarioAsync()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var usuario = new Usuario
        {
            Nome = "Usuario de teste",
            Email = $"usuario-{Guid.NewGuid():N}@teste.local",
            SenhaHash = SenhaHasher.Gerar(Senha),
            Perfil = PerfilUsuario.Usuario
        };

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return usuario;
    }

    // Títulos sem acento: o Razor codifica caracteres acentuados no HTML,
    // o que faria o Assert.Contains falhar por um motivo que não é o testado.
    private async Task<Chamado> CriarChamadoAsync(int solicitanteId, string titulo)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var chamado = new Chamado
        {
            Titulo = titulo,
            Descricao = "Chamado criado pelo teste de integracao.",
            Categoria = new Categoria { Nome = $"Categoria {Guid.NewGuid():N}" },
            SolicitanteId = solicitanteId
        };

        db.Chamados.Add(chamado);
        await db.SaveChangesAsync();
        return chamado;
    }
}
