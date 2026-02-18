# Postman Collection Setup Guide

This guide explains how to import and use the Postman collection to test the OrderApi.

## 📦 Files Included

1. **`Postman_Collection.json`** - Main collection with all test requests
2. **`Postman_Environment.json`** - Environment variables for easy URL switching
3. **`POSTMAN_SETUP.md`** - This setup guide

## 🚀 Quick Start

### Step 1: Import the Collection

1. Open Postman
2. Click **Import** button (top left)
3. Select **`Postman_Collection.json`**
4. Click **Import**

### Step 2: Import the Environment (Optional but Recommended)

1. Click **Environments** in the left sidebar (or use the gear icon)
2. Click **Import**
3. Select **`Postman_Environment.json`**
4. Click **Import**
5. Select the imported environment from the dropdown (top right)

### Step 3: Start the OrderApi

Before running the requests, make sure the OrderApi is running:

```bash
cd OrderApi
dotnet run
```

The API should start on:
- **HTTP**: `http://localhost:5182`
- **HTTPS**: `https://localhost:7241`

### Step 4: Run the Tests

1. Open the **"Azure Service Bus POC - OrderApi"** collection
2. Select a request (start with **"Create Order - Success"**)
3. Click **Send**
4. Check the response and test results

## 📋 Available Requests

### ✅ Success Cases

1. **Create Order - Success**
   - Valid order with customer name and amount
   - Expected: `201 Created` with `OrderId` and success message
   - Use this to verify the complete flow works

2. **Create Order - Large Amount**
   - Tests with a large monetary value
   - Expected: `201 Created` (should handle large numbers)

### ❌ Validation Error Cases

3. **Create Order - Validation Error (Empty Customer Name)**
   - Empty customer name
   - Expected: `400 Bad Request` with error message

4. **Create Order - Validation Error (Negative Amount)**
   - Negative total amount
   - Expected: `400 Bad Request` with error message

5. **Create Order - Missing Customer Name**
   - Missing `customerName` field
   - Expected: `400 Bad Request`

## 🔧 Environment Variables

The collection uses the `{{baseUrl}}` variable. Default values:

- **HTTP**: `http://localhost:5182`
- **HTTPS**: `https://localhost:7241` (available as `baseUrlHttps`)

### To Change the Base URL:

1. Click on the environment dropdown (top right)
2. Click the eye icon to view/edit variables
3. Update the `baseUrl` value
4. Or edit the collection variable directly

## 🧪 Running All Tests

### Option 1: Run Collection (Postman UI)

1. Right-click on the collection name
2. Select **Run collection**
3. Review the test results in the **Collection Runner** window

### Option 2: Using Newman (CLI)

```bash
# Install Newman (if not already installed)
npm install -g newman

# Run the collection
newman run Postman_Collection.json -e Postman_Environment.json
```

## 📊 Test Assertions

Each request includes automated tests that verify:

- ✅ Correct HTTP status code
- ✅ Response structure (has `orderId`, `message`, etc.)
- ✅ Response time (should be under 2000ms)
- ✅ Error messages (for validation errors)

View test results in the **Test Results** tab after sending a request.

## 🔍 Verifying Service Bus Integration

After sending a successful request:

1. **Check the OrderApi logs** - Should show message published
2. **Check Azure Portal**:
   - Go to your Service Bus namespace
   - Navigate to **Queues** → **orders-queue**
   - Check **Active message count** (should increase)
3. **Check InventoryProcessorFunction logs** (if running):
   - Should show message received and processed
   - Message count should decrease after processing

## 🐛 Troubleshooting

### "Could not get response" Error

- **Check if OrderApi is running**: `dotnet run` in the OrderApi folder
- **Verify the port**: Check `launchSettings.json` for the correct port
- **Check firewall**: Ensure localhost connections are allowed

### "400 Bad Request" for Valid Data

- **Check Service Bus connection string**: Verify it's correct in `appsettings.Development.json`
- **Check queue name**: Should be `orders-queue`
- **Check Service Bus namespace**: Ensure it exists and is accessible

### "500 Internal Server Error"

- **Service Bus connection issue**: Verify connection string is valid
- **Queue doesn't exist**: Create `orders-queue` in Azure Portal
- **Check OrderApi logs**: Look for detailed error messages

### Tests Failing

- **Response format changed**: Update test assertions in Postman
- **Different status code**: Check API implementation
- **Timeout issues**: Increase timeout in Postman settings

## 📝 Example Request Body

```json
{
    "customerName": "John Doe",
    "totalAmount": 99.99
}
```

## 🎯 Next Steps

1. **Test all requests** to verify API behavior
2. **Run InventoryProcessorFunction** to see end-to-end flow
3. **Monitor Azure Portal** to see messages in the queue
4. **Check logs** in both applications

## 💡 Tips

- Use **Collection Variables** if you need to share data between requests
- Use **Pre-request Scripts** to generate dynamic test data
- Use **Tests** tab to add custom assertions
- Save example responses for documentation

---

*Happy Testing! 🚀*

