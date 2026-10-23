using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaBancario.Dominio.Entidades;

namespace SistemaBancario.Dominio.Persistencia.Configuracoes;

public class ContaCorrenteConfiguracao : IEntityTypeConfiguration<ContaCorrente>
{
    public void Configure(EntityTypeBuilder<ContaCorrente> builder)
    {
        builder.ToTable("ContasCorrentes");
        builder.HasKey(c => c.Id);

        // Token de concorrência: duas transferências simultâneas sobre a mesma conta não podem
        // sobrescrever o saldo uma da outra (a segunda falha com DbUpdateConcurrencyException).
        builder.Property(c => c.Saldo)
            .HasPrecision(18, 2)
            .IsConcurrencyToken();

        builder.Property(c => c.Ativa).IsRequired();

        // RF-18: contas pré-cadastradas. O legado começava com saldo zero; aqui já iniciam com saldo
        // para permitir transferências logo de início.
        builder.HasData(
            new { Id = 1, Saldo = 1000m, Ativa = true },
            new { Id = 2, Saldo = 1000m, Ativa = true });
    }
}
