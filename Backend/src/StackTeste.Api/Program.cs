using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using StackTeste.Api.Middleware;
using StackTeste.Application;
using StackTeste.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "StackTeste API",
        Version = "v1"
    });
    options.UseAllOfToExtendReferenceSchemas();
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

const string AngularCorsPolicy = "AngularDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(AngularCorsPolicy);
app.MapControllers();

app.Run();
