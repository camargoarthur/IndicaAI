using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Domain;
using Microsoft.EntityFrameworkCore;
namespace IndicaAI.Tests.Integration;

public class CatalogIntegrityTests
{
    [Fact]
    public async Task SpecificFieldsCannotAttachToWrongMedium()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); await using (var db = f.Open()) { db.Games.Add(new() { WorkId = "cidade-distante", Studio = "Inválido" }); await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync()); }
        await using (var db = f.Open()) { var film = await db.Films.SingleOrDefaultAsync(f => f.WorkId == "mare-de-estrelas"); if (film is null) db.Films.Add(new() { WorkId = "mare-de-estrelas", DurationMinutes = -1 }); else film.DurationMinutes = -1; await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync()); }
        await using (var db = f.Open()) { db.Credits.Add(new() { WorkId = "cidade-distante", Role = "director", Position = 1, Name = "Inválido" }); await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync()); }
    }
}
