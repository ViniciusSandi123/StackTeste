using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackTeste.Domain.Interfaces;
using StackTeste.Infrastructure.Data;
using StackTeste.Infrastructure.Repositories;

namespace StackTeste.Infrastructure
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<Context>(options =>
                options.UseSqlite(connectionString));

            services.AddScoped<ILeadRepository, LeadRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();

            return services;
        }
    }
}
