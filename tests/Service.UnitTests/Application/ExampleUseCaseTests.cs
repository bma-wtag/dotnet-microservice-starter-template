using FluentValidation;
using NSubstitute;
using Service.Application.Abstractions;
using Service.Application.Examples;
using Service.Domain.Examples;

namespace Service.UnitTests.Application;

public sealed class ExampleUseCaseTests
{
    private readonly IExampleItemRepository _repository = Substitute.For<IExampleItemRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CreateAsync_PersistsAndMapsItem()
    {
        var useCase = CreateUseCase();

        var result = await useCase.CreateAsync(new CreateExampleRequest("mapped item"));

        Assert.Equal("mapped item", result.Name);
        await _repository.Received(1).AddAsync(
            Arg.Is<ExampleItem>(item => item.Id == result.Id),
            CancellationToken.None);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CreateAsync_RejectsInvalidRequestBeforePersistence()
    {
        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<ValidationException>(() =>
            useCase.CreateAsync(new CreateExampleRequest(string.Empty)));

        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullWhenItemDoesNotExist()
    {
        var useCase = CreateUseCase();

        var result = await useCase.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    private ExampleUseCase CreateUseCase() =>
        new(_repository, _unitOfWork, new CreateExampleRequestValidator());
}
