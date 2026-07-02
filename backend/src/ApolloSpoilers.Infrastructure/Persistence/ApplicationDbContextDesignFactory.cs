using ApolloSpoilers.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.IO;

namespace ApolloSpoilers.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by EF Core tools (dotnet ef migrations add …) to
/// construct a DbContext without booting the full ASP.NET host.
/// It tries to load connection strings from appsettings.json, or uses a fallback.
/// </summary>
public class ApplicationDbContextDesignFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private const string ConnectionStringEnvVar = "DefaultConnection";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Try to load connection string from environment variables first
        var connectionString = GetConnectionStringFromEnv();

        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback to appsettings.json
            connectionString = GetConnectionStringFromAppSettings();

            if (string.IsNullOrEmpty(connectionString))
            {
                // Last resort: development local connection string
                connectionString = "Server=apollospoilers-dev-sql.database.windows.net;Database=ApolloSpoilers;User Id=ramitadmin;Password=Enlightened*123!@#;TrustServerCertificate=True;Encrypt=False";
            }
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Try to get connection string from environment variables.
    /// Supports both ConfigBinder format (ConnectionStrings__DefaultConnection) and simple format (DefaultConnection).
    /// </summary>
    private string GetConnectionStringFromEnv()
    {
        // Try standard environment variable
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
                        Environment.GetEnvironmentVariable("DefaultConnection");

        if (!string.IsNullOrEmpty(connection))
        {
            return connection;
        }

        // Try from Render/Cloud environment variables
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
        {
            connection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
                        Environment.GetEnvironmentVariable("DefaultConnection");

            if (!string.IsNullOrEmpty(connection))
            {
                return connection;
            }
        }

        return null;
    }

    /// <summary>
    /// Try to load connection string from appsettings.json file.
    /// </summary>
    private string GetConnectionStringFromAppSettings()
    {
        try
        {
            // Get the project directory (where appsettings.json should be)
            var projectDirectory = GetProjectDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(projectDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            // Try both ConfigBinder format and simple format
            return configuration["ConnectionStrings:DefaultConnection"] ??
                   configuration["ConnectionStrings__DefaultConnection"] ??
                   configuration["DefaultConnection"];
        }
        catch
        {
            // If we can't read appsettings, return null and let fallback take over
            return null;
        }
    }

    /// <summary>
    /// Get the project directory by finding the .csproj file.
    /// </summary>
    private string GetProjectDirectory()
    {
        // Start from current directory and go up until we find a .csproj file
        var current = Directory.GetCurrentDirectory();

        while (current != null && current.Length > 1)
        {
            if (File.Exists(Path.Combine(current, "ApolloSpoilers.Api.csproj")) ||
                File.Exists(Path.Combine(current, "ApolloSpoilers.Infrastructure.csproj")))
            {
                return current;
            }

            var parent = Directory.GetParent(current);
            if (parent == null)
                break;

            current = parent.FullName;
        }

        // If not found, return current directory
        return Directory.GetCurrentDirectory();
    }
}
