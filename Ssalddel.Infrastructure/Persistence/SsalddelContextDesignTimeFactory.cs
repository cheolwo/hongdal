using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace 살뜰.Data
{
    public sealed class SsalddelContextDesignTimeFactory : IDesignTimeDbContextFactory<SsalddelContext>
    {
        public SsalddelContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                   ?? configuration["ConnectionStrings:DefaultConnection"]
                                   ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection configuration is required for design-time DbContext creation.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<SsalddelContext>();
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 0)), mysqlOptions =>
            {
                mysqlOptions.MigrationsAssembly("Ssalddel");
            });

            return new SsalddelContext(optionsBuilder.Options, new DummyPersonalDataEncryptionService());
        }

        private sealed class DummyPersonalDataEncryptionService : 살뜰.Infrastructure.Security.IPersonalDataEncryptionService
        {
            public string? Protect(string? value) => value;
            public string? Unprotect(string? value) => value;
        }
    }
}
