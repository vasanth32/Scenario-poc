## Microservices Concurrency POCs – Cursor Prompt Script

Below are **ready-to-use Cursor AI prompts** for **all 7 POCs**.  
Run them **one-by-one** (each POC separate project or separate folder).

---

## Key Learnings (By Completing All POCs)

- Identify and reproduce real-world concurrency issues like deadlocks and conflicting updates.
- Apply optimistic concurrency control using `RowVersion` and handle `DbUpdateConcurrencyException`.
- Understand distributed transaction gaps across services and why inconsistency happens.
- Implement Saga patterns using both orchestration and choreography styles.
- Build reliable event-driven communication using asynchronous messaging.
- Prevent duplicate processing with idempotency keys and request replay handling.
- Strengthen service-to-service calls with retry, timeout, and circuit-breaker resilience policies.
- Improve observability by adding logging and troubleshooting flows under failure conditions.
---

---

## POC 3 — Distributed Transaction Problem

### Prompt

```text
Create two microservices:

1. OrderService API (Own SQL database)
2. PaymentService API (Own SQL database)

OrderService endpoint:
POST /api/orders/create
Steps:
1. Insert order record
2. Call PaymentService API to process payment

Simulate failure:
After inserting order, throw exception before payment success.

Demonstrate inconsistent data problem (order created but payment failed).
```

---

## POC 4 — Saga Pattern (Orchestration)

### Prompt

```text
Create SagaOrchestrator API.

Flow:
1. Receive CreateOrder request
2. Call OrderService
3. Call PaymentService
4. Call InventoryService
5. Call NotificationService

If payment fails:
Call compensation endpoint in OrderService to cancel order.

Implement orchestration logic using HTTP calls.
```

---

## POC 5 — Event-Driven Saga (Choreography)

### Prompt

```text
Integrate RabbitMQ into OrderService, PaymentService and InventoryService.

Flow:
1. OrderService publishes OrderCreated event
2. PaymentService consumes event and publishes PaymentProcessed event
3. InventoryService consumes PaymentProcessed event and reserves stock

Implement event publisher and consumer using RabbitMQ .NET client.
```

---

## POC 6 — Idempotency Handling

### Prompt

```text
Add IdempotencyKeys table:
IdempotencyKey (PK)
RequestHash
CreatedDate

Modify CreateOrder API:
Require Idempotency-Key header.
If key already exists, return previous response instead of processing again.
Store key after successful request.
```

---

## POC 7 — Circuit Breaker & Retry Strategy

### Prompt

```text
Implement Polly resilience policies in OrderService when calling PaymentService:

1. Retry policy (3 retries with exponential backoff)
2. Circuit breaker policy (break after 5 failures for 30 seconds)
3. Timeout policy (5 seconds)

Add logging to show retry attempts and circuit state.
```

---

## Done


## POC 2 — Optimistic Concurrency (RowVersion)

### Prompt

```text
Create a new Web API project named OptimisticConcurrencyDemo.

Create Orders table:
OrderId PK
ProductId
Quantity
Status
RowVersion (rowversion/timestamp column)

Configure EF Core concurrency token using [Timestamp].

Create PUT /api/orders/update endpoint that updates order.

Simulate concurrent update by calling same endpoint twice and handle DbUpdateConcurrencyException with retry logic.
```
