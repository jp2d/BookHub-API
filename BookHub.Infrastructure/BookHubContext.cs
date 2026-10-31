using Microsoft.EntityFrameworkCore;

namespace BookHub.Infrastructure
{
    public class BookHubContext : DbContext
    {
        public BookHubContext(DbContextOptions<BookHubContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookHubContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
