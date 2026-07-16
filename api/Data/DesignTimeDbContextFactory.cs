using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Drivolution.Data;

// Factory utilizada pelo Entity Framework durante o design-time
// (migrações, atualização da base de dados, etc.)
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    // Método chamado automaticamente pelo Entity Framework
    // para criar uma instância do ApplicationDbContext
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Cria o objeto que irá conter a configuração da ligação
        // à base de dados
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Configura a ligação à base de dados PostgreSQL
        optionsBuilder.UseNpgsql(
            "Host=localhost;" +
            "Port=5433;" +
            "Database=drivolution;" +
            "Username=drivolution;" +
            "Password=drivolution"
        );

        // Devolve uma instância do contexto já configurada
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}