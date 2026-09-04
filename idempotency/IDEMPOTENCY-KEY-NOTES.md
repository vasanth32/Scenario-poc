# Idempotency-Key: How It Works in Real Life

## Who generates it

The **client** (not the server) generates the key — typically a GUID/UUID — once per _logical business operation_, not per HTTP call.

```
Idempotency-Key: 3f29a1c4-9e3b-4b7a-8e21-4b6e5b6a9d10
```

## When a new key is generated vs reused

| Scenario                                                          | Key behavior                                                           |
| ----------------------------------------------------------------- | ---------------------------------------------------------------------- |
| User clicks "Send Wire" button                                    | Client generates **one new GUID**, stores it locally (in memory/state) |
| Network times out / user double-clicks / auto-retry by client SDK | Client **resends the same GUID** with the same payload                 |
| User clicks "Send Wire" again for a genuinely new transfer        | Client generates a **brand-new GUID**                                  |

The client generates a unique ID **once per user intent/action**, then reuses that exact key for all retries of _that specific attempt_. It never reuses a key across different logical requests.

## Does the DB store every key forever?

Yes — every key+request combination is stored, as `WireRequestsController.cs` does via the `IdempotencyRecord` model. That's expected and by design:

1. **Storage is cheap** — one row per attempted operation (not per retry).
2. Rows are typically **not kept forever in practice** — production systems add a `CreatedAt`-based expiry (e.g., 24 hours) and a background job or DB TTL that purges old records, since idempotency keys are only useful for the retry window (seconds to a few hours), not permanently.
3. The current schema has `CreatedAt`/`CompletedAt` but no cleanup job — that's something to add for production, e.g.:
   ```sql
   DELETE FROM IdempotencyRecords WHERE CreatedAt < DATEADD(day, -1, GETUTCDATE());
   ```

## Practical flow end-to-end

1. Client generates GUID → `idempotencyKey = Guid.NewGuid()`.
2. Client sends `POST /api/wires` with header `Idempotency-Key: <guid>` and the JSON body.
3. Server hashes the body (`GenerateHash(request)`) and checks if that key already exists:
   - **Key not found** → process normally, insert `IdempotencyRecord` with `Status = Processing`, do the work, update to `Completed`, store the `Response`.
   - **Key found + same request hash** → it's a retry of the same attempt → return the previously stored result instead of re-processing (prevents double wire transfer).
   - **Key found + different request hash** → client reused a key for a different payload → return `409 Conflict` (client bug/misuse case).
4. Client keeps its GUID only for the duration it might retry (e.g., until it gets a definitive success/failure), then discards it — a new operation gets a new GUID.

## Summary

One key per attempted business operation, generated client-side, reused only for retries of that same operation, and stored server-side with an expiry policy (which the current implementation doesn't yet enforce — the DB keeps records indefinitely as it stands).
