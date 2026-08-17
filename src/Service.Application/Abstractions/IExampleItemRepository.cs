using Service.Domain.Examples;

namespace Service.Application.Abstractions;

public interface IExampleItemRepository
{
    Task AddAsync(ExampleItem item, CancellationToken cancellationToken = default);

    Task<ExampleItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
