using Moq;
using Microsoft.Extensions.Caching.Memory;
using HackerNews.Services;

public class HackerNewsServiceTests
{
    [Fact]
    public async Task GetTopStoriesAsync_ReturnsStories()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var handlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(handlerMock.Object);
        var service = new HackerNewsService(httpClient, memoryCache);

        // Act
        var stories = await service.GetTopStoriesAsync(5);

        // Assert
        Assert.NotNull(stories);
    }
}
