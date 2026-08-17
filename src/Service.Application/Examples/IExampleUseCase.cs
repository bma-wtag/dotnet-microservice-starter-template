namespace Service.Application.Examples;

public interface IExampleUseCase
{
    Task<ExampleItemDto> CreateAsync(
        CreateExampleRequest request,
        CancellationToken cancellationToken = default);

    Task<ExampleItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
