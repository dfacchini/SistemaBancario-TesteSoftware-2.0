namespace SistemaBancario.Dominio.Entidades;

public class ContaCorrente
{
    public int Id { get; private set; }
    public decimal Saldo { get; private set; }
    public bool Ativa { get; private set; }

    // Usado pelo EF Core na materialização.
    private ContaCorrente()
    {
    }

    /// <summary>Cadastra uma conta corrente ativa (RF-11).</summary>
    public ContaCorrente(decimal saldoInicial = 0m)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(saldoInicial);

        Saldo = saldoInicial;
        Ativa = true;
    }

    public void Ativar() => Ativa = true;

    public void Desativar() => Ativa = false;

    /// <summary>
    /// RF-15: debita desta conta e credita no destino quando ambas estão ativas e o saldo é maior ou
    /// igual ao valor (zerar o saldo é permitido). Caso contrário, nenhum saldo é alterado e retorna
    /// <c>false</c>.
    /// </summary>
    public bool TransfereValor(decimal valor, ContaCorrente destino)
    {
        ArgumentNullException.ThrowIfNull(destino);
        // O sistema legado aceitava valores não positivos, o que inverteria o sentido da transferência.
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(valor);

        // O sistema legado não verificava a situação das contas.
        if (!Ativa || !destino.Ativa)
            return false;

        if (Saldo < valor)
            return false;

        Saldo -= valor;
        destino.Saldo += valor;
        return true;
    }
}
