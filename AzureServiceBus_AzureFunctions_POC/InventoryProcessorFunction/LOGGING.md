# File Logging Configuration

The `InventoryProcessorFunction` is configured to log all messages to a text file for easy tracking and debugging.

## 📁 Log File Location

Logs are written to:
```
InventoryProcessorFunction/logs/inventory-processor-YYYYMMDD.log
```

**Example:**
- `logs/inventory-processor-20260218.log` (for February 18, 2026)

## 📋 Log File Features

- **Daily Rolling**: A new log file is created each day
- **Retention**: Keeps the last 7 days of log files (older files are automatically deleted)
- **Format**: Includes timestamp, log level, and message
- **Console Output**: Logs also appear in the console (for real-time monitoring)

## 📝 Log Format

Each log entry includes:
```
{Timestamp} [{Level}] {Message}
```

**Example log entry:**
```
2026-02-18 06:15:30.123 +05:30 [INF] Received message from Service Bus queue. Message body: {"OrderId":"123e4567-e89b-12d3-a456-426614174000","CustomerName":"John Doe","TotalAmount":99.99,"CreatedAt":"2026-02-18T06:15:30Z"}
2026-02-18 06:15:30.125 +05:30 [INF] Processing order 123e4567-e89b-12d3-a456-426614174000 for customer John Doe with total amount 99.99. Decreasing inventory.
2026-02-18 06:15:30.230 +05:30 [INF] Inventory updated for order 123e4567-e89b-12d3-a456-426614174000. Reserved items for customer John Doe.
2026-02-18 06:15:30.231 +05:30 [INF] Successfully processed order 123e4567-e89b-12d3-a456-426614174000. Inventory updated.
```

## 🔍 How to Find Logs for a Specific OrderId

### Method 1: Search in Log File

1. Navigate to `InventoryProcessorFunction/logs/`
2. Open the log file for today (e.g., `inventory-processor-20260218.log`)
3. Use `Ctrl+F` to search for the `OrderId`
4. You'll see all log entries related to that order

### Method 2: PowerShell Search

```powershell
cd InventoryProcessorFunction/logs
Select-String -Path "inventory-processor-*.log" -Pattern "123e4567-e89b-12d3-a456-426614174000"
```

### Method 3: Command Prompt Search

```cmd
cd InventoryProcessorFunction\logs
findstr "123e4567-e89b-12d3-a456-426614174000" inventory-processor-*.log
```

## 📊 Log Levels

The function uses these log levels:

- **`[INF]` Information**: Normal processing flow (most common)
- **`[WRN]` Warning**: Non-critical issues (e.g., deserialization warnings)
- **`[ERR]` Error**: Critical errors (exceptions, processing failures)
- **`[FTL]` Fatal**: Application startup failures

## ✅ Verifying Message Processing

To verify a message was processed for a specific `OrderId`:

1. **Get the OrderId** from the OrderApi response
2. **Open today's log file** in `logs/` folder
3. **Search for the OrderId** (Ctrl+F)
4. **Look for these entries:**
   - `Received message from Service Bus queue` (contains OrderId)
   - `Processing order {OrderId}`
   - `Inventory updated for order {OrderId}`
   - `Successfully processed order {OrderId}`

If all four entries appear, the message was successfully processed! ✅

## 🗂️ Log File Management

- **Automatic Cleanup**: Files older than 7 days are automatically deleted
- **Manual Cleanup**: You can delete old log files manually if needed
- **File Size**: Log files grow as messages are processed. Typical size: 1-10 MB per day (depends on message volume)

## 🔧 Configuration

Logging is configured in `Program.cs` using Serilog:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()  // Also logs to console
    .WriteTo.File(
        path: "logs/inventory-processor-.log",
        rollingInterval: RollingInterval.Day,  // New file each day
        retainedFileCountLimit: 7,  // Keep 7 days
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
```

## 📌 Notes

- Log files are excluded from Git (see `.gitignore`)
- Logs are written in real-time as messages are processed
- Both console and file logging are active simultaneously
- Log files are created automatically when the function starts

---

*Log files provide a permanent record of all message processing for debugging and auditing purposes.*

