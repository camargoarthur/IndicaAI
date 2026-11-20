using System.Net;
using System.Net.Http.Headers;
using System.Text;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Catalog;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Operations;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IndicaAI.Tests.Integration;

public sealed class WatchListImportTests
{
    [Fact]
    public async Task ReusesTypedMatchesAndExistingTrackingAcrossRepeatedImports()
    {
        var ct = TestContext.Current.CancellationToken;
        var database = new TestDatabase();
        await database.InitializeAsync();
        var catalog = new TmdbCatalog(database.Paths);
        var filmId = await catalog.ImportMovieAsync(new(42, "Filme", "Movie", 2021, null, null, null, []), false, ct);
        await new TrackingService(database.Paths).AddAsync(new("original", filmId, "completed", new(null, null, false), "manter"), ct);
        var provider = new FakeTmdb();
        var importer = new WatchListImporter(database.Paths, Movies(provider));
        var items = WatchList.Read(new StringReader("Séries:\n- Série (Show - 2022): Descrição\n\nFilmes:\n- Filme (Movie - 2021): Descrição"));

        var first = await importer.RunAsync(items, _ => { }, ct);
        var requestCount = provider.RequestCount;
        var second = await importer.RunAsync(items, _ => { }, ct);

        Assert.Equal(first, second);
        Assert.Equal(requestCount, provider.RequestCount);
        await using var db = database.Open();
        var series = await db.Works.SingleAsync(work => work.TmdbSeriesId == 42, ct);
        Assert.Equal("serie", series.MediaCode);
        var existing = await db.Trackings.SingleAsync(tracking => tracking.WorkId == filmId, ct);
        Assert.Equal("completed", existing.StateCode);
        Assert.Equal("manter", existing.Note);
        Assert.Equal("planned", (await db.Trackings.SingleAsync(tracking => tracking.WorkId == series.Id, ct)).StateCode);
        Assert.Equal(2, await db.Events.CountAsync(ct));
    }

    [Fact]
    public async Task AmbiguousAndMissingMatchesRemainLocal()
    {
        var ct = TestContext.Current.CancellationToken;
        var database = new TestDatabase();
        await database.InitializeAsync();
        var importer = new WatchListImporter(database.Paths, Movies(new FakeTmdb()));
        var items = WatchList.Read(new StringReader("Filmes:\n- Ambíguo (Ambíguo - 2021): descrição\n- Ausente (Ausente - 2023): descrição"));

        var results = await importer.RunAsync(items, _ => { }, ct);

        Assert.Equal(new[] { "local_ambiguous", "local_not_found" }, results.Select(result => result.Status));
        await using var db = database.Open();
        var works = await db.Works.Where(work => work.Title == "Ambíguo" || work.Title == "Ausente").ToListAsync(ct);
        Assert.Equal(2, works.Count);
        Assert.All(works, work => Assert.Null(work.TmdbMovieId));
        Assert.Equal(2, await db.Trackings.CountAsync(tracking => tracking.StateCode == "planned", ct));
    }

    [Fact]
    public async Task ExplainsMissingPostersPerImportedWork()
    {
        var ct = TestContext.Current.CancellationToken;
        var database = new TestDatabase();
        await database.InitializeAsync();
        var provider = new FakeTmdb { IncludeSeriesPoster = true };
        var source = new FakeImages();
        using var images = new HttpClient(source);
        var importer = new WatchListImporter(database.Paths, Movies(provider), new TmdbPosterCache(database.Paths, images));
        var items = WatchList.Read(new StringReader("Filmes:\n- Ambíguo (Ambíguo - 2021): descrição\n- Ausente (Ausente - 2023): descrição\n- Filme (Movie - 2021): descrição\nSéries:\n- Série (Show - 2022): descrição"));

        var results = await importer.RunAsync(items, _ => { }, ct);

        Assert.Equal(new[] { "local_ambiguous", "local_not_found", "linked", "linked" }, results.Select(result => result.Status));
        Assert.All(results, result => Assert.NotNull(result.WorkId));
        Assert.Equal(new[] { "ambiguous_match", "no_match", "no_poster_source", "download_failed" }, results.Select(result => result.PosterStatus));

        provider.Override = (path, _) => path == "/3/movie/42" ? new(HttpStatusCode.NotFound) : null;
        source.Available = true;

        var resumed = await importer.ResumePostersAsync(items, ct);

        Assert.Equal(new[] { "ambiguous_match", "no_match", "tmdb_not_found", "downloaded" }, resumed.Select(result => result.PosterStatus));
    }

    [Fact]
    public async Task ResumesPendingPostersWithoutChangingTheCatalogOrTracking()
    {
        var ct = TestContext.Current.CancellationToken;
        var database = new TestDatabase();
        await database.InitializeAsync();
        var filmId = await new TmdbCatalog(database.Paths).ImportMovieAsync(new(42, "Filme", "Movie", 2021, null, null, null, []), false, ct);
        await new TrackingService(database.Paths).AddAsync(new("original", filmId, "completed", new(null, null, false), "manter"), ct);
        var provider = new FakeTmdb { IncludeSeriesPoster = true };
        var source = new FakeImages { Available = true };
        using var images = new HttpClient(source);
        var posters = new TmdbPosterCache(database.Paths, images);
        Assert.True(await posters.TryStoreAsync("filme", 42, "https://image.tmdb.org/t/p/w342/show.jpg", ct));
        var importer = new WatchListImporter(database.Paths, Movies(provider), posters);
        var items = WatchList.Read(new StringReader("Filmes:\n- Filme (Movie - 2021): descrição\nSéries:\n- Série (Show - 2022): descrição\n- Série (Show - 2022): descrição\n- Inédita (Inédita - 2023): descrição"));
        source.Available = false;
        var imported = await importer.RunAsync(items.Take(2).ToArray(), _ => { }, ct);
        Assert.Equal(new[] { "cached", "download_failed" }, imported.Select(result => result.PosterStatus));
        var before = await File.ReadAllBytesAsync(database.Paths.Database, ct);
        var previousRequests = provider.RequestCount;
        source.Available = true;

        var resumed = await importer.ResumePostersAsync(items, ct);

        Assert.Equal(new[] { "cached", "downloaded", "downloaded", "not_imported" }, resumed.Select(result => result.PosterStatus));
        Assert.Equal(imported[1].WorkId, resumed[2].WorkId);
        Assert.Null(resumed[3].WorkId);
        Assert.Equal(1, provider.RequestCount - previousRequests);
        Assert.NotNull(posters.Find("serie", 42));
        Assert.Equal(before, await File.ReadAllBytesAsync(database.Paths.Database, ct));

        var repeated = await importer.ResumePostersAsync(items, ct);

        Assert.Equal(new[] { "cached", "cached", "cached", "not_imported" }, repeated.Select(result => result.PosterStatus));
        Assert.Equal(1, provider.RequestCount - previousRequests);
        Assert.Equal(before, await File.ReadAllBytesAsync(database.Paths.Database, ct));
    }

    [Fact]
    public async Task PosterMaintenanceUsesReceiptsAndLeavesTheDatabaseUntouched()
    {
        var ct = TestContext.Current.CancellationToken;
        var database = new TestDatabase();
        await database.InitializeAsync();
        var items = "Filmes:\n- Ausente (Ausente - 2023): descrição\n- Inédito (Inédito - 2024): descrição";
        var importer = new WatchListImporter(database.Paths, Movies(new FakeTmdb()));
        await importer.RunAsync(WatchList.Read(new StringReader(items)).Take(1).ToArray(), _ => { }, ct);
        var list = Path.Combine(database.Paths.Root, "watch-list.txt");
        await File.WriteAllTextAsync(list, items, ct);
        var before = await File.ReadAllBytesAsync(database.Paths.Database, ct);

        var result = await MaintenanceCommand.RunAsync(["--maintenance", "import-watch-list", "--data-root", database.Paths.Root,
            "--list", list, "--posters-only"], ct);

        Assert.Equal(0, result);
        Assert.Equal(before, await File.ReadAllBytesAsync(database.Paths.Database, ct));
        var missingRoot = Path.Combine(database.Paths.Root, "unused");

        var missing = await MaintenanceCommand.RunAsync(["--maintenance", "import-watch-list", "--data-root", missingRoot,
            "--list", list, "--posters-only"], ct);

        Assert.Equal(2, missing);
        Assert.False(Directory.Exists(missingRoot));
    }

    [Fact]
    public async Task RateLimitPausesWithoutCreatingAFallbackAndResumesCommittedRows()
    {
        var ct = TestContext.Current.CancellationToken;
        var database = new TestDatabase();
        await database.InitializeAsync();
        var items = WatchList.Read(new StringReader("Filmes:\n- Filme (Movie - 2021): Descrição\n- Interrompido (Interrompido - 2022): descrição"));
        var provider = new FakeTmdb
        {
            Override = (_, query) => query == "Interrompido"
                ? new(HttpStatusCode.TooManyRequests) { Headers = { RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(60)) } }
                : null
        };
        var importer = new WatchListImporter(database.Paths, Movies(provider));

        var interrupted = await importer.RunAsync(items, _ => { }, ct);
        Assert.Equal(new[] { "linked", "tmdb_rate_limit" }, interrupted.Select(result => result.Status));
        await using var db = database.Open();
        Assert.False(await db.Works.AnyAsync(work => work.Title == "Interrompido", ct));

        provider.Override = null;
        var previousRequests = provider.RequestCount;
        var resumed = await importer.RunAsync(items, _ => { }, ct);

        Assert.Equal(1, provider.RequestCount - previousRequests);
        Assert.Equal(new[] { "linked", "local_not_found" }, resumed.Select(result => result.Status));
        Assert.Equal(2, await db.Trackings.CountAsync(ct));
    }

    [Fact]
    public async Task TrackingWriteFailureRollsBackTheWorkAndCanBeRetried()
    {
        var ct = TestContext.Current.CancellationToken;
        var database = new TestDatabase();
        await database.InitializeAsync();
        await using var db = database.Open();
        await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER case_reject_tracking BEFORE INSERT ON Trackings BEGIN SELECT RAISE(ABORT, 'test'); END;", ct);
        var items = WatchList.Read(new StringReader("Filmes:\n- Filme (Movie - 2021): Descrição"));
        var importer = new WatchListImporter(database.Paths, Movies(new FakeTmdb()));

        var failed = Assert.Single(await importer.RunAsync(items, _ => { }, ct));
        Assert.Equal("watch_list_write_failed", failed.Status);
        Assert.False(await db.Works.AnyAsync(work => work.TmdbMovieId == 42, ct));

        await db.Database.ExecuteSqlRawAsync("DROP TRIGGER case_reject_tracking;", ct);
        var retried = Assert.Single(await importer.RunAsync(items, _ => { }, ct));

        Assert.Equal("linked", retried.Status);
        Assert.Single(await db.Trackings.ToListAsync(ct));
        Assert.Single(await db.Events.ToListAsync(ct));
    }

    [Fact]
    public async Task UnknownAnimeMediaRequiresReviewBeforeImport()
    {
        var ct = TestContext.Current.CancellationToken;
        var database = new TestDatabase();
        await database.InitializeAsync();
        var items = WatchList.Read(new StringReader("Animes:\n- Desenho (Desenho - 2021): descrição; texto permanece local:"));
        var importer = new WatchListImporter(database.Paths, Movies(new FakeTmdb()));

        Assert.Equal("review_anime_type", Assert.Single(await importer.RunAsync(items, _ => { }, ct)).Status);
        Assert.Equal("local_not_found", Assert.Single(await importer.RunAsync([items[0] with { MediaCode = "serie" }], _ => { }, ct)).Status);

        await using var db = database.Open();
        var anime = await db.Works.SingleAsync(work => work.Title == "Desenho", ct);
        Assert.Equal("serie", anime.MediaCode);
        Assert.True(anime.IsAnime);
        Assert.Equal("descrição; texto permanece local:", anime.Description);
    }

    private static TmdbMovies Movies(FakeTmdb handler) => new(new FakeFactory(handler),
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["IndicaAI:TmdbReadToken"] = "fake" }).Build());

    private sealed class FakeFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false)
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/")
        };
    }

    private sealed class FakeTmdb : HttpMessageHandler
    {
        private int requestCount;
        public int RequestCount => Volatile.Read(ref requestCount);
        public Func<string, string, HttpResponseMessage?>? Override { get; set; }
        public bool IncludeSeriesPoster { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Interlocked.Increment(ref requestCount);
            var uri = request.RequestUri!;
            var query = QueryHelpers.ParseQuery(uri.Query).GetValueOrDefault("query").ToString();
            if (Override?.Invoke(uri.AbsolutePath, query) is { } overridden) return Task.FromResult(overridden);
            var json = (uri.AbsolutePath, query) switch
            {
                ("/3/search/tv", "Série" or "Show") => """{"results":[{"id":42,"name":"Série","original_name":"Show","first_air_date":"2022-06-03"}],"total_pages":1}""",
                ("/3/tv/42", _) when IncludeSeriesPoster => """{"id":42,"name":"Série","original_name":"Show","first_air_date":"2022-06-03","poster_path":"/show.jpg"}""",
                ("/3/tv/42", _) => """{"id":42,"name":"Série","original_name":"Show","first_air_date":"2022-06-03","created_by":[{"name":"Criadora"}]}""",
                ("/3/search/movie", "Filme" or "Movie") => """{"results":[{"id":42,"title":"Filme","original_title":"Movie","release_date":"2021-01-01"}],"total_pages":1}""",
                ("/3/movie/42", _) => """{"id":42,"title":"Filme","original_title":"Movie","release_date":"2021-01-01"}""",
                ("/3/search/movie", "Ambíguo") => """{"results":[{"id":43,"title":"Ambíguo","release_date":"2021-01-01"},{"id":44,"title":"Ambíguo","release_date":"2021-01-01"}],"total_pages":1}""",
                _ => """{"results":[],"total_pages":1}"""
            };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        }
    }

    private sealed class FakeImages : HttpMessageHandler
    {
        public bool Available { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Assert.Equal("https://image.tmdb.org/t/p/w342/show.jpg", request.RequestUri!.AbsoluteUri);
            Assert.Null(request.Headers.Authorization);
            return Task.FromResult(Available
                ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([0xff, 0xd8, 0xff, 0xd9]) }
                : new HttpResponseMessage(HttpStatusCode.BadGateway));
        }
    }
}
