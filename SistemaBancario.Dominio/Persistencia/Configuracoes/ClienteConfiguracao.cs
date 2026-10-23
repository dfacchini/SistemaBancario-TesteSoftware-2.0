using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaBancario.Dominio.Entidades;

namespace SistemaBancario.Dominio.Persistencia.Configuracoes;

public class ClienteConfiguracao : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome)
            .HasMaxLength(Cliente.NomeTamanhoMaximo)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasMaxLength(Cliente.EmailTamanhoMaximo)
            .IsRequired();

        builder.Property(c => c.Ativo).IsRequired();

        // Cada cliente tem uma conta corrente e cada conta pertence a um único cliente.
        // Restrict impede remover uma conta ainda referenciada, evitando a referência órfã do
        // sistema legado (RF-13); remover o cliente não remove a conta (RF-07).
        builder.HasOne(c => c.ContaCorrente)
            .WithOne()
            .HasForeignKey<Cliente>(c => c.ContaCorrenteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // RF-18: clientes pré-cadastrados.
        builder.HasData(
            new { Id = 1, Nome = "Maria Silva", Idade = 31, Email = "mariasilva@gmail.com", Ativo = true, ContaCorrenteId = 1 },
            new { Id = 2, Nome = "Felipe Augusto", Idade = 34, Email = "felipeaugusto@gmail.com", Ativo = true, ContaCorrenteId = 2 });
    }
}
