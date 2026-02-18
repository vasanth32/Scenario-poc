## Azure Service Bus + Azure Functions POC – Overview



### Service Bus Namespace, Queue, and Connection String

**What is a Service Bus namespace?**
A Service Bus namespace is a container in Azure that holds your messaging resources (queues and topics). Think of it as a dedicated messaging hub for your application. In this POC, you created a namespace named `sb-messaging-poc-<unique>`.

**What is a queue?**
A queue is like a mailbox that stores messages in order (first in, first out). Messages wait in the queue until something reads them. You created a queue called `orders-queue` where your `OrderApi` will send order messages and your `InventoryProcessorFunction` will receive them.

**Setup steps you followed:**
1. Created a resource group `rg-azure-messaging-poc` to organize your resources.
2. Created a Service Bus namespace with the Basic pricing tier.
3. Added a queue named `orders-queue` inside the namespace.
4. Retrieved the connection string: **Settings** → **Shared access policies** → **RootManageSharedAccessKey** → copied the **Primary connection string**.

**About the connection string:**
The connection string contains your namespace address and an authentication key. Your applications use it to connect to Service Bus. **Treat it like a password** – don't share it publicly, don't commit it to Git, and store it securely in local configuration files like `appsettings.Development.json` or `local.settings.json`.

---

## Connection String Management & Security Best Practices

### Where to Store Connection Strings for Local Development

**For .NET Web API Projects (like `OrderApi`):**
1. **`appsettings.Development.json`** (Recommended for POC)
   - Simple and straightforward
   - Already configured in this project
   - ⚠️ **Must be excluded from Git** (already in `.gitignore`)

2. **User Secrets** (Better for real projects)
   - More secure - stored outside your project folder
   - Use: `dotnet user-secrets set "ServiceBus:ConnectionString" "your-connection-string"`
   - Access via `IConfiguration` - no code changes needed
   - Automatically excluded from Git

**For Azure Functions (like `InventoryProcessorFunction`):**
1. **`local.settings.json`** (Standard for Functions)
   - Required for local Azure Functions development
   - Already configured in this project
   - ⚠️ **Must be excluded from Git** (already in `.gitignore`)

### What Should NEVER Be Committed to Git

**❌ Never Commit:**
- `appsettings.Development.json` (contains connection strings)
- `local.settings.json` (contains connection strings and secrets)
- Any file with actual connection strings, API keys, or passwords
- `.env` files (if using environment variables)

**✅ Safe to Commit:**
- `appsettings.json` (production template, no real secrets)
- `appsettings.Production.json` (if using placeholders only)
- `.gitignore` (ensures sensitive files are excluded)
- Code files (they reference configuration, don't contain secrets)

**How to Verify:**
- Check `.gitignore` includes: `**/appsettings.Development.json` and `local.settings.json`
- Before committing, run: `git status` to see what files are staged
- If you see sensitive files, they should show as "untracked" or not appear at all

### Practical Tips for Production & Real Projects

**1. Use Azure Key Vault for Production**
   - Store connection strings in Azure Key Vault
   - Reference them in App Settings or use `Azure.Identity` SDK
   - Benefits:
     - Centralized secret management
     - Automatic rotation support
     - Access control and auditing
     - No secrets in code or configuration files

**2. Rotate Connection Strings Regularly**
   - **When to rotate:**
     - If a connection string is exposed or compromised
     - Quarterly or bi-annually as a security practice
     - When team members leave the project
   
   - **How to rotate:**
     1. Create a new Shared Access Policy in Azure Portal
     2. Generate new Primary/Secondary keys
     3. Update connection strings in Azure App Settings (for deployed apps)
     4. Update local development files (user secrets or `local.settings.json`)
     5. Test the new connection string
     6. Delete old keys after verification (keep secondary as backup initially)

**3. Use Least-Privilege Access Policies**
   - **Don't use `RootManageSharedAccessKey` in production**
   - Create specific policies with minimal required permissions:
     - **Send-only policy** for publishers (like `OrderApi`)
     - **Listen-only policy** for consumers (like `InventoryProcessorFunction`)
   - Benefits:
     - Limits damage if one key is compromised
     - Easier to track which service uses which key
     - Can revoke access per service without affecting others

**Example: Creating a Send-Only Policy:**
1. Go to Service Bus namespace → **Shared access policies**
2. Click **+ Add** → Name: `orders-send-policy`
3. Select only **Send** permission
4. Copy the connection string and use it in `OrderApi`

**Example: Creating a Listen-Only Policy:**
1. Go to Service Bus namespace → **Shared access policies**
2. Click **+ Add** → Name: `orders-listen-policy`
3. Select only **Listen** permission
4. Copy the connection string and use it in `InventoryProcessorFunction`

### Quick Security Checklist

- [ ] Connection strings stored in local config files (not in code)
- [ ] `appsettings.Development.json` excluded from Git
- [ ] `local.settings.json` excluded from Git
- [ ] `.gitignore` properly configured
- [ ] Using separate policies for send/listen (production)
- [ ] Connection strings rotated periodically
- [ ] Using Azure Key Vault for production deployments
- [ ] Team members know not to share connection strings in chat/email
