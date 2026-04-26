using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StackTeste.Application.Interfaces;
using StackTeste.Application.Services;

namespace StackTeste.Application
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ILeadService, LeadService>();
            services.AddScoped<ITaskService, TaskService>();

            services.AddAutoMapper(cfg => cfg.AddMaps(typeof(ApplicationExtensions).Assembly));

            services.AddValidatorsFromAssembly(typeof(ApplicationExtensions).Assembly);

            return services;
        }
    }
}
