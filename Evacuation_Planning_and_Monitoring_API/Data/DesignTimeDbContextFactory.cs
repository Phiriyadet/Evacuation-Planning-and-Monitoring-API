using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Evacuation_Planning_and_Monitoring_API.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDBContext>
    {
        public ApplicationDBContext CreateDbContext(string[] args)
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            var config = configBuilder.Build();

            var keyVaultUri = config["VaultUri"];
            if (!string.IsNullOrEmpty(keyVaultUri))
            {
                configBuilder.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
                config = configBuilder.Build(); // reload with key vault
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDBContext>();
            var connectionString = config["DatabaseConnection"];
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDBContext(optionsBuilder.Options);
        }
    }
}
