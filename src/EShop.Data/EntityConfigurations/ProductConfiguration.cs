using EShop.Domain;
using EShop.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
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

        builder
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);
    }
}
