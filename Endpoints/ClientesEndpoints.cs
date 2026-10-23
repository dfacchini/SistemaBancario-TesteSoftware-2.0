using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SistemaBancario.Dominio.Entidades;
using SistemaBancario.Dominio.Excecoes;
using SistemaBancario.Dominio.Persistencia;

namespace SistemaBancario.Endpoints;

public static class ClientesEndpoints
{
    public const string ClienteNaoEncontrado = "Cliente não encontrado!";

    public static RouteGroupBuilder MapClientes(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/clientes").WithTags("Clientes");

        // RF-09
        grupo.MapGet("/", async (BancoDbContext db) =>
            await db.Clientes.AsNoTracking()
                .OrderBy(c => c.Id)
                .Select(c => new ClienteDto(c.Id, c.Nome, c.Idade, c.Email, c.Ativo, c.ContaCorrenteId))
                .ToListAsync());

        // RF-01 (inativos também são encontrados) e RF-04 (campo Ativo).
        grupo.MapGet("/{id:int}", async Task<Results<Ok<ClienteDto>, ProblemHttpResult>> (int id, BancoDbContext db) =>
        {
            var cliente = await db.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            return cliente is null
                ? TypedResults.Problem(ClienteNaoEncontrado, statusCode: StatusCodes.Status404NotFound)
                : TypedResults.Ok(ParaDto(cliente));
        });

        // RF-05 com RF-06 aplicada no cadastro.
        grupo.MapPost("/", async Task<Results<Created<ClienteDto>, ProblemHttpResult>> (NovoClienteRequisicao req, BancoDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.Nome) || req.Nome.Length > Cliente.NomeTamanhoMaximo)
                return Requisicao($"Informe o nome do cliente (até {Cliente.NomeTamanhoMaximo} caracteres).");

            if (string.IsNullOrWhiteSpace(req.Email) || req.Email.Length > Cliente.EmailTamanhoMaximo)
                return Requisicao($"Informe o e-mail do cliente (até {Cliente.EmailTamanhoMaximo} caracteres).");

            try
            {
                Cliente.ValidaIdade(req.Idade);
            }
            catch (IdadeNaoPermitidaException ex)
            {
                return Requisicao(ex.Message);
            }

            await using var transacao = await db.Database.BeginTransactionAsync();

            int contaId;
            if (req.ContaCorrenteId is int idInformado)
            {
                if (!await db.ContasCorrentes.AnyAsync(c => c.Id == idInformado))
                    return TypedResults.Problem(ContasEndpoints.ContaNaoEncontrada, statusCode: StatusCodes.Status404NotFound);

                // Cada conta pertence a um único cliente.
                if (await db.Clientes.AnyAsync(c => c.ContaCorrenteId == idInformado))
                    return TypedResults.Problem($"A conta {idInformado} já pertence a outro cliente.", statusCode: StatusCodes.Status409Conflict);

                contaId = idInformado;
            }
            else
            {
                var conta = new ContaCorrente();
                db.ContasCorrentes.Add(conta);
                await db.SaveChangesAsync();
                contaId = conta.Id;
            }

            var cliente = new Cliente(req.Nome.Trim(), req.Idade, req.Email.Trim(), contaId);
            db.Clientes.Add(cliente);
            await db.SaveChangesAsync();
            await transacao.CommitAsync();

            return TypedResults.Created($"/api/clientes/{cliente.Id}", ParaDto(cliente));
        });

        // RF-02 / HU-02
        grupo.MapPost("/{id:int}/ativar", (int id, BancoDbContext db) =>
            AlteraSituacao(id, db, c => c.Ativar(), "Cliente ativado com sucesso!", "Cliente já ativo!"));

        // RF-03 / HU-03
        grupo.MapPost("/{id:int}/desativar", (int id, BancoDbContext db) =>
            AlteraSituacao(id, db, c => c.Desativar(), "Cliente desativado com sucesso!", "Cliente já inativo!"));

        // RF-07: a conta corrente do cliente não é removida.
        grupo.MapDelete("/{id:int}", async Task<Results<NoContent, ProblemHttpResult>> (int id, BancoDbContext db) =>
        {
            var removidos = await db.Clientes.Where(c => c.Id == id).ExecuteDeleteAsync();
            return removidos == 0
                ? TypedResults.Problem(ClienteNaoEncontrado, statusCode: StatusCodes.Status404NotFound)
                : TypedResults.NoContent();
        });

        // RF-08: as contas correntes permanecem.
        grupo.MapDelete("/", async (BancoDbContext db) =>
        {
            var removidos = await db.Clientes.ExecuteDeleteAsync();
            return new LimpezaClientesResposta("Base de clientes limpa com sucesso!", removidos);
        });

        return grupo;
    }

    private static async Task<Results<Ok<OperacaoClienteResposta>, ProblemHttpResult>> AlteraSituacao(
        int id, BancoDbContext db, Func<Cliente, bool> operacao, string mensagemSucesso, string mensagemSemAlteracao)
    {
        var cliente = await db.Clientes.FindAsync(id);
        if (cliente is null)
            return TypedResults.Problem(ClienteNaoEncontrado, statusCode: StatusCodes.Status404NotFound);

        var alterado = operacao(cliente);
        if (alterado)
            await db.SaveChangesAsync();

        return TypedResults.Ok(new OperacaoClienteResposta(alterado, alterado ? mensagemSucesso : mensagemSemAlteracao, ParaDto(cliente)));
    }

    private static ProblemHttpResult Requisicao(string detalhe) =>
        TypedResults.Problem(detalhe, statusCode: StatusCodes.Status400BadRequest);

    private static ClienteDto ParaDto(Cliente c) => new(c.Id, c.Nome, c.Idade, c.Email, c.Ativo, c.ContaCorrenteId);
}
