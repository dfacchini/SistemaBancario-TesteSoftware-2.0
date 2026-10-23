using SistemaBancario.Dominio.Excecoes;

namespace SistemaBancario.Dominio.Entidades;

public class Cliente
{
    public const int IdadeMinima = 18;
    public const int IdadeMaxima = 65;
    public const int NomeTamanhoMaximo = 150;
    public const int EmailTamanhoMaximo = 254;

    public int Id { get; private set; }
    public string Nome { get; private set; }
    public int Idade { get; private set; }
    public string Email { get; private set; }
    public bool Ativo { get; private set; }

    public int ContaCorrenteId { get; private set; }
    public ContaCorrente? ContaCorrente { get; private set; }

    // Usado pelo EF Core na materialização.
    private Cliente()
    {
        Nome = null!;
        Email = null!;
    }

    /// <summary>Cadastra um cliente ativo (RF-05), já aplicando a validação de idade (RF-06).</summary>
    public Cliente(string nome, int idade, string email, int contaCorrenteId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ValidaIdade(idade);

        Nome = nome;
        Idade = idade;
        Email = email;
        ContaCorrenteId = contaCorrenteId;
        Ativo = true;
    }

    /// <summary>RF-06: aceita idades de 18 a 65 anos, inclusive.</summary>
    /// <exception cref="IdadeNaoPermitidaException">Idade fora do intervalo.</exception>
    public static bool ValidaIdade(int idade)
    {
        if (idade < IdadeMinima || idade > IdadeMaxima)
            throw new IdadeNaoPermitidaException();

        return true;
    }

    /// <summary>RF-02. Retorna <c>false</c> se o cliente já estava ativo (HU-02: "Cliente já ativo!").</summary>
    public bool Ativar()
    {
        if (Ativo)
            return false;

        Ativo = true;
        return true;
    }

    /// <summary>RF-03. Retorna <c>false</c> se o cliente já estava inativo (HU-03: "Cliente já inativo!").</summary>
    public bool Desativar()
    {
        if (!Ativo)
            return false;

        Ativo = false;
        return true;
    }
}
