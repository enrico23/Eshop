using EShop.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Data.EntityConfigurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);

        builder.HasData(
            new Brand { Id = 1, Name = "Microsoft" },
            new Brand { Id = 2, Name = "Apple" },
            new Brand { Id = 3, Name = "Samsung" }
        );
    }
}