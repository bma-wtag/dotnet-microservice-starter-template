using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Service.Application.Examples;

namespace Service.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateExampleRequestValidator>();
        services.AddScoped<IExampleUseCase, ExampleUseCase>();
        return services;
    }
}
