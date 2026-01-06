using EShop.Data;
using EShop.Shared.Testing;

/// <summary>
/// Provides a reusable test harness for repository tests.
/// Handles DbContext creation, data seeding, SUT construction,
/// and deterministic cleanup.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TSut"></typeparam>
public abstract class RepositoryTestBase<TEntity, TSut, TContext, TUnitOfWork>
    where TEntity : class
    where TSut : class
    where TContext : DbContext
    where TUnitOfWork : IUnitOfWork
{
    protected Task TestAsync(
        int seedCount,
        Func<TestScope, Task> testBody) =>
        CreateAsync(
            seedFactory: f => f.CreateMany<TEntity>(seedCount),
            testBody: testBody);

    protected Task TestAsync(
        IEnumerable<TEntity> seed,
        Func<TestScope, Task> testBody) =>
        CreateAsync(
            seedFactory: _ => seed,
            testBody: testBody);

    protected abstract TSut CreateSut(TContext context);

    protected abstract void RegisterEntityFactory(IFixture fixture);

    protected abstract TContext CreateContext();

    protected abstract TUnitOfWork CreateUnitOfWOrk(TContext context);

    protected virtual async Task SeedAsync(TContext context, IReadOnlyCollection<TEntity> entities)
    {
         await context.Set<TEntity>().AddRangeAsync(entities);
    }

    private async Task CreateAsync(
        Func<IFixture, IEnumerable<TEntity>> seedFactory,
        Func<TestScope, Task> testBody)
    {
        var context = CreateContext();
        var fixture = new Fixture();
        RegisterEntityFactory(fixture);

        try
        {
            var entities = seedFactory(fixture)?.ToList() ?? new List<TEntity>();

            if (entities.Count > 0)
            {
                // await context.SeedAsync(entities).ConfigureAwait(false);
                await SeedAsync(context, entities).ConfigureAwait(false);
                await context.SaveChangesAsync();
            }
                

            var uow = CreateUnitOfWOrk(context);
            var sut = CreateSut(context);

            await using var scope = new TestScope(context, fixture, entities, sut, uow);
            await testBody(scope).ConfigureAwait(false);
        }
        catch
        {
            await context.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    protected sealed class TestScope : IAsyncDisposable
    {
        private readonly TContext _context;

        internal TestScope(
            TContext context,
            IFixture fixture,
            IReadOnlyList<TEntity> entities,
            TSut sut,
            IUnitOfWork uow)
        {
            _context = context;
            Fixture = fixture;
            Entities = entities;
            Sut = sut;
            Uow = uow;
        }

        public TSut Sut { get; }
        public IUnitOfWork Uow { get; }
        public IFixture Fixture { get; }
        public IReadOnlyList<TEntity> Entities { get; }

        public Task SaveChangesAsync(CancellationToken ct = default) =>
            Uow.SaveChangesAsync(ct);

        public ValueTask DisposeAsync() =>
            _context.DisposeAsync();
    }
}
