using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace LibraryManagementSystem.Infrastructure.Data.Context;

public class LibraryDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
{
    public LibraryDbContext CreateDbContext(string[] args)
    {
        // Get the base path of the application
        var basePath = Directory.GetCurrentDirectory();
        
        // Read the configuration file
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .Build();
            
        // Get the connection string from configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        // Create DbContextOptions with SQL Server provider
        var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        
        return new LibraryDbContext(optionsBuilder.Options);
    }
}