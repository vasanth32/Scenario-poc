# How to Verify Message Processing in InventoryProcessorFunction

## 📋 Expected Log Messages

When the function successfully processes a message, you should see these log entries in sequence:

### 1. Message Received
```
[Information] Received message from Service Bus queue. Message body: {"OrderId":"...","CustomerName":"...","TotalAmount":...}
```

### 2. Processing Started
```
[Information] Processing order {OrderId} for customer {CustomerName} with total amount {TotalAmount}. Decreasing inventory.
```

### 3. Inventory Update
```
[Information] Inventory updated for order {OrderId}. Reserved items for customer {CustomerName}.
```

### 4. Processing Complete
```
[Information] Successfully processed order {OrderId}. Inventory updated.
```

---

## 🔍 How to Check Logs

### Method 1: Console Output (When Running Locally)

When you run `dotnet run` in the `InventoryProcessorFunction` folder, logs appear directly in the console.

**Look for:**
- Log entries with `[Information]` level
- Messages containing "Processing order" followed by the `OrderId`
- Messages containing "Successfully processed order" with the same `OrderId`

**Example Console Output:**
```
info: InventoryProcessorFunction.Functions.InventoryProcessorFunction[0]
      Received message from Service Bus queue. Message body: {"OrderId":"123e4567-e89b-12d3-a456-426614174000","CustomerName":"John Doe","TotalAmount":99.99,"CreatedAt":"2026-02-18T06:00:00Z"}
info: InventoryProcessorFunction.Functions.InventoryProcessorFunction[0]
      Processing order 123e4567-e89b-12d3-a456-426614174000 for customer John Doe with total amount 99.99. Decreasing inventory.
info: InventoryProcessorFunction.Functions.InventoryProcessorFunction[0]
      Inventory updated for order 123e4567-e89b-12d3-a456-426614174000. Reserved items for customer John Doe.
info: InventoryProcessorFunction.Functions.InventoryProcessorFunction[0]
      Successfully processed order 123e4567-e89b-12d3-a456-426614174000. Inventory updated.
```

### Method 2: Search for Specific OrderId

If you have a specific `OrderId` from the OrderApi response, search for it in the console:

**In PowerShell/Command Prompt:**
- Use `Ctrl+F` to search in the console
- Search for the `OrderId` (e.g., `123e4567-e89b-12d3-a456-426614174000`)
- You should find at least 3-4 log entries mentioning that OrderId

### Method 3: Check Azure Portal (If Deployed)

If the function is deployed to Azure:
1. Go to Azure Portal → Your Function App
2. Navigate to **Log stream** or **Monitor**
3. Filter logs by time range
4. Search for your `OrderId`

---

## ✅ Verification Checklist

To confirm a message was processed for a specific `OrderId`, verify:

- [ ] **Message Received**: Log shows "Received message from Service Bus queue" with the message body containing your `OrderId`
- [ ] **Processing Started**: Log shows "Processing order {OrderId}" with your specific `OrderId`
- [ ] **Inventory Updated**: Log shows "Inventory updated for order {OrderId}"
- [ ] **Processing Complete**: Log shows "Successfully processed order {OrderId}. Inventory updated."
- [ ] **No Errors**: No `[Error]` or `[Warning]` messages related to that `OrderId`

---

## 🧪 Test Scenario

### Step 1: Get OrderId from OrderApi Response

Send a POST request to create an order:
```bash
POST http://localhost:5182/api/orders
Content-Type: application/json

{
  "customerName": "Test Customer",
  "totalAmount": 150.00
}
```

**Response:**
```json
{
  "orderId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "message": "Order created and published to Service Bus successfully."
}
```

### Step 2: Check Function Console

In the `InventoryProcessorFunction` console, search for `a1b2c3d4-e5f6-7890-abcd-ef1234567890`.

**Expected logs:**
```
[Information] Received message from Service Bus queue. Message body: {"OrderId":"a1b2c3d4-e5f6-7890-abcd-ef1234567890",...}
[Information] Processing order a1b2c3d4-e5f6-7890-abcd-ef1234567890 for customer Test Customer...
[Information] Inventory updated for order a1b2c3d4-e5f6-7890-abcd-ef1234567890...
[Information] Successfully processed order a1b2c3d4-e5f6-7890-abcd-ef1234567890. Inventory updated.
```

---

## ⚠️ Troubleshooting

### No Logs Appearing

**Possible causes:**
1. **Function not running**: Ensure `dotnet run` is active
2. **Message not in queue**: Check Azure Portal → Service Bus → `orders-queue` → Active message count
3. **Connection string issue**: Verify `local.settings.json` has correct `ServiceBusConnection`
4. **Queue name mismatch**: Ensure queue name is exactly `orders-queue`

### Logs Show Error

**If you see:**
```
[Error] Failed to deserialize message...
```
- **Cause**: Message format doesn't match `OrderCreatedEvent`
- **Solution**: Check message body format in OrderApi

**If you see:**
```
[Error] Error processing order message...
```
- **Cause**: Exception during processing
- **Solution**: Check the exception details in the log

### Message Received But Not Processed

**If you see:**
```
[Information] Received message from Service Bus queue...
```
But no "Processing order" log:
- **Cause**: Deserialization failed (orderEvent is null)
- **Solution**: Check message body format matches `OrderCreatedEvent`

---

## 📊 Log Levels

The function uses these log levels:

- **`[Information]`**: Normal processing flow (what you want to see)
- **`[Warning]`**: Non-critical issues (e.g., deserialization failure)
- **`[Error]`**: Critical errors (exceptions, processing failures)

---

## 🎯 Quick Verification Command

If you have the `OrderId`, you can quickly verify processing by:

1. **Copy the OrderId** from OrderApi response
2. **Search in Function Console** (Ctrl+F)
3. **Look for these patterns:**
   - `Processing order {YourOrderId}`
   - `Successfully processed order {YourOrderId}`

If both appear, the message was successfully processed! ✅

---

*Remember: Logs appear in real-time in the console when running `dotnet run` locally.*

