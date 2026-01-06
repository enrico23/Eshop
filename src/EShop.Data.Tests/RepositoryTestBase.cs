using EShop.Data;

/// <summary>
/// Base class for repsotory tests 
/// build around the Unit Of Work pattern.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TRepository"></typeparam>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TUnitOfWork"></typeparam>
/// <remarks>
/// Derived classes are responsible for providing 
/// infrastructure-specific construction logic via abstract factory methods.
/// </remarks>
public abstract class RepositoryTestBase<TEntity, TRepository, TContext, TUnitOfWork>
    where TEntity : class
    where TRepository : class
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

    /// <summary>
    /// Creates a new database context instance
    /// for single execution
    /// </summary>
    /// <remarks>
    /// The returned context defines the lifetime boundary for the test.
    /// It must be configured for isolation (in-memory or test database).
    /// The context will be disposed automatically by the test harness.
    /// </remarks>
    protected abstract TContext CreateContext();

    /// <summary>
    /// Creates the Unit Of Work that coordinates persistence operations.
    /// </summary>
    /// <param name="context"></param>
    /// <remarks>
    /// The Unit Of Work must be scope to the provided context and must not
    /// manage its context lifetime.
    /// </remarks>
    protected abstract TUnitOfWork CreateUnitOfWOrk(TContext context);

    /// <summary>
    /// Create an instance of the Repository under test
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected abstract TRepository CreateRepository(TContext context);

    /// <summary>
    /// Registers an AutoFixture factory for creating valid instances of 
    /// <typeparamref name="TEntity" />
    /// </summary>
    /// <param name="fixture">The AutoFixture instance used by the test harness</param>
    /// <remarks>
    /// The Registered factory must producee fully valid entitities that
    /// can be persisted y the context without additional
    /// initialization.
    /// </remarks>
    protected abstract void RegisterEntityFactory(IFixture fixture);

    private async Task CreateAsync(
        Func<IFixture, IEnumerable<TEntity>> seedFactory,
        Func<TestScope, Task> testBody)
    {
        var context = CreateContext();
        var fixture = new Fixture();
        RegisterEntityFactory(fixture);

        try
        {
            var entities = seedFactory(fixture)?.ToList() ?? [];

            if (entities.Count > 0)
            {
                await SeedAsync(context, entities).ConfigureAwait(false);
                await context.SaveChangesAsync();
            }

            var uow = CreateUnitOfWOrk(context);
            var sut = CreateRepository(context);

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
            TRepository sut,
            IUnitOfWork uow)
        {
            _context = context;
            Fixture = fixture;
            Entities = entities;
            Sut = sut;
            Uow = uow;
        }

        public TRepository Sut { get; }
        public IUnitOfWork Uow { get; }
        public IFixture Fixture { get; }
        public IReadOnlyList<TEntity> Entities { get; }

        public Task SaveChangesAsync(CancellationToken ct = default) =>
            Uow.SaveChangesAsync(ct);

        public ValueTask DisposeAsync() =>
            _context.DisposeAsync();
    }

    private async Task SeedAsync(TContext context, IReadOnlyCollection<TEntity> entities)
    {
        await context.Set<TEntity>().AddRangeAsync(entities);
    }
}
