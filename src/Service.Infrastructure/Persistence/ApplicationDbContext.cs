using Microsoft.EntityFrameworkCore;
using Service.Application.Abstractions;
using Service.Domain.Examples;

namespace Service.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<ExampleItem> ExampleItems => Set<ExampleItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
