using FluentValidation;
using Service.Domain.Examples;

namespace Service.Application.Examples;

public sealed class CreateExampleRequestValidator : AbstractValidator<CreateExampleRequest>
{
    public CreateExampleRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(ExampleName.MaxLength);
    }
}
