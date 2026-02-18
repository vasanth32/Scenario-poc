## Azure Service Bus + Azure Functions POC (1-Day Plan)

This file is designed so you can copy/paste prompts into Cursor and quickly build a simple .NET microservices-style POC that uses **Azure Service Bus** and **Azure Functions**.  
Focus: one small API that publishes messages to Service Bus and one Function that processes them.

---

### 0. Learning Goals and Setup

- **What you will build**
  - A simple `OrderApi` microservice (ASP.NET Core Web API, .NET 8) that sends order messages to an Azure Service Bus queue.
  - An `InventoryProcessorFunction` (Azure Function, .NET 8 isolated) that is triggered by the queue and logs "inventory updated" for each order.

- **Prerequisites**
  - Azure subscription (you can use a free trial if needed).
  - .NET 8 SDK installed.
  - Cursor (or VS Code) on your machine.
  - Azure Functions Core Tools (optional but helpful for local runs).
  - Azure CLI or Azure Portal access.

#### Prompt: Generate friendly overview notes

Paste this into Cursor to generate a short, friendly overview of what you are about to do:

```markdown
You are my .NET and Azure mentor.  
Write a short, friendly overview for a 1-day hands-on POC where I build:
- A small .NET 8 Web API called `OrderApi` that sends messages to Azure Service Bus.
- An Azure Functions .NET 8 isolated function called `InventoryProcessorFunction` that is triggered by a Service Bus queue.

Explain:
- What Azure Service Bus is (in simple language).
- What Azure Functions are and why they pair well with Service Bus.
- The final architecture at a high level (client -> OrderApi -> Service Bus queue -> InventoryProcessorFunction).
Keep it beginner-friendly, concise, and motivating (max 2–3 paragraphs).
```

---

### 1. Azure Portal – Create Service Bus Namespace and Queue

- **Goal**: Create an Azure Service Bus namespace and queue that both the API and Function will use.

- **Steps in Azure Portal**
  - Sign in to the Azure Portal.
  - Search for **Service Bus** in the top search bar.
  - Click **Create** and fill in:
    - **Resource group**: create a new one like `rg-azure-messaging-poc`.
    - **Namespace name**: e.g., `sb-messaging-poc-<unique>`.
    - **Pricing tier**: `Basic` is fine for this POC.
    - Region: choose the region closest to you.
  - After the namespace is created:
    - Open the namespace.
    - Go to **Queues** → **+ Queue** and create a queue named `orders-queue`.
  - Get the **connection string**:
  
    - In the namespace, go to **Shared access policies**.
    - Click `RootManageSharedAccessKey` (or create a specific policy).
    - Copy the **Primary connection string** – you will use it in both the API and Function.

#### Prompt: Generate friendly portal instructions summary

```markdown 
Based on the following steps I just performed in the Azure Portal, write friendly notes that:
- Briefly explain what a Service Bus namespace and queue are.
- Summarize the exact steps I followed to create a namespace and a queue named `orders-queue`.
- Highlight where I got the connection string and why it is sensitive.

Keep the tone conversational and beginner-friendly.  
Here are the steps I followed (you can rephrase them in your own words):
- Create resource group `rg-azure-messaging-poc`.
- Create Service Bus namespace `sb-messaging-poc-<unique>` with Basic tier.
- Inside namespace, create queue `orders-queue`.
- Open Shared access policies → `RootManageSharedAccessKey` → copy Primary connection string.
```

---

### 2. Create the .NET Solution and Projects (Using Cursor)

- **Goal**: Have a clean solution with:
  - `AzureMessagingPoc.sln`
  - `OrderApi` (ASP.NET Core Web API, .NET 8)
  - `InventoryProcessorFunction` (Azure Functions .NET 8 isolated)
  - Optionally: `Shared.Contracts` class library with shared message contracts.

#### Prompt: Create the solution and base projects

Use this prompt in an empty folder (e.g., `D:\PracticeProjects\AzureMessagingPoc`) and let Cursor guide you:

```markdown
You are an AI coding agent working in Cursor on Windows.  
I want you to scaffold a simple .NET 8 messaging POC with Azure Service Bus and Azure Functions.

Tasks:
1. Create a new solution `AzureMessagingPoc.sln`.
2. Add a .NET 8 Web API project named `OrderApi`.
3. Add a .NET 8 Azure Functions isolated worker project named `InventoryProcessorFunction`.
4. Add a .NET 8 class library named `Shared.Contracts` to hold message DTOs.
5. Add project references so both `OrderApi` and `InventoryProcessorFunction` can reference `Shared.Contracts`.

Constraints:
- Keep the folder and project names simple and clean.
- Use minimal dependencies: prefer the default templates plus the Azure Service Bus client library where needed.

Output:
- The exact `dotnet new` and `dotnet sln` commands to run.
- The updated solution and project structure.
- Any NuGet packages that I must add (just list the commands, e.g., `dotnet add package ...`).
```

#### Prompt: Generate friendly notes about the solution structure

```markdown
Based on the final solution layout we just created (`AzureMessagingPoc.sln` with `OrderApi`, `InventoryProcessorFunction`, and `Shared.Contracts`):
- Explain in friendly language why this structure feels like “microservices style”.
- Clarify the responsibilities of each project.
- Mention how `Shared.Contracts` helps avoid duplication and keeps message shapes consistent.

Keep it short (2–3 paragraphs) and beginner-friendly.
```

---

### 3. Define the Shared Message Contract

- **Goal**: Have a simple `OrderCreatedEvent` DTO shared by both the API and the Function.

#### Prompt: Create the shared DTO

```markdown
We have a .NET 8 class library project called `Shared.Contracts`.  
Create a single DTO class named `OrderCreatedEvent` with properties suitable for a simple e-commerce example:
- `Guid OrderId`
- `string CustomerName`
- `decimal TotalAmount`
- `DateTime CreatedAt`

Requirements:
- Use C# 12 features if helpful, but keep it simple (a standard class or record is fine).
- Place it in a namespace like `Shared.Contracts.Messages`.
- Make sure it is serializable to JSON using System.Text.Json.

Show me the full C# file.
```

#### Prompt: Friendly notes about message contracts

```markdown
Write friendly notes (1–2 paragraphs) explaining:
- What a “message contract” is in the context of messaging and microservices.
- Why having a shared `OrderCreatedEvent` type is useful in this POC.
- How this helps both the API and the Function agree on the same data format.
Use non-academic, conversational language.
```

---

### 4. Implement `OrderApi` – Publish Messages to Azure Service Bus

- **Goal**: A minimal Web API endpoint that:
  - Accepts an order payload via HTTP POST.
  - Creates an `OrderCreatedEvent`.
  - Sends it to the `orders-queue` on Azure Service Bus.

#### Prompt: Add Service Bus configuration and client setup

```markdown
In the `OrderApi` (.NET 8 Web API) project:
- Use `Azure.Messaging.ServiceBus` to send messages to Azure Service Bus.

Tasks:
1. Add the NuGet package: `Azure.Messaging.ServiceBus`.
2. In `appsettings.Development.json`, add configuration like:
   "ServiceBus": {
     "ConnectionString": "<placeholder>",
     "QueueName": "orders-queue"
   }
3. In `Program.cs`, register a singleton `ServiceBusClient` using the configured connection string.
4. Register a transient or scoped service `IOrderMessagePublisher` that:
   - Takes `ServiceBusClient` and queue name via DI.
   - Has a method `Task PublishAsync(OrderCreatedEvent orderEvent, CancellationToken ct = default)` that:
     - Serializes the event to JSON.
     - Sends it as a message to the configured queue.

Please:
- Show the relevant configuration classes or records (e.g., `ServiceBusOptions` if you create one).
- Show the updated `Program.cs` with DI registrations.
- Show the implementation of `OrderMessagePublisher`.
Assume the `OrderCreatedEvent` type lives in `Shared.Contracts` and reference it appropriately.
```

#### Prompt: Add the HTTP endpoint

```markdown
In `OrderApi`, add an HTTP POST endpoint to create an order and publish it to Service Bus.

Requirements:
- Endpoint: `POST /api/orders`.
- Request body: simple DTO with:
  - `string CustomerName`
  - `decimal TotalAmount`
- In the handler:
  - Map the request DTO to `OrderCreatedEvent` (generate `OrderId` and `CreatedAt`).
  - Call `IOrderMessagePublisher.PublishAsync`.
  - Return `201 Created` with a simple response containing the new `OrderId`.

Implementation style:
- Use minimal APIs or a simple controller, whichever is cleanest, but keep the code small and readable.
- Include basic validation (e.g., reject empty customer name or non-positive amount).

Show all necessary C# code.
```

#### Prompt: Friendly notes about the API and sending messages

```markdown
Write friendly notes explaining:
- What happens when I call `POST /api/orders` in this POC.
- How the API converts my HTTP request into a message and sends it to Azure Service Bus.
- Why using DI and a separate `IOrderMessagePublisher` service is a good idea.

Keep it simple and practical; focus on the “story” of a single order flowing into the system.
```

---

### 5. Implement `InventoryProcessorFunction` – Process Messages from the Queue

- **Goal**: An Azure Functions project that:
  - Has a single Function triggered by `orders-queue`.
  - Deserializes `OrderCreatedEvent`.
  - Logs or simulates inventory update.

#### Prompt: Create the Service Bus–triggered function (isolated worker)

```markdown
In the `InventoryProcessorFunction` project (Azure Functions .NET 8 isolated):

Tasks:
1. Ensure the project is configured as an isolated worker function app using `Microsoft.Azure.Functions.Worker`.
2. Add NuGet packages as needed:
   - `Microsoft.Azure.Functions.Worker`
   - `Microsoft.Azure.Functions.Worker.Extensions.ServiceBus`
   - `Azure.Messaging.ServiceBus` (if needed)
3. Create a function class `InventoryProcessorFunction` with:
   - A method annotated as a Service Bus trigger for the `orders-queue`.
   - Parameter type: `string` or `BinaryData` for the message body, plus `FunctionContext`.
   - Logic:
     - Deserialize the body into `OrderCreatedEvent` from `Shared.Contracts`.
     - Log something like: "Processing order {OrderId} for {CustomerName}. Decreasing inventory."

Configuration:
- Use `local.settings.json` with a setting like:
  "ServiceBusConnection": "<placeholder-connection-string>"
- Bind the trigger to this connection setting.

Please show:
- The `Program.cs` / host builder setup for the isolated worker.
- The function class and method with correct attributes.
- A sample `local.settings.json` (with fake connection string value).
```

#### Prompt: Friendly notes about the function and message processing

```markdown
Write friendly notes describing:
- How the `InventoryProcessorFunction` gets triggered when a new message arrives on `orders-queue`.
- How it reads and deserializes the `OrderCreatedEvent`.
- How this simulates a real inventory service in a microservices architecture.

Keep it short, story-like, and beginner-friendly.
```

---

### 6. Wiring Up Configuration and Secrets

- **Goal**: Safely connect local code to Azure Service Bus using connection strings (for the POC).

- **Local configuration tips**
  - In `OrderApi`, store the connection string in `appsettings.Development.json` or in user secrets.
  - In `InventoryProcessorFunction`, store it in `local.settings.json` under a key like `ServiceBusConnection`.
  - Never commit real connection strings to source control.

#### Prompt: Generate configuration and secrets guidance

```markdown
Based on this POC (OrderApi + InventoryProcessorFunction using Azure Service Bus):
- Explain where connection strings should live for local development (appsettings.Development.json, user secrets, local.settings.json).
- Explain what should never be committed to Git.
- Give 2–3 practical tips for rotating and protecting Service Bus connection strings in a real project.

Keep it conversational and not too long.
```

---

### 7. Local Testing Flow

- **Goal**: Run everything locally and confirm the end-to-end flow.

- **Manual steps**
  - Ensure `OrderApi` is running locally (e.g., `https://localhost:5001`).
  - Ensure `InventoryProcessorFunction` is running via `func start` or F5 in your IDE.
  - Use a tool like `curl`, Postman, or the built-in `.http` file to call:
    - `POST https://localhost:<port>/api/orders` with a JSON body like:
      ```json
      {
        "customerName": "Alice",
        "totalAmount": 99.99
      }
      ```
  - Verify:
    - The API returns `201 Created` with an `orderId`.
    - The Function logs indicate it processed the message for that `orderId`.

#### Prompt: Generate friendly testing checklist

```markdown
Create a short, friendly checklist for testing this POC end-to-end:
- Start OrderApi.
- Start InventoryProcessorFunction.
- Send a test HTTP request.
- Confirm logs and behavior.

Make it bullet-based and mark each step with a checkbox style (e.g., [ ]).  
Use a tone like a helpful tutor guiding me through my first integration test.
```

---

### 8. (Optional) Deploy to Azure

You can keep this for later if 1 day is too short, but here is a simple outline.

- **High-level Azure steps**
  - Create an **Azure Function App** (Consumption or Premium plan) targeting `.NET 8 isolated`.
  - Deploy `InventoryProcessorFunction` to the Function App.
  - Configure **Application settings** for the Function App:
    - Set `ServiceBusConnection` with the Service Bus connection string.
  - Optionally deploy `OrderApi` to an **Azure App Service** or **Azure Container Apps**.

#### Prompt: High-level deployment guidance notes

```markdown
Write a high-level, beginner-friendly explanation of:
- How I would deploy `InventoryProcessorFunction` to Azure.
- How I would configure its `ServiceBusConnection` setting in the Azure Portal.
- (Optional) How I could later deploy `OrderApi` to Azure App Service and point it at the same queue.

Keep it conceptual (not step-by-step click instructions), focusing on the big picture.
```

---

### 9. Wrap-Up and Reflection

- **Goal**: Cement learning by writing a short reflection.

#### Prompt: Generate reflection questions and notes

```markdown
Create:
- 5 short reflection questions about what I learned in this POC (Azure Service Bus, Functions, microservices messaging).
- A short example reflection answer (1–2 paragraphs) that a beginner might write after completing this POC.

Keep the tone encouraging and focused on practical understanding, not theory.
```


