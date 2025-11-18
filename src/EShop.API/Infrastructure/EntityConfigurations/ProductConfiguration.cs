using EShop.API.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.API.Infrastructure.EntityConfigurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name).IsRequired().HasMaxLength(100);

            builder.Property(p => p.Description).HasMaxLength(255);

            builder.Property(p => p.Price).HasPrecision(18, 2);

            builder.Property(p => p.Stock).HasDefaultValue(0);

            builder.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId);

            builder.HasOne(p => p.Brand).WithMany(c => c.Products).HasForeignKey(p => p.BrandId);
        }
    }
}
