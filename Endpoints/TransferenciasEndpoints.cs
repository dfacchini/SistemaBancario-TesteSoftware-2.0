using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SistemaBancario.Dominio.Persistencia;

namespace SistemaBancario.Endpoints;

public static class TransferenciasEndpoints
{
    public static RouteGroupBuilder MapTransferencias(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/transferencias").WithTags("Transferências");

        // RF-15
        grupo.MapPost("/", async Task<Results<Ok<TransferenciaResposta>, ProblemHttpResult>> (TransferenciaRequisicao req, BancoDbContext db) =>
        {
            if (req.Valor <= 0)
                return TypedResults.Problem("O valor da transferência precisa ser maior que zero.", statusCode: StatusCodes.Status400BadRequest);

            if (!ContasEndpoints.TemAteDuasCasas(req.Valor))
                return TypedResults.Problem("O valor da transferência pode ter no máximo 2 casas decimais.", statusCode: StatusCodes.Status400BadRequest);

            if (req.ContaOrigemId == req.ContaDestinoId)
                return TypedResults.Problem("A conta de destino precisa ser diferente da conta de origem.", statusCode: StatusCodes.Status400BadRequest);

            var origem = await db.ContasCorrentes.FindAsync(req.ContaOrigemId);
            if (origem is null)
                return TypedResults.Problem("Conta de origem não encontrada!", statusCode: StatusCodes.Status404NotFound);

            var destino = await db.ContasCorrentes.FindAsync(req.ContaDestinoId);
            if (destino is null)
                return TypedResults.Problem("Conta de destino não encontrada!", statusCode: StatusCodes.Status404NotFound);

            if (!origem.TransfereValor(req.Valor, destino))
            {
                var motivo = !origem.Ativa || !destino.Ativa
                    ? "As duas contas precisam estar ativas."
                    : "Saldo insuficiente na conta de origem.";
                return TypedResults.Problem($"Transferência não realizada: {motivo}", statusCode: StatusCodes.Status422UnprocessableEntity);
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return TypedResults.Problem("O saldo foi alterado por outra operação. Tente novamente.", statusCode: StatusCodes.Status409Conflict);
            }

            return TypedResults.Ok(new TransferenciaResposta(
                "Transferência realizada com sucesso!",
                (await ContasEndpoints.ObtemConta(db, origem.Id))!,
                (await ContasEndpoints.ObtemConta(db, destino.Id))!));
        });

        return grupo;
    }
}
