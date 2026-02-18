# Installing Azure Functions Core Tools

For **Service Bus triggered** functions, you **MUST** use `func start` which requires Azure Functions Core Tools.

## 🚀 Quick Installation (Windows)

### Option 1: Using npm (Recommended)

```powershell
npm install -g azure-functions-core-tools@4 --unsafe-perm true
```

### Option 2: Using Chocolatey

```powershell
choco install azure-functions-core-tools-4
```

### Option 3: Using winget

```powershell
winget install Microsoft.AzureFunctionsCoreTools
```

### Option 4: Direct Download

1. Download from: https://github.com/Azure/azure-functions-core-tools/releases
2. Download the latest `Azure.Functions.Cli.win-x64.zip`
3. Extract to a folder (e.g., `C:\tools\func`)
4. Add to PATH environment variable

## ✅ Verify Installation

After installation, verify it works:

```powershell
func --version
```

You should see something like:
```
4.0.5455
```

## 🎯 Running the Function

Once installed, run the function:

```powershell
cd D:\PracticeProjects\Scenario-poc\AzureServiceBus_AzureFunctions_POC\InventoryProcessorFunction
func start
```

## 📝 Why `func start` is Required

- **Service Bus triggers** need the Azure Functions host runtime
- The isolated worker connects to the host via gRPC
- `dotnet run` alone cannot handle Service Bus triggers
- The Functions host manages Service Bus connections and message processing

## 🔧 Troubleshooting

### "func: command not found"
- Ensure Functions Core Tools is installed
- Check PATH environment variable includes the func executable
- Restart your terminal/PowerShell after installation

### "Unable to find project root"
- Ensure you're in the `InventoryProcessorFunction` folder
- Verify `host.json` and `local.settings.json` exist in that folder

### Port conflicts
- Default port is 7071
- Change port: `func start --port 7072`

---

*After installing Functions Core Tools, use `func start` to run your Service Bus triggered function.*

