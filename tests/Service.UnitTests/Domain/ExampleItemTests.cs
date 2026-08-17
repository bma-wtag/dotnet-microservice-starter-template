using Service.Domain.Examples;
using Service.Domain.Exceptions;

namespace Service.UnitTests.Domain;

public sealed class ExampleItemTests
{
    [Fact]
    public void Create_NormalizesNameAndRaisesDomainEvent()
    {
        var createdAt = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);

        var item = ExampleItem.Create("  example  ", createdAt);

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal("example", item.Name.Value);
        Assert.Equal(createdAt, item.CreatedAt);
        var domainEvent = Assert.IsType<ExampleItemCreated>(Assert.Single(item.DomainEvents));
        Assert.Equal(item.Id, domainEvent.ExampleItemId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_RejectsEmptyName(string name)
    {
        Assert.Throws<DomainException>(() => ExampleItem.Create(name, DateTimeOffset.UtcNow));
    }
}
