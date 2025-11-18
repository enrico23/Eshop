using EShop.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Data.EntityConfigurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);


        builder.HasData(
            new Category { Id = 1, Name = "Laptops" },
            new Category { Id = 2, Name = "Smartphones" },
            new Category { Id = 3, Name = "Tablets" }
        );
    }
}
