# EF Core migrations

Migrations live in the `EShop.Data` class library, while the API project provides the runtime services
and configuration needed to build the model. Use the API project as the startup assembly when
creating or applying migrations:

```bash
# from the repo root
dotnet ef migrations add <MigrationName> -p src/EShop.Data -s src/EShop.API -o Migrations

# apply the migrations to your configured database
dotnet ef database update -p src/EShop.Data -s src/EShop.API
```

This ensures EF Core can locate the `CatalogDbContext` and any related configuration while keeping
the migrations alongside the data layer.

