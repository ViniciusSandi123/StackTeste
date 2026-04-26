using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace StackTeste.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, problem) = MapException(exception, context);

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = statusCode;

            var payload = JsonSerializer.Serialize(problem, problem.GetType(), _jsonOptions);
            return context.Response.WriteAsync(payload);
        }

        private static (int statusCode, ProblemDetails problem) MapException(Exception exception, HttpContext context)
        {
            switch (exception)
            {
                case ValidationException validationEx:
                {
                    var errors = validationEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray());

                    var problem = new ValidationProblemDetails(errors)
                    {
                        Title = "Erro de validação.",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Um ou mais erros de validação ocorreram.",
                        Instance = context.Request.Path
                    };
                    return (StatusCodes.Status400BadRequest, problem);
                }

                case KeyNotFoundException:
                {
                    var problem = new ProblemDetails
                    {
                        Title = "Recurso não encontrado.",
                        Status = StatusCodes.Status404NotFound,
                        Detail = exception.Message,
                        Instance = context.Request.Path
                    };
                    return (StatusCodes.Status404NotFound, problem);
                }

                default:
                {
                    var problem = new ProblemDetails
                    {
                        Title = "Erro interno do servidor.",
                        Status = StatusCodes.Status500InternalServerError,
                        Detail = "Ocorreu um erro inesperado ao processar a requisição.",
                        Instance = context.Request.Path
                    };
                    return (StatusCodes.Status500InternalServerError, problem);
                }
            }
        }
    }
}
