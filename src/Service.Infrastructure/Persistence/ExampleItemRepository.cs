using Microsoft.EntityFrameworkCore;
using Service.Application.Abstractions;
using Service.Domain.Examples;

namespace Service.Infrastructure.Persistence;

internal sealed class ExampleItemRepository(ApplicationDbContext dbContext) : IExampleItemRepository
{
    public async Task AddAsync(ExampleItem item, CancellationToken cancellationToken = default) =>
        await dbContext.ExampleItems.AddAsync(item, cancellationToken);

    public Task<ExampleItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.ExampleItems.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
}
