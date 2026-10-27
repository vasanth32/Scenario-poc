# Azure + .NET Hands-On Guide (Copilot for .NET, Portal for Azure)

Companion to [Azure_DotNet_Microservice_POC_Copilot_Prompts.md](./Azure_DotNet_Microservice_POC_Copilot_Prompts.md). Use that file for local .NET work. Use this file when a POC needs a real Azure resource: write the .NET code with a Copilot prompt, create the resource manually in the Azure Portal, test, then **delete it immediately**.

## Golden Rules (Cost Control)

1. One resource group per POC (`rg-poc06-keyvault`). Deleting the resource group deletes everything inside it.
2. Delete the resource group **right after testing**. Do not leave it overnight.
3. Create a **Budget** first (steps below) with email alerts.
4. Prefer Free/Basic/Consumption SKUs. Never pick Premium, Business Critical, or AKS node pools for learning.
5. Use one region close to you for all resources (e.g. `Central India`, `East US`).
6. After deletion, check **Cost Management + Billing > Cost analysis** next day to confirm 0 spend.
7. Never commit secrets. Use user-secrets or environment variables for connection strings.

## One-Time Setup

### A. Free account

1. Go to portal.azure.com and sign in (new accounts get free credits and 12 months of free services; verify current offer on the sign-up page).
2. Open **Subscriptions** and confirm the subscription is Active.

### B. Budget and alert

1. Portal > **Cost Management + Billing** > **Budgets** > **Add**.
2. Scope: your subscription. Name: `learning-budget`. Reset period: Monthly. Amount: e.g. 5 (your currency).
3. Alert conditions: 50%, 80%, 100% actual. Add your email. **Create**.

### C. Tools

1. Install Azure CLI (optional, used only for cleanup) and sign in: `az login`.
2. In VS Code, install the Azure Resources extension (optional, for viewing).

### D. Standard cleanup (use after every POC)

Portal: **Resource groups** > select the group > **Delete resource group** > type its name > **Delete**.

CLI alternative:

```
az group delete --name rg-poc06-keyvault --yes --no-wait
```

Verify: **Resource groups** list no longer shows it. Key Vault note: deleted vaults are soft-deleted; purge to reuse the name (see POC 6).

---

## POC-by-POC: Where Azure Is Needed

POCs 1-5 and 7-9, 11-13, 15-17 are local-first. Azure is optional; use it only where listed below.

| POC                            | Azure resource                                               | Cost risk   |
| ------------------------------ | ------------------------------------------------------------ | ----------- |
| 6 Key Vault + Managed Identity | Key Vault (+ App Service/Container App for managed identity) | Low         |
| 7 Messaging                    | Service Bus (Basic/Standard)                                 | Low (Basic) |
| 3 / 17 (optional)              | Azure SQL Database (free offer / serverless)                 | Medium      |
| 10 (optional)                  | API Management (Consumption tier)                            | Low-Medium  |
| 11 Monitoring                  | Application Insights + Log Analytics                         | Low         |
| 12/13                          | Container Registry (Basic)                                   | Low         |
| 14 Deployment                  | Container Apps + above                                       | Medium      |
| 13                             | GitHub Container Registry is free; no Azure needed           | None        |

Suggested Azure order: **6 -> 7 -> 11 -> 12 (ACR) -> 14**.

**Progress:** POCs 1-5 are done locally, POC 6 Key Vault configuration is complete, Azure POC 7 (Service Bus) and POC 11 (Application Insights) are done. **Continue with Azure POC 12 (Container Registry) below.**

---

## Azure POC 6: Key Vault and Managed Identity

### Copilot prompt (code)

```
In Claims.Api, add optional Azure Key Vault configuration using Azure.Extensions.AspNetCore.Configuration.Secrets and DefaultAzureCredential, enabled only when KeyVault:Uri exists. Load the secret "Jwt--SigningKey" so it maps to Jwt:SigningKey. Keep local behavior unchanged when KeyVault:Uri is missing. Target net10.0.
```

### Portal steps

1. **Resource group**: Create > name `rg-poc06-keyvault`, region of choice.
2. **Key Vault**: Create a resource > Key Vault > Create.
   - Resource group: `rg-poc06-keyvault`; name: globally unique (e.g. `kv-poc06-<initials><digits>`); pricing tier: **Standard**.
   - Access configuration tab: **Azure role-based access control (RBAC)**.
   - Networking: Public access (fine for learning). **Review + create**.
3. **Give yourself access**: Key Vault > **Access control (IAM)** > Add role assignment > **Key Vault Secrets Officer** > Members: your user > Assign.
4. **Add secret**: Key Vault > **Secrets** > Generate/Import > name `Jwt--SigningKey`, value: a random 32+ char string > Create.

5. **Run locally**: sign in via `az login` (or VS Code Azure sign-in). Set `KeyVault__Uri` = vault URI (Overview page > Vault URI). Run Claims.Api; DefaultAzureCredential uses your login.
6. **Break it** (learn errors): remove your role assignment, rerun, observe 403; wait a few minutes for RBAC propagation after re-adding.
7. **Managed identity (optional)**: deploy API to an App Service (F1/B1) or Container App > Identity > System assigned > On. In Key Vault IAM, assign **Key Vault Secrets User** to that identity. Set app setting `KeyVault__Uri`. Run without any stored credential.

### Test the functionality

1. Set `KeyVault__Uri` to the vault URI and start Claims.Api.
2. Call `POST /api/auth/token` in Swagger and request a token for the `Customer` role.
3. Click **Authorize**, enter `Bearer <token>`, then call `GET /api/auth/me`.
4. Expected result: `200 OK`. This confirms the application loaded `Jwt:SigningKey` from Key Vault.
5. Stop the API, remove your Key Vault secret-read role, and start it again. Expected result: startup fails with a `403 Forbidden` Key Vault error.
6. Restore the role before cleanup.

### Cleanup

1. Delete resource group `rg-poc06-keyvault`.
2. Portal > **Key Vaults** > **Manage deleted vaults** > select vault > **Purge** (needed if you want to reuse the name).

---

## Azure POC 7: Service Bus

### Copilot prompt (code)

```
Step 1 of 3: Create the event abstraction and add Azure Service Bus publishing.

First inspect the existing solution. Claims and policies must continue to be stored in the existing SQL Server databases; do not replace or change that persistence.

If IEventPublisher, ClaimSubmitted, and an in-memory publisher do not already exist, create the smallest versions needed for this POC. The in-memory publisher is only for local event delivery and must not replace SQL persistence. Keep it available as the local fallback.

Add the latest Azure.Messaging.ServiceBus package compatible with net10.0. Implement AzureServiceBusEventPublisher : IEventPublisher using a singleton ServiceBusClient and a sender for the queue "claim-submitted". Serialize ClaimSubmitted as JSON and assign a stable MessageId from the event. Register the publisher abstractions with dependency injection without adding the consumer yet. Preserve the in-memory publisher when Azure Service Bus configuration is missing. Do not add Azure Service Bus connection strings or hardcoded secrets. Target net10.0.
```

After accepting Step 1, verify that the project builds and that the existing in-memory flow still works.

```text
Step 2 of 3: Add the Service Bus consumer.

Create a BackgroundService that uses ServiceBusProcessor for the "claim-submitted" queue. Configure peek-lock processing, deserialize ClaimSubmitted messages, log successful processing, manually complete successful messages, and abandon failed messages so they can be retried. Respect the queue's maximum delivery count and log when a poison message is moved to the dead-letter subqueue. Use cancellation tokens, dispose the processor correctly, and do not create a ServiceBusClient per message. Target net10.0.
```

After accepting Step 2, verify that the project builds and review the success, failure, abandon, and shutdown paths.

```text
Step 3 of 3: Add configuration, authentication, fallback, and tests.

Read the Service Bus namespace from configuration key ServiceBus:FullyQualifiedNamespace. Authenticate with DefaultAzureCredential. When the setting is missing or empty, use the existing in-memory publisher and do not start the Azure Service Bus consumer. When it is present, register one singleton ServiceBusClient, the Azure publisher, and the consumer. Add focused xUnit tests using fake handlers or test doubles for publisher serialization, stable MessageId, successful completion, failed-message abandon, cancellation, and fallback to the in-memory implementation. Do not use connection strings or hardcoded secrets. Target net10.0.
```

### Portal steps

1. Resource group `rg-poc07-servicebus`.
2. Create **Service Bus Namespace**: pricing tier **Basic** (queues only) or **Standard** (needed for topics; has a monthly base charge, so keep it short-lived). Name must be globally unique.
3. Namespace > **Queues** > **+ Queue**: name `claim-submitted`, Max delivery count `5`, leave other defaults.
4. IAM: assign yourself **Azure Service Bus Data Owner** on the namespace.
5. Set `ServiceBus__FullyQualifiedNamespace` = `<name>.servicebus.windows.net`; run the API and Payments consumer.
6. Observe: queue **Overview** shows active/dead-letter message counts. Use **Service Bus Explorer** (queue > Service Bus Explorer) to peek messages and view the **Dead-letter** subqueue after forced failures.
7. Topics (Standard only): create topic `claim-events` and subscriptions `payments-sub`, `audit-sub` to see fan-out.

### Test the functionality

1. Set `ServiceBus__FullyQualifiedNamespace` to the namespace host name and start Claims.Api.
2. Submit a claim through Swagger using a valid `Customer` token.
3. Open the queue's **Service Bus Explorer** and select **Peek**.
4. Expected result: a JSON `ClaimSubmitted` message appears in `claim-submitted` and is completed by the consumer.
5. Temporarily make message processing fail, submit another message, and observe retries.
6. After the configured delivery limit, verify the message appears in the queue's **Dead-letter** subqueue.
7. Remove `ServiceBus__FullyQualifiedNamespace`, restart Claims.Api, and verify the local in-memory fallback starts without an Azure consumer.

### Cleanup

Delete resource group `rg-poc07-servicebus`.

---

## Azure POC 11: Application Insights

### Copilot prompt (code)

```
Add Azure Monitor OpenTelemetry (Azure.Monitor.OpenTelemetry.AspNetCore) to Claims.Api and Policy.Api, enabled only when APPLICATIONINSIGHTS_CONNECTION_STRING is set. Keep the console exporter otherwise. Ensure W3C trace context propagates across the HttpClient calls. Target net10.0.
```

### Portal steps

1. Resource group `rg-poc11-monitor`.
2. Create **Log Analytics workspace** (Pay-as-you-go; set daily cap in **Usage and estimated costs** > Daily cap, e.g. 0.1 GB).
3. Create **Application Insights** (workspace-based) linked to that workspace.
4. Overview > copy **Connection String**; store via user-secrets as `APPLICATIONINSIGHTS_CONNECTION_STRING`.
5. Run both APIs, call POST /api/claims a few times.
6. Explore: **Transaction search**, **Application map**, **Live metrics**, **Logs** (KQL, e.g. `requests | summarize count() by resultCode`, `dependencies | where success == false`).
7. Data takes 2-5 minutes to appear.

### Test the functionality

1. Generate traffic by calling the Claims and Policy endpoints several times.
2. Open Application Insights > **Transaction search** and filter for recent requests.
3. Open **Application map** and confirm the Claims-to-Policy dependency is visible.
4. Run `requests | summarize count() by resultCode` in **Logs**.
5. Expected result: requests and dependency telemetry appear with success or failure status codes.

### Cleanup

Delete resource group `rg-poc11-monitor` (removes workspace and App Insights).

---

## Azure POC 12: Container Registry

### What this POC teaches

An **image** is a packaged version of an application. A **Dockerfile** is the recipe used to build that image. Azure Container Registry (ACR) is a private Azure storage location for Docker images.

The flow is:

```text
.NET project -> Dockerfile -> Docker image -> Azure Container Registry
```

ACR does not create the Dockerfile and does not build the application code. We create the Dockerfiles first, build the images locally, and then push the images to ACR.

### Step 1: Create Dockerfiles with Copilot

#### Copilot prompt (code)

```
Create Dockerfiles only; do not modify application source code, project files, or configuration.

Create these files:
- InsuranceClaims/Claims.Api/Dockerfile
- InsuranceClaims/Policy.Api/Dockerfile

Use multi-stage Docker builds targeting net10.0. Use `mcr.microsoft.com/dotnet/sdk:10.0` for the build and publish stages and `mcr.microsoft.com/dotnet/aspnet:10.0` for the runtime stages. Build from the repository root so each Dockerfile can copy its project files correctly. Restore, build, and publish the correct API project. Run the final container as a non-root user where supported, expose port 8080, and set ASPNETCORE_URLS to http://+:8080. Keep the images small and do not put secrets in them.
```

After accepting the prompt, verify that both Dockerfiles exist and review the generated `COPY`, `dotnet restore`, `dotnet publish`, port, and user settings.

### Step 2: Create the Azure Container Registry manually

1. Portal > **Resource groups** > **Create** > name `rg-poc12-acr`.
2. Create **Container Registry**.
3. Registry name: globally unique, lowercase letters and numbers only, for example `acrpoc12vasanth123`.
4. Pricing plan: **Basic**.
5. Disable **Admin user**.
6. Select **Review + create** > **Create**.
7. Open the registry > **Access control (IAM)** > add the **AcrPush** role to your Azure user.

The registry stores images. It does not store your source code or application secrets.

### Step 3: Build, tag, and push the images

Open PowerShell at the repository root:

```powershell
cd "D:\PracticeProjects\Scenario-poc\Azure + .Net Microservice Poc"

$acrName = "<your-acr-name>"
$loginServer = az acr show --name $acrName --query loginServer --output tsv

az acr login --name $acrName
```

> **>>> CONTINUE FROM HERE <<<**

Build the local Docker images:

```powershell
docker build --file ".\InsuranceClaims\Claims.Api\Dockerfile" --tag "claims-api:local" .
docker build --file ".\InsuranceClaims\Policy.Api\Dockerfile" --tag "policy-api:local" .
```

Tag the images with the ACR address:

```powershell
docker tag claims-api:local "$loginServer/claims-api:latest"
docker tag policy-api:local "$loginServer/policy-api:latest"
```

Push the images to ACR:

```powershell
docker push "$loginServer/claims-api:latest"
docker push "$loginServer/policy-api:latest"
```

Useful checks:

```powershell
az acr repository list --name $acrName --output table
az acr repository show-tags --name $acrName --repository claims-api --output table
az acr repository show-tags --name $acrName --repository policy-api --output table
```

### Test the functionality

1. Portal > ACR > **Repositories**.
2. Confirm `claims-api` and `policy-api` repositories exist.
3. Confirm each repository contains the `latest` tag and an image digest.
4. Pull the images locally:
   ```powershell
   docker pull "$loginServer/claims-api:latest"
   docker pull "$loginServer/policy-api:latest"
   ```
5. Expected result: both images download successfully using your Azure identity.
6. Do not expect the APIs to run correctly in a container yet if they still require local SQL Server or local configuration. Container runtime configuration is covered in POC 14.

### Cleanup

Delete resource group `rg-poc12-acr`.

---

## Azure POC 14: Container Apps Deployment (Bicep reviewed, deployed manually)

### Copilot prompt (code)

```
Create a deployment architecture doc with a Mermaid diagram and Bicep templates for Azure Container Apps, Service Bus, Key Vault, and Application Insights. Do not deploy. Use user-assigned managed identity and RBAC role assignments; keep all SKUs at the lowest cost tier.
```

### Portal steps (manual deployment)

1. Resource group `rg-poc14-aca`.
2. Create ACR (as in POC 12) and push images.
3. Create **Container Apps Environment** (Consumption plan only; connect to a new Log Analytics workspace).
4. Create **Container App** `claims-api`:
   - Image source: Azure Container Registry; select image and tag.
   - Ingress: Enabled, accepting traffic from anywhere, target port 8080.
   - Scale: min replicas `0`, max `1` (scales to zero to save cost).
5. Repeat for `policy-api`; set `PolicyApi__BaseUrl` on claims-api to the policy app's internal FQDN (ingress: **limited to Container Apps environment** for policy).
6. Container App > **Identity** > System assigned > On; grant roles (Key Vault Secrets User, Service Bus Data Sender/Receiver, AcrPull).
7. Container App > **Revisions and replicas**: create a new revision, split traffic 90/10 to practice revision-based deployment; roll back by shifting traffic to 100% old revision.
8. Check **Log stream** and **Logs** for diagnostics.

### Test the functionality

1. Open the public Claims.Api Container App URL in a browser and append `/swagger`.
2. Call the health or Swagger endpoint and confirm an HTTP success response.
3. Submit a claim through the public API and verify the response in the Claims logs.
4. Stop or break the Policy.Api revision and submit another claim.
5. Expected result: Claims.Api reports a dependency failure rather than silently accepting the claim.
6. Create a new revision, send 10 requests, and confirm traffic follows the configured revision split.
7. Shift traffic to the previous revision and confirm rollback works.

### Cleanup

Delete resource group `rg-poc14-aca` (removes environment, apps, ACR, workspace).

---

## Optional: Azure SQL Database

### Copilot prompt (code)

```
Add a SQL Server connection string option for Azure SQL using Microsoft Entra authentication (Authentication=Active Directory Default) in Claims.Api. Keep LocalDB as the default for development. Show how to apply EF Core migrations to it with dotnet-ef.
```

### Portal steps

1. Resource group `rg-sql-demo`.
2. Create **SQL Database**: create new server, authentication **Microsoft Entra-only**, set yourself as admin.
3. Compute: choose the **Free offer** if shown ("Apply free offer"), else **Serverless** with auto-pause (1 hour) and minimum vCores.
4. Networking: Public endpoint, **Add current client IP address** = Yes, allow Azure services = No (unless needed).
5. Run `dotnet ef database update` with the Azure connection string.
6. Query via Portal **Query editor** to verify tables.

### Test the functionality

1. Run the Claims API with the Azure SQL connection string.
2. Call `GET /api/claims` and then create a claim through Swagger.
3. Open the Portal **Query editor** and query the Claims table.
4. Expected result: the created claim appears in Azure SQL and can be read by the API.

### Cleanup

Delete resource group `rg-sql-demo`.

---

## Optional: API Management (APIM)

1. Use the **Consumption** tier (pay per call; Developer tier bills hourly, avoid leaving it).
2. Resource group `rg-apim-demo` > Create **API Management** > tier Consumption.
3. APIs > Add API > HTTP (or OpenAPI import) pointing to a publicly reachable backend (Container App URL).
4. Add policies in the inbound section for `validate-jwt`, `rate-limit-by-key`, `set-header` (generated by the POC 10 Copilot prompt).
5. Test via **Test** tab and subscription key.

### Test the functionality

1. Call the API through the APIM **Test** tab using a valid subscription key.
2. Expected result: APIM forwards the request and displays the backend response.
3. Call without the subscription key and confirm the request is rejected.
4. Send requests above the configured rate limit and confirm `429 Too Many Requests`.
5. If JWT validation is configured, test no token, an invalid token, and a valid token. Expected results are `401`, `401`, and a successful backend response.

6. Delete resource group `rg-apim-demo`. (Consumption tier creates fast; non-Consumption tiers can take 30+ min.)

---

## Per-Session Checklist

- [ ] Resource group created with POC name
- [ ] Budget alert exists
- [ ] Code written via Copilot prompt and tested locally first
- [ ] Azure test completed and screenshots/notes taken
- [ ] Resource group **deleted**
- [ ] Key Vault purged (if used)
- [ ] Resource groups list empty of leftovers
- [ ] Cost analysis checked next day

## Quick Cleanup Audit

Find anything still running:

```
az group list --query "[].name" -o tsv
az resource list --query "[].{name:name,type:type,rg:resourceGroup}" -o table
```

Portal: **All resources** (filter subscription) should be empty when finished.
