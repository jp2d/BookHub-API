using System.Text.Json;
using BookHub.API.ErrorHandling;
using BookHub.Application.Exceptions;
using BookHub.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookHub.Tests.Api
{
    public class GlobalExceptionHandlerTests
    {
        private static async Task<(int Status, JsonElement Body)> HandleAsync(Exception exception)
        {
            var services = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
            var handler = new GlobalExceptionHandler(
                services.GetRequiredService<IProblemDetailsService>(),
                NullLogger<GlobalExceptionHandler>.Instance);

            var context = new DefaultHttpContext { RequestServices = services };
            context.Response.Body = new MemoryStream();

            var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);
            Assert.True(handled);

            context.Response.Body.Position = 0;
            var body = await JsonDocument.ParseAsync(context.Response.Body);
            return (context.Response.StatusCode, body.RootElement);
        }

        [Theory]
        [InlineData(typeof(DomainException), 400)]
        [InlineData(typeof(NotFoundException), 404)]
        [InlineData(typeof(ConflictException), 409)]
        public async Task Maps_known_exceptions_to_their_status_and_exposes_the_message(Type type, int expectedStatus)
        {
            var exception = (Exception)Activator.CreateInstance(type, "mensagem de negócio")!;

            var (status, body) = await HandleAsync(exception);

            Assert.Equal(expectedStatus, status);
            Assert.Equal("mensagem de negócio", body.GetProperty("detail").GetString());
        }

        [Fact]
        public async Task Maps_client_errors_raised_by_the_server_to_their_own_status()
        {
            var (status, body) = await HandleAsync(new BadHttpRequestException("Request body too large.", StatusCodes.Status413PayloadTooLarge));

            Assert.Equal(StatusCodes.Status413PayloadTooLarge, status);
            Assert.Equal(StatusCodes.Status413PayloadTooLarge, body.GetProperty("status").GetInt32());
        }

        [Fact]
        public async Task Maps_unexpected_exceptions_to_500_without_leaking_details()
        {
            var (status, body) = await HandleAsync(new InvalidOperationException("segredo interno"));

            Assert.Equal(500, status);
            Assert.False(body.TryGetProperty("detail", out var detail) && detail.GetString()?.Contains("segredo") == true);
        }
    }
}
