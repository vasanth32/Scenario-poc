# Azure Configuration Modernization Plan

> **Status:** Validated

Generated: 2026-10-21

---

## 1. Project Overview

**Goal:** Remove database connection strings and JWT signing keys from tracked configuration, support local environment variables and development user-secrets, and optionally load Azure Key Vault through `DefaultAzureCredential` when `KeyVault:Uri` is configured.

**Path:** Modernize Existing (application configuration only; no deployment or infrastructure provisioning requested)

---

## 2. Requirements

| Attribute      | Value                                                                  |
| -------------- | ---------------------------------------------------------------------- |
| Classification | POC / local learning                                                   |
| Scale          | Small                                                                  |
| Budget         | Cost-optimized; no Azure resources provisioned by this change          |
| Compliance     | No stated compliance or data-residency requirement                     |
| Subscription   | Not applicable until Key Vault provisioning or deployment is requested |
| Location       | Not applicable until Key Vault provisioning or deployment is requested |

---

## 3. Components Detected

| Component    | Type                   | Technology                                        | Path                               |
| ------------ | ---------------------- | ------------------------------------------------- | ---------------------------------- |
| Claims API   | API                    | ASP.NET Core Web API, .NET 10, EF Core SQL Server | `InsuranceClaims/Claims.Api`       |
| Policy API   | API                    | ASP.NET Core Web API, .NET 10, EF Core SQL Server | `InsuranceClaims/Policy.Api`       |
| Claims tests | Integration/unit tests | xUnit, WebApplicationFactory                      | `InsuranceClaims/Claims.Api.Tests` |

Both APIs currently read SQL Server connection strings from tracked `appsettings.json` and a shared JWT signing key from tracked `appsettings.Development.json`.

---

## 4. Recipe Selection

**Selected:** Code-only Azure SDK integration

**Rationale:** The request is limited to configuration providers. No hosting target, deployment, `azure.yaml`, IaC, Dockerfile, subscription, or Azure region is required. The application will be ready to consume an existing Key Vault URI and Azure identity later.

---

## 5. Configuration Architecture

Configuration precedence for secrets:

1. Standard ASP.NET Core JSON configuration contains only non-secret settings.
2. Development user-secrets load automatically through each project's `UserSecretsId`.
3. Environment variables override JSON/user-secrets locally, using `ConnectionStrings__ClaimsDb`, `ConnectionStrings__PolicyDb`, and `Jwt__SigningKey`.
4. When `KeyVault__Uri` or `KeyVault:Uri` exists, Azure Key Vault is added last and supplies secret names such as `ConnectionStrings--ClaimsDb` and `Jwt--SigningKey`.
5. `DefaultAzureCredential` uses the developer identity locally and managed identity when hosted in Azure.

The APIs retain fail-fast checks when required connection strings or JWT settings are absent.

---

## 6. Execution Checklist

### Phase 1: Planning

- [x] Analyze workspace
- [x] Gather requirements from the POC guide and current request
- [x] Scan codebase
- [x] Select code-only recipe
- [x] Plan configuration architecture
- [x] **User approved this plan**

### Phase 2: Execution

- [x] Add Azure Identity and Key Vault configuration packages to both APIs
- [x] Add a unique `UserSecretsId` to each API project
- [x] Add optional Key Vault configuration before services consume configuration
- [x] Remove DB connection strings and JWT signing keys from tracked JSON
- [x] Make integration tests inject isolated JWT configuration
- [x] Document environment-variable, user-secrets, and Key Vault secret names
- [x] Build both APIs and run all tests
- [x] Update status to `Ready for Validation`

### Phase 3: Validation

- [x] Invoke azure-validate guidance for configuration readiness
- [x] Record validation proof below
- [x] Update status to `Validated`

---

## 7. Validation Proof

| Check              | Command Run                                                                                  | Result                               | Timestamp  |
| ------------------ | -------------------------------------------------------------------------------------------- | ------------------------------------ | ---------- |
| Solution build     | `dotnet build InsuranceClaims/InsuranceClaims.slnx --nologo`                                 | Pass: 0 warnings, 0 errors           | 2026-10-21 |
| Full test suite    | `dotnet test InsuranceClaims/InsuranceClaims.slnx --nologo`                                  | Pass: 46 passed, 0 failed, 0 skipped | 2026-10-21 |
| Package audit      | `dotnet list InsuranceClaims/InsuranceClaims.slnx package --vulnerable --include-transitive` | Pass: no vulnerable packages         | 2026-10-21 |
| Secret scan        | Source scan for prior DB strings and JWT signing-key values                                  | Pass: no tracked secret values       | 2026-10-21 |
| Editor diagnostics | Workspace error check                                                                        | Pass: no errors                      | 2026-10-21 |

**Validated by:** azure-validate workflow

**Validation timestamp:** 2026-10-21

---

## 8. Files to Modify

| File                                                   | Purpose                                | Status   |
| ------------------------------------------------------ | -------------------------------------- | -------- |
| `.azure/plan.md`                                       | Source of truth for this change        | Created  |
| `InsuranceClaims/Claims.Api/Claims.Api.csproj`         | User-secrets ID and Azure packages     | Complete |
| `InsuranceClaims/Policy.Api/Policy.Api.csproj`         | User-secrets ID and Azure packages     | Complete |
| Both API `Program.cs` files                            | Optional Key Vault provider            | Complete |
| Both API `appsettings*.json` files                     | Remove tracked secrets                 | Complete |
| `InsuranceClaims/Claims.Api.Tests/ClaimsApiFactory.cs` | Inject test JWT configuration          | Complete |
| `InsuranceClaims/README.md`                            | Local and Key Vault setup instructions | Complete |

---

## 9. Next Steps

> Current: Validated; deployment not requested

1. Configure local secrets using `InsuranceClaims/README.md`.
2. Provisioning and deployment remain out of scope until explicitly requested.
