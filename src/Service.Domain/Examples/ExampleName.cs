using Service.Domain.Exceptions;

namespace Service.Domain.Examples;

public sealed record ExampleName
{
    public const int MaxLength = 200;

    private ExampleName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ExampleName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Example name cannot be empty.");
        }

        var normalizedValue = value.Trim();
        if (normalizedValue.Length > MaxLength)
        {
            throw new DomainException($"Example name cannot exceed {MaxLength} characters.");
        }

        return new ExampleName(normalizedValue);
    }

    public override string ToString() => Value;
}
