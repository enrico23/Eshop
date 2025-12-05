using EShop.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
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


        builder.HasData(
            new Category( new Guid("a3b4f1de-8241-4ff0-9c5e-af4c4c7a1e10"), "Laptops" ),
            new Category(new Guid("c1e2d8ba-992f-4e8e-a0a9-5bcff3df8c77"), "Desktops" ),
            new Category(new Guid("f5e1a4cc-2b9b-4db9-9b52-6d3f72440365"), "Smartphones" ),
            new Category(new Guid("0b73b90f-9610-4f64-893e-37edc85c0a5f"), "Tablets" ),
            new Category(new Guid("49d2e88d-241d-4fdb-9d41-cf4a6c9ab3c9"), "Wearables" ),
            new Category(new Guid("7fb5c7d1-c2a4-4ff2-9f76-4e0b55a389b6"),  "Audio" ),
            new Category(new Guid("d0c52f04-4cd2-4e47-bc9f-06b251f8eae9"),  "Gaming" ),
            new Category (new Guid("3a4b77df-1a28-4f7e-b764-c4d62a6f6ed8"),  "Cameras" )
        );
    }
}
