# LinkedIn Post Image Design Specification

## Image Concept: Azure Service Bus + Azure Functions Architecture Diagram

### Visual Style
- **Format**: Landscape orientation (1200x627px for LinkedIn optimal size)
- **Style**: Modern, clean, professional tech diagram
- **Color Scheme**: 
  - Azure blue (#0078D4) for Azure services
  - Dark gray (#2D2D2D) for background
  - White/light gray for text
  - Green (#00BC13) for success/flow
  - Orange (#FF8C00) for highlights

### Layout Structure

```
┌─────────────────────────────────────────────────────────────┐
│  HEADER: "Azure Service Bus + Azure Functions POC"          │
│  Subtitle: "Event-Driven Microservices Architecture"        │
└─────────────────────────────────────────────────────────────┘

┌──────────────┐         ┌──────────────┐         ┌──────────────┐
│   OrderApi   │  ────►  │ Azure Service│  ────►  │   Function    │
│  (.NET 8)    │  HTTP   │     Bus      │  Queue  │  (Isolated)   │
│              │         │              │         │               │
│  POST /api/  │         │  orders-     │         │ ProcessOrder  │
│   orders     │         │   queue      │         │               │
└──────────────┘         └──────────────┘         └──────────────┘
     │                          │                          │
     │                          │                          │
     ▼                          ▼                          ▼
┌─────────────────────────────────────────────────────────────┐
│              KEY LEARNINGS (Bottom Section)                  │
│                                                              │
│  🏗️ Two-Process Model  │  🔌 gRPC Communication  │  📦 Auto-Scaling │
│  Host + Worker         │  Fast binary protocol   │  Serverless      │
└─────────────────────────────────────────────────────────────┘
```

### Detailed Components

#### Top Section (Header)
- **Title**: "Azure Service Bus + Azure Functions POC"
- **Subtitle**: "Building Event-Driven Microservices with .NET 8"
- **Font**: Bold, large, white text
- **Background**: Gradient from Azure blue to dark blue

#### Middle Section (Architecture Flow)

**1. OrderApi Box:**
- Icon: API/Web service icon
- Text: "OrderApi (.NET 8)"
- Arrow pointing right (green)
- Label: "Publishes Message"

**2. Service Bus Box:**
- Icon: Queue/messaging icon
- Text: "Azure Service Bus"
- Subtext: "orders-queue"
- Two arrows: incoming (from OrderApi) and outgoing (to Function)
- Visual: Queue with messages (small rectangles)

**3. Function Box:**
- Icon: Serverless/function icon
- Text: "InventoryProcessorFunction"
- Subtext: ".NET 8 Isolated Worker"
- Arrow pointing down
- Label: "Processes Message"

#### Bottom Section (Key Learnings - 3 Columns)

**Column 1: Two-Process Model**
- Icon: Two connected boxes
- Title: "Two-Process Architecture"
- Bullet points:
  - Functions Host (infrastructure)
  - Isolated Worker (your code)
  - gRPC communication

**Column 2: Automatic Handling**
- Icon: Gear/automation icon
- Title: "Automatic Management"
- Bullet points:
  - Connection retries
  - Dead-letter queue
  - Message acknowledgment

**Column 3: Security & Best Practices**
- Icon: Shield/lock icon
- Title: "Security First"
- Bullet points:
  - Least-privilege policies
  - Key rotation
  - Secure storage

### Text Content for Image

**Main Title:**
"Azure Service Bus + Azure Functions POC"

**Subtitle:**
"Event-Driven Microservices with .NET 8"

**Flow Labels:**
- "HTTP POST" (OrderApi → Service Bus)
- "Message Queue" (Service Bus)
- "Auto-Trigger" (Service Bus → Function)

**Key Points (Bottom):**
1. "Two-Process Model: Host manages infrastructure, Worker executes your code"
2. "gRPC Communication: Fast, efficient binary protocol"
3. "Automatic Scaling: Serverless handles concurrency"

### Alternative Simpler Design

**Option 2: Minimalist Flow Diagram**

```
┌─────────────────────────────────────────────────────────────┐
│                                                              │
│     [OrderApi]  ──►  [Service Bus]  ──►  [Function]        │
│                                                              │
│                    Key Learnings:                            │
│                                                              │
│  • Two-Process Architecture (Host + Worker)                 │
│  • gRPC Communication                                        │
│  • Automatic Message Handling                                │
│  • Security Best Practices                                   │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### Tools to Create This Image

1. **Canva** (Free)
   - Search "LinkedIn post" template
   - Use tech/cloud icons
   - Add flow arrows

2. **Figma** (Free)
   - Create custom diagram
   - Use Azure icon library
   - Export as PNG

3. **Draw.io / diagrams.net** (Free)
   - Use Azure architecture templates
   - Export as image

4. **PowerPoint / Google Slides**
   - Use shapes and arrows
   - Export as image

5. **AI Image Generators**
   - DALL·E: "Professional tech architecture diagram showing Azure Service Bus connecting to Azure Functions, modern blue color scheme, clean design"
   - Midjourney: Similar prompt

### Quick Canva Instructions

1. Go to canva.com
2. Create custom size: 1200 x 627px
3. Add background: Dark blue gradient
4. Add text boxes for each component
5. Use arrow shapes to show flow
6. Add icons from Canva's icon library
7. Use Azure blue (#0078D4) for main elements
8. Export as PNG

### ASCII Art Version (For Quick Reference)

```
╔═══════════════════════════════════════════════════════════╗
║  Azure Service Bus + Azure Functions POC                   ║
║  Event-Driven Microservices with .NET 8                    ║
╠═══════════════════════════════════════════════════════════╣
║                                                             ║
║   ┌──────────┐      ┌──────────┐      ┌──────────┐        ║
║   │ OrderApi │ ───► │ Service  │ ───► │ Function │        ║
║   │ .NET 8   │      │   Bus    │      │ Isolated │        ║
║   └──────────┘      └──────────┘      └──────────┘        ║
║                                                             ║
║   Key Learnings:                                           ║
║   • Two-Process Model (Host + Worker)                      ║
║   • gRPC Communication                                     ║
║   • Automatic Message Handling                             ║
║   • Security Best Practices                                ║
║                                                             ║
╚═══════════════════════════════════════════════════════════╝
```

---

**Recommended Approach:**
Use Canva with the LinkedIn post template, add the architecture flow diagram, and include the key learnings as text overlays. This will create a professional, shareable image perfect for LinkedIn.

