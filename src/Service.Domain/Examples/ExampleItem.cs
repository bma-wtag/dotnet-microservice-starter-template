using Service.Domain.Abstractions;

namespace Service.Domain.Examples;

public sealed class ExampleItem
{
    private readonly List<IDomainEvent> _domainEvents = [];

    private ExampleItem()
    {
    }

    private ExampleItem(Guid id, ExampleName name, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;
        _domainEvents.Add(new ExampleItemCreated(id, createdAt));
    }

    public Guid Id { get; private set; }

    public ExampleName Name { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static ExampleItem Create(string name, DateTimeOffset createdAt) =>
        new(Guid.NewGuid(), ExampleName.Create(name), createdAt);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
