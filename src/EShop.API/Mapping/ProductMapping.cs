using EShop.Api.Representations.Products;
using EShop.API.Representations.Products;
using EShop.Domain;
using Mapster;

namespace EShop.API.Mapping;

public class ProductMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductDto>();
        config.NewConfig<CreateProductDto, Product>()
             .Ignore(dest => dest.Id);
        config.NewConfig<UpdateProductDto, Product>()
             .Ignore(dest => dest.Id);
    }
}
