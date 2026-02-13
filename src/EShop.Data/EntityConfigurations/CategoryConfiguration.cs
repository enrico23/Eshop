using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Data.EntityConfigurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.CreatedUtc).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(p => p.UpdatedUtc);
        
        builder.Property(p => p.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.Ignore(c => c.Images);
        var categoryImages = builder.OwnsMany<CatalogImage>("_images", img =>
        {
            img.ToTable("CatalogImages");
            img.WithOwner().HasForeignKey("CategoryId");
            img.HasKey(i => i.Id);
            img.Property(i => i.Id).ValueGeneratedNever();
            img.Property(i => i.Url).IsRequired().HasMaxLength(1000);
            img.Property(i => i.AltText).HasMaxLength(500);
            img.Property(i => i.SortOrder).IsRequired();
            img.Property(i => i.IsPrimary).IsRequired();
            img.HasIndex("CategoryId", nameof(CatalogImage.Url)).IsUnique();
            img.HasIndex("CategoryId", nameof(CatalogImage.IsPrimary)).IsUnique().HasFilter("[IsPrimary] = 1");
            img.ToTable(t => t.HasCheckConstraint("CK_CatalogImages_SortOrder_NonNegative", "[SortOrder] >= 0"));
        });

        builder.Navigation("_images").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(p => p.Name).IsUnique();
    }
}
