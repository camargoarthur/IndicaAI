using System.Net;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Catalog;

namespace IndicaAI.Tests.Integration;

public sealed class TmdbPosterCacheTests
{
    [Fact]
    public async Task CachesTypedPostersLocallyWithoutRequiringTheProviderAndExpiresThem()
    {
        var fixture = new TestDatabase();
        var now = new DateTimeOffset(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);
        var clock = new TestClock(now);
        using var http = new HttpClient(new PosterResponse());
        var posters = new TmdbPosterCache(fixture.Paths, http, clock);
        const string url = "https://image.tmdb.org/t/p/w342/test.jpg";
        var ct = TestContext.Current.CancellationToken;

        Assert.False(await posters.TryStoreAsync("filme", 45, "https://example.org/t/p/w342/test.jpg", ct));
        Assert.True(await posters.TryStoreAsync("filme", 45, url, ct));
        Assert.True(await posters.TryStoreAsync("serie", 45, url, ct));
        var film = new Work { MediaCode = "filme", TmdbMovieId = 45, CoverAsset = "local.jpg" };
        var series = new Work { MediaCode = "serie", TmdbSeriesId = 45 };
        Assert.Equal("/tmdb-posters/filme/45", posters.CoverUrl(film));
        Assert.Equal("/tmdb-posters/serie/45", posters.CoverUrl(series));
        Assert.Equal("image/jpeg", posters.Find("serie", 45)!.Value.ContentType);
        Assert.False(Directory.Exists(fixture.Paths.Assets));

        var covers = Path.Combine(fixture.Paths.Assets, "covers");
        Directory.CreateDirectory(covers);
        await File.WriteAllBytesAsync(Path.Combine(covers, "local.jpg"), [0xff, 0xd8, 0xff, 0xd9], ct);
        Assert.Equal("/covers/local.jpg", posters.CoverUrl(film));

        clock.Now = now.AddDays(91);
        posters.PruneExpired();
        Assert.Equal("/covers/local.jpg", posters.CoverUrl(film));
        Assert.Null(posters.Find("filme", 45));
        File.Delete(Path.Combine(covers, "local.jpg"));
        Assert.Null(posters.CoverUrl(film));
        Assert.Null(posters.CoverUrl(series));
    }

    private sealed class TestClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class PosterResponse : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal("image.tmdb.org", request.RequestUri!.Host);
            Assert.Null(request.Headers.Authorization);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent([0xff, 0xd8, 0xff, 0xd9])
            });
        }
    }
}
