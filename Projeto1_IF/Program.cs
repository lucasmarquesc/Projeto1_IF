using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Data;
using Projeto1_IF.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDbContext<db_IFContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // Isso "avisa" ao ASP.NET Identity que seu aplicativo usará o sistema de Funções (Roles).
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Chama a função para criar as Roles (Funções) na inicialização
// Isso será executado uma vez e criará as roles no banco de dados.
await SeedRoles(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

async Task SeedRoles(WebApplication app)
{
    // Cria um "escopo" para pegar os serviços necessários
    using (var scope = app.Services.CreateScope())
    {
        // Pega o RoleManager (serviço do Identity para gerenciar funções)
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Pega o Logger (para registrar informação no console)
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

       // Lista de nomes das roles
        string[] roleNames = {
            "Médico",
            "Nutricionista",
            "Gerente Médico",
            "Gerente Nutricionista",
            "Gerente Geral"
        };

        foreach (var roleName in roleNames)
        {
            try
            {
                // Verifica se a role já existe no banco de dados
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Se não existir, cria a role
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    logger.LogInformation($"Role '{roleName}' criada com sucesso.");
                }
            }
            catch (Exception ex)
            {
                // Registra qualquer erro que possa acontecer
                logger.LogError(ex, $"Erro ao criar a role '{roleName}'.");
            }
        }
    }
}