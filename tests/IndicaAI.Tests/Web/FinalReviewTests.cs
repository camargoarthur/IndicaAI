using System.Net;
using System.Text.RegularExpressions;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Features.Tags;
using IndicaAI.Web.Persistence;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Tests.Web;
public class FinalReviewTests
{
 [Fact]public async Task ImportActionFormatsItsRecordCount()
 {
  await using var app=new WebFixture();using var client=app.CreateClient();await client.GetAsync("/Library");var group=await new TagService(app.Paths).SaveGroupAsync("group",null,0,"G");var csv=$"tag_id;nome;grupo_id;midias\nx;Aventura;{group.Id};livro\n";var preview=await new IndicaAI.Web.Features.Imports.ImportPreviewService(app.Paths).CreateAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv)),"tags");var html=WebUtility.HtmlDecode(await client.GetStringAsync("/Admin/Import/Preview?id="+preview.BatchId));Assert.DoesNotContain("{0",html);Assert.Contains("Importar um registro válido",html);
 }
 [Fact]public async Task ReplayingTheOriginalInclusionFormReturnsSuccessOnce()
 {
  await using var app=new WebFixture();using var client=app.CreateClient(new(){AllowAutoRedirect=false});var html=await client.GetStringAsync("/Works/Details?id=cidade-distante");var token=Regex.Match(html,"name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
  var data=new Dictionary<string,string>{{"__RequestVerificationToken",token},{"Input.OperationId","repeat-form"},{"Input.Version","0"},{"Input.StateCode","in_progress"},{"Input.Note",""}};
  for(var i=0;i<2;i++)Assert.Equal(HttpStatusCode.Redirect,(await client.PostAsync("/Works/Details?id=cidade-distante&handler=Save",new FormUrlEncodedContent(data))).StatusCode);
  await using var db=new AppDbContext(app.Paths);Assert.Single(await db.Trackings.ToArrayAsync());Assert.Single(await db.Cycles.ToArrayAsync());Assert.Single(await db.Events.ToArrayAsync());
 }
 [Fact]public async Task FilterFormRetainsMediaAndOffersUnambiguousApplicableTags()
 {
  await using var app=new WebFixture();using var client=app.CreateClient();await client.GetAsync("/Library");var service=new TagService(app.Paths);var a=await service.SaveGroupAsync("g1",null,0,"Gênero");var b=await service.SaveGroupAsync("g2",null,0,"Ambiente");await service.SaveAsync(new("t1",null,0,a.Id,"Aventura",["livro"]));await service.SaveAsync(new("t2",null,0,b.Id,"Aventura",["livro"]));await service.SaveAsync(new("t3",null,0,a.Id,"Só jogos",["jogo"]));var html=await client.GetStringAsync("/Catalog?Media=livro");
  Assert.Matches("name=\"Media\"[^>]*value=\"livro\"[^>]*type=\"hidden\"|type=\"hidden\"[^>]*name=\"Media\"[^>]*value=\"livro\"",html);Assert.DoesNotContain("Só jogos",WebUtility.HtmlDecode(html));Assert.Contains("Gênero",WebUtility.HtmlDecode(html));Assert.Contains("Ambiente",html);
 }
 [Fact]public async Task EditableAdministrativeAndHistoryFormsDeclareDraftSupport()
 {
  await using var app=new WebFixture();using var client=app.CreateClient();await client.GetAsync("/Library");var tracking=await new IndicaAI.Web.Features.Tracking.TrackingService(app.Paths).AddAsync(new("a","cidade-distante","planned",new(null,null,false)));
  foreach(var path in new[]{"/Lists","/Admin/Tags","/Admin/Associations","/Settings","/Works/History?id="+tracking.TrackingId}){var html=await client.GetStringAsync(path);foreach(Match form in Regex.Matches(html,@"<form\b[^>]*>[\s\S]*?</form>")){if(form.Value.Contains("data-quick-preferences")||!Regex.IsMatch(form.Value,@"<(input[^>]+type=\""(?:text|date)\""|select|textarea)"))continue;Assert.Contains("data-draft",form.Value);}}
 }
}
