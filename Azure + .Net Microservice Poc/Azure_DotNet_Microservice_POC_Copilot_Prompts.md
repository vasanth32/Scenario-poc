# Azure + .NET Microservices — GitHub Copilot Prompt Guide

Companion to [Azure + .Net Microservice Poc.md](./Azure%20%2B%20.Net%20Microservice%20Poc.md). Each POC has ready-to-paste prompts for GitHub Copilot Chat. Backend microservices only, free tools, SQL Server LocalDB/Express, and .NET 10.

## How to Use

1. One POC = one Git branch (`poc-01-microservices`, `poc-02-http-comm`, ...).
2. Per POC follow the loop: **Learn (Ask mode) -> Build (Agent mode) -> Break it -> Review -> Interview answer**.
3. Read and understand every generated change before accepting. Type small parts yourself.
4. Keep POCs small: one concept, minimal code, tests for success and failure.
5. Commit after each working step.

### Starter `.github/copilot-instructions.md`

```markdown
- Target .NET 10 (`net10.0`) for every application and test project; use the .NET 10 SDK and C# 14.
- Use ASP.NET Core Web API and the current .NET 10 project conventions. Prefer the built-in OpenAPI support; add Swashbuckle only when interactive Swagger UI is explicitly required.
- Database: SQL Server (LocalDB/Express) with EF Core 10. Do not use SQLite.
- Use async/await, dependency injection, and the options pattern.
- Keep code minimal and readable; add comments only where non-obvious.
- Use xUnit for tests. Use free/local tooling only; no paid Azure resources.
- Services: Claims API, Policy API, Payments API (insurance domain).
- Keep all NuGet packages, container images, GitHub Actions setup, and .NET Aspire packages compatible with .NET 10.
- When I ask to explain, explain first and do not change code.
```

### .NET 10 Baseline

Apply this baseline to every POC unless a prompt explicitly says otherwise:

- Set `<TargetFramework>net10.0</TargetFramework>` in every application, worker, AppHost, and test project.
- Use the .NET 10 SDK in `global.json` and CI (`actions/setup-dotnet` with `dotnet-version: 10.0.x`).
- Use EF Core 10 packages and matching `dotnet-ef` tooling for migrations.
- Use official .NET 10 container tags, such as `mcr.microsoft.com/dotnet/aspnet:10.0` and `mcr.microsoft.com/dotnet/sdk:10.0`.
- Use the .NET Aspire release that supports .NET 10 and keep its packages on the same compatible release line.
- If a package or tool does not yet support .NET 10, stop and report the compatibility issue rather than silently targeting an older framework.

### Per-POC Prompt Pattern

- **P1 Learn (Ask):** explain the concept, the problem it solves, trade-offs.
- **P2 Build (Agent):** implement the smallest working example.
- **P3 Break (Agent):** simulate failures and add tests.

- **P4 Review (Ask):** review for bugs, then produce an interview answer.

---

## Phase 1: Microservices Fundamentals

### POC 1: Build Independent Microservices

**P1 Learn**

```
Explain microservices vs monolith for a .NET developer. Cover service boundaries, independent deployment, and separation of concerns using an insurance Claims and Policy example. Include when NOT to use microservices.
```

**P2 Build**

```
Create a .NET 10 solution "InsuranceClaims" with two ASP.NET Core Web API projects targeting `net10.0`: Claims.Api and Policy.Api, each in its own folder with its own .csproj. Policy.Api: GET /api/policies/{id}. Claims.Api: GET /api/claims/{id}, POST /api/claims. Use in-memory lists for now, DTOs (not entities) in responses, built-in OpenAPI support plus Swashbuckle for interactive Swagger UI, and different launch ports. Do not share code between the projects.
```

**P3 Break**

```
Add validation and proper status codes (400, 404) to both APIs. Add a .http file with sample requests for each endpoint, including invalid input.
```

**P4 Review**

```
Review these two services for service-boundary violations or hidden coupling. Then give me a concise senior-level interview answer: "How do you decide service boundaries?"
```

**Done:** both APIs run independently, tested via Swagger and Postman.

### POC 2: Service-to-Service Communication

**P1 Learn**

```
Explain synchronous service-to-service HTTP calls in .NET: IHttpClientFactory, typed clients, why not `new HttpClient()` per call (socket exhaustion, DNS), and the coupling downsides.
```

**P2 Build**

```
In Claims.Api, add a typed HttpClient "PolicyClient" registered with AddHttpClient that calls Policy.Api GET /api/policies/{id}. On POST /api/claims, verify the policy exists and is active before creating the claim. Read the Policy API base URL from configuration using the options pattern.
```

**P3 Break**

```
Handle Policy.Api returning 404, 500, timeouts, and being down. Set an HttpClient timeout, map failures to meaningful responses (e.g., 422, 503), and log each case. Add xUnit tests using a fake HttpMessageHandler.
```

**P4 Review**

```
Review PolicyClient for misuse of HttpClient, missing cancellation tokens, and swallowed exceptions. Give me an interview answer: "What are the risks of synchronous calls between microservices?"
```

**Done:** claim creation depends on Policy API; failures handled cleanly.

### POC 3: Database per Service

**P1 Learn**

```
Explain database-per-service, data ownership, and why cross-service joins/foreign keys are avoided. Explain eventual consistency and how services share data (APIs/events, not tables).
```

**P2 Build**

```
Add EF Core 10 with SQL Server (LocalDB) to Claims.Api and Policy.Api, each targeting `net10.0` and having its own DbContext, its own database (ClaimsDb, PolicyDb), entities, and initial migration. Implement CRUD endpoints. Claims stores only PolicyId (no foreign key to the Policy database). Connection strings come from appsettings/environment variables. Use EF Core 10-compatible migration and `dotnet-ef` commands.
```

**P3 Break**

```
Add a migration that adds a column, apply it, then show how to roll it back. Add integration tests using a test database per run. Demonstrate what happens if a claim references a policy that was later deleted, and propose a handling strategy.
```

**P4 Review**

```
Review the data access code for N+1 queries, missing AsNoTracking on reads, and missing indexes. Give me an interview answer: "How do you handle reporting across databases-per-service?"
```

**Done:** two separate databases, migrations applied, CRUD works.

---

## Phase 2: Resilience and Security

### POC 4: Resilience and Fault Tolerance

**P1 Learn**

```
Explain timeouts, retries with exponential backoff and jitter, circuit breaker, and bulkhead. Explain cascading failures and which operations are safe to retry.
```

**P2 Build**

```
Add Microsoft.Extensions.Http.Resilience (or Polly v8) to the PolicyClient in Claims.Api: timeout per attempt, retry with exponential backoff and jitter for transient errors only, a circuit breaker, and an overall timeout. Make settings configurable.
```

**P3 Break**

```
Add a toggle endpoint in Policy.Api to simulate latency, random 500s, and downtime. Write a test/script showing retries happening, the circuit opening, and recovery. Log each retry and circuit state change.
```

**P4 Review**

```
Review my resilience configuration for retry storms and non-idempotent retries. Give me an interview answer: "How do you prevent cascading failures?"
```

**Done:** circuit breaker opens under failure and recovers.

### POC 5: Authentication and Authorization

**P1 Learn**

```
Explain OAuth 2.0, OpenID Connect, JWT structure (header, payload, signature), access vs refresh tokens, and authentication vs authorization. Explain where Microsoft Entra ID fits.
```

**P2 Build**

```
Add JWT bearer authentication to Claims.Api and Policy.Api. For local learning, create a small token-issuing endpoint or tool that signs JWTs with a symmetric key from configuration. Add roles (Adjuster, Customer) and authorization policies, e.g., only Adjuster can approve claims.
```

**P3 Break**

```
Write tests for: no token (401), expired token (401), wrong role (403), tampered signature (401), valid token (200). Show how to read the claims in a controller.
```

**P4 Review**

```
Review my JWT validation settings (issuer, audience, lifetime, clock skew, key handling). Then explain how this would change using Microsoft Entra ID, and give an interview answer: "401 vs 403, and how do you secure service-to-service calls?"
```

**Done:** protected endpoints behave correctly for all token scenarios.

### POC 6: Azure Key Vault and Managed Identity

Continue from here
**P1 Learn**

```
Explain secrets management, Azure Key Vault, Managed Identity (system vs user-assigned), least privilege RBAC, secret rotation, and why secrets must not be in source control.
```

**P2 Build**

```
Refactor configuration so secrets (DB connection string, JWT signing key) come from environment variables locally and from user-secrets in development. Then add optional Azure Key Vault configuration using DefaultAzureCredential, enabled only when a KeyVault:Uri setting exists.
```

**P3 Break**

```
Show how DefaultAzureCredential resolves credentials locally vs in Azure. List common access errors (403 Forbidden, missing RBAC role, network rules) and how to troubleshoot each.
```

**P4 Review**

```
Scan the repo for hardcoded secrets and risky config. Give me an interview answer: "How does a service access Key Vault without storing credentials?"
```

**Done:** no secrets in code; Key Vault integration understood.

---

## Phase 3: Event-Driven Microservices

### POC 7: Asynchronous Messaging

**P1 Learn**

```
Compare synchronous vs asynchronous communication. Explain queues vs topics, Azure Service Bus concepts (peek-lock, complete/abandon, retries, dead-letter queue), and at-least-once delivery.
```

**P2 Build**

```
Define an IEventPublisher abstraction. Publish a ClaimSubmitted event when a claim is created. Implement an in-memory Channel<T> publisher and a BackgroundService consumer in a separate Payments.Api (or worker) that logs and processes it. Keep the abstraction so Azure Service Bus can replace it later.
```

**P3 Break**

```
Make the consumer fail randomly. Add retry with a max delivery count and a dead-letter store for poison messages. Add tests proving failed messages end up in the dead-letter store.
```

**P4 Review**

```
Review the messaging code for lost messages and ordering assumptions. Show how to swap in Azure Service Bus (ServiceBusClient/Processor) and the free emulator or free-tier options. Interview answer: "Queue vs topic, and what is a dead-letter queue?"
```

**Done:** event flows asynchronously with retry and dead-lettering.

### POC 8: Saga and Transactional Outbox

**P1 Learn**

```
Explain why distributed transactions (2PC) are avoided, eventual consistency, Saga orchestration vs choreography, compensating actions, and the Transactional Outbox pattern with the dual-write problem.
```

**P2 Build**

```
Implement a claim workflow: ClaimSubmitted -> PolicyValidated -> PaymentReserved -> ClaimApproved. Use choreography via events. In Claims.Api, implement the Transactional Outbox: save the claim and an OutboxMessage row in the same DB transaction; a background worker publishes pending outbox messages and marks them sent.
```

**P3 Break**

```
Simulate payment failure and implement compensation (cancel claim, release reserved amount). Simulate a crash between saving and publishing and show the outbox still delivers. Add tests for both.
```

**P4 Review**

```
Review for race conditions in the outbox publisher and for missing compensation paths. Interview answer: "How do you keep data consistent across services without distributed transactions?"
```

**Done:** workflow completes, compensates on failure, no lost events.

### POC 9: Idempotency and Duplicate Message Handling

**P1 Learn**

```
Explain idempotency for HTTP APIs (Idempotency-Key header) and for message consumers. Explain why at-least-once delivery requires idempotent handlers.
```

**P2 Build**

```
Add an idempotency middleware/filter for POST /api/claims using an Idempotency-Key header stored in SQL Server with a unique index and stored response. In the consumers, add an inbox/processed-messages table to skip duplicates by MessageId.
```

**P3 Break**

```
Write tests sending the same request twice concurrently and the same message twice. Verify only one claim and one payment are created, and the second call returns the original response.
```

**P4 Review**

```
Review for concurrency gaps (check-then-insert races) and key expiry. Interview answer: "How do you prevent duplicate payments?"
```

**Done:** duplicates are safely ignored.

---

## Phase 4: Azure Architecture and Observability

### POC 10: API Gateway and Azure API Management

**P1 Learn**

```
Explain the API gateway pattern, its responsibilities (routing, auth, rate limiting, transformation), Azure API Management policies, subscription keys, backend auth, and mTLS.
```

**P2 Build**

```
Create a Gateway project using YARP (free reverse proxy) routing /claims/* to Claims.Api and /policies/* to Policy.Api. Add JWT validation at the gateway, rate limiting (built-in .NET rate limiter), and header transformation (add X-Correlation-Id).
```

**P3 Break**

```
Send traffic exceeding the rate limit and show 429 responses with Retry-After. Stop a backend and show gateway behavior (502/503). Add tests.
```

**P4 Review**

```
Show the equivalent Azure APIM policy XML for JWT validation, rate-limit-by-key, and set-header, with explanations. Interview answer: "Why use an API gateway, and what are its downsides?"
```

**Done:** all traffic goes through the gateway.

### POC 11: Logging, Monitoring and Distributed Tracing

**P1 Learn**

```
Explain structured logging, correlation IDs, OpenTelemetry (traces, metrics, logs), health checks, and Application Insights basics.
```

**P2 Build**

```
Add Serilog structured logging (console + file) to all services, enrich with CorrelationId, propagate it across HTTP calls and messages. Add OpenTelemetry tracing and metrics with a console or OTLP exporter, plus /health/live and /health/ready (including SQL Server check).
```

**P3 Break**

```
Add endpoints that simulate slow responses and database failure. Show how a single request can be traced across Gateway, Claims, Policy, and Payments in the logs/traces.
```

**P4 Review**

```
Check logs for sensitive data leaks (PII, tokens). Give me 5 Application Insights KQL queries for failures, slow requests, and dependency errors. Interview answer: "How do you troubleshoot a slow request across services?"
```

**Done:** one correlation ID visible end to end.

---

## Phase 5: Containers, CI/CD and Deployment

### POC 12: Docker and Local Orchestration

**P1 Learn**

```
Explain images vs containers, multi-stage Dockerfiles for .NET 10, Docker Compose networking, health checks, volumes for persistent storage, and what .NET Aspire adds.
```

**P2 Build**

```
Create multi-stage Dockerfiles targeting .NET 10 for each API, using `mcr.microsoft.com/dotnet/sdk:10.0` for build stages and `mcr.microsoft.com/dotnet/aspnet:10.0` for runtime stages. Run as a non-root user with a small runtime image. Create a docker-compose.yml running all APIs, the gateway, and SQL Server (developer/express image) with a volume. Configure via environment variables and service names for networking.
```

**P3 Break**

```
Add container health checks and depends_on conditions. Kill one container and show how the others behave. Then create a .NET Aspire AppHost targeting .NET 10, using the Aspire release compatible with .NET 10, that runs the same services for comparison.
```

**P4 Review**

```
Review Dockerfiles for image size, layer caching, secrets in images, and running as root. Interview answer: "How do you containerize a .NET service for production?"
```

**Done:** `docker compose up` runs the whole platform.

### POC 13: CI/CD Pipeline

**P1 Learn**

```
Explain CI vs CD, GitHub Actions workflow structure, build/test/publish stages, environment-specific configuration, and branch protection.
```

**P2 Build**

```
Create xUnit unit tests and integration tests (using WebApplicationFactory and a SQL Server test container or LocalDB). Create .github/workflows/ci.yml that installs the .NET 10 SDK with `actions/setup-dotnet` (`10.0.x`), restores, builds, tests with coverage, and runs dotnet format --verify-no-changes on every push and PR.
```

**P3 Break**

```
Add a Docker build job and push to GitHub Container Registry on main. Add a deploy job stub with environment approvals. Show how a failing test blocks the merge.
```

**P4 Review**

```
Review the workflow for secret handling, caching, and permissions (least privilege GITHUB_TOKEN). Interview answer: "Describe your CI/CD pipeline for microservices."
```

**Done:** green pipeline on every push.

### POC 14: Azure Deployment

**P1 Learn**

```
Compare Azure App Service, Container Apps, and AKS: use cases, scaling, cost, operational complexity. Explain managed identity, Key Vault, VNet networking, and monitoring in production.
```

**P2 Build**

```
Create a deployment architecture document (with a Mermaid diagram) for the microservices on Azure Container Apps with Service Bus, SQL Database, Key Vault, and Application Insights. Write Bicep templates for the resources but do NOT deploy.
```

**P3 Break**

```
List the cost drivers and free-tier limits I must verify before deploying anything. Add a checklist for rollback, health probes, and blue/green or revision-based deployment.
```

**P4 Review**

```
Review the architecture for single points of failure and security gaps. Interview answer: "When would you choose AKS over Container Apps?"
```

**Done:** deployable design and Bicep reviewed; deploy only after cost check.

---

## Phase 6: Senior-Level Architecture

### POC 15: Microservices Design Patterns

**P1 Learn**

```
For each pattern, give a 5-line explanation, a problem it solves, and a trade-off: API Gateway, Saga, Transactional Outbox, CQRS, Event-driven architecture, Database per service, Strangler Fig, Idempotency, Retry/Circuit Breaker.
```

**P2 Build**

```
Implement a small CQRS example in Claims.Api (separate command and query models, MediatR optional) and a Strangler Fig demo where the gateway routes one route to a "legacy" endpoint and another to the new service.
```

**P3 Break**

```
Map each pattern to where it appears in my repo with file references. Identify any pattern I misused or over-engineered.
```

**P4 Review**

```
Give me a one-page cheat sheet of these patterns with "when to use / when not to use", plus 10 interview questions with short answers.
```

**Done:** can explain each pattern with a real example from the repo.

### POC 16: Production Troubleshooting

**P1 Learn**

```
Give a troubleshooting playbook for HTTP 401, 403, 404, 429, 500, timeouts, slow SQL queries, duplicate messages, and inconsistent data across services. For each: likely causes, what to check first, tools.
```

**P2 Build**

```
Add a "chaos" toggle set to the services: inject 401/403/500, latency, DB connection failure, and duplicate message delivery. Document how to turn each on and the expected symptoms.
```

**P3 Break**

```
For each injected fault, walk me through diagnosing it using only logs, metrics, and traces. Write the findings as runbook entries (symptom, diagnosis, fix, prevention).
```

**P4 Review**

```
Quiz me with 10 production-incident scenarios, one at a time. Wait for my answer, then grade it and explain gaps.
```

**Done:** runbook exists; faults can be reproduced and diagnosed.

### POC 17: Final Mini Insurance Claims Platform

**P1 Plan**

```
Based on my repo, produce a gap list against this target: Claims, Policy, Payments APIs; separate databases; gateway; auth; async messaging; saga and outbox; idempotency; resilience; logging/tracing; Docker; tests; CI. Prioritize the gaps.
```

**P2 Build**

```
Close the top gap from the list. Make the smallest change, add tests, and update the README and architecture diagram. Then stop and wait.
```

**P3 Break**

```
Create an end-to-end test script: submit a claim through the gateway with a valid token, verify the saga completes, kill Payments mid-flow, verify recovery and no duplicates.
```

**P4 Review**

```
Act as a senior interviewer. Ask me 15 questions about my project architecture, decisions, and trade-offs, one at a time, and grade each answer.
```

**Done:** README, diagram, passing CI, and a 5-minute project walkthrough.

---

## Reusable Prompt Library

- **Explain error:** `Explain this error and the most likely root cause. Give 3 things to check before changing code: <paste error>`
- **Code review:** `Review the selected code for bugs, security issues, concurrency problems, and performance. List only high-confidence issues.`
- **Refactor:** `Refactor this to follow SOLID and async best practices without changing behavior. Show a diff summary.`
- **Generate tests:** `Write xUnit tests for success, validation failure, and dependency failure cases for this class.`
- **Interview prep:** `Turn this POC into 5 senior-level interview questions with concise, spoken-style answers and one follow-up each.`
- **Document decision:** `Write an ADR (context, decision, consequences, alternatives) for the choice I just made.`
- **Stuck:** `I'm stuck on <problem>. Ask me up to 3 clarifying questions, then suggest the smallest next step.`

## Free Tools

.NET 10 SDK, VS Code / Visual Studio Community, GitHub Copilot, EF Core 10 + SQL Server Express/LocalDB, Docker Desktop, Postman/Swagger, YARP, Serilog, OpenTelemetry, xUnit, GitHub Actions, .NET Aspire for .NET 10.

**Cost rule:** finish all POCs locally first; verify free-tier eligibility and limits before creating any Azure resource.
