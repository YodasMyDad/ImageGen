using Microsoft.Extensions.Logging;
using Moq;

namespace ImageGen.Tests;

/// <summary>
/// Base class for all tests providing common setup and utilities.
/// </summary>
public abstract class TestBase
{
    protected Mock<ILogger<T>> CreateMockLogger<T>() where T : class
    {
        var logger = new Mock<ILogger<T>>();
        logger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Verifiable();

        return logger;
    }

    protected ILogger<T> GetLogger<T>() where T : class => CreateMockLogger<T>().Object;
}
