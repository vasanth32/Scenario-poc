# Azure Service Bus + Azure Functions POC - Progress

## ✅ Completed: OrderApi Implementation

### Overview
Successfully implemented the `OrderApi` project with a POST endpoint that creates orders and publishes them to Azure Service Bus. The implementation follows clean architecture principles with proper separation of concerns.

---

## 📦 What Was Created

### 1. **Shared Message Contract** (`Shared.Contracts`)
- **`OrderCreatedEvent`** (`Messages/OrderCreatedEvent.cs`)
  - A record type representing the order event that flows through the system
  - Properties: `OrderId` (Guid), `CustomerName` (string), `TotalAmount` (decimal), `CreatedAt` (DateTime)
  - JSON-serializable using `System.Text.Json`
  - Shared between `OrderApi` (publisher) and `InventoryProcessorFunction` (consumer)

### 2. **Service Bus Configuration** (`OrderApi/Configuration`)
- **`ServiceBusOptions`** class
  - Configuration class bound to `appsettings.json` section `"ServiceBus"`
  - Properties: `ConnectionString`, `QueueName`
  - Used for dependency injection and configuration management

### 3. **Message Publishing Service** (`OrderApi/Services`)
- **`IOrderMessagePublisher`** interface
  - Defines contract: `Task PublishAsync(OrderCreatedEvent orderEvent, CancellationToken cancellationToken = default)`
  
- **`OrderMessagePublisher`** implementation
  - Uses `ServiceBusClient` to send messages to Azure Service Bus
  - Serializes `OrderCreatedEvent` to JSON
  - Creates `ServiceBusMessage` with proper metadata:
    - `MessageId`: OrderId as string
    - `ContentType`: "application/json"
    - `Subject`: "OrderCreated"
  - Sends message to the configured queue (`orders-queue`)

### 4. **API Models** (`OrderApi/Models`)
- **`CreateOrderRequest`** record
  - Request DTO with validation attributes:
    - `CustomerName`: Required, minimum length 1
    - `TotalAmount`: Required, must be greater than 0.01
  - Used for HTTP POST request body validation

- **`CreateOrderResponse`** record
  - Response DTO containing:
    - `OrderId`: The generated order ID
    - `Message`: Success message

### 5. **HTTP Endpoint** (`OrderApi/Program.cs`)
- **`POST /api/orders`** endpoint (minimal API)
  - Accepts `CreateOrderRequest` in request body
  - **Validation**:
    - Checks for empty/null customer name
    - Validates total amount is positive
    - Returns `400 Bad Request` for invalid input
  - **Processing**:
    - Generates new `Guid` for `OrderId`
    - Sets `CreatedAt` to `DateTime.UtcNow`
    - Creates `OrderCreatedEvent` from request
    - Publishes event to Service Bus via `IOrderMessagePublisher`
  - **Response**:
    - `201 Created` with `CreateOrderResponse` on success
    - `500 Internal Server Error` if Service Bus publish fails
  - **OpenAPI/Swagger**: Fully documented with `.WithOpenApi()`

### 6. **Dependency Injection Setup** (`OrderApi/Program.cs`)
- Registered `ServiceBusOptions` from configuration
- Registered `ServiceBusClient` as singleton (efficient connection reuse)
- Registered `IOrderMessagePublisher` as scoped service
- All services properly injected into the endpoint handler

### 7. **Configuration** (`OrderApi/appsettings.Development.json`)
- Added `ServiceBus` section with:
  - `ConnectionString`: Placeholder for Azure Service Bus connection string
  - `QueueName`: "orders-queue"
- **⚠️ Important**: Replace `<YOUR_SERVICE_BUS_CONNECTION_STRING_HERE>` with your actual connection string from Azure Portal

---

## 🔧 NuGet Packages Added

- **`Azure.Messaging.ServiceBus`** (v7.20.1)
  - Official Azure SDK for Service Bus messaging
  - Provides `ServiceBusClient`, `ServiceBusSender`, `ServiceBusMessage`

---

## 🏗️ Architecture Highlights

### Separation of Concerns
- **Configuration**: Isolated in `ServiceBusOptions` class
- **Business Logic**: Encapsulated in `OrderMessagePublisher` service
- **API Layer**: Minimal API endpoint handles HTTP concerns only
- **Shared Contracts**: Message DTOs in separate project for reuse

### Dependency Injection
- All dependencies injected via constructor
- Service lifetime management:
  - `ServiceBusClient`: Singleton (expensive to create, thread-safe)
  - `OrderMessagePublisher`: Scoped (per HTTP request)
  - Configuration: Bound from `IConfiguration`

### Error Handling
- Input validation with clear error messages
- Try-catch around Service Bus operations
- Proper HTTP status codes (400, 201, 500)

---

## ✅ Build Status

- **Solution builds successfully** with no errors or warnings
- All projects compile correctly
- Project references properly configured

---

## 🚀 Next Steps

1. **Add your Service Bus connection string** to `appsettings.Development.json`
2. **Test the endpoint**:
   - Run `OrderApi` project
   - Use Swagger UI at `/swagger` or send POST request to `/api/orders`
   - Example request body:
     ```json
     {
       "customerName": "John Doe",
       "totalAmount": 99.99
     }
     ```
3. **Verify messages in Azure Portal**:
   - Go to Service Bus namespace → `orders-queue`
   - Check "Active message count" to see messages arriving
4. **Implement `InventoryProcessorFunction`** to consume messages from the queue

---

## 📝 Code Quality

- ✅ Clean, readable code with XML documentation comments
- ✅ Proper error handling and validation
- ✅ Follows .NET 8 best practices
- ✅ Uses modern C# features (records, minimal APIs, nullable reference types)
- ✅ No linter errors or warnings

---

## 🎯 Key Learnings

1. **Service Bus Client Management**: Using singleton for `ServiceBusClient` is efficient because it manages connection pooling internally.

2. **Message Serialization**: JSON serialization is straightforward with `System.Text.Json`, and the message body is sent as UTF-8 bytes.

3. **Minimal APIs**: Clean and concise way to define endpoints in .NET 8, with built-in OpenAPI support.

4. **Configuration Pattern**: Using strongly-typed options classes (`ServiceBusOptions`) makes configuration management cleaner and type-safe.

5. **Shared Contracts**: Having message DTOs in a separate project ensures both publisher and consumer agree on the same data structure, preventing breaking changes.

---

## ✅ Completed: InventoryProcessorFunction Implementation

### Overview
Successfully implemented the `InventoryProcessorFunction` Azure Functions project that automatically processes order messages from the Service Bus queue. The function uses the isolated worker model (.NET 8) and is triggered whenever a new message arrives in the `orders-queue`.

---

## 📦 What Was Created

### 1. **Service Bus Trigger Function** (`InventoryProcessorFunction/Functions`)
- **`InventoryProcessorFunction`** class
  - Contains `ProcessOrderCreated` method with `[Function]` attribute
  - Triggered by Service Bus queue: `orders-queue`
  - Connection string bound from `local.settings.json` via `ServiceBusConnection` setting
  - **Message Processing**:
    - Receives message body as `string`
    - Deserializes JSON to `OrderCreatedEvent` using `System.Text.Json`
    - Logs order processing details
    - Simulates inventory update operation
  - **Error Handling**:
    - Catches `JsonException` for deserialization errors
    - Catches general exceptions for processing errors
    - Re-throws exceptions to trigger retry/dead-letter queue behavior
  - **Logging**: Comprehensive logging at each step (Information, Warning, Error levels)

### 2. **Configuration** (`InventoryProcessorFunction/local.settings.json`)
- Added `ServiceBusConnection` setting
  - Placeholder: `<YOUR_SERVICE_BUS_CONNECTION_STRING_HERE>`
  - **⚠️ Important**: Replace with your actual connection string from Azure Portal
- Existing settings maintained:
  - `AzureWebJobsStorage`: For local development storage
  - `FUNCTIONS_WORKER_RUNTIME`: `dotnet-isolated`

### 3. **Program.cs** (Already Configured)
- Uses `FunctionsApplication.CreateBuilder` for isolated worker model
- Configured with Application Insights telemetry
- No changes needed - function discovery is automatic

---

## 🔧 NuGet Packages Added

- **`Microsoft.Azure.Functions.Worker.Extensions.ServiceBus`** (v5.24.0)
  - Provides `[ServiceBusTrigger]` attribute for queue binding
  - Handles connection to Azure Service Bus
  - Automatically manages message receiving and acknowledgment

**Dependencies automatically installed:**
- `Microsoft.Azure.Functions.Worker.Extensions.Rpc`
- `Azure.Identity`
- `Microsoft.Extensions.Azure`
- `Grpc.Net.Client` and related gRPC packages

---

## 🏗️ Function Architecture

### Service Bus Trigger Binding
```csharp
[ServiceBusTrigger("orders-queue", Connection = "ServiceBusConnection")]
```
- **Queue Name**: `orders-queue` (must match the queue created in Azure)
- **Connection**: References `ServiceBusConnection` from `local.settings.json`
- **Message Type**: `string` (message body as JSON string)

### Message Flow
1. **Message Arrives**: Service Bus queue receives message from `OrderApi`
2. **Function Triggered**: Azure Functions runtime automatically invokes `ProcessOrderCreated`
3. **Deserialization**: JSON string converted to `OrderCreatedEvent` object
4. **Processing**: Simulated inventory update operation
5. **Completion**: Function completes, message is automatically acknowledged and removed from queue

### Error Handling Strategy
- **Deserialization Errors**: Logged and re-thrown → message goes to dead-letter queue
- **Processing Errors**: Logged and re-thrown → triggers retry policy (if configured)
- **Successful Processing**: Message automatically acknowledged and removed

---

## ✅ Build Status

- **Solution builds successfully** with no errors or warnings
- All projects compile correctly
- Function project properly configured as isolated worker
- Service Bus extension package integrated

---

## 🚀 Testing the Complete Flow

### Prerequisites
1. **Add connection strings** to both projects:
   - `OrderApi/appsettings.Development.json`: `ServiceBus.ConnectionString`
   - `InventoryProcessorFunction/local.settings.json`: `ServiceBusConnection`

### Test Steps

1. **Start the Function App**:
   ```bash
   cd InventoryProcessorFunction
   func start
   ```
   - Function will listen for messages on `orders-queue`

2. **Start the OrderApi**:
   ```bash
   cd OrderApi
   dotnet run
   ```

3. **Send a test order**:
   - Use Swagger UI at `https://localhost:7xxx/swagger`
   - Or use HTTP client:
     ```http
     POST https://localhost:7xxx/api/orders
     Content-Type: application/json
     
     {
       "customerName": "John Doe",
       "totalAmount": 99.99
     }
     ```

4. **Observe the flow**:
   - OrderApi receives request → publishes to Service Bus
   - Function automatically triggered → processes message
   - Check logs in both applications
   - Verify message count in Azure Portal (should decrease as messages are processed)

---

## 📝 Code Quality

- ✅ Clean, readable code with XML documentation comments
- ✅ Proper error handling with try-catch blocks
- ✅ Comprehensive logging at all levels
- ✅ Follows Azure Functions isolated worker best practices
- ✅ Uses dependency injection for logger
- ✅ No linter errors or warnings

---

## 🎯 Key Learnings

1. **Service Bus Trigger Binding**: The `[ServiceBusTrigger]` attribute automatically handles:
   - Message receiving from the queue
   - Connection management
   - Message acknowledgment after successful processing
   - Dead-letter queue routing on failures

2. **Isolated Worker Model**: .NET 8 isolated worker provides:
   - Full control over the hosting process
   - Better performance and compatibility
   - Support for dependency injection
   - Modern .NET features

3. **Message Deserialization**: Using `System.Text.Json` for deserialization:
   - Same serializer used by `OrderApi` ensures compatibility
   - Error handling important for malformed messages
   - Type safety with `OrderCreatedEvent` from shared contracts

4. **Automatic Scaling**: Azure Functions automatically scales:
   - Multiple instances can process messages concurrently
   - Each instance processes messages independently
   - No manual scaling configuration needed

5. **Connection String Management**: 
   - `local.settings.json` for local development
   - In Azure, use App Settings or Key Vault references
   - Never commit connection strings to source control

---

## 🔧 How Service Bus Triggers Work: Architecture Deep Dive

### Why Service Bus Triggers Need the Functions Host

**The Problem with `dotnet run` Alone:**
When you run `dotnet run` directly, you're starting a standalone .NET application. However, Service Bus triggers require:
- **Persistent connection management** to Azure Service Bus
- **Message polling and receiving** from the queue
- **Connection retry logic** and error handling
- **Message acknowledgment** after processing
- **Dead-letter queue** handling for failed messages

These capabilities are provided by the **Azure Functions Host Runtime**, not by your isolated worker code alone.

### The Two-Process Architecture

When you run `func start`, two processes work together:

```
┌─────────────────────────────────────────────────────────────┐
│                    Azure Functions Host                     │
│  (Started by: func start)                                   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Service Bus Connection Manager                      │  │
│  │  - Maintains connection to Azure Service Bus        │  │
│  │  - Polls queue for new messages                      │  │
│  │  - Handles connection retries                        │  │
│  │  - Manages message locks and timeouts                │  │
│  └──────────────────────────────────────────────────────┘  │
│                          │                                   │
│                          │ gRPC Communication               │
│                          │ (gRPC = Google Remote Procedure  │
│                          │  Call - fast binary protocol)    │
│                          ▼                                   │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  gRPC Server                                          │  │
│  │  - Listens for worker connections                     │  │
│  │  - Sends trigger invocations to worker               │  │
│  │  - Receives execution results                        │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                          │
                          │ gRPC Channel
                          │ (localhost:7071)
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│              Isolated Worker Process                         │
│  (Your .NET code - InventoryProcessorFunction)              │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  gRPC Client                                          │  │
│  │  - Connects to Functions Host                        │  │
│  │  - Receives trigger invocations                      │  │
│  │  - Sends execution results back                      │  │
│  └──────────────────────────────────────────────────────┘  │
│                          │                                   │
│                          ▼                                   │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Your Function Code                                   │  │
│  │  ProcessOrderCreated()                                │  │
│  │  - Deserializes message                               │  │
│  │  - Processes order                                    │  │
│  │  - Updates inventory                                  │  │
│  │  - Returns success/failure                            │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Step-by-Step: What Happens in the Background

**1. Starting the Functions Host (`func start`):**
```
func start
  ↓
Functions Host starts on port 7071
  ↓
Host reads host.json and local.settings.json
  ↓
Host discovers your function (ProcessOrderCreated)
  ↓
Host establishes gRPC server endpoint
```

**2. Starting the Isolated Worker:**
```
Your .NET code starts (via dotnet run or func start)
  ↓
Worker connects to Functions Host via gRPC
  ↓
Worker registers itself: "I'm ready to process triggers"
  ↓
Host acknowledges: "I'll send you Service Bus messages"
```

**3. Service Bus Connection (Managed by Host):**
```
Functions Host connects to Azure Service Bus
  ↓
Host authenticates using ServiceBusConnection from local.settings.json
  ↓
Host subscribes to 'orders-queue'
  ↓
Host starts polling for messages (continuous loop)
```

**4. Message Arrives in Queue:**
```
OrderApi publishes message to Service Bus
  ↓
Message arrives in 'orders-queue'
  ↓
Functions Host detects new message
  ↓
Host locks the message (prevents other consumers from taking it)
  ↓
Host creates trigger invocation via gRPC
```

**5. Function Execution (Your Code):**
```
Host sends gRPC message to Worker:
  "Execute ProcessOrderCreated with this message body"
  ↓
Worker receives trigger invocation
  ↓
Worker deserializes message body
  ↓
Worker calls your ProcessOrderCreated() method
  ↓
Your code processes the order
  ↓
Worker returns result to Host via gRPC:
  "Execution succeeded" or "Execution failed"
```

**6. Message Acknowledgment:**
```
If execution succeeded:
  Host acknowledges message to Service Bus
  ↓
Service Bus removes message from queue
  ↓
Message processing complete ✅

If execution failed:
  Host releases message lock
  ↓
Message becomes available again (retry)
  ↓
After max retries, message goes to dead-letter queue
```

### Why gRPC Communication?

**gRPC (Google Remote Procedure Call)** is used because:

1. **Performance**: Binary protocol is faster than JSON/HTTP
2. **Efficiency**: Lower overhead for frequent trigger invocations
3. **Bidirectional**: Host can send triggers, worker can send status
4. **Streaming**: Supports real-time communication
5. **Type Safety**: Strongly-typed contracts between host and worker

**The gRPC Channel:**
- **Local Development**: `http://127.0.0.1:7071` (localhost)
- **In Azure**: Managed automatically by the platform
- **Protocol**: HTTP/2 with Protocol Buffers

### Why `dotnet run` Alone Doesn't Work

When you run `dotnet run` directly:

```
Your .NET Application Starts
  ↓
Tries to use ConfigureFunctionsWorkerDefaults()
  ↓
Looks for Functions Host endpoint
  ↓
❌ ERROR: "Configuration is missing the 'HostEndpoint' information"
```

**The Missing Piece:**
- `ConfigureFunctionsWorkerDefaults()` expects a Functions Host to be running
- It tries to connect via gRPC to `Functions:Worker:HostEndpoint`
- Without the host, there's no Service Bus connection manager
- Without the host, there's no gRPC server to connect to

### The Complete Flow: End-to-End

```
1. Developer runs: func start
   ↓
2. Functions Host starts (port 7071)
   ↓
3. Host launches isolated worker process
   ↓
4. Worker connects to host via gRPC
   ↓
5. Host connects to Azure Service Bus
   ↓
6. Host polls queue for messages
   ↓
7. OrderApi publishes message → Service Bus queue
   ↓
8. Host detects message, locks it
   ↓
9. Host sends trigger invocation to worker via gRPC
   ↓
10. Worker executes ProcessOrderCreated()
    ↓
11. Your code processes the order
    ↓
12. Worker returns result to host via gRPC
    ↓
13. Host acknowledges message to Service Bus
    ↓
14. Message removed from queue ✅
```

### Key Takeaways

1. **Functions Host is Required**: Service Bus triggers need the host runtime for connection management
2. **Two-Process Model**: Host manages connections, worker executes your code
3. **gRPC Communication**: Fast, efficient communication between host and worker
4. **Separation of Concerns**: 
   - Host = Infrastructure (connections, retries, error handling)
   - Worker = Business Logic (your code)
5. **Why `func start`**: It starts both the host AND the worker together

### Benefits of This Architecture

- **Reliability**: Host handles connection failures and retries automatically
- **Scalability**: Multiple worker instances can connect to one host
- **Isolation**: Your code runs in a separate process (isolated worker)
- **Flexibility**: Can update worker code without restarting host
- **Monitoring**: Host provides execution metrics and logging

---

*Last Updated: Implementation completed for both OrderApi and InventoryProcessorFunction*

