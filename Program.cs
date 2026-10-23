using Microsoft.EntityFrameworkCore;
using SistemaBancario.Dominio.Persistencia;
using SistemaBancario.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Erros respondem como ProblemDetails; o cliente Vue exibe o campo "detail".
builder.Services.AddProblemDetails();

// Corpo JSON inválido vira 400, também em Development (onde o padrão é lançar exceção → 500).
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = false);

// As migrations ficam na API, junto com a escolha do provedor; o domínio segue independente de banco.
builder.Services.AddDbContext<BancoDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("Banco"),
        sqlite => sqlite.MigrationsAssembly(typeof(Program).Assembly.GetName().Name)));

var app = builder.Build();

// Cria/atualiza o banco ao iniciar, incluindo os dados pré-cadastrados (RF-18).
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<BancoDbContext>().Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    // Em Development o proxy do Vite chama http://localhost:5285; redirecionar para https://localhost:7091
    // faria o navegador sair do proxy e esbarrar em CORS (a API não tem política de CORS).
    app.UseHttpsRedirection();
}

app.MapClientes();
app.MapContas();
app.MapTransferencias();

app.Run();
