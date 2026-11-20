using IndicaAI.Web.Hosting;
using IndicaAI.Web.Persistence;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using IndicaAI.Web.Features.Tracking;
using IndicaAI.Web.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using IndicaAI.Web.Operations;
using Microsoft.Extensions.FileProviders;
using System.Net;

if(args.Contains("--maintenance")){Environment.ExitCode=await MaintenanceCommand.RunAsync(args);return;}

var builder = WebApplication.CreateBuilder(args);
var port=int.TryParse(Environment.GetEnvironmentVariable("INDICAAI_PORT"),out var requestedPort)&&requestedPort is >0 and <65536?requestedPort:5273;
builder.WebHost.ConfigureKestrel(options=>options.Listen(IPAddress.Loopback,port));
builder.Services.AddRazorPages().AddMvcOptions(options=>{options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes=true;options.ModelMetadataDetailsProviders.Add(new TextBindingMetadata());});
builder.Services.AddSingleton(sp => DataPaths.ForRoot(builder.Configuration["IndicaAI:DataRoot"]
    ?? Environment.GetEnvironmentVariable("INDICAAI_DATA_ROOT")
    ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IndicaAI")));
builder.Services.AddSingleton<DatabaseBootstrapper>();
builder.Services.AddScoped<TrackingService>();
builder.Services.AddScoped<IndicaAI.Web.Features.Library.LibraryQuery>();
builder.Services.AddScoped<IndicaAI.Web.Features.Batch.BatchService>();
builder.Services.AddScoped<IndicaAI.Web.Features.Lists.ListService>();
builder.Services.AddScoped<IndicaAI.Web.Features.Archive.ArchiveService>();
builder.Services.AddScoped<StateLabelService>();
builder.Services.AddScoped<UiMessages>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IndicaAI.Web.Features.Imports.ImportPreviewService>();
builder.Services.AddScoped<IndicaAI.Web.Features.Imports.ImportApplyService>();
builder.Services.AddScoped<IndicaAI.Web.Features.Tags.TagService>();
builder.Services.AddScoped<IndicaAI.Web.Features.Settings.PreferenceService>();
builder.Services.AddOptions<KeyManagementOptions>().Configure<DataPaths>((options, paths) =>
{
    var dir = Directory.CreateDirectory(Path.Combine(paths.Root, "keys"));
    options.XmlRepository = new FileSystemXmlRepository(dir, NullLoggerFactory.Instance);
});
builder.Logging.SetMinimumLevel(LogLevel.Warning);
var app = builder.Build();
app.UseExceptionHandler("/Error");
using var instance=LocalInstance.Acquire(app.Services.GetRequiredService<DataPaths>());
await app.Services.GetRequiredService<DatabaseBootstrapper>().InitializeAsync();
await new IndicaAI.Web.Features.Imports.ImportRecoveryService(app.Services.GetRequiredService<DataPaths>()).ReconcileAsync();
app.Use(async(context,next)=>{
    var host=context.Request.Host.Host;
    var origin=context.Request.Headers.Origin.ToString();
    if((context.Connection.RemoteIpAddress is IPAddress remote&&!IPAddress.IsLoopback(remote))||!(host is "127.0.0.1" or "localhost" or "::1")||(!string.IsNullOrEmpty(origin)&&!string.Equals(origin,$"{context.Request.Scheme}://{context.Request.Host}",StringComparison.OrdinalIgnoreCase))){context.Response.StatusCode=403;return;}
    context.Response.Headers["Content-Security-Policy"]="default-src 'self'; img-src 'self'; style-src 'self'; script-src 'self'; frame-ancestors 'none'; form-action 'self'; base-uri 'none'";
    context.Response.Headers["X-Content-Type-Options"]="nosniff";
    context.Response.Headers["Referrer-Policy"]="same-origin";
    await next();
});
app.Use(async (context, next) =>
{
    await using var db = new AppDbContext(context.RequestServices.GetRequiredService<DataPaths>());
    var pref = await db.Preferences.SingleAsync(context.RequestAborted);
    context.Items["StateLabels"]=await db.StateLabels.Where(l=>l.IdentityId==pref.IdentityId).AsNoTracking().ToDictionaryAsync(l=>l.MediaCode+":"+l.StateCode,l=>l.Label,context.RequestAborted);
    var culture = CultureInfo.GetCultureInfo(LocaleRegistry.GetSupported().Any(l=>l.Code==pref.Locale)?pref.Locale:"pt-BR");
    CultureInfo.CurrentCulture = culture; CultureInfo.CurrentUICulture = culture;
    await next();
});
app.UseStaticFiles(new StaticFileOptions{FileProvider=new PhysicalFileProvider(app.Services.GetRequiredService<DataPaths>().Assets)});
app.UseStaticFiles();
app.MapRazorPages();
app.MapGet("/", () => Results.Redirect("/Library"));
app.Run();
public partial class Program;
