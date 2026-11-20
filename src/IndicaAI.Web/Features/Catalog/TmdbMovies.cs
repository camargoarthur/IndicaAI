using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using IndicaAI.Web.Domain;
using Microsoft.AspNetCore.WebUtilities;

namespace IndicaAI.Web.Features.Catalog;

public sealed record TmdbMovie(int Id, string Title, string? OriginalTitle, int? Year,
    string? Overview, string? PosterUrl, int? Minutes, string[] Directors);

public sealed record TmdbSearch(IReadOnlyList<TmdbMovie> Movies, int TotalPages);
public sealed record TmdbSeries(int Id, string Title, string? OriginalTitle, int? Year,
    string? Overview, string? PosterUrl, string[] Creators);
public sealed record TmdbSeriesSearch(IReadOnlyList<TmdbSeries> Series, int TotalPages);

public sealed class TmdbMovies(IHttpClientFactory clients, IConfiguration configuration)
{
    private string? ReadToken => configuration["IndicaAI:TmdbReadToken"] ?? configuration["INDICAAI_TMDB_READ_TOKEN"];
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ReadToken);

    public async Task<TmdbSearch> SearchAsync(string query, int? year, int page, string language, CancellationToken ct)
    {
        var path = QueryHelpers.AddQueryString("search/movie", new Dictionary<string, string?>
        {
            ["query"] = query,
            ["language"] = language,
            ["page"] = page.ToString(CultureInfo.InvariantCulture),
            ["include_adult"] = "false",
            ["primary_release_year"] = year?.ToString(CultureInfo.InvariantCulture)
        });
        var data = await GetAsync<SearchData>(path, ct);
        var movies = (data.Results ?? []).OfType<MovieData>().Select(ToMovie).OfType<TmdbMovie>().ToArray();
        return new(movies, Math.Clamp(data.TotalPages, 0, 500));
    }

    public async Task<TmdbMovie> GetMovieAsync(int id, string language, CancellationToken ct)
    {
        var path = QueryHelpers.AddQueryString("movie/" + id.ToString(CultureInfo.InvariantCulture),
            new Dictionary<string, string?> { ["language"] = language, ["append_to_response"] = "credits" });
        var data = await GetAsync<MovieData>(path, ct);
        return data.Id == id ? ToMovie(data) ?? throw new DomainException("tmdb_unavailable")
            : throw new DomainException("tmdb_unavailable");
    }

    public async Task<TmdbSeriesSearch> SearchSeriesAsync(string query, int? year, int page, string language, CancellationToken ct)
    {
        var path = QueryHelpers.AddQueryString("search/tv", new Dictionary<string, string?>
        {
            ["query"] = query,
            ["language"] = language,
            ["page"] = page.ToString(CultureInfo.InvariantCulture),
            ["include_adult"] = "false",
            ["first_air_date_year"] = year?.ToString(CultureInfo.InvariantCulture)
        });
        var data = await GetAsync<SeriesSearchData>(path, ct);
        var series = (data.Results ?? []).OfType<SeriesData>().Select(ToSeries).OfType<TmdbSeries>().ToArray();
        return new(series, Math.Clamp(data.TotalPages, 0, 500));
    }

    public async Task<TmdbSeries> GetSeriesAsync(int id, string language, CancellationToken ct)
    {
        var path = QueryHelpers.AddQueryString("tv/" + id.ToString(CultureInfo.InvariantCulture), "language", language);
        var data = await GetAsync<SeriesData>(path, ct);
        return data.Id == id ? ToSeries(data) ?? throw new DomainException("tmdb_unavailable")
            : throw new DomainException("tmdb_unavailable");
    }

    private async Task<T> GetAsync<T>(string path, CancellationToken ct)
    {
        if (!IsConfigured) throw new DomainException("tmdb_configuration");
        using var client = clients.CreateClient("Tmdb");
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ReadToken!.Trim());
        try
        {
            using var response = await client.SendAsync(request, ct);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var header = response.Headers.RetryAfter;
                var delay = header?.Delta ?? (header?.Date is { } date ? date - DateTimeOffset.UtcNow : null);
                throw delay is TimeSpan retryAfter
                    ? new DomainException("tmdb_rate_limit", retryAfter) : new DomainException("tmdb_rate_limit");
            }
            if (!response.IsSuccessStatusCode)
                throw new DomainException(response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "tmdb_authentication",
                    HttpStatusCode.NotFound => "tmdb_not_found",
                    _ => "tmdb_unavailable"
                });
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct)
                ?? throw new DomainException("tmdb_unavailable");
        }
        catch (Exception e) when (e is HttpRequestException or JsonException or NotSupportedException
            || e is OperationCanceledException && !ct.IsCancellationRequested)
        {
            throw new DomainException("tmdb_unavailable");
        }
    }

    private static TmdbMovie? ToMovie(MovieData data)
    {
        var title = Clean(data.Title) ?? Clean(data.OriginalTitle);
        if (data.Id < 1 || title is null) return null;
        var directors = (data.Credits?.Crew ?? []).OfType<CrewMember>().Where(c => c.Job == "Director")
            .Select(c => Clean(c.Name)).OfType<string>().Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        return new(data.Id, title, Clean(data.OriginalTitle), Year(data.ReleaseDate), Clean(data.Overview), Poster(data.PosterPath),
            data.Runtime > 0 ? data.Runtime : null, directors);
    }

    private static TmdbSeries? ToSeries(SeriesData data)
    {
        var title = Clean(data.Name) ?? Clean(data.OriginalName);
        if (data.Id < 1 || title is null) return null;
        var creators = (data.CreatedBy ?? []).OfType<Creator>().Select(creator => Clean(creator.Name))
            .OfType<string>().Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        return new(data.Id, title, Clean(data.OriginalName), Year(data.FirstAirDate), Clean(data.Overview), Poster(data.PosterPath), creators);
    }

    private static int? Year(string? value) => DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
        DateTimeStyles.None, out var date) ? date.Year : null;

    private static string? Poster(string? path) => path is not null && Regex.IsMatch(path, @"\A/[A-Za-z0-9_-]+\.(jpg|png|webp)\z")
        ? "https://image.tmdb.org/t/p/w342" + path : null;

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record SearchData(MovieData?[]? Results, [property: JsonPropertyName("total_pages")] int TotalPages);
    private sealed record MovieData(int Id, string? Title, [property: JsonPropertyName("original_title")] string? OriginalTitle,
        [property: JsonPropertyName("release_date")] string? ReleaseDate,
        [property: JsonPropertyName("poster_path")] string? PosterPath, string? Overview, int? Runtime, CreditData? Credits);
    private sealed record CreditData(CrewMember?[]? Crew);
    private sealed record CrewMember(string? Name, string? Job);
    private sealed record SeriesSearchData(SeriesData?[]? Results, [property: JsonPropertyName("total_pages")] int TotalPages);
    private sealed record SeriesData(int Id, string? Name, [property: JsonPropertyName("original_name")] string? OriginalName,
        [property: JsonPropertyName("first_air_date")] string? FirstAirDate, [property: JsonPropertyName("poster_path")] string? PosterPath,
        string? Overview, [property: JsonPropertyName("created_by")] Creator?[]? CreatedBy);
    private sealed record Creator(string? Name);
}
