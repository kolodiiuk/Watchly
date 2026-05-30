using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Text.Json;
using Watchly.Application.Interfaces;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.DbContexts;

namespace Watchly.Application.Services;

public class DataImportService : IDataImportService
{
    private readonly IConfiguration _configuration;
    private readonly WatchlyDbContext _dbContext;

    public DataImportService(IConfiguration configuration, WatchlyDbContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
    }

    public async Task<Result> ImportTmdbDataAsync(int titleId)
    {
        try
        {
            var query =
               from t in _dbContext.Titles
               where t.Id == titleId
               select t;

            var title = await query.FirstOrDefaultAsync();

            using var titleDoc = GetTmdbResponseAsync($"https://api.themoviedb.org/3/search/movie?query={title.Name}&year={title.ReleaseDate?.Year}").Result;

            var titleElement = titleDoc.RootElement
               .GetProperty("results")
               .EnumerateArray()
               .FirstOrDefault(x => x.GetProperty("title").GetString() == title.Name &&
                                    x.GetProperty("release_date").GetString() == title.ReleaseDate?.ToString("yyyy-MM-dd"));

            int timdbId = titleElement.GetProperty("id").GetInt32();
            bool isAdult = titleElement.GetProperty("adult").GetBoolean();

            //Credits
            using var creditsDoc = GetTmdbResponseAsync($"https://api.themoviedb.org/3/movie/{timdbId}/credits").Result;

            var actorNames = creditsDoc.RootElement
                .GetProperty("cast")
                .EnumerateArray()
                .Select(x => x.GetProperty("name").GetString())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            string actors = string.Join(", ", actorNames);

            string director = creditsDoc.RootElement
                .GetProperty("crew")
                .EnumerateArray()
                .FirstOrDefault(x => x.GetProperty("job").GetString() == "Director")
                .GetProperty("name").GetString();

            title.Actors = actors;
            title.Director = director;
            title.IsAdult = isAdult;

            _dbContext.Titles.Update(title);
            await _dbContext.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
       }
    }
        
    private async Task<JsonDocument> GetTmdbResponseAsync(string url)
    {
        var request = new RestRequest("");
        request.AddHeader("accept", "application/json");
        request.AddHeader("Authorization", "Bearer " + _configuration["Tmdb:ApiKey"]);
        var options = new RestClientOptions(url);
        var client = new RestClient(options);

        var response = await client.GetAsync(request);

        if (response.Content is null)
            throw new Exception("Getting response from TMDB api failed");

        return JsonDocument.Parse(response.Content);
    }
}