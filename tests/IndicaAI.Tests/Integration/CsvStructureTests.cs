using System.Text;
using IndicaAI.Tests.Fixtures;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Features.Imports;
namespace IndicaAI.Tests.Integration;

public class CsvStructureTests
{
    [Fact]
    public async Task BlankRecordsAreCountedAndStructureLimitsAreStrict()
    {
        var f = new TestDatabase(); await f.InitializeAsync(); var service = new ImportPreviewService(f.Paths); var plan = await service.CreateAsync(new MemoryStream(Encoding.UTF8.GetBytes("\ntag_id;nome\n\nx;Nome\n;\n")), "tags");
        await using var db = f.Open(); var batch = await db.ImportBatches.FindAsync(plan.BatchId); Assert.Equal(3, batch!.IgnoredRecords);
        await Assert.ThrowsAsync<DomainException>(() => CsvInputReader.ReadAsync(new MemoryStream([0xff, 0xfe, 0xff]), "tags"));
        await Assert.ThrowsAsync<DomainException>(() => CsvInputReader.ReadAsync(new MemoryStream(Encoding.UTF8.GetBytes("tag_id;nome\nx;nome;extra\n")), "tags"));
        await Assert.ThrowsAsync<DomainException>(() => CsvInputReader.ReadAsync(new MemoryStream(Encoding.UTF8.GetBytes("tag_id\n" + string.Join('\n', Enumerable.Range(0, 10001).Select(i => "id" + i)))), "tags"));
    }
}
