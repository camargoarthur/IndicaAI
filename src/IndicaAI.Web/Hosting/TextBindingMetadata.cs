using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
namespace IndicaAI.Web.Hosting;
// Plain text fields can deliberately be empty (notably a private note).
public sealed class TextBindingMetadata : IDisplayMetadataProvider
{
    public void CreateDisplayMetadata(DisplayMetadataProviderContext context) { if (context.Key.ModelType == typeof(string)) context.DisplayMetadata.ConvertEmptyStringToNull = false; }
}
