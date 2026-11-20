using System.Text.RegularExpressions;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Hosting;

namespace IndicaAI.Web.Features.Catalog;

public sealed class TmdbPosterCache(DataPaths paths, HttpClient http, TimeProvider? clock = null)
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(90);
    private const int MaximumBytes = 2 * 1024 * 1024;
    private readonly TimeProvider time = clock ?? TimeProvider.System;
    private string CacheRoot => Path.Combine(paths.Root, "cache");
    private string DirectoryPath => Path.Combine(CacheRoot, "tmdb-posters");

    public string? CoverUrl(Work work)
    {
        if (HasLocalCover(paths, work)) return "/covers/" + Uri.EscapeDataString(work.CoverAsset!);
        var id = work.MediaCode switch { "filme" => work.TmdbMovieId, "serie" => work.TmdbSeriesId, _ => null };
        return id is int externalId && Find(work.MediaCode, externalId) is not null
            ? $"/tmdb-posters/{work.MediaCode}/{externalId}" : null;
    }

    internal static bool HasLocalCover(DataPaths paths, Work work) => work.CoverAsset is { Length: > 0 } name
        && Path.GetFileName(name) == name && File.Exists(Path.Combine(paths.Assets, "covers", name));

    public (string Path, string ContentType)? Find(string media, int externalId)
    {
        var path = PathFor(media, externalId);
        if (path is null) return null;
        try
        {
            if (!IsOrdinaryDirectory(CacheRoot) || !IsOrdinaryDirectory(DirectoryPath)) return null;
            var file = new FileInfo(path);
            if (!file.Exists || file.Attributes.HasFlag(FileAttributes.ReparsePoint) || file.Length > MaximumBytes) return null;
            if (file.LastWriteTimeUtc <= time.GetUtcNow().UtcDateTime - Lifetime)
            {
                File.Delete(path);
                return null;
            }
            using var stream = File.OpenRead(path);
            Span<byte> prefix = stackalloc byte[12];
            var size = stream.ReadAtLeast(prefix, prefix.Length, false);
            return ContentType(prefix[..size]) is { } mime ? (path, mime) : null;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { return null; }
    }

    public async Task<bool> TryStoreAsync(string media, int externalId, string? posterUrl, CancellationToken ct)
    {
        var path = PathFor(media, externalId);
        if (path is null || posterUrl is null || !Regex.IsMatch(posterUrl,
            @"\Ahttps://image\.tmdb\.org/t/p/w342/[A-Za-z0-9_-]+\.(jpg|png|webp)\z")) return false;
        if (Find(media, externalId) is not null) return true;

        string? temporary = null;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        var transfer = timeout.Token;
        try
        {
            using var response = await http.GetAsync(posterUrl, HttpCompletionOption.ResponseHeadersRead, transfer);
            if (!response.IsSuccessStatusCode || response.Content.Headers.ContentLength > MaximumBytes) return false;
            await using var source = await response.Content.ReadAsStreamAsync(transfer);
            using var bytes = new MemoryStream();
            var buffer = new byte[16 * 1024];
            int read;
            while ((read = await source.ReadAsync(buffer, transfer)) > 0)
            {
                if (bytes.Length + read > MaximumBytes) return false;
                bytes.Write(buffer, 0, read);
            }
            if (ContentType(bytes.GetBuffer().AsSpan(0, (int)bytes.Length)) is null) return false;
            if (Directory.Exists(CacheRoot) && !IsOrdinaryDirectory(CacheRoot)) return false;
            Directory.CreateDirectory(DirectoryPath);
            if (!IsOrdinaryDirectory(DirectoryPath)) return false;
            temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            await File.WriteAllBytesAsync(temporary, bytes.ToArray(), ct);
            File.SetLastWriteTimeUtc(temporary, time.GetUtcNow().UtcDateTime);
            File.Move(temporary, path, overwrite: true);
            return true;
        }
        catch (Exception error) when (error is HttpRequestException or IOException or UnauthorizedAccessException
            || error is OperationCanceledException && !ct.IsCancellationRequested) { return false; }
        finally
        {
            if (temporary is not null)
            {
                try { File.Delete(temporary); }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
            }
        }
    }

    public void PruneExpired()
    {
        try
        {
            if (!IsOrdinaryDirectory(CacheRoot) || !IsOrdinaryDirectory(DirectoryPath)) return;
            var cutoff = time.GetUtcNow().UtcDateTime - Lifetime;
            foreach (var file in new DirectoryInfo(DirectoryPath).EnumerateFiles("*.image"))
                if (!file.Attributes.HasFlag(FileAttributes.ReparsePoint) && file.LastWriteTimeUtc <= cutoff) file.Delete();
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
    }

    private string? PathFor(string media, int id) => media is "filme" or "serie" && id > 0
        ? Path.Combine(DirectoryPath, $"{media}-{id}.image") : null;

    private static bool IsOrdinaryDirectory(string path) => Directory.Exists(path)
        && !File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint);

    private static string? ContentType(ReadOnlySpan<byte> bytes)
    {
        if (bytes.StartsWith(new byte[] { 0xff, 0xd8, 0xff })) return "image/jpeg";
        if (bytes.StartsWith(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a })) return "image/png";
        return bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes[8..12].SequenceEqual("WEBP"u8) ? "image/webp" : null;
    }
}
