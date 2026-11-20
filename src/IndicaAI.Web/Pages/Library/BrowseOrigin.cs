using Microsoft.AspNetCore.Mvc;

namespace IndicaAI.Web.Pages.Library;

public static class BrowseOrigin
{
    public static string? SafeUrl(IUrlHelper urls, string? url)
    {
        if (url is null || !urls.IsLocalUrl(url)) return null;
        return Route(url) switch
        {
            "/library" or "/library/index" or "/catalog" or "/catalog/index" or "/archive" or "/archive/index" => url,
            _ => null
        };
    }

    public static string? CatalogUrl(IUrlHelper urls, string? url) =>
        SafeUrl(urls, url) is { } local && Route(local) is "/catalog" or "/catalog/index" ? local : null;

    private static string Route(string url) => url.Split('?', '#')[0].TrimEnd('/').ToLowerInvariant();
}
