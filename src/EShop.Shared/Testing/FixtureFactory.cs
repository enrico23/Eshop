using AutoFixture;
using AutoFixture.AutoNSubstitute;

namespace EShop.Shared.Testing;

public static class FixtureFactory
{
    public static IFixture Create()
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

        return fixture;
    }
}

