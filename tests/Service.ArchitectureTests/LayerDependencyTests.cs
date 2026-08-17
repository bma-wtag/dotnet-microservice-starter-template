using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using Service.Application.Examples;
using Service.Domain.Examples;
using Service.Infrastructure.Persistence;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Service.ArchitectureTests;

public sealed class LayerDependencyTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(ExampleItem).Assembly,
            typeof(IExampleUseCase).Assembly,
            typeof(ApplicationDbContext).Assembly,
            typeof(Program).Assembly)
        .Build();

    private static readonly IObjectProvider<IType> DomainLayer =
        Types().That().ResideInAssembly("Service.Domain").As("Domain layer");

    private static readonly IObjectProvider<IType> ApplicationLayer =
        Types().That().ResideInAssembly("Service.Application").As("Application layer");

    private static readonly IObjectProvider<IType> InfrastructureLayer =
        Types().That().ResideInAssembly("Service.Infrastructure").As("Infrastructure layer");

    private static readonly IObjectProvider<IType> ApiLayer =
        Types().That().ResideInAssembly("Service.Api").As("API layer");

    [Fact]
    public void Domain_DoesNotDependOnOtherLayers()
    {
        Types().That().Are(DomainLayer).Should().NotDependOnAny(ApplicationLayer)
            .WithoutRequiringPositiveResults().Check(Architecture);
        Types().That().Are(DomainLayer).Should().NotDependOnAny(InfrastructureLayer)
            .WithoutRequiringPositiveResults().Check(Architecture);
        Types().That().Are(DomainLayer).Should().NotDependOnAny(ApiLayer)
            .WithoutRequiringPositiveResults().Check(Architecture);
    }

    [Fact]
    public void Application_DoesNotDependOnOuterLayers()
    {
        Types().That().Are(ApplicationLayer).Should().NotDependOnAny(InfrastructureLayer)
            .WithoutRequiringPositiveResults().Check(Architecture);
        Types().That().Are(ApplicationLayer).Should().NotDependOnAny(ApiLayer)
            .WithoutRequiringPositiveResults().Check(Architecture);
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnApi()
    {
        Types().That().Are(InfrastructureLayer).Should()
            .NotDependOnAny(ApiLayer)
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Fact]
    public void ApiEndpoints_DoNotDependOnDbContext()
    {
        var endpoints = Types().That().ResideInNamespace("Service.Api.Endpoints");
        var dbContext = Types().That().HaveFullName(typeof(ApplicationDbContext).FullName!);

        Types().That().Are(endpoints).Should().NotDependOnAny(dbContext).Check(Architecture);
    }
}
