namespace SistemaBancario.Dominio.Excecoes;

/// <summary>Lançada quando a idade do cliente está fora do intervalo permitido (RF-06).</summary>
public class IdadeNaoPermitidaException : Exception
{
    public const string MensagemPadrao = "A idade do cliente precisa estar entre 18 e 65 anos.";

    public IdadeNaoPermitidaException() : base(MensagemPadrao)
    {
    }
}
