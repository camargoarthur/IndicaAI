using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Catalog;
using IndicaAI.Web.Hosting;
using IndicaAI.Web.Localization;
using IndicaAI.Web.Pages.Library;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IndicaAI.Web.Pages.Catalog;

public sealed class TmdbModel(TmdbMovies movies, TmdbCatalog catalog, TmdbPosterCache posters, DataPaths paths,
    UiMessages text, IDataProtectionProvider protection) : PageModel
{
    private readonly ITimeLimitedDataProtector previewProtector = protection.CreateProtector("Catalog.Tmdb.MoviePreview.v1").ToTimeLimitedDataProtector();

    [BindProperty(SupportsGet = true)] public string? Query { get; set; }
    [BindProperty(SupportsGet = true)] public string? Prefill { get; set; }
    [BindProperty(SupportsGet = true)] public int? Year { get; set; }
    [BindProperty(SupportsGet = true)] public int ResultPage { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public bool Remember { get; set; }
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }
    [BindProperty] public string? PreviewTicket { get; set; }
    [BindProperty] public bool IsAnime { get; set; }

    public TmdbSearch? Results { get; private set; }
    public TmdbMovie? Movie { get; private set; }
    public string? ErrorKey { get; private set; }
    public bool IsConfigured => movies.IsConfigured;
    public bool RecordRecentSearch => Remember && ResultPage == 1 && Results is not null;
    public string RecentSearchScope { get; } = Convert.ToHexString(SHA256.HashData(
        Encoding.UTF8.GetBytes(OperatingSystem.IsWindows() ? paths.Root.ToUpperInvariant() : paths.Root)));
    public string? CatalogReturnUrl => BrowseOrigin.CatalogUrl(Url, ReturnUrl);
    private string Language => text.Locale == "en" ? "en-US" : "pt-BR";

    public async Task OnGetAsync(CancellationToken ct)
    {
        if (!Request.Query.ContainsKey(nameof(Query)))
        {
            if (Prefill?.Trim() is { Length: > 0 and <= 160 } prefill) Query = prefill;
            return;
        }
        Query = Query?.Trim();
        if (!ModelState.IsValid || Query is not { Length: > 0 and <= 160 }
            || Year is <= 0 or > 9999 || ResultPage is < 1 or > 500)
        {
            ErrorKey = "TmdbInvalidSearch";
            return;
        }
        try { Results = await movies.SearchAsync(Query, Year, ResultPage, Language, ct); }
        catch (DomainException e) { ErrorKey = e.Code; }
    }

    public async Task<IActionResult> OnGetPreviewAsync(int id, CancellationToken ct)
    {
        if (id < 1) return InvalidPreview();
        if (await catalog.FindMovieAsync(id, ct) is { } saved) return OpenWork(saved);
        try
        {
            Movie = await movies.GetMovieAsync(id, Language, ct);
            var reviewed = Movie with { Overview = null };
            PreviewTicket = previewProtector.Protect(JsonSerializer.Serialize(reviewed), TimeSpan.FromMinutes(30));
        }
        catch (DomainException e) { ErrorKey = e.Code; }
        return Page();
    }

    public async Task<IActionResult> OnPostConfirmAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid || string.IsNullOrEmpty(PreviewTicket)) return InvalidPreview();
        TmdbMovie? movie;
        try { movie = JsonSerializer.Deserialize<TmdbMovie>(previewProtector.Unprotect(PreviewTicket)); }
        catch (Exception e) when (e is CryptographicException or JsonException) { return InvalidPreview(); }
        if (movie is null) return InvalidPreview();
        var workId = await catalog.ImportMovieAsync(movie, IsAnime, ct);
        await posters.TryStoreAsync("filme", movie.Id, movie.PosterUrl, ct);
        return OpenWork(workId);
    }

    private IActionResult InvalidPreview()
    {
        Response.StatusCode = StatusCodes.Status400BadRequest;
        ErrorKey = "TmdbInvalidPreview";
        return Page();
    }

    private RedirectToPageResult OpenWork(string id) => RedirectToPage("/Works/Details", new { id, BrowseReturnUrl = CatalogReturnUrl });
}
