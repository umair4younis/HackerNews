using HackerNews.Services;
using Microsoft.AspNetCore.Mvc;


namespace HackerNews.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly HackerNewsService _service;

    public StoriesController(HackerNewsService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get the top N best stories from Hacker News.
    /// </summary>
    /// <param name="n">Number of stories to return (1-100).</param>
    /// <returns>List of top stories.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTopStories([FromQuery] int n = 10)
    {
        if (n <= 0 || n > 100)
            return BadRequest("Please specify a number between 1 and 100.");
        
        var stories = await _service.GetTopStoriesAsync(n);
        return Ok(stories);
    }
}
