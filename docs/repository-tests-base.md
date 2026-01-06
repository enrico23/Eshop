# RepositoryTestsBase Pattern

## Purpose

`RepositoryTestBase` provides a reusable harness for repository tests by 
creating a fresh EF Core context, seeding data, building the SUT, and 
ensuring deterministic cleanup.

## Pattern flow

1. Call `TestAsync` with either a seed count or explicit seed set.
2. A new `DbContext` is created.
3. The test fixture is created and `RegisterEntityFactory` is invoked to register 
entity creation rules.
4. Seed data is generated, persisted via `SeedAsync`, and a `EfUnitOfWork` and SUT are created.
5. The test body runs inside a `TestScope` that exposes the SUT, fixture, unit of work, and seeded entities.
6. The context is disposed on scope disposal; exceptions also dispose the context before rethrow.

## TestScope surface

- `Sut`: repository under test.
- `Uow`: `IUnitOfWork` for explicit `SaveChangesAsync` calls.
- `Fixture`: AutoFixture instance for building entities.
- `Entities`: the seeded entities (if any).
- `SaveChangesAsync`: helper that delegates to the unit of work.

## Implementing a repository test base

Derived bases provide only what is specific to the repository:

- `CreateContext` creates a new database context instance.
- `CreateUnitOfWOrk(TContext context)` creates the Unit Of Work that coordinates 
persistence operations.
- `CreateRepository(TContext context)` builds the repository instance (SUT).
- `RegisterEntityFactory(IFixture fixture)` configures how to create the entity type used in tests.

Example: `ProductRepositoryTestsBase` wires the product repository 
and registers a `Product` factory.

## Example usage

```csharp
public sealed class AddAsyncTests : ProductRepositoryTestsBase
{
    [Fact]
    public Task AddAsync_WhenCommitted_PersistsAndCanBeReadBack() =>
        TestAsync(seedCount: 0, async t =>
        {
            var product = t.Fixture.Create<Product>();
            await t.Sut.AddAsync(product, CancellationToken.None);
            await t.SaveChangesAsync(CancellationToken.None);

            var loaded = await t.Sut.GetByIdAsync(product.Id, false, CancellationToken.None);

            Assert.NotNull(loaded);
            Assert.Equal(product.Id, loaded!.Id);
        });
}
```

## Related sources

- `src/EShop.Data.Tests/RepositoryTestsBase.cs`
- `src/EShop.Data.Tests/Repositories/ProductRepositoryTests/ProductRepositoryTestsBase.cs`
- `src/EShop.Data.Tests/Repositories/ProductRepositoryTests/AddAsyncTests.cs`
