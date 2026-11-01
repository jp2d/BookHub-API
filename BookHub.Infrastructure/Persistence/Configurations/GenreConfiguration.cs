using BookHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookHub.Infrastructure.Persistence.Configurations
{
    public class GenreConfiguration : IEntityTypeConfiguration<Genre>
    {
        public void Configure(EntityTypeBuilder<Genre> builder)
        {
            builder.ToTable("genres");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(Genre.NameMaxLength);

            // The unique, case-insensitive index on lower("Name") is created by raw SQL in the
            // CaseInsensitiveUniqueGenreName migration: EF Core cannot model expression indexes.

            builder.Navigation(g => g.Books).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
