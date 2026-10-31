using BookHub.Application.Abstractions;
using BookHub.Application.Exceptions;
using BookHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookHub.Infrastructure
{
    public class BookHubContext : DbContext, IUnitOfWork
    {
        public BookHubContext(DbContextOptions<BookHubContext> options) : base(options)
        {
        }

        public DbSet<Genre> Genres => Set<Genre>();
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Book> Books => Set<Book>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookHubContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
            {
                throw pg.SqlState switch
                {
                    PostgresErrorCodes.UniqueViolation => new ConflictException("Já existe um registro com o mesmo valor."),
                    PostgresErrorCodes.ForeignKeyViolation => new ConflictException("A operação viola um relacionamento existente."),
                    _ => ex
                };
            }
        }
    }
}
