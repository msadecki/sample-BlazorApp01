using Ardalis.Result;
using BlazorApp01.Features.Facade.CQRSMediator;
using BlazorApp01.Features.Facade.CQRSMediator.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BlazorApp01.UnitTests.Features.Facade.CQRSMediator;

public sealed class SenderFacadeTests
{
    private readonly Mock<Mediator.ISender> _mockSender;
    private readonly Mock<ILogger<SenderFacade>> _mockLogger;
    private readonly SenderFacade _sut;

    public SenderFacadeTests()
    {
        _mockSender = new Mock<Mediator.ISender>();
        _mockLogger = new Mock<ILogger<SenderFacade>>();
        _sut = new SenderFacade(_mockSender.Object, _mockLogger.Object);
    }

    #region Command with Response Tests

    [Fact]
    public async Task SendAsync_CommandWithResponse_Success_ReturnsOkResult()
    {
        // Arrange
        TestCommandWithResponse command = new TestCommandWithResponse();
        TestResponse expectedResponse = new TestResponse { Value = "Test" };
        Result<TestResponse> expectedResult = Result<TestResponse>.Success(expectedResponse);

        _mockSender
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        Result<TestResponse> result = await _sut.SendAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedResponse.Value, result.Value.Value);
        _mockSender.Verify(s => s.Send(command, It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handling")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_CommandWithResponse_ReturnsNoContent_LogsCorrectly()
    {
        // Arrange
        TestCommandWithResponse command = new TestCommandWithResponse();
        Result<TestResponse> expectedResult = Result<TestResponse>.NoContent();

        _mockSender
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        Result<TestResponse> result = await _sut.SendAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(ResultStatus.NoContent, result.Status);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handled") && v.ToString().Contains("NoContent")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_CommandWithResponse_ReturnsCreated_LogsCorrectly()
    {
        // Arrange
        TestCommandWithResponse command = new TestCommandWithResponse();
        TestResponse expectedResponse = new TestResponse { Value = "Created" };
        Result<TestResponse> expectedResult = Result<TestResponse>.Created(expectedResponse);

        _mockSender
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        Result<TestResponse> result = await _sut.SendAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(ResultStatus.Created, result.Status);
        Assert.Equal(expectedResponse.Value, result.Value.Value);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SendAsync_CommandWithResponse_ReturnsError_LogsErrors()
    {
        // Arrange
        TestCommandWithResponse command = new TestCommandWithResponse();
        Result<TestResponse> expectedResult = Result<TestResponse>.Error("Validation failed");

        _mockSender
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        Result<TestResponse> result = await _sut.SendAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Validation failed", result.Errors);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Errors")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_CommandWithResponse_ThrowsException_ReturnsErrorResult()
    {
        // Arrange
        TestCommandWithResponse command = new TestCommandWithResponse();
        Exception exception = new InvalidOperationException("Test exception");

        _mockSender
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        Result<TestResponse> result = await _sut.SendAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unexpected error occurred.", result.Errors);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exception thrown")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_CommandWithResponse_WithCancellationToken_PassesCancellationToken()
    {
        // Arrange
        TestCommandWithResponse command = new TestCommandWithResponse();
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken cancellationToken = cts.Token;
        Result<TestResponse> expectedResult = Result<TestResponse>.Success(new TestResponse());

        _mockSender
            .Setup(s => s.Send(command, cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        Result<TestResponse> result = await _sut.SendAsync(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockSender.Verify(s => s.Send(command, cancellationToken), Times.Once);
    }

    #endregion

    #region Command without Response Tests

    [Fact]
    public async Task SendAsync_CommandWithoutResponse_Success_ReturnsOkResult()
    {
        // Arrange
        TestCommand command = new TestCommand();
        Result expectedResult = Result.Success();

        _mockSender
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        Result result = await _sut.SendAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockSender.Verify(s => s.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_CommandWithoutResponse_ReturnsNoContent_LogsCorrectly()
    {
        // Arrange
        TestCommand command = new TestCommand();
        Result expectedResult = Result.NoContent();

        _mockSender
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        Result result = await _sut.SendAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(ResultStatus.NoContent, result.Status);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SendAsync_CommandWithoutResponse_ReturnsError_LogsErrors()
    {
        // Arrange
        TestCommand command = new TestCommand();
        Result expectedResult = Result.Error("Command failed");

        _mockSender
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        Result result = await _sut.SendAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Command failed", result.Errors);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Errors")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_CommandWithoutResponse_ThrowsException_ReturnsErrorResult()
    {
        // Arrange
        TestCommand command = new TestCommand();
        Exception exception = new InvalidOperationException("Command exception");

        _mockSender
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        Result result = await _sut.SendAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unexpected error occurred.", result.Errors);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_CommandWithoutResponse_WithCancellationToken_PassesCancellationToken()
    {
        // Arrange
        TestCommand command = new TestCommand();
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken cancellationToken = cts.Token;
        Result expectedResult = Result.Success();

        _mockSender
            .Setup(s => s.Send(command, cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        Result result = await _sut.SendAsync(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockSender.Verify(s => s.Send(command, cancellationToken), Times.Once);
    }

    #endregion

    #region Query Tests

    [Fact]
    public async Task SendAsync_Query_Success_ReturnsOkResult()
    {
        // Arrange
        TestQuery query = new TestQuery();
        TestResponse expectedResponse = new TestResponse { Value = "Query Result" };
        Result<TestResponse> expectedResult = Result<TestResponse>.Success(expectedResponse);

        _mockSender
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        Result<TestResponse> result = await _sut.SendAsync(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedResponse.Value, result.Value.Value);
        _mockSender.Verify(s => s.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_Query_ReturnsError_LogsErrors()
    {
        // Arrange
        TestQuery query = new TestQuery();
        Result<TestResponse> expectedResult = Result<TestResponse>.Error("Query failed");

        _mockSender
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        Result<TestResponse> result = await _sut.SendAsync(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Query failed", result.Errors);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Errors")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_Query_ThrowsException_ReturnsErrorResult()
    {
        // Arrange
        TestQuery query = new TestQuery();
        Exception exception = new InvalidOperationException("Query exception");

        _mockSender
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        Result<TestResponse> result = await _sut.SendAsync(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unexpected error occurred.", result.Errors);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_Query_WithCancellationToken_PassesCancellationToken()
    {
        // Arrange
        TestQuery query = new TestQuery();
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken cancellationToken = cts.Token;
        Result<TestResponse> expectedResult = Result<TestResponse>.Success(new TestResponse());

        _mockSender
            .Setup(s => s.Send(query, cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        Result<TestResponse> result = await _sut.SendAsync(query, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockSender.Verify(s => s.Send(query, cancellationToken), Times.Once);
    }

    #endregion

    #region Test Helper Classes

    private sealed record TestCommandWithResponse : ICommand<TestResponse>
    {
        public override string ToString() => "TestCommandWithResponse";
    }

    private sealed record TestCommand : ICommand
    {
        public override string ToString() => "TestCommand";
    }

    private sealed record TestQuery : IQuery<TestResponse>
    {
        public override string ToString() => "TestQuery";
    }

    private sealed record TestResponse
    {
        public string Value { get; set; } = string.Empty;
        public override string ToString() => Value;
    }

    #endregion
}