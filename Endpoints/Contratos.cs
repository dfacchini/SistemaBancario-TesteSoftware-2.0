namespace SistemaBancario.Endpoints;

public record ClienteDto(int Id, string Nome, int Idade, string Email, bool Ativo, int ContaCorrenteId);

/// <summary>Sem <c>ContaCorrenteId</c>, uma nova conta com saldo zero é aberta para o cliente.</summary>
public record NovoClienteRequisicao(string? Nome, int Idade, string? Email, int? ContaCorrenteId);

public record OperacaoClienteResposta(bool Alterado, string Mensagem, ClienteDto Cliente);

public record LimpezaClientesResposta(string Mensagem, int Removidos);

public record ContaDto(int Id, decimal Saldo, bool Ativa, int? TitularId, string? TitularNome);

public record NovaContaRequisicao(decimal SaldoInicial);

public record OperacaoContaResposta(bool Alterado, string Mensagem, ContaDto Conta);

public record TransferenciaRequisicao(int ContaOrigemId, int ContaDestinoId, decimal Valor);

public record TransferenciaResposta(string Mensagem, ContaDto Origem, ContaDto Destino);
