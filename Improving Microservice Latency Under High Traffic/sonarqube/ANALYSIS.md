# SonarQube — Analysis Notes & How to Use It (Microservices POC)

This doc explains what your current SonarQube dashboard is showing for `microservices-security-poc`, and a practical workflow to use SonarQube effectively while you learn.

---

## What SonarQube is (in one line)

**SonarQube is a static analysis server**: you run a scanner against your code, it uploads results, and you triage findings in the UI (bugs/vulnerabilities/code smells/security hotspots/duplication/coverage).

---

## How your setup is structured

- **One SonarQube “Project”**: `microservices-security-poc`
- **Three modules inside it** (as seen in the **Code** tab):
  - `OrderService`
  - `PaymentService`
  - `ProductService`

This is normal when you scan a .NET solution containing multiple projects.

---

## What your dashboard is showing (based on your screenshots)

### Quality Gate: **Passed**
- Your configured gate conditions are currently satisfied.
- For a learning POC, that’s fine—but **don’t confuse “Passed” with “no work left”**. You still have hotspots/code smells/duplication/coverage to improve.

### Reliability / Security / Maintainability ratings
- **Bugs**: 0 (good)
- **Vulnerabilities**: 0 (good)
- **Code smells**: 4 (these are maintainability / readability / correctness-risk findings)
- **Security hotspots**: 3 (**not confirmed vulnerabilities**; they require human review)

### Coverage: **0.0%**
- SonarQube is not seeing test coverage reports.
- This usually means either:
  - you have no tests, or
  - tests exist but you didn’t generate/attach coverage data in the scan.

### Duplications: **~21%** (high)
- SonarQube detected repeated code blocks.
- In microservice demos this is common (similar `Program.cs` setup, middleware registration, etc.).
- Later you can refactor shared patterns into shared packages or a shared “building blocks” project.

### “Last analysis had 1 warning”
That’s typically scanner-side info (not necessarily a code issue). If scans still complete and results show up, you can treat this as non-blocking unless it keeps happening.

---

## SonarQube UI: how to read each tab

### Overview
The “health dashboard”:
- High-level ratings (A–E)

- Counts for bugs/vulns/hotspots/smells
- Coverage & duplication summaries

### Issues
Action list of findings SonarQube considers “issues”:
- **Bugs**: likely incorrect behavior
- **Vulnerabilities**: confirmed security problems (highest priority)
- **Code smells**: maintainability problems

Use filters:
- **Severity** (Blocker/Critical/Major/Minor/Info)
- **Type** (Bug/Vulnerability/Code Smell)
- **Language**, **File**, etc.

### Security Hotspots
Security Hotspots are **“review required”** items:
- They indicate **security-sensitive code** (auth, crypto, logging, input handling, etc.).
- They are **not automatically vulnerabilities**.

What to do:
- Open **Security Hotspots**
- Click each hotspot → read the explanation → decide:
  - **To Review** (understand it)
  - **Safe** (acceptable for your context)
  - **Fix** (change code)

Goal: get **Reviewed %** off 0%.

### Code
The “module view” of your codebase:
- Lets you browse `OrderService`, `PaymentService`, `ProductService`
- Shows per-module counts (smells/hotspots/duplication)

### Measures
All metrics in one place:
- Coverage
- Duplications
- Complexity (if enabled)
- Size metrics

### Activity
Your analysis timeline:
- Confirms scans are arriving
- Lets you compare runs after fixes

---

## How the .NET scan actually works (why begin/build/end matters)

SonarScanner for .NET works in 3 phases:

- **`begin`**: creates an analysis context (writes config under `.sonarqube/`)
- **`build`**: MSBuild runs and the scanner collects info during compilation
- **`end`**: post-processes and **uploads** results to SonarQube

If **`end`** fails, you typically need to rerun the full sequence again.

---

## Recommended workflow (how to use SonarQube without getting overwhelmed)

### 1) Start with security hotspots (review first, then fix)
Hotspots are great for learning because:
- they explain *why* a pattern is risky
- they teach secure defaults

Target: review all hotspots, fix at least 1.

### 2) Fix “Major” code smells next
In your current results, one smell is **Major** (the others are informational).
Major smells are usually the best ROI because they often correlate with real bugs later (nullability, resource handling, incorrect assumptions).

### 3) Reduce duplication (later)
Duplications are high (~21%) largely because services share similar scaffolding.
Treat this as a **refactoring phase** (not urgent for the POC), e.g.:
- extract shared middleware/config into a shared library
- standardize logging/health check configuration

### 4) Add tests + coverage (optional but powerful)
Right now coverage is 0%.
Even adding a handful of tests (controller/service-level) will:
- reduce regressions
- improve SonarQube’s “confidence” signals

---

## What your current Issues list likely contains (based on the UI)

You currently have **4 code smells**, including:
- “Prefer `static readonly` fields…” in multiple `Program.cs` files (Info)
- A nullability-related warning in `ProductService/Controllers/ProductsController.cs` (Major)

Suggested order:
1) Fix the **Major** nullability issue
2) Fix the `static readonly` suggestions (quick cleanups)
3) Re-scan to see the dashboard move

---

## “NO LONGER ACTIVE” banner in the UI (what it means)

The banner means the SonarQube version you’re running is **end-of-life** (no longer receiving fixes).

- For a **local POC**, it’s OK to proceed.
- If you want to remove the banner, upgrade the Docker image tag to a newer Community version and restart the stack.

Tip: prefer pinning an explicit tag (repeatable builds) rather than floating tags.

---

## Troubleshooting (things you already hit)

### Port bind error on 9000 (Windows)
Symptom:
- Docker fails with “ports are not available … bind … forbidden by its access permissions”

Fix:
- Run SonarQube on another port (you used **9001** successfully).

### SonarScanner `end` fails with Java class version errors
Symptom:
- `UnsupportedClassVersionError` mentioning class file version **61.0**

Cause:
- Scanner post-processing requires a newer Java runtime (Java 17+).

Fix (session-only):
- Set `JAVA_HOME` to Java 17 and prepend `%JAVA_HOME%\bin` in the same shell running the scan.

### Token handling
- Tokens start with `sqp_...`
- Treat them like passwords
- Don’t commit them; use session environment variables (`$env:SONAR_TOKEN`)

---

## Practical “next actions” checklist (do this now)

- [ ] Review all **Security Hotspots** (mark Safe / Fix as needed)
- [ ] Fix the **Major** code smell (nullability) in `ProductService`
- [ ] Re-run scan and confirm **Activity** shows a new successful analysis
- [ ] Pick 1 duplicated block and decide if it should be extracted/shared (optional)
- [ ] Add 1–3 unit tests and wire coverage (optional; big learning payoff)

---

## Quick reference commands (copy/paste)

### Start / stop SonarQube (Docker)

```powershell
cd "D:\PracticeProjects\Scenario-poc\Improving Microservice Latency Under High Traffic\sonarqube"

# If 9000 is blocked, set a different host port (example: 9001)
$env:SONAR_HOST_PORT = 9001

docker compose up -d
docker compose ps
docker compose logs -f --tail 200

# Stop (keeps data volumes)
docker compose down
```

### Run a scan (SonarScanner for .NET)

```powershell
# Ensure Java 17+ is active in THIS window (needed for `dotnet sonarscanner end`)
$env:JAVA_HOME = "C:\Program Files\Eclipse Adoptium\jdk-17.0.18.8-hotspot"
$env:Path = "$env:JAVA_HOME\bin;$env:Path"
java -version

# Session-only token
$env:SONAR_TOKEN = "<paste-token-here>"

cd "D:\PracticeProjects\Scenario-poc\Improving Microservice Latency Under High Traffic"

# Optional cleanup if a previous scan failed mid-way
Remove-Item -Recurse -Force ".sonarqube" -ErrorAction SilentlyContinue

dotnet tool update --global dotnet-sonarscanner

dotnet sonarscanner begin /k:"microservices-security-poc" /n:"microservices-security-poc" /d:sonar.host.url="http://localhost:9001" /d:sonar.token="$env:SONAR_TOKEN"
dotnet build ".\Microservices.SecurityPoc.sln" -c Release
dotnet sonarscanner end /d:sonar.token="$env:SONAR_TOKEN"
```


