using HelpDesk.Models;
using HelpDesk.Services;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Data;

public static class DbInitializer
{
    /// <summary>
    /// Cria o banco (se não existir) e popula com dados iniciais para testes.
    /// </summary>
    public static void Seed(AppDbContext db)
    {
        db.Database.EnsureCreated();

        if (!db.Categorias.Any())
        {
            db.Categorias.AddRange(
                new Categoria { Nome = "Hardware", Descricao = "Computadores, impressoras e periféricos" },
                new Categoria { Nome = "Software", Descricao = "Instalação, erros e licenças de programas" },
                new Categoria { Nome = "Rede e Internet", Descricao = "Conectividade, Wi-Fi, VPN" },
                new Categoria { Nome = "Acesso e Senhas", Descricao = "Contas, permissões e redefinição de senha" },
                new Categoria { Nome = "E-mail", Descricao = "Caixa postal, envio e recebimento" },
                new Categoria { Nome = "Outros", Descricao = "Solicitações diversas" }
            );
            db.SaveChanges();
        }

        if (!db.Usuarios.Any())
        {
            db.Usuarios.AddRange(
                new Usuario
                {
                    Nome = "Administrador",
                    Email = "admin@helpdesk.local",
                    SenhaHash = SenhaHasher.Gerar("admin123"),
                    Perfil = PerfilUsuario.Admin,
                    Setor = "TI"
                },
                new Usuario
                {
                    Nome = "Gabriel Sandre",
                    Email = "tecnico@helpdesk.local",
                    SenhaHash = SenhaHasher.Gerar("tecnico123"),
                    Perfil = PerfilUsuario.Tecnico,
                    Setor = "TI"
                },
                new Usuario
                {
                    Nome = "Maria Souza",
                    Email = "usuario@helpdesk.local",
                    SenhaHash = SenhaHasher.Gerar("usuario123"),
                    Perfil = PerfilUsuario.Usuario,
                    Setor = "Financeiro"
                }
            );
            db.SaveChanges();
        }

        if (!db.Chamados.Any())
        {
            var admin = db.Usuarios.First(u => u.Perfil == PerfilUsuario.Admin);
            var tecnico = db.Usuarios.First(u => u.Perfil == PerfilUsuario.Tecnico);
            var usuario = db.Usuarios.First(u => u.Perfil == PerfilUsuario.Usuario);

            var hardware = db.Categorias.First(c => c.Nome == "Hardware");
            var rede = db.Categorias.First(c => c.Nome == "Rede e Internet");
            var acesso = db.Categorias.First(c => c.Nome == "Acesso e Senhas");

            var chamados = new List<Chamado>
            {
                new Chamado
                {
                    Titulo = "Notebook não liga após queda de energia",
                    Descricao = "O notebook do setor financeiro parou de ligar depois da queda de energia de ontem. O LED de carga não acende.",
                    CategoriaId = hardware.Id,
                    Prioridade = Prioridade.Alta,
                    Status = StatusChamado.EmAndamento,
                    SolicitanteId = usuario.Id,
                    TecnicoId = tecnico.Id,
                    DataAbertura = DateTime.Now.AddDays(-3),
                    DataAtualizacao = DateTime.Now.AddDays(-1)
                },
                new Chamado
                {
                    Titulo = "Wi-Fi caindo constantemente na sala 2",
                    Descricao = "A conexão do Wi-Fi cai a cada 10 minutos aproximadamente. Afeta toda a equipe da sala 2.",
                    CategoriaId = rede.Id,
                    Prioridade = Prioridade.Critica,
                    Status = StatusChamado.Aberto,
                    SolicitanteId = usuario.Id,
                    DataAbertura = DateTime.Now.AddDays(-1),
                    DataAtualizacao = DateTime.Now.AddDays(-1)
                },
                new Chamado
                {
                    Titulo = "Redefinir senha do sistema interno",
                    Descricao = "Esqueci a senha de acesso ao sistema interno e a conta foi bloqueada após 3 tentativas.",
                    CategoriaId = acesso.Id,
                    Prioridade = Prioridade.Media,
                    Status = StatusChamado.Resolvido,
                    SolicitanteId = admin.Id,
                    TecnicoId = tecnico.Id,
                    Solucao = "Senha redefinida e conta desbloqueada. Orientado o usuário a trocar no primeiro acesso.",
                    DataAbertura = DateTime.Now.AddDays(-8),
                    DataAtualizacao = DateTime.Now.AddDays(-7),
                    DataFechamento = DateTime.Now.AddDays(-7)
                }
            };

            db.Chamados.AddRange(chamados);
            db.SaveChanges();

            db.Comentarios.Add(new Comentario
            {
                ChamadoId = chamados[0].Id,
                AutorId = tecnico.Id,
                Texto = "Testado com outra fonte de alimentação. Suspeita de problema na placa de carga, encaminhado para bancada.",
                CriadoEm = DateTime.Now.AddDays(-1)
            });
            db.SaveChanges();
        }
    }
}
