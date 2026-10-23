using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SistemaBancario.Dominio.Entidades;
using SistemaBancario.Dominio.Persistencia;

namespace SistemaBancario.Endpoints;

public static class ContasEndpoints
{
    public const string ContaNaoEncontrada = "Conta não encontrada!";

    public static RouteGroupBuilder MapContas(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/contas").WithTags("Contas correntes");

        // RF-14
        grupo.MapGet("/", (BancoDbContext db) => ConsultaContas(db).ToListAsync());

        // RF-10 e RF-12 (campo Ativa).
        grupo.MapGet("/{id:int}", async Task<Results<Ok<ContaDto>, ProblemHttpResult>> (int id, BancoDbContext db) =>
        {
            var conta = await ObtemConta(db, id);
            return conta is null
                ? TypedResults.Problem(ContaNaoEncontrada, statusCode: StatusCodes.Status404NotFound)
                : TypedResults.Ok(conta);
        });

        // RF-11
        grupo.MapPost("/", async Task<Results<Created<ContaDto>, ProblemHttpResult>> (NovaContaRequisicao req, BancoDbContext db) =>
        {
            if (req.SaldoInicial < 0)
                return TypedResults.Problem("O saldo inicial não pode ser negativo.", statusCode: StatusCodes.Status400BadRequest);

            if (!TemAteDuasCasas(req.SaldoInicial))
                return TypedResults.Problem("O saldo inicial pode ter no máximo 2 casas decimais.", statusCode: StatusCodes.Status400BadRequest);

            var conta = new ContaCorrente(req.SaldoInicial);
            db.ContasCorrentes.Add(conta);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/api/contas/{conta.Id}", new ContaDto(conta.Id, conta.Saldo, conta.Ativa, null, null));
        });

        grupo.MapPost("/{id:int}/ativar", (int id, BancoDbContext db) =>
            AlteraSituacao(id, db, ativar: true));

        grupo.MapPost("/{id:int}/desativar", (int id, BancoDbContext db) =>
            AlteraSituacao(id, db, ativar: false));

        // RF-13: o banco impede remover conta vinculada a cliente (sem referência órfã).
        grupo.MapDelete("/{id:int}", async Task<Results<NoContent, ProblemHttpResult>> (int id, BancoDbContext db) =>
        {
            var titular = await db.Clientes.AsNoTracking()
                .Where(c => c.ContaCorrenteId == id)
                .Select(c => c.Nome)
                .FirstOrDefaultAsync();
            if (titular is not null)
                return TypedResults.Problem($"A conta {id} pertence ao cliente {titular} e não pode ser removida.", statusCode: StatusCodes.Status409Conflict);

            var removidas = await db.ContasCorrentes.Where(c => c.Id == id).ExecuteDeleteAsync();
            return removidas == 0
                ? TypedResults.Problem(ContaNaoEncontrada, statusCode: StatusCodes.Status404NotFound)
                : TypedResults.NoContent();
        });

        return grupo;
    }

    private static async Task<Results<Ok<OperacaoContaResposta>, ProblemHttpResult>> AlteraSituacao(int id, BancoDbContext db, bool ativar)
    {
        var conta = await db.ContasCorrentes.FindAsync(id);
        if (conta is null)
            return TypedResults.Problem(ContaNaoEncontrada, statusCode: StatusCodes.Status404NotFound);

        var alterado = conta.Ativa != ativar;
        string mensagem;
        if (!alterado)
        {
            mensagem = ativar ? "Conta já ativa!" : "Conta já inativa!";
        }
        else
        {
            if (ativar)
                conta.Ativar();
            else
                conta.Desativar();

            await db.SaveChangesAsync();
            mensagem = ativar ? "Conta ativada com sucesso!" : "Conta desativada com sucesso!";
        }

        return TypedResults.Ok(new OperacaoContaResposta(alterado, mensagem, (await ObtemConta(db, id))!));
    }

    internal static Task<ContaDto?> ObtemConta(BancoDbContext db, int id) =>
        ConsultaContas(db, db.ContasCorrentes.Where(c => c.Id == id)).FirstOrDefaultAsync();

    // O SQLite não aplica a precisão decimal(18,2): sem esta checagem, 10.999 seria gravado como está.
    internal static bool TemAteDuasCasas(decimal valor) => decimal.Round(valor, 2) == valor;

    // O filtro entra antes da projeção: o EF não traduz predicados sobre o record ContaDto.
    // Ordena só por Id: no SQLite o decimal vira TEXT e comparações/ordenação por Saldo não são traduzidas.
    private static IQueryable<ContaDto> ConsultaContas(BancoDbContext db, IQueryable<ContaCorrente>? contas = null) =>
        from conta in (contas ?? db.ContasCorrentes).AsNoTracking()
        join cliente in db.Clientes.AsNoTracking() on conta.Id equals cliente.ContaCorrenteId into titulares
        from titular in titulares.DefaultIfEmpty()
        orderby conta.Id
        select new ContaDto(conta.Id, conta.Saldo, conta.Ativa, titular == null ? null : titular.Id, titular == null ? null : titular.Nome);
}
