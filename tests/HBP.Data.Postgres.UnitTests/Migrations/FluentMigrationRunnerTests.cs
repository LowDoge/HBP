using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HBP.Data.Postgres.UnitTests.Migrations;

public class FluentMigrationRunnerTests
{
    [Fact]
    public void ConstructorThrowsForNullServiceProvider()
    {
        var act = () => new FluentMigrationRunner(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("serviceProvider");
    }

    [Fact]
    public async Task RunAsyncResolvesAndInvokesMigrateUp()
    {
        var fluentRunner = new Mock<FluentMigrator.Runner.IMigrationRunner>();

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(CreateScopeFactory(fluentRunner.Object));

        var runner = new FluentMigrationRunner(serviceProvider.Object);

        await runner.RunAsync();

        fluentRunner.Verify(x => x.MigrateUp(), Times.Once);
    }

    private static IServiceScopeFactory CreateScopeFactory(
        FluentMigrator.Runner.IMigrationRunner fluentRunner
    )
    {
        var scope = new Mock<IServiceScope>();
        scope
            .Setup(x =>
                x.ServiceProvider.GetService(typeof(FluentMigrator.Runner.IMigrationRunner))
            )
            .Returns(fluentRunner);

        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory.Setup(x => x.CreateScope()).Returns(scope.Object);
        return scopeFactory.Object;
    }
}
