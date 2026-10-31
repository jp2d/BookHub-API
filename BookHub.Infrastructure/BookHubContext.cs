using BookHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookHub.Infrastructure
{
    public class BookHubContext : DbContext
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
    }
}
