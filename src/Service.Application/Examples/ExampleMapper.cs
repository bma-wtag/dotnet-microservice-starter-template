using Riok.Mapperly.Abstractions;
using Service.Domain.Examples;

namespace Service.Application.Examples;

[Mapper]
public static partial class ExampleMapper
{
    [MapperIgnoreSource(nameof(ExampleItem.DomainEvents))]
    public static partial ExampleItemDto ToDto(ExampleItem item);

    private static string MapName(ExampleName name) => name.Value;
}
