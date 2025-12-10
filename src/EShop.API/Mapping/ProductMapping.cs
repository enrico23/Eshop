using EShop.API.Models;
using EShop.Shared.Representations.Product;
using Mapster;

namespace EShop.API.Mapping;

public class ProductMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest=> dest.Name, src=> src.Name)
            .Map(dest => dest.Description, src => src.Description.Value)
            .Map(dest => dest.Price, src => src.Price.Amount)
            .Map(dest => dest.Stock, src => src.Stock.Value)
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest.CategoryName, src => src.Category!.Name)
            .Map(dest => dest.Sku, src => src.Sku.Value);

        config.NewConfig<ProductFormDto, CreateProductDto>()
           .Map(dest => dest.Name, src => src.Name)
           .Map(dest => dest.Description, src => src.Description)
           .Map(dest => dest.Price, src => src.Price)
           .Map(dest => dest.Stock, src => src.Stock)
           .Map(dest => dest.CategoryId, src => src.CategoryId)
           .Map(dest => dest.Sku, src => src.Sku);

        config.NewConfig<ProductFormDto, UpdateProductDto>()
          .Map(dest => dest.Name, src => src.Name)
          .Map(dest => dest.Description, src => src.Description)
          .Map(dest => dest.Price, src => src.Price)
          .Map(dest => dest.Stock, src => src.Stock)
          .Map(dest => dest.CategoryId, src => src.CategoryId)
          .Map(dest => dest.Sku, src => src.Sku);
    }
}
