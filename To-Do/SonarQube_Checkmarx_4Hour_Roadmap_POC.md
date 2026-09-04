## 4-Hour Roadmap POC — SonarQube + Checkmarx for .NET (Microservices) using Cursor AI

Goal: become **comfortable running scans, reading findings, fixing issues, and re-scanning** for a .NET microservice codebase using **SonarQube** and **Checkmarx** — within **4 hours**.

---

## What you will produce (POC deliverables)

- **SonarQube running locally** (Docker) and a project created.
- **One microservices solution scanned** (3 services) with SonarScanner for .NET.
- **At least 5 issues fixed** (mix of Vulnerabilities / Security Hotspots / Code Smells) + re-scan shows improvement.
- **Checkmarx in Cursor** (Developer Assist) showing IDE findings + remediation on the same code.
- A short **comparison note**: what each tool caught + what you’d use each for.

---

## Timebox plan (4 hours total)

- **0:00–0:15**: Pick target microservices + create a solution (`.sln`) to scan
- **0:15–0:55**: SonarQube local setup (Docker) + create project/token
- **0:55–1:35**: SonarScanner for .NET scan (baseline) + read results
- **1:35–2:15**: Fix findings + re-scan (prove improvement)
- **2:15–2:55**: Checkmarx in Cursor (Developer Assist) + run/observe IDE findings
- **2:55–3:35**: Remediate using Checkmarx guidance + validate
- **3:35–4:00**: Compare tools + write your “what I learned” notes

---

## Prerequisites (do these once)

- **Docker Desktop**: required for local SonarQube.
- **.NET SDK**: you already have .NET projects; confirm `.NET 8` is installed.
- **Cursor**: you’ll use Cursor AI prompts throughout.
- **Access for Checkmarx**: you need a Checkmarx account/org to authenticate the extension. If you don’t have access, you can still complete the SonarQube POC and keep the Checkmarx section as “setup + exploration”.

### Checkmarx “is it free?”

- **Creating an account**: typically **free** (signup).
- **Running scans (SAST/SCA/IaC) with results**: typically requires a **trial or paid org license** (varies by company/org plan).
- **IDE extension**: usually **free to install**, but you still need to **authenticate** to a Checkmarx org to get real findings.

---

## Target codebase for the POC (use your existing microservices)

Use this folder (already in your repo):

- `D:\PracticeProjects\Scenario-poc\Improving Microservice Latency Under High Traffic\`
  - `OrderService\OrderService.csproj`
  - `PaymentService\PaymentService.csproj`
  - `ProductService\ProductService.csproj`

SonarScanner for .NET works best from a **solution**. This folder doesn’t currently have a `.sln`, so you’ll create one.

---

## Phase 0 (0:00–0:15) — Create a solution for scanning

### Cursor Prompt (copy/paste)

```text
In the folder:
D:\PracticeProjects\Scenario-poc\Improving Microservice Latency Under High Traffic\

Create a new solution named Microservices.SecurityPoc.sln and add these projects:
- OrderService\OrderService.csproj
- PaymentService\PaymentService.csproj
- ProductService\ProductService.csproj

Provide the exact PowerShell commands to:
1) Create the .sln
2) Add each .csproj to it
3) Build the solution

Explain briefly why SonarScanner for .NET prefers a solution.
```

### Exact PowerShell commands (you can run these now)

```powershell
cd "D:\PracticeProjects\Scenario-poc\Improving Microservice Latency Under High Traffic"

# Create solution
dotnet new sln -n "Microservices.SecurityPoc"

# Add projects
dotnet sln ".\Microservices.SecurityPoc.sln" add ".\OrderService\OrderService.csproj"
dotnet sln ".\Microservices.SecurityPoc.sln" add ".\PaymentService\PaymentService.csproj"
dotnet sln ".\Microservices.SecurityPoc.sln" add ".\ProductService\ProductService.csproj"

# Build
dotnet build ".\Microservices.SecurityPoc.sln"
```

### Expected outcome
- A new solution file (e.g., `Microservices.SecurityPoc.sln`) at:
  - `D:\PracticeProjects\Scenario-poc\Improving Microservice Latency Under High Traffic\`
- `dotnet build` succeeds.

---

## Phase 1 (0:15–0:55) — Run SonarQube locally (Docker) and create a project

### Cursor Prompt (copy/paste)

```text
Help me run SonarQube locally (community edition is fine) using Docker Compose.

Requirements:
- Expose SonarQube on http://localhost:9000
- Persist data using Docker volumes
- Include exact commands to start/stop

Then guide me through the UI:
- initial login
- create a new project named "microservices-security-poc"
- generate a token for local scanning

Also include a small "common troubleshooting" section (ports, docker memory, startup time).
```

### Notes
- **Do not commit tokens**. Store the token in an environment variable (PowerShell) during the session.
- SonarQube **Community** has limitations (example: no branch analysis in many setups). That’s OK for this POC.

### Run SonarQube locally with Docker Compose (Community Edition)

#### Prereqs
- **Docker Desktop** installed and running (Compose included).

#### Docker Compose file (persisted volumes)
- Create this file in your repo at:
  - `D:\PracticeProjects\Scenario-poc\Improving Microservice Latency Under High Traffic\sonarqube\docker-compose.yml`

```yaml
services:
  sonarqube:
    image: sonarqube:lts-community
    container_name: sonarqube
    restart: unless-stopped
    ports:
      - "9000:9000"
    environment:
      # Local/dev convenience. If you prefer strict checks, remove this and follow the vm.max_map_count troubleshooting note.
      - SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true
    volumes:
      - sonarqube_data:/opt/sonarqube/data
      - sonarqube_extensions:/opt/sonarqube/extensions
      - sonarqube_logs:/opt/sonarqube/logs
    ulimits:
      nofile:
        soft: 131072
        hard: 131072
      nproc:
        soft: 8192
        hard: 8192

volumes:
  sonarqube_data:
  sonarqube_extensions:
  sonarqube_logs:
```

#### Exact commands (start/stop)
Run from PowerShell:

```powershell
cd "D:\PracticeProjects\Scenario-poc\Improving Microservice Latency Under High Traffic\sonarqube"

# Start (pulls image on first run)
docker compose up -d

# Check status/logs
docker compose ps
docker compose logs -f --tail 200

# Stop containers (keeps volumes/data)
docker compose down

# OPTIONAL: remove volumes (wipes SonarQube data)
docker compose down -v
```

#### Open SonarQube
- Browse to **`http://localhost:9000`**
- First startup can take a few minutes. Wait until the login page loads.

### UI walkthrough (login → create project → token)

#### 1) Initial login
- Go to `http://localhost:9000`
- Login with:
  - **Username**: `admin`
  - **Password**: `admin`
- SonarQube will prompt you to **change the admin password**.

#### 2) Create a new project: `microservices-security-poc`
- In the SonarQube UI, choose **Create Project**
- Select **Manually**
- Set:
  - **Project key**: `microservices-security-poc` (recommended)
  - **Display name**: `microservices-security-poc`
- Continue.

#### 3) Generate a token for local scanning
- When prompted for analysis method, choose the option that lets you **generate a token**
- Create a token name like: `local-dev`
- Copy the token once (you won’t be able to see it again)
- Store it for the current PowerShell session only:

```powershell
$env:SONAR_TOKEN = "<paste-token-here>"
```

### Common troubleshooting (quick)

#### Ports
- If `http://localhost:9000` doesn’t load:
  - Check port usage: `netstat -ano | findstr :9000`
  - If something else is using 9000 (or Windows blocks binding), run SonarQube on a different host port (example: 9001):

```powershell
$env:SONAR_HOST_PORT = 9001
docker compose up -d
```

```bat
set SONAR_HOST_PORT=9001
docker compose up -d
```

  - Then open `http://localhost:9001`.

#### Docker memory / resources
- SonarQube can fail to start if Docker Desktop has too little memory.
  - In **Docker Desktop → Settings → Resources**, try **4–6 GB RAM** (more is better).

#### Startup time / health
- First run is slower because images are pulled and extensions are prepared.
  - Watch logs: `docker compose logs -f --tail 200`
  - If it loops on Elasticsearch / bootstrap errors, see the `vm.max_map_count` note below.

#### `vm.max_map_count` (Elasticsearch) on Windows/WSL2
- If you removed `SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true` (or want strict settings) and see errors about `vm.max_map_count`,
  set it inside the Docker Desktop WSL VM and then restart the compose stack:

```powershell
wsl -d docker-desktop -u root sysctl -w vm.max_map_count=262144
cd "D:\PracticeProjects\Scenario-poc\Improving Microservice Latency Under High Traffic\sonarqube"
docker compose down
docker compose up -d
```

---

## Phase 2 (0:55–1:35) — Baseline scan with SonarScanner for .NET

### Cursor Prompt (copy/paste)

```text
I want to scan my .NET solution with SonarQube using SonarScanner for .NET (dotnet tool).

Context:
- SonarQube URL: http://localhost:9000
- Solution path:
  D:\PracticeProjects\Scenario-poc\Improving Microservice Latency Under High Traffic\Microservices.SecurityPoc.sln

Give me step-by-step PowerShell commands to:
1) Install the scanner as a global dotnet tool (if missing)
2) Set SONAR_TOKEN env var (session only)
3) Run:
   dotnet sonarscanner begin ...
   dotnet build ...
   dotnet test ... (if tests exist; if not, skip cleanly)
   dotnet sonarscanner end ...

Include the minimal sonar properties (project key/name, host url, token).
Explain what "begin/build/end" is doing.
```

### What to look at in SonarQube UI

- **Issues**: Vulnerabilities vs Code Smells
- **Security Hotspots**: “review required” items
- **Quality Gate**: pass/fail and why
- **Measures**: duplicated lines, maintainability, reliability

### Cursor Prompt (reading findings)

```text
I just ran a SonarQube scan. Help me triage results fast.

I want:
- the top 10 issues ordered by (Severity, then effort)
- pick 5 to fix in the next 40 minutes
- explain each issue in 2-3 sentences (why it matters, exploit/impact)
- point me to the exact files/lines to edit

Assume this is a learning POC: prefer issues that teach security + clean code habits.
```

---

## Phase 3 (1:35–2:15) — Fix 5 findings + re-scan to prove improvement

### A fast “learning set” of fixes (pick from these)

- **Hardcoded secrets**: move to user secrets / environment variables
- **Logging sensitive data**: remove/obfuscate
- **Input validation**: validate IDs, pagination params, bounds
- **Exception handling**: don’t swallow exceptions; return safe problem details
- **SQL injection style patterns** (if present): remove string-concatenated SQL; use EF parameterization / LINQ

### Cursor Prompt (apply fixes safely)

```text
Help me fix these SonarQube issues in-place, but keep behavior stable.

Rules:
- Make the smallest correct changes
- Don’t add heavy frameworks
- Explain each fix with: (root cause, secure pattern, how to verify)

After changes, tell me the exact commands to re-run the SonarScanner scan.
```

### Proof step

After re-scan, capture:
- number of issues reduced (or severities reduced)
- the Quality Gate status change (if any)

---

## Phase 4 (2:15–2:55) — Checkmarx in Cursor (Developer Assist) setup + first findings

### Cursor Prompt (copy/paste)

```text
I want to use Checkmarx inside Cursor to scan my .NET microservice code.

Guide me through:
- installing the Checkmarx Developer Assist extension in Cursor
- authenticating to my Checkmarx org (tell me what I need: URL/tenant/client id/token, etc.)
- enabling real-time scanning for a .NET solution

Then tell me how to:
- open a file and see findings
- interpret severity and confidence
- generate remediation suggestions and review them safely
```

### What to capture (for learning)

- 3 examples of findings in code (title + why it matters)
- 1 example of a false positive (if you hit one) and how to handle it

---

## Phase 5 (2:55–3:35) — Remediate using Checkmarx guidance, then validate

### Cursor Prompt (remediation loop)

```text
Pick 3 Checkmarx findings from my current solution and help me fix them.

For each finding:
- show the vulnerable pattern
- show the secure pattern
- update the code
- explain how to validate locally (unit test, manual call, or simple reproduction)

Finally, summarize what rules/categories these belonged to (e.g., injection, secrets, auth, crypto).
```

---

## Phase 6 (3:35–4:00) — Compare SonarQube vs Checkmarx (your final notes)

### Cursor Prompt (comparison table)

```text
Create a comparison table for SonarQube vs Checkmarx based on what we just saw in this repo.

Include:
- what each tool caught that the other didn’t (examples from my codebase)
- how severity models differ (Vulnerability vs Hotspot vs Code Smell, etc.)
- where each fits in a .NET microservices workflow:
  local IDE, PR checks, CI pipeline, release gates

End with a recommended workflow I can actually follow daily.
```

### Final checklist (mark as you finish)

- [ ] Created `Microservices.SecurityPoc.sln` and built it
- [ ] SonarQube running locally + project created + token saved safely
- [ ] Baseline Sonar scan completed and results reviewed
- [ ] Fixed at least 5 Sonar findings and re-scanned
- [ ] Checkmarx extension installed + authenticated
- [ ] Observed findings in Cursor and fixed at least 3
- [ ] Wrote a short Sonar vs Checkmarx comparison note

---

## Optional (only if time remains): “CI gate” POC

If you want a quick pipeline-style demo:

### Cursor Prompt

```text
Create a minimal CI example for this repo that runs SonarScanner for .NET and fails on a Quality Gate.

I don’t need a full real pipeline; give me:
- a simple script (PowerShell) I can run locally to simulate CI
- notes on where to store secrets (SONAR_TOKEN)
- what a typical PR gate would enforce
```


