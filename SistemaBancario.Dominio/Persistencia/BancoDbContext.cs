using Microsoft.EntityFrameworkCore;
using SistemaBancario.Dominio.Entidades;

namespace SistemaBancario.Dominio.Persistencia;

/// <summary>
/// Independente de provedor: o banco (SQL Server, PostgreSQL, SQLite...) é escolhido pela aplicação
/// ao registrar o contexto, por exemplo <c>AddDbContext&lt;BancoDbContext&gt;(o =&gt; o.UseSqlServer(...))</c>.
/// </summary>
public class BancoDbContext(DbContextOptions<BancoDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ContaCorrente> ContasCorrentes => Set<ContaCorrente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BancoDbContext).Assembly);
    }
}
