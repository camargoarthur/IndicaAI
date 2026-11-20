using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Operations;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Tests.Integration;
public class RestoreDamagedActiveTests
{
 [Theory][InlineData(true)][InlineData(false)]
 public async Task HealthyBundleRepairsDamagedActiveAndPreservesIt(bool corruptDatabase)
 {
  var f=new TestDatabase();await f.InitializeAsync();var bundle=await new BackupService(f.Paths).CreateAsync("manual");
  if(corruptDatabase)await File.WriteAllBytesAsync(f.Paths.Database,[1,2,3,4,5]);else File.Delete(Path.Combine(f.Paths.Assets,"covers","cidade-distante.png"));
  var damaged=await File.ReadAllBytesAsync(f.Paths.Database);await new RestoreService(f.Paths).RestoreAsync(bundle);
  await using var db=f.Open();Assert.Equal(5,await db.Works.CountAsync());Assert.True(File.Exists(Path.Combine(f.Paths.Assets,"covers","cidade-distante.png")));
  var previous=Assert.Single(Directory.GetDirectories(f.Paths.Root,"recovery-*"));Assert.Equal(damaged,await File.ReadAllBytesAsync(Path.Combine(previous,"data","indicaai.db")));
 }
}
