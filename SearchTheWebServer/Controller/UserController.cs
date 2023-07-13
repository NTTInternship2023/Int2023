using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SearchTheWebServer.Data;
using SearchTheWebServer.Models;

namespace SearchTheWebServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private string ApiKey { get; } = "70158d7bdcmsh70b948f4980dad3p1269b9jsn557b43f311ed";
    private string ApiHost { get; } = "moviesdatabase.p.rapidapi.com";

    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    /*
     * Function to get movie's rating from the API. It makes a GET request to the API and returns the rating.
     * Parameters: string movieId
     * Returns: double
     */
    private async Task<ActionResult<double>> GetMovieRating(string movieId)
    {
        double rating = 0;
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://moviesdatabase.p.rapidapi.com/titles/{movieId}/ratings"),
            Headers =
            {
                { "X-RapidAPI-Key", "70158d7bdcmsh70b948f4980dad3p1269b9jsn557b43f311ed" },
                { "X-RapidAPI-Host", "moviesdatabase.p.rapidapi.com" }
            }
        };

        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            var results = root.GetProperty("results");

            if (results.ValueKind != JsonValueKind.Null)
            {
                var ratingElement = results.GetProperty("averageRating");
                rating = ratingElement.ValueKind != JsonValueKind.Null
                    ? results.GetProperty("averageRating").GetDouble()
                    : 0;
            }
        }

        return Ok(rating);
    }

    /*
    * Function to get data from the API. It makes a GET request to the API and returns DTOs.
     * Adds the search to the database.
     * Parameters: string title, int userId
     * Returns: Dto
     * Throws: Exception if the API call fails
    */
    [HttpGet]
    public async Task<ActionResult<List<MovieDto>>> GetDataFromApi(string title, int userId)
    {
        //Request to the API
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://moviesdatabase.p.rapidapi.com/titles/search/title/{title}");
        request.Headers.Add("X-RapidAPI-Host", ApiHost);
        request.Headers.Add("X-RapidAPI-Key", ApiKey);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent("false"), "exact");
        content.Add(new StringContent("movie,tvSeries"), "titleType");
        request.Content = content;

        var movieDtos = new List<MovieDto>();
        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            var results = root.GetProperty("results");
            var resultList = results.EnumerateArray().ToList();

            foreach (var element in resultList)
            {
                var titleString = element.GetProperty("titleText").GetProperty("text").ToString();
                var imageElement = element.GetProperty("primaryImage");
                var idString = element.GetProperty("id").ToString();
                double rating;

                var imageString = imageElement.ValueKind == JsonValueKind.Null
                    ? "https://gomagcdn.ro/domains/dorianpopa.ro/files/product/medium/tricou-hatz-college-48-1205.jpg"
                    : imageElement.GetProperty("url").ToString();
                var releaseYear = element.GetProperty("releaseYear").GetProperty("year").GetInt32();

                var result = await GetMovieRating(idString);
                rating = result.Value;
                
                movieDtos.Add(new MovieDto
                {
                    Id = idString,
                    Title = titleString,
                    ImageUrl = imageString,
                    ReleaseYear = releaseYear,
                    Rating = rating
                });
            }


            var searchLog = new SearchLog
            {
                IdUser = userId,
                Date = DateTime.Now,
                Action = "search",
                ActionDetail = title
            };

            _context.SearchLogs.Add(searchLog);
            await _context.SaveChangesAsync();


            return Ok(movieDtos);
        }
    }
}