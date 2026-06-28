using ApolloSpoilers.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ApolloSpoilers.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by EF Core tools (dotnet ef migrations add …) to
/// construct a DbContext without booting the full ASP.NET host. The connection
/// string here is a placeholder — the real one comes from appsettings at runtime.
/// </summary>
public class ApplicationDbContextDesignFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost,1433;Database=ApolloSpoilers;User Id=sa;Password=Placeholder!1;TrustServerCertificate=True;Encrypt=False")
            .Options;

        return new ApplicationDbContext(options);
    }
}
