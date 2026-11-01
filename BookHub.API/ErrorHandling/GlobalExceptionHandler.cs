using BookHub.Application.Exceptions;
using BookHub.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BookHub.API.ErrorHandling
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
        {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (status, title) = exception switch
            {
                DomainException => (StatusCodes.Status400BadRequest, "Requisição inválida."),
                NotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado."),
                ConflictException => (StatusCodes.Status409Conflict, "Conflito."),
                BadHttpRequestException badRequest => (badRequest.StatusCode, "Requisição inválida."),
                _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor.")
            };

            if (status == StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Erro não tratado em {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
            else
                _logger.LogInformation("{ExceptionType} em {Method} {Path}: {Message}",
                    exception.GetType().Name, httpContext.Request.Method, httpContext.Request.Path, exception.Message);

            httpContext.Response.StatusCode = status;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = status == StatusCodes.Status500InternalServerError ? null : exception.Message
                }
            });
        }
    }
}
