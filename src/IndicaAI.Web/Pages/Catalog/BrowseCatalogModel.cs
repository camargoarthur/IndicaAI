using IndicaAI.Web.Features.Library;
using IndicaAI.Web.Hosting;
namespace IndicaAI.Web.Pages.Catalog;

public sealed class IndexModel(DataPaths paths, LibraryQuery query) : Library.BrowseModel(paths, query);
