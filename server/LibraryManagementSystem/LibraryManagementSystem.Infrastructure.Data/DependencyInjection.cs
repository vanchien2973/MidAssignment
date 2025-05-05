using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Infrastructure.Data.Context;
using LibraryManagementSystem.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagementSystem.Infrastructure.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddDataInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext with SQL Server
        services.AddDbContext<LibraryDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(LibraryDbContext).Assembly.FullName)));
        
        // Register Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBookBorrowingRequestRepository, BookBorrowingRequestRepository>();
        services.AddScoped<IBookBorrowingRequestDetailRepository, BookBorrowingRequestDetailRepository>();
        services.AddScoped<IUserActivityLogRepository, UserActivityLogRepository>();
        
        // Register UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
} 