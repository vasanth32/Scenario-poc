# POC: Distributed Transaction Management with Saga Pattern

## Quick Setup Guide (2-3 hours)

### Prerequisites
- .NET 8 SDK installed
- SQL Server LocalDB or SQL Server Express
- Cursor AI ready

---

## Phase 1: Project Setup (15 minutes)

### Prompt 1: Create Solution Structure
```
Create a .NET solution for a microservices POC demonstrating Saga pattern for distributed transactions.

Create 4 microservices:
1. EnrollmentService - .NET 8 Web API
2. ActivityService - .NET 8 Web API  
3. FeeManagementService - .NET 8 Web API
4. NotificationService - .NET 8 Web API

Each service should:
- Use Entity Framework Core with SQL Server
- Have its own database (Database per Service pattern)
- Include Swagger/OpenAPI
- Have health check endpoints
- Use dependency injection

Create a shared class library: Common.Events for event definitions

Structure:
- SagaPatternPOC.sln
- src/
  - EnrollmentService/
  - ActivityService/
  - FeeManagementService/
  - NotificationService/
  - Common.Events/
```

### Prompt 2: Setup Databases
```
For each microservice, create:
1. DbContext with Entity Framework Core
2. Database connection string in appsettings.json
3. Initial migration
4. Seed data (optional)

EnrollmentService database: EnrollmentDb
- Enrollment table: Id, StudentId, ActivityId, Status, CreatedAt

ActivityService database: ActivityDb  
- Activity table: Id, Name, Capacity, ReservedCount
- ActivityReservation table: Id, ActivityId, StudentId, ReservedAt

FeeManagementService database: FeeDb
- FeeCalculation table: Id, EnrollmentId, StudentId, Amount, Status, CalculatedAt

NotificationService database: NotificationDb
- Notification table: Id, EnrollmentId, StudentId, Message, SentAt, Status
```

---

## Phase 2: Core Services Implementation (45 minutes)

### Prompt 3: Enrollment Service
```
Create EnrollmentService with:

1. EnrollmentController with POST endpoint: /api/enrollment
   - Request: { StudentId, ActivityId }
   - This will be the Saga orchestrator

2. Enrollment entity and DbContext
   - Status enum: Pending, Completed, Failed, Compensated

3. EnrollmentService class with:
   - CreateEnrollmentAsync method (local transaction)
   - CancelEnrollmentAsync method (compensation)
   - GetEnrollmentAsync method

4. Use HttpClient to call other services (ActivityService, FeeManagementService)
   - Configure HttpClient in Program.cs with base URLs from appsettings

5. Add logging with ILogger
```

### Prompt 4: Activity Service
```
Create ActivityService with:

1. ActivityController with endpoints:
   - POST /api/activity/reserve - Reserve capacity
   - POST /api/activity/release - Release capacity (compensation)
   - GET /api/activity/{id}/capacity - Check available capacity

2. Activity and ActivityReservation entities
   - Activity: Id, Name, Capacity, ReservedCount
   - ActivityReservation: Id, ActivityId, StudentId, ReservedAt

3. ActivityService class with:
   - ReserveCapacityAsync(activityId, studentId) - Returns bool
   - ReleaseCapacityAsync(activityId, studentId) - Compensation
   - CheckCapacityAsync(activityId) - Returns available capacity

4. Business logic: 
   - ReserveCapacity: Increment ReservedCount if Capacity > ReservedCount
   - ReleaseCapacity: Decrement ReservedCount
   - Both should be atomic (use transactions)
```

### Prompt 5: Fee Management Service
```
Create FeeManagementService with:

1. FeeManagementController with endpoints:
   - POST /api/fee/calculate - Calculate fees
   - POST /api/fee/cancel - Cancel fee calculation (compensation)

2. FeeCalculation entity
   - Id, EnrollmentId, StudentId, Amount, Status, CalculatedAt

3. FeeManagementService class with:
   - CalculateFeesAsync(enrollmentId, studentId) - Returns FeeCalculation
   - CancelFeeCalculationAsync(enrollmentId) - Compensation
   - Simple fee calculation: Base fee = 100, Activity fee = 50

4. Business logic:
   - CalculateFees: Create FeeCalculation record with Amount = 150
   - CancelFeeCalculation: Delete or mark as Cancelled
```

### Prompt 6: Notification Service
```
Create NotificationService with:

1. NotificationController with endpoint:
   - POST /api/notification/send - Send notification

2. Notification entity
   - Id, EnrollmentId, StudentId, Message, SentAt, Status

3. NotificationService class with:
   - SendNotificationAsync(enrollmentId, studentId, message)
   - This is async/event-driven (not part of Saga, eventual consistency)

4. For POC: Just log the notification (simulate sending email)
   - Log: "Notification sent to Student {studentId} for Enrollment {enrollmentId}"
```

---

## Phase 3: Saga Pattern Implementation (60 minutes)

### Prompt 7: Saga Orchestrator
```
In EnrollmentService, create EnrollmentSaga class that orchestrates the distributed transaction:

1. EnrollmentSaga class with method:
   - EnrollStudentAsync(EnrollmentRequest request) returns EnrollmentResult

2. Saga Steps (in order):
   Step 1: Create enrollment locally (local transaction)
   Step 2: Call ActivityService to reserve capacity
   Step 3: Call FeeManagementService to calculate fees
   Step 4: Publish event for NotificationService (async)

3. Compensation Logic:
   - If Step 2 fails: Cancel enrollment (Step 1 compensation)
   - If Step 3 fails: Release capacity (Step 2 compensation) + Cancel enrollment (Step 1 compensation)

4. Use try-catch for error handling
5. Log each step and compensation
6. Return EnrollmentResult with Success/Failure status

6. Update EnrollmentController to use EnrollmentSaga
```

### Prompt 8: HTTP Client Configuration
```
In EnrollmentService Program.cs:

1. Register HttpClient for ActivityService
   - Base URL from appsettings: "ActivityService:BaseUrl"
   - Add timeout: 5 seconds
   - Add retry policy (Polly): 3 retries with exponential backoff

2. Register HttpClient for FeeManagementService
   - Base URL from appsettings: "FeeManagementService:BaseUrl"
   - Add timeout: 5 seconds
   - Add retry policy: 3 retries

3. Register EnrollmentSaga as scoped service

4. Add appsettings.json with service URLs:
   - ActivityService:BaseUrl: "https://localhost:7001"
   - FeeManagementService:BaseUrl: "https://localhost:7002"
```

### Prompt 9: Error Handling and Compensation
```
Enhance EnrollmentSaga with:

1. Detailed error handling:
   - Catch HttpRequestException for service calls
   - Catch TimeoutException for timeouts
   - Log errors with context

2. Compensation methods:
   - CompensateStep1() - Cancel enrollment
   - CompensateStep2() - Release capacity
   - CompensateStep3() - Cancel fee calculation

3. Compensation should be idempotent (safe to call multiple times)

4. Return detailed error messages in EnrollmentResult
```

---

## Phase 4: Event-Driven Pattern (30 minutes)

### Prompt 10: Event Definitions
```
In Common.Events project, create:

1. IEvent interface (marker interface)

2. EnrollmentCreatedEvent class:
   - EnrollmentId, StudentId, ActivityId, CreatedAt

3. EnrollmentFailedEvent class:
   - EnrollmentId, StudentId, Reason, FailedAt

4. Make classes serializable (for future message bus integration)
```

### Prompt 11: Outbox Pattern (Simplified)
```
In EnrollmentService:

1. Create OutboxEvent entity:
   - Id (Guid), EventType (string), EventData (JSON string), Status, CreatedAt

2. Add OutboxEvent to EnrollmentDbContext

3. Modify EnrollmentSaga:
   - After successful enrollment, create OutboxEvent record
   - Store EnrollmentCreatedEvent as JSON in EventData
   - Save in same transaction as enrollment

4. Create background service (IHostedService):
   - Polls OutboxEvent table every 5 seconds
   - Finds Pending events
   - For POC: Just log the event (simulate publishing)
   - Mark as Published

5. Register background service in Program.cs
```

### Prompt 12: Notification Service Event Handler
```
In NotificationService:

1. Create NotificationEventHandler class

2. For POC: Create HTTP endpoint:
   - POST /api/notification/enrollment-created
   - Receives EnrollmentCreatedEvent
   - Creates Notification record
   - Logs notification sent

3. In real implementation, this would subscribe to message bus
```

---

## Phase 5: Testing and Validation (30 minutes)

### Prompt 13: Test Scenarios
```
Create a test client or Postman collection to test:

1. Happy Path:
   - POST /api/enrollment
   - Body: { "studentId": 1, "activityId": 1 }
   - Verify: Enrollment created, capacity reserved, fees calculated

2. Failure Scenario - Activity Full:
   - Set Activity Capacity = 0
   - Try to enroll
   - Verify: Enrollment cancelled (compensation)

3. Failure Scenario - Fee Service Down:
   - Stop FeeManagementService
   - Try to enroll
   - Verify: Capacity released, enrollment cancelled

4. Verify data in each database:
   - EnrollmentDb: Check enrollment status
   - ActivityDb: Check ReservedCount
   - FeeDb: Check FeeCalculation records
```

### Prompt 14: Add Logging and Observability
```
Add comprehensive logging:

1. In EnrollmentSaga, log:
   - Saga started
   - Each step execution
   - Step success/failure
   - Compensation execution
   - Saga completion

2. Add correlation ID:
   - Generate GUID at start of saga
   - Include in all logs
   - Include in all service calls (header)

3. Add request/response logging middleware

4. Use structured logging (Serilog recommended)
```

---

## Phase 6: Polish and Documentation (15 minutes)

### Prompt 15: API Documentation
```
1. Ensure Swagger is configured for all services
2. Add XML comments to controllers
3. Add example requests/responses
4. Document error codes and messages
```

### Prompt 16: README
```
Create README.md with:
1. Architecture diagram (text-based)
2. How to run the POC
3. Database setup instructions
4. Test scenarios
5. Key learnings
```

---

## Quick Test Commands

### Start All Services (separate terminals):
```bash
# Terminal 1
cd EnrollmentService
dotnet run --urls="https://localhost:7000"

# Terminal 2  
cd ActivityService
dotnet run --urls="https://localhost:7001"

# Terminal 3
cd FeeManagementService
dotnet run --urls="https://localhost:7002"

# Terminal 4
cd NotificationService
dotnet run --urls="https://localhost:7003"
```

### Test Enrollment:
```bash
curl -X POST https://localhost:7000/api/enrollment \
  -H "Content-Type: application/json" \
  -d '{"studentId": 1, "activityId": 1}'
```

---

## Key Concepts Demonstrated

✅ **Saga Pattern** - Orchestrated workflow with compensation
✅ **Database per Service** - Each service has own database
✅ **Distributed Transactions** - Multiple services, eventual consistency
✅ **Compensation** - Rollback mechanism for failed steps
✅ **Event-Driven** - Async processing with Outbox pattern
✅ **Resilience** - Error handling and retry logic

---

## Expected Outcome

After completing this POC, you'll have:
- 4 working microservices
- Saga pattern implementation
- Distributed transaction handling
- Compensation logic
- Event-driven architecture (simplified)
- Hands-on experience with microservices patterns

**Time Estimate: 2-3 hours**

---

## How to Use This Guide

1. **Start with Prompt 1** - Create the solution structure
2. **Follow prompts sequentially** - Each builds on the previous
3. **Test after each phase** - Verify services work independently
4. **Use Cursor AI** - Copy each prompt into Cursor AI chat
5. **Run services separately** - Use different terminals/ports
6. **Test scenarios** - Use Postman or curl to test APIs

---

## Troubleshooting Tips

### Service Communication Issues
- Verify service URLs in appsettings.json
- Check CORS configuration
- Ensure services are running on correct ports
- Verify SSL certificates (if using HTTPS)

### Database Issues
- Ensure LocalDB is running
- Check connection strings
- Run migrations: `dotnet ef database update`
- Verify database names are unique

### Compensation Not Working
- Check logs for compensation execution
- Verify compensation endpoints are accessible
- Ensure compensation methods are idempotent
- Check database state after compensation

---

## Next Steps After POC

1. **Add Circuit Breaker** - Implement resilience patterns
2. **Add Message Bus** - Replace HTTP calls with RabbitMQ/Azure Service Bus
3. **Add Monitoring** - Application Insights or Prometheus
4. **Add Tests** - Unit tests and integration tests
5. **Add API Gateway** - Ocelot or Azure API Management
6. **Add Authentication** - JWT tokens and service-to-service auth

---

## Learning Outcomes

After completing this POC, you'll understand:

- **Saga Pattern** - How to orchestrate distributed transactions
- **Compensation** - How to rollback distributed operations
- **Event-Driven** - How to handle async operations
- **Database per Service** - Data isolation in microservices
- **Service Communication** - HTTP-based service calls
- **Error Handling** - Handling failures in distributed systems

---

*"The best way to learn microservices patterns is by building them."*

