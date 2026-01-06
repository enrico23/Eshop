using EShop.Domain.Catalog;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Data.EntityConfigurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);
        builder.Property(p => p.Id)
        .ValueGeneratedOnAdd();

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);

        builder.Property(p => p.CreatedUtc)
          .IsRequired()
          .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(p => p.UpdatedUtc);

        builder.HasIndex(p => p.Name).IsUnique();
    }
}
