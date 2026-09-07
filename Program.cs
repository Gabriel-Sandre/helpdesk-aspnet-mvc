using HelpDesk.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Banco de dados (EF Core) - o provedor e escolhido em appsettings.json
// "DatabaseProvider": "Sqlite"    -> banco em arquivo, nao precisa instalar nada
// "DatabaseProvider": "SqlServer" -> SQL Server local, visivel no SSMS
var provider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    switch (provider.Trim().ToLowerInvariant())
    {
        case "sqlserver":
            options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")
                ?? throw new InvalidOperationException(
                    "Configure ConnectionStrings:SqlServer no appsettings.json."));
            break;

        case "sqlite":
            options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")
                ?? "Data Source=helpdesk.db");
            break;

        default:
            throw new InvalidOperationException(
                $"DatabaseProvider '{provider}' invalido. Use \"Sqlite\" ou \"SqlServer\".");
    }
});

// Autenticação por cookie
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Conta/Login";
        options.LogoutPath = "/Conta/Logout";
        options.AccessDeniedPath = "/Conta/AcessoNegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "HelpDesk.Auth";
        options.Cookie.HttpOnly = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Equipe", policy => policy.RequireRole("Admin", "Tecnico"));
    options.AddPolicy("SomenteAdmin", policy => policy.RequireRole("Admin"));
});

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Cria e popula o banco na inicialização
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbInitializer.Seed(db);
    app.Logger.LogInformation("Banco de dados em uso: {Provider}", provider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
