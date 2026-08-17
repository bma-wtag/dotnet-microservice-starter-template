namespace Service.Domain.Abstractions;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
