using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Data.EntityConfigurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
         .ValueGeneratedOnAdd();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasConversion(
                v => v.Value, 
                v => new Domain.Products.ProductDescription(v))
            .HasMaxLength(2000);

        builder.Property(p => p.Price)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasConversion(
                v => v.Amount, 
                v => new Domain.Money(v)
             );

        builder.Property(p => p.Stock)
            .HasConversion(
                v => v.Value,
                v => new StockQuantity(v)
            );

        builder.Property(p => p.Status).IsRequired();

        builder.Property(p => p.Sku)
           .IsRequired()
           .HasMaxLength(50)
           .HasConversion(
               v => v.Value,
               v => new Sku(v)
           );

        builder.Property(p => p.CreatedUtc)
          .IsRequired()
          .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(p => p.UpdatedUtc);

        builder
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);

        builder.OwnsMany(p => p.Images, img =>
        {
            img.ToTable("ProductImages");

            img.WithOwner().HasForeignKey("ProductId");

            img.HasKey(i => i.Id);

            img.Property(i => i.Id)
                .ValueGeneratedNever();

            img.Property(i => i.Url)
                .IsRequired()
                .HasMaxLength(1000);

            img.Property(i => i.AltText)
                .HasMaxLength(500);

            img.Property(i => i.SortOrder)
                .IsRequired();

            img.Property(i => i.IsPrimary)
                .IsRequired();
        });

        builder.HasIndex(p => p.Name).IsUnique();
    }
}
