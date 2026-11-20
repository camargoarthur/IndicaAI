using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IndicaAI.Web.Domain;
using IndicaAI.Web.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IndicaAI.Web.Persistence;

public static class DatabaseGate
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Gates = new(StringComparer.OrdinalIgnoreCase);
    public static SemaphoreSlim For(DataPaths paths) => Gates.GetOrAdd(paths.Root, _ => new(1, 1));
}

public sealed class CommandRunner(DataPaths paths, IInterceptor[]? interceptors = null)
{
    public async Task<T> RunAsync<T>(string operationId, string kind, object payload, Func<AppDbContext, string, Task<T>> action, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(operationId) || operationId.Length > 128) throw new DomainException("operation_invalid");
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(kind + JsonSerializer.Serialize(payload))));
        var gate = DatabaseGate.For(paths); await gate.WaitAsync(ct);
        try
        {
            await using var db = new AppDbContext(paths, interceptors ?? []);
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var owner = (await db.Identities.SingleAsync(ct)).Id;
            var previous = await db.Receipts.FindAsync([owner, operationId], ct);
            if (previous is not null)
            {
                if (previous.RequestHash != hash) throw new DomainException("operation_reused");
                return JsonSerializer.Deserialize<T>(previous.Result)!;
            }
            var result = await action(db, owner);
            db.Receipts.Add(new() { IdentityId = owner, OperationId = operationId, Kind = kind, RequestHash = hash, Result = JsonSerializer.Serialize(result), RecordedAtUtc = DateTime.UtcNow.ToString("O") });
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return result;
        }
        catch (DbUpdateConcurrencyException) { throw new DomainException("conflict"); }
        finally { gate.Release(); }
    }
}
