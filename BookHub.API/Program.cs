using BookHub.API.ErrorHandling;
using BookHub.Application.Abstractions;
using BookHub.Application.Authors;
using BookHub.Application.Books;
using BookHub.Application.Genres;
using BookHub.Infrastructure;
using BookHub.Infrastructure.Persistence.Repositories;
using BookHub.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.Limits.MaxRequestBodySize = 1_000_000; // the payloads are tiny JSON documents
});

#region 🔹 Logs estruturados (Serilog)
builder.Host.UseSerilog((context, services, logger) =>
{
    logger
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();

    if (context.Configuration.GetValue<bool>("Logging:UseJson"))
        logger.WriteTo.Console(new RenderedCompactJsonFormatter());
    else
        logger.WriteTo.Console();
});
#endregion

#region 🔹 Configuração do CORS (origens permitidas do frontend em Cors:AllowedOrigins)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins);
        else
            policy.AllowAnyOrigin();

        policy.AllowAnyHeader()
              .AllowAnyMethod();
    });
});
#endregion

#region 🔹 Configuração do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BookHub API",
        Version = "v1",
        Description = "API para gerenciamento de livros, autores e gêneros."
    });
});
#endregion

#region 🔹 Injeção de dependências
// Banco de dados (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");

builder.Services.AddDbContext<BookHubContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<BookHubContext>());

// Repositórios (Infrastructure)
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();

// Seed de dados iniciais (Infrastructure)
builder.Services.AddScoped<DatabaseSeeder>();

// Serviços (Application)
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IBookService, BookService>();
#endregion

builder.Services.AddHealthChecks().AddDbContextCheck<BookHubContext>("database");

#region 🔹 Tratamento global de erros (ProblemDetails)
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
#endregion

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<BookHubContext>().Database.MigrateAsync();

    if (app.Configuration.GetValue("Seed:Enabled", true))
        await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
}

// 🔹 Middleware
// Request logging must wrap the exception handler: it then records the final status (409, 404...)
// instead of seeing every handled business error as an unhandled 500.
app.UseSerilogRequestLogging(options =>
    options.GetLevel = (httpContext, _, exception) =>
        httpContext.Request.Path.StartsWithSegments("/health") ? LogEventLevel.Verbose // probes would flood the log
        : exception is not null || httpContext.Response.StatusCode >= 500 ? LogEventLevel.Error
        : LogEventLevel.Information);
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();


public partial class Program;
