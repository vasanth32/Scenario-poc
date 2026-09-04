using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using idempotency.Data;
using idempotency.DTOs;
using idempotency.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace idempotency.Controllers;

[ApiController]
[Route("api/wires")]
public class WireRequestsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public WireRequestsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> CreateWire(
        [FromBody] CreateWireRequest request)
    {
        // 1. Read idempotency key
        var idempotencyKey = Request.Headers["Idempotency-Key"]
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return BadRequest("Idempotency-Key is required.");
        }

        // 2. Generate hash of request
        var requestHash = GenerateHash(request);

        // 3. Check if key already exists
        var existingRecord =
            await _db.IdempotencyRecords
                .FirstOrDefaultAsync(x => x.Key == idempotencyKey);

        if (existingRecord != null)
        {
            // Same key but different request
            if (existingRecord.RequestHash != requestHash)
            {
                return Conflict(
                    "Same idempotency key was used with a different request.");
            }

            // Request is still being processed
            if (existingRecord.Status == "Processing")
            {
                return Conflict(
                    "Request is already being processed.");
            }

            // Already completed
            return Ok(new
            {
                Message = "Returning previous result.",
                WireRequestId = existingRecord.WireRequestId,
                Status = existingRecord.Status
            });
        }

        // 4. Create wire
        var wire = new WireRequest
        {
            FromAccount = request.FromAccount,
            ToAccount = request.ToAccount,
            Amount = request.Amount,
            Currency = request.Currency,
            Status = "Created",
            CreatedAt = DateTime.UtcNow
        };

        _db.WireRequests.Add(wire);

        // 5. Create idempotency record
        var idempotencyRecord = new IdempotencyRecord
        {
            Key = idempotencyKey,
            RequestHash = requestHash,
            Status = "Processing",
            CreatedAt = DateTime.UtcNow
        };

        _db.IdempotencyRecords.Add(idempotencyRecord);

        // 6. Save both
        await _db.SaveChangesAsync();

        // Link the wire
        idempotencyRecord.WireRequestId = wire.Id;

        // Simulate processing
        wire.Status = "Completed";
        wire.CompletedAt = DateTime.UtcNow;

        idempotencyRecord.Status = "Completed";
        idempotencyRecord.Response = JsonSerializer.Serialize(new
        {
            WireRequestId = wire.Id,
            Status = wire.Status
        });

        idempotencyRecord.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            WireRequestId = wire.Id,
            Status = wire.Status
        });
    }

    private static string GenerateHash(CreateWireRequest request)
    {
        var json = JsonSerializer.Serialize(request);

        using var sha256 = SHA256.Create();

        var bytes = Encoding.UTF8.GetBytes(json);

        var hash = sha256.ComputeHash(bytes);

        return Convert.ToHexString(hash);
    }
}
