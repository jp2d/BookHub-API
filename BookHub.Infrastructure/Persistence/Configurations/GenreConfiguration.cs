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

            builder.HasIndex(g => g.Name).IsUnique();

            builder.Navigation(g => g.Books).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
