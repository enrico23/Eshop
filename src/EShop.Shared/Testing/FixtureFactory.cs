using AutoFixture;
using AutoFixture.AutoNSubstitute;

namespace EShop.Shared.Testing;

/// <summary>
/// Creates preconfigured <see cref="IFixture"/> instances for unit tests.
/// 
/// The factory centralizes AutoFixture configuration to ensure consistent
/// behavior across the test suite (mocking, recursion handling, etc.).
/// </summary>
public static class FixtureFactory
{
    /// <summary>
    /// Creates a default <see cref="IFixture"/> for tests that do not require
    /// ASP.NET Core file abstractions.
    ///
    /// Use this method for the majority of tests. It configures:
    /// <list type="bullet">
    /// <item><description>AutoNSubstitute integration with member auto-configuration</description></item>
    /// <item><description>Safe recursion handling for EF entity graphs</description></item>
    /// </list>
    ///
    /// <para>
    /// This fixture <b>does not</b> register <see cref="Microsoft.AspNetCore.Http.IFormFile"/>.
    /// Attempting to create an object graph that depends on <c>IFormFile</c> will result
    /// in an <see cref="AutoFixture.ObjectCreationException"/>.
    /// </para>
    /// </summary>
    /// <returns>
    /// A fully configured <see cref="IFixture"/> suitable for most unit tests.
    /// </returns>
    public static IFixture Create()
     => CreateCore(includeFormFile: false);

    /// <summary>
    /// Creates a <see cref="IFixture"/> configured to support
    /// <see cref="Microsoft.AspNetCore.Http.IFormFile"/> creation.
    ///
    /// <para>
    /// Use this method <b>only</b> for tests that exercise upload scenarios
    /// or object graphs that include <c>IFormFile</c> (e.g. commands, DTOs,
    /// or API models with file inputs).
    /// </para>
    ///
    /// <para>
    /// The registered <c>IFormFile</c> implementation uses an in-memory stream
    /// with valid metadata (file name, length, headers) and is suitable for
    /// mapping, validation, and stream access.
    /// </para>
    /// </summary>
    /// <returns>
    /// A fully configured <see cref="IFixture"/> that can safely create
    /// <see cref="Microsoft.AspNetCore.Http.IFormFile"/> instances.
    /// </returns>
    public static IFixture CreateWithFormFile()
        => CreateCore(includeFormFile: true);

    public static IFixture CreateCore(bool includeFormFile)
    {
        var fixture = new Fixture();

        fixture.Customize(new AutoNSubstituteCustomization
        {
            ConfigureMembers = true
        });

        // Avoid recursion issues on EF entities
        fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        foreach (var b in fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList())
            fixture.Behaviors.Remove(b);

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        if (includeFormFile)
            fixture.Customize(new FormFileCustomization());

        return fixture;
    }
}

