using System;
using System.Threading.Tasks;
using LibraryManagementSystem.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace LibraryManagementSystem.IntegrationTests
{
    public abstract class TestBase
    {
        protected LibraryDbContext DbContext;
        protected IServiceProvider ServiceProvider;

        [SetUp]
        public virtual async Task Setup()
        {
            var services = new ServiceCollection();
            
            // Tạo DbContext sử dụng SQLite in-memory để test
            services.AddDbContext<LibraryDbContext>(options =>
            {
                options.UseSqlite($"DataSource=:memory:");
            });

            // Đăng ký các service khác cho test
            RegisterServices(services);

            ServiceProvider = services.BuildServiceProvider();
            DbContext = ServiceProvider.GetRequiredService<LibraryDbContext>();

            // Tạo database
            await DbContext.Database.OpenConnectionAsync();
            await DbContext.Database.EnsureCreatedAsync();
            
            // Seed test data nếu cần
            await SeedDataAsync();
        }

        [TearDown]
        public virtual async Task TearDown()
        {
            await DbContext.Database.CloseConnectionAsync();
            await DbContext.DisposeAsync();
        }

        protected virtual void RegisterServices(IServiceCollection services)
        {
            // Đăng ký các service cần thiết cho test
        }

        protected virtual Task SeedDataAsync()
        {
            // Seed test data cơ bản
            return Task.CompletedTask;
        }
    }
} 