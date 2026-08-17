using Service.Api.Authentication;
using Service.Application.Examples;

namespace Service.Api.Endpoints;

public static class ExampleEndpoints
{
    public static IEndpointRouteBuilder MapExampleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/examples")
            .WithTags("Examples")
            .RequireAuthorization(AuthorizationPolicies.ExampleRead);

        group.MapPost("/", CreateAsync)
            .WithName("CreateExample")
            .WithSummary("Creates an example item.")
            .Produces<ExampleItemDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetExampleById")
            .WithSummary("Gets an example item by identifier.")
            .Produces<ExampleItemDto>()
            .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> CreateAsync(
        CreateExampleRequest request,
        IExampleUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.CreateAsync(request, cancellationToken);
        return Results.Created($"/api/examples/{result.Id}", result);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IExampleUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.GetByIdAsync(id, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
}
