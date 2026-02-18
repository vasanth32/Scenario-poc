# How to Run InventoryProcessorFunction

## ✅ Required Method: `func start`

For **Service Bus triggered** isolated worker functions, you **MUST** use `func start` because Service Bus triggers require the Azure Functions host runtime:

```powershell
cd D:\PracticeProjects\Scenario-poc\AzureServiceBus_AzureFunctions_POC\InventoryProcessorFunction
func start
```

**Why `func start` is required:**
- Service Bus triggers need the Functions host to manage connections
- The isolated worker connects to the host via gRPC
- `dotnet run` alone won't work for Service Bus triggers

**Prerequisites:**
- Azure Functions Core Tools v4 installed
- Download: https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local#install-the-azure-functions-core-tools

**Expected output:**
```
Azure Functions Core Tools
Core Tools Version: 4.x.x
Function Runtime Version: 4.x.x

Functions:
    ProcessOrderCreated: ServiceBusTrigger

For detailed output, run func start --verbose
```

**Note:** If you get "Unable to find project root", ensure you're in the `InventoryProcessorFunction` folder that contains `host.json` and `local.settings.json`.

---

## Using Visual Studio / VS Code (F5)

### Visual Studio:
1. Open `AzureMessagingPoc.sln`
2. Right-click `InventoryProcessorFunction` → **Set as Startup Project**
3. Press **F5** or click **Start**

### VS Code:
1. Open the `InventoryProcessorFunction` folder
2. Press **F5**
3. Select **".NET Core"** if prompted

---

## Verifying It's Running

Look for these log messages:

```
[Information] Worker process started and initialized.
[Information] Worker indexing completed. 1 functions found.
```

When a message arrives from Service Bus:

```
[Information] Received message from Service Bus queue. Message body: {...}
[Information] Processing order {OrderId} for customer {CustomerName}...
[Information] Successfully processed order {OrderId}. Inventory updated.
```

---

## Testing the Complete Flow

1. **Start InventoryProcessorFunction** (in one terminal):
   ```powershell
   cd InventoryProcessorFunction
   dotnet run
   ```

2. **Start OrderApi** (in another terminal):
   ```powershell
   cd OrderApi
   dotnet run
   ```

3. **Send a test request** using Postman or curl:
   ```bash
   POST http://localhost:5182/api/orders
   Content-Type: application/json
   
   {
     "customerName": "John Doe",
     "totalAmount": 99.99
   }
   ```

4. **Watch both consoles** - you should see:
   - OrderApi: "Order created and published to Service Bus successfully"
   - InventoryProcessorFunction: Message received and processed

---

## Troubleshooting

### "Unable to find project root"
- **Solution**: Use `dotnet run` instead of `func start`
- Isolated worker functions don't require `func start`

### "Connection string not found"
- Check `local.settings.json` has `ServiceBusConnection` set
- Verify the connection string is correct

### Function not triggering
- Verify queue name: `orders-queue`
- Check Service Bus connection string
- Ensure queue exists in Azure Portal

---

*For .NET 8 isolated worker functions, `dotnet run` is the recommended approach!*

