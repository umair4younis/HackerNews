using HackerNews.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;


namespace HackerNews.Services;

public class HackerNewsService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private const string BestStoriesUrl = "https://hacker-news.firebaseio.com/v0/beststories.json";
    private const string ItemUrlTemplate = "https://hacker-news.firebaseio.com/v0/item/{0}.json";

    public HackerNewsService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<List<StoryDto>> GetTopStoriesAsync(int n)
    {
        var storyIds = await _httpClient.GetFromJsonAsync<List<int>>(BestStoriesUrl);
        var topNIds = storyIds?.Take(100).ToList() ?? new List<int>();

        var tasks = topNIds.Select(id => GetStoryAsync(id)).ToList();
        var stories = await Task.WhenAll(tasks);
        return stories
            .Where(s => s != null)
            .OrderByDescending(s => s.Score)
            .Take(n)
            .ToList();
    }

    private async Task<StoryDto> GetStoryAsync(int id)
    {
        if (_cache.TryGetValue(id, out StoryDto cachedStory))
            return cachedStory;

        var url = string.Format(ItemUrlTemplate, id);
        try
        {
            var json = await _httpClient.GetStringAsync(url);
            var story = JsonSerializer.Deserialize<JsonElement>(json);

            var dto = new StoryDto
            {
                Title = story.GetProperty("title").GetString(),
                Uri = story.TryGetProperty("url", out var uri) ? uri.GetString() : null,
                PostedBy = story.GetProperty("by").GetString(),
                Time = DateTimeOffset.FromUnixTimeSeconds(story.GetProperty("time").GetInt64()).ToString("o"),
                Score = story.GetProperty("score").GetInt32(),
                CommentCount = story.TryGetProperty("descendants", out var comments) ? comments.GetInt32() : 0
            };

            _cache.Set(id, dto, TimeSpan.FromMinutes(5));
            return dto;
        }
        catch
        {
            return null;
        }
    }
}
