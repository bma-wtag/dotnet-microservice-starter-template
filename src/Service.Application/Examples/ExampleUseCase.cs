using FluentValidation;
using Service.Application.Abstractions;
using Service.Domain.Examples;

namespace Service.Application.Examples;

internal sealed class ExampleUseCase(
    IExampleItemRepository repository,
    IUnitOfWork unitOfWork,
    IValidator<CreateExampleRequest> validator) : IExampleUseCase
{
    public async Task<ExampleItemDto> CreateAsync(
        CreateExampleRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var item = ExampleItem.Create(request.Name, DateTimeOffset.UtcNow);
        await repository.AddAsync(item, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ExampleMapper.ToDto(item);
    }

    public async Task<ExampleItemDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await repository.GetByIdAsync(id, cancellationToken);
        return item is null ? null : ExampleMapper.ToDto(item);
    }
}
