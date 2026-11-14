# Azure SQL Database Deployment – Updated Enterprise POC Roadmap

**Project:** InsuranceClaims Microservices  
**Stack:** .NET 10, EF Core, GitHub, Azure DevOps Pipelines, Azure SQL, Docker, Azure Container Registry (ACR), Kubernetes / Azure Kubernetes Service (AKS), Helm. Azure Container Apps remains an existing comparison/deployment baseline. Monitoring and observability will cover Azure-native tooling and commonly used industry platforms.

## 1. Objective

Implement a realistic, repeatable database-and-application release workflow for the existing InsuranceClaims solution. GitHub remains the source-control system; Azure DevOps is used for CI/CD. The pending Claims migration will be the first automated database deployment and will not be manually applied through SSMS. Extend the same solution to demonstrate Docker image creation, ACR publishing, and Kubernetes/AKS deployment using Helm. Avoid creating a separate application or duplicating the existing Container Apps work.

## 2. Current environment and verified state

| Item | Current state |
|---|---|
| Source control | InsuranceClaims repository already exists in GitHub |
| CI/CD | Azure DevOps project/pipeline not yet created |
| Applications | `Claims.Api` and `Policy.Api` |
| Claims context/database | `ClaimsDbContext` → `ClaimsDb` |
| Policy context/database | `PolicyDbContext` → `PolicyDb` |
| Azure SQL server | `sql-poc15-policy.database.windows.net` |
| Database ownership | Claims.Api owns ClaimsDb; Policy.Api owns PolicyDb |
| Existing Claims migrations | 3 previously applied migrations verified in local and Azure migration history |
| Existing Policy migrations | 1 previously applied migration verified in local and Azure migration history |
| EF tooling | `dotnet-ef` 10.0.13 |
| SQL connectivity | Local machine TCP 1433 test succeeded; client IP firewall rule was added |
| SQL permissions | `db_owner` membership check returned 1 in ClaimsDb and PolicyDb for the connected SSMS user |
| Azure hosting | Claims.Api and Policy.Api are hosted in Azure Container Apps; ACR and Container Apps environment already exist from the POC |

### Current pending schema change

A nullable `DeploymentNotes` property was added to the `Claim` entity for this deployment exercise. EF migration `20261113130307_AddClaimDeploymentNotes` was created successfully. Its `Up()` method adds `Claims.DeploymentNotes` as nullable `nvarchar(max)` and includes an `UpdateData` operation for the configured seeded claim, setting the new value to `NULL`.

The idempotent SQL file `artifacts/claims-deployment-v2.sql` was generated and inspected. It contains migration-history guards, the `ALTER TABLE`, the seed-data update, and the migration-history insert. **This migration has not been applied to Azure SQL. Keep it pending for the automated Azure DevOps deployment. Do not execute the SQL manually in SSMS.**

Previously generated Linux x64 migration bundles and initial idempotent scripts exist in the local `artifacts` folder, but they predate the new Claims migration. The CI pipeline must generate fresh bundles from the committed migration files.

## 3. Target architecture

```text
Developer changes code / EF migration
                 |
                 v
        GitHub repository
                 |
                 v
       Azure DevOps CI pipeline
       Restore -> Build -> Test
                 |
                 v
       Build release artifacts
       - Claims EF migration bundle
       - Policy EF migration bundle
       - Claims.Api Docker image
       - Policy.Api Docker image
                 |
                 v
       Azure DevOps deployment stages
       Approval / environment checks
                 |
                 v
       Run database migrations first
       - Claims bundle -> ClaimsDb
       - Policy bundle  -> PolicyDb
                 |
          both successful?
                 |
                 v
       Deploy API container images
       - Claims.Api -> AKS via Helm
       - Policy.Api  -> AKS via Helm
                 |
                 v
       Smoke tests / health checks
                 |
                 v
       Release result and deployment logs
```

Each microservice owns and migrates only its own database. Database deployment is a separate release stage; API startup must not run production migrations. The API runtime identity should not have schema-owner permissions. The existing Container Apps deployment is retained as a comparison/baseline; the added container-orchestration learning track will deploy the same API images to AKS.

## 4. Implementation roadmap

| Phase | Workstream | Expected result | Status |
|---|---|---|---|
| 0 | Current-state assessment | Existing solution, databases, contexts and migrations documented | Completed |
| 1 | Azure SQL verification | Existing migration history checked in both databases | Completed |
| 2 | Local EF migration validation | EF CLI 10.0.13; local migration state checked | Completed |
| 3 | Migration artifacts preparation | Initial bundles/scripts generated; new Claims migration and reviewed idempotent SQL generated | In progress; fresh CI artifacts pending |
| 4 | Azure SQL connectivity and permissions | Firewall connectivity and database permissions checked | Basic checks completed; pipeline identity/network still pending |
| 5 | GitHub/Azure DevOps integration | Private Azure DevOps project connected to existing GitHub repository | Pending |
| 6 | Secure Azure service connection | Azure DevOps authenticates to Azure using workload identity federation where supported | Pending |
| 7 | CI pipeline | Restore, build, test, migration bundle generation and Docker build validation | Pending |
| 8 | ACR publishing | Versioned Claims, Policy and migration images published to ACR | Pending |
| 9 | Automated database deployment | Azure DevOps triggers dedicated migration execution for ClaimsDb and PolicyDb | Pending |
| 10 | API deployment | Deploy compatible API images to Azure Container Apps after database stage succeeds | Pending |
| 11 | Release verification | Migration history, schema, health checks, API smoke tests and logs verified | Pending |
| 12 | Failure handling and production readiness | Approval gates, recovery, expand/contract, monitoring and backup practices demonstrated | Pending |
| 13 | Docker packaging | Multi-stage Dockerfiles for Claims.Api and Policy.Api; local build/run and health validation | Pending |
| 14 | Kubernetes fundamentals | Pods, Deployments, Services, Namespaces, ConfigMaps, Secrets, probes and replicas practised | Pending |
| 15 | Helm packaging | Helm chart, values files per environment, upgrade/install and rollback practised | Pending |
| 16 | AKS and ACR integration | AKS cluster can pull versioned images from ACR using secure identity integration | Pending |
| 17 | AKS deployment pipeline | Azure DevOps deploys the approved image versions to AKS with Helm after DB stage succeeds | Pending |
| 18 | AKS validation and troubleshooting | Rollout, health probes, pod logs, image-pull failures, scaling and rollback demonstrated | Pending |
| 19 | Monitoring and observability | Metrics, logs, traces, dashboards and alerts for APIs, AKS and Azure SQL using Azure Monitor / Application Insights and OpenTelemetry; compare Prometheus/Grafana and commercial platforms | Pending |

## 5. Detailed execution plan

### Phase 5 – Create Azure DevOps project and connect GitHub

1. Sign in at https://dev.azure.com/ and create/select an Azure DevOps organization.
2. Create a private project named `InsuranceClaims` using Git version control and Agile work items.
3. Connect the existing GitHub `InsuranceClaims` repository as the pipeline source. Do not import or move the repository to Azure Repos.
4. Authorize the GitHub connection with the minimum required repository permissions.
5. Confirm Azure DevOps can read the default branch and create a pipeline from the repository.

**Completion:** Azure DevOps can read the existing GitHub repository.

### Phase 6 – Secure Azure connection and deployment identity

1. Create an Azure Resource Manager service connection in Azure DevOps.
2. Prefer workload identity federation (OIDC) over a client secret where supported.
3. Scope Azure RBAC permissions to the resources required by the pipeline (ACR push, Container Apps or AKS deployment as applicable, and the migration execution mechanism).
4. Keep database deployment permissions separate from API runtime database permissions.
5. Do not store SQL passwords, JWT signing keys, or other secrets in YAML or source control.
6. Decide the database deployment network path before enabling the release stage. Microsoft-hosted agents do not automatically have private access to the SQL server. Prefer a private/self-hosted agent or a dedicated Container Apps migration job with an approved network path. Do not open SQL firewall access broadly just to make the pipeline work.

**Completion:** Azure DevOps can deploy only to the intended Azure resources using short-lived/federated authentication where available.

### Phase 7 – CI pipeline

Create `azure-pipelines.yml` in the GitHub repository.

Pipeline stages:

1. Checkout GitHub source.
2. Install/use the .NET 10 SDK.
3. Restore NuGet packages.
4. Build the solution in Release configuration.
5. Run available automated tests; add tests if none currently exist.
6. Validate that both EF migration projects build.
7. Generate fresh Linux x64 self-contained migration bundles for `ClaimsDbContext` and `PolicyDbContext`.
8. Generate idempotent SQL scripts as review/audit artifacts.
9. Build Claims.Api and Policy.Api Docker images.
10. Publish bundles, SQL review scripts, test results and build metadata as pipeline artifacts; publish images to ACR in the appropriate release stage.

Suggested triggers:
- Pull requests: build, test and validate only; no Azure database deployment.
- Merge/push to `main`: produce immutable release artifacts.
- Deployment stages: use explicit environment approvals/checks.

**Completion:** A GitHub change triggers a successful build and produces versioned, traceable artifacts.

### Phase 8 – ACR image publishing

1. Reuse the existing ACR if it is still available and correctly configured.
2. Build separate images for Claims.Api and Policy.Api.
3. Tag images with the build ID and/or Git commit SHA; avoid relying only on mutable `latest` tags.
4. Build dedicated migration images containing the respective self-contained EF bundle if using Container Apps Jobs for execution.
5. Push images to ACR using the Azure DevOps service connection with least privilege.
6. Record the exact image digests/tags in the release artifacts.

**Completion:** API and migration images are available in ACR and traceable to a Git commit/build.

### Phase 9 – Automated database deployment

Use one controlled deployment mechanism. Preferred POC design: Azure DevOps orchestrates two dedicated, one-off Azure Container Apps migration jobs (or invokes the equivalent secured deployment runner) using the corresponding migration image/bundle.

1. Add a database deployment stage after CI and before API deployment.
2. Require the configured environment approval/check before applying database changes.
3. Execute Claims migration against ClaimsDb only.
4. Execute Policy migration against PolicyDb only.
5. Configure each job for one execution at a time, with bounded timeout and retry policy.
6. Use a dedicated migration identity with only the schema-change permissions needed for its database.
7. Pass configuration securely; do not print connection strings or secrets in logs.
8. Stop the release if either required migration fails. Inspect actual database state before retrying; do not blindly rerun a partially failed deployment.
9. Do not run migrations from API startup or from multiple API replicas.

**First automated deployment:** Commit `AddClaimDeploymentNotes` and its designer/snapshot files. The pipeline generates a fresh Claims migration bundle. Azure DevOps then triggers the Claims migration execution. The expected result is the nullable `DeploymentNotes` column and the new migration ID `20261113130307_AddClaimDeploymentNotes` in ClaimsDb history. This is the first real database deployment in this POC. Do not apply it manually beforehand.

**Completion:** Azure DevOps can apply the pending migration without manually executing SQL in SSMS.

### Phase 10 – Deploy APIs after database success (existing Container Apps track)

1. Make API deployment depend on successful completion of the required database migration stage.
2. Deploy the exact Claims.Api and Policy.Api image versions produced by the same release build.
3. Update Container Apps revisions rather than making untraceable in-place changes.
4. Configure environment-specific settings and secrets securely.
5. Use controlled traffic shifting/health validation where appropriate.
6. Do not deploy an API version that requires a schema change if that migration failed.

**Completion:** Application versions are deployed only after their compatible database changes succeed. This phase preserves the already-established Container Apps deployment as a working baseline.

### Phase 13 – Docker packaging (detailed)

1. Add a multi-stage Dockerfile for each API, using the .NET 10 SDK image for restore/build/publish and the smaller ASP.NET runtime image for execution.
2. Configure the container to listen on the expected HTTP port (verify the actual application configuration; do not assume the host port).
3. Keep connection strings, JWT signing keys and environment-specific values outside the image.
4. Build and run each image locally; verify startup, health endpoint and API connectivity.
5. Tag images with the pipeline build ID and/or Git commit SHA. Do not rely only on `latest`.

**Completion:** Both API images run successfully locally and are ready to publish to ACR.

### Phase 14 – Kubernetes fundamentals

Practise these concepts using the Claims.Api and Policy.Api images:

- Pod: the smallest deployable Kubernetes unit; a Pod runs one or more containers.
- Deployment: declares desired replicas and manages rollout/replace of Pods.
- Service: stable network endpoint for reaching Pods.
- Namespace: logical isolation/grouping for resources.
- ConfigMap: non-secret configuration.
- Secret: sensitive configuration; use a secure external secret integration for production rather than treating base64 encoding as encryption.
- Readiness/liveness probes, resource requests/limits, replica count and rolling updates.

**Completion:** Deploy a simple API workload and expose it inside the cluster with a Service.

### Phase 15 – Helm packaging

1. Create a Helm chart with `Chart.yaml`, `values.yaml`, Deployment and Service templates.
2. Parameterize image repository, immutable image tag, replica count, resource settings, probes and environment-specific configuration.
3. Keep development/test/production values separate; do not commit secrets.
4. Practise `helm upgrade --install`, checking rollout status and inspecting rendered manifests.
5. Practise `helm rollback` and explain what Helm rollback does and does not revert (it does not reverse database schema/data changes).

**Completion:** The APIs can be installed and upgraded repeatably using Helm values.

### Phase 16 – AKS and ACR integration

1. Create or reuse a dedicated AKS cluster only when ready to practise; check current pricing and delete the cluster/resources after the exercise to avoid ongoing charges.
2. Configure AKS-to-ACR image pull access using the supported Azure identity integration and least privilege. Do not enable the ACR admin account merely for convenience.
3. Connect to the cluster with an appropriately scoped identity and create a dedicated namespace such as `insuranceclaims`.
4. Deploy the Claims.Api and Policy.Api Helm releases using the versioned images already published to ACR.
5. Configure service-to-service connectivity, ingress/exposure and secrets deliberately; do not expose internal APIs publicly without a requirement.

**Completion:** AKS successfully pulls and runs both API images from ACR.

### Phase 17 – Azure DevOps to AKS deployment

1. Add an AKS deployment stage after the required database migration stage succeeds.
2. Use a secured Azure Resource Manager/Kubernetes connection and least-privilege deployment permissions.
3. Download the release metadata and deploy the exact ACR image tags/digests produced by that build.
4. Run Helm upgrade/install with the appropriate environment values.
5. Add environment approval/checks for higher environments.
6. Wait for rollout readiness and stop the release if the deployment or health validation fails.

**Completion:** A pipeline run can build, publish and deploy a traceable application version to AKS after the database stage.

### Phase 18 – AKS validation and troubleshooting

Practise and be able to explain:

- `kubectl get pods`, `get deployments`, `get services` and `describe pod`.
- `kubectl logs` and previous-container logs for crash/restart investigation.
- ImagePullBackOff (image name/tag, ACR permissions, network).
- CrashLoopBackOff (startup exception, configuration, secrets, health probes).
- Readiness/liveness probe failures and resource limits.
- `kubectl rollout status`, `rollout history` and `rollout undo`.
- Horizontal scaling and how Deployment replica count affects availability.

**Completion:** Demonstrate a successful rollout, diagnose one deliberately introduced failure, and recover safely.

### Phase 19 – Monitoring, logging and observability (industry tools)

Learn the three core telemetry signals and how they work together:

- **Logs:** discrete events and diagnostic details (for example, an API exception or failed SQL command).
- **Metrics:** numeric measurements over time (for example, request rate, error rate, latency, CPU, memory, pod restarts and database DTU/vCore utilisation).
- **Distributed traces:** one request's journey across API, downstream service, database and messaging dependencies, correlated with trace/span IDs.

#### Primary hands-on stack for this POC (Azure-first)

| Tool | What to practise |
|---|---|
| Azure Monitor | Central monitoring experience, metrics, alerts and action groups for Azure resources |
| Application Insights | .NET API request/dependency tracking, exceptions, performance, availability and distributed traces |
| Log Analytics Workspace | Centralise and query collected logs using Kusto Query Language (KQL) |
| Container Insights / Azure Monitor for containers | AKS node, pod and container health, resource usage, restarts and logs |
| Azure Managed Prometheus | Scrape and query Prometheus-compatible Kubernetes metrics |
| Azure Managed Grafana | Dashboards for AKS and Prometheus metrics |
| OpenTelemetry | Instrument .NET services and propagate W3C trace context; export telemetry to a supported backend |
| Azure SQL monitoring / Query Store | Review database health, query performance, waits and regressions; correlate slow database calls with API traces |

#### Other widely used industry platforms to recognise

These are alternatives or complementary platforms, not all required for this POC:

- **Datadog:** infrastructure, APM, logs, traces, dashboards and alerting.
- **Dynatrace:** full-stack observability, application performance and dependency analysis.
- **New Relic:** APM, distributed tracing, infrastructure and browser monitoring.
- **Elastic Observability (ELK/Elastic Stack):** log ingestion/search, metrics and APM.
- **Splunk Observability / Splunk Enterprise:** logs, metrics, traces and operational analytics.
- **Prometheus + Grafana:** common open-source metrics collection and dashboard combination, especially in Kubernetes environments.

#### Practical exercises

1. Enable Application Insights / Azure Monitor OpenTelemetry in Claims.Api and Policy.Api.
2. Send a request from Claims.Api to Policy.Api and verify end-to-end trace correlation.
3. Generate a controlled exception and a slow dependency; find both in Application Insights.
4. Use Log Analytics and KQL to query failures, request duration and dependency duration.
5. Enable AKS monitoring; inspect node/pod CPU, memory, restarts and container logs.
6. Create a dashboard for request rate, failure rate, latency, pod health and database health.
7. Create actionable alerts for elevated 5xx errors, high latency, pod restarts and resource saturation; route them through an action group.
8. Use Azure SQL Query Store / monitoring views to identify a slow query and relate it to the API dependency trace.
9. Add correlation IDs and ensure sensitive data, tokens and connection strings are not written to logs.

**Interview principle:** Know the difference between logs, metrics and traces; explain what you used, what problem each tool solves, and how you move from an alert to root cause. For this POC, implement the Azure-native stack first. Learn the Datadog, Dynatrace, New Relic, Elastic and Splunk names and use cases at comparison level only; do not install every platform.

**Completion:** Demonstrate one correlated request trace, one KQL investigation, one AKS dashboard and one actionable alert.

### Phase 11 – Verify the release

For ClaimsDb:

```sql
SELECT MigrationId, ProductVersion
FROM dbo.__EFMigrationsHistory
ORDER BY MigrationId;

SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Claims'
  AND COLUMN_NAME = 'DeploymentNotes';
```

Expected: the fourth Claims migration is recorded and `DeploymentNotes` is nullable. For PolicyDb, verify its existing migration history and schema remain as expected unless a Policy migration is intentionally included in the release.

Also verify:
- Migration job exit code and logs.
- Container App revision health.
- API health endpoints and authentication.
- Claims API read/write operations and Policy API reads.
- Application Insights traces/errors.
- Release/build ID and Git commit are traceable.

### Phase 12 – Failure handling and production readiness

Cover these scenarios:

| Scenario | Expected handling |
|---|---|
| Build or test fails | Stop before publishing/deploying release |
| Migration bundle generation fails | Stop release |
| SQL connectivity/authentication fails | Stop migration and API deployment; inspect identity/network |
| Migration fails | Inspect transaction, migration history and schema before retry |
| API deployment fails | Restore prior compatible Container Apps revision; keep schema compatibility in mind |
| Schema change is not backward compatible | Use staged expand-and-contract releases |
| Data loss or corruption risk | Use verified backup/restore plan or forward-fix as appropriate |

Production practices to demonstrate:
- Azure SQL backup and point-in-time restore awareness.
- Least-privilege identities and secretless authentication where supported.
- Environment approvals and deployment audit trail.
- Expand-and-contract schema evolution.
- Forward-fix preference when a down migration could destroy data written after deployment.
- Application Insights monitoring, alerts and post-deployment smoke tests.
- No assumption that rolling back an API image automatically rolls back a database schema.

## 6. Source-control checklist

Commit the pending Claims schema change and all EF-generated migration files:

- `Claims.Api/Models/Claim.cs` (or the actual file containing the Claim entity).
- `Claims.Api/Migrations/<timestamp>_AddClaimDeploymentNotes.cs`.
- `Claims.Api/Migrations/<timestamp>_AddClaimDeploymentNotes.Designer.cs`.
- Updated `ClaimsDbContextModelSnapshot.cs`.

Also commit the pipeline YAML and Dockerfile changes when introduced. Do not commit connection strings, passwords, signing keys, local user secrets, generated executable bundles, or local-only build output. Keep generated deployment artifacts in pipeline artifacts/ACR.

## 7. Execution tracker

- [x] Existing database ownership and DbContexts identified.
- [x] Existing Azure SQL migration history checked: ClaimsDb 3, PolicyDb 1.
- [x] EF CLI updated to 10.0.13; local migration list checked.
- [x] Initial Claims and Policy Linux x64 migration bundles generated (these predate the new Claims migration).
- [x] Initial idempotent SQL scripts generated and inspected.
- [x] Azure SQL client firewall rule added and TCP 1433 connectivity tested successfully.
- [x] SSMS `db_owner` membership verified in ClaimsDb and PolicyDb for the connected user.
- [x] New nullable `DeploymentNotes` property and migration `20261113130307_AddClaimDeploymentNotes` created.
- [x] Fresh `artifacts/claims-deployment-v2.sql` generated and reviewed; not executed.
- [ ] Commit the new Claims entity/migration files to GitHub.
- [ ] Create/select Azure DevOps organization and private `InsuranceClaims` project.
- [ ] Connect GitHub repository to Azure DevOps.
- [ ] Configure secure Azure service connection and deployment identity.
- [ ] Implement CI pipeline and tests.
- [ ] Generate fresh migration bundles and SQL review artifacts in CI.
- [ ] Publish versioned API and migration images to ACR.
- [ ] Configure secure network path for migration execution.
- [ ] Run the pending Claims migration through Azure DevOps-controlled deployment.
- [ ] Verify ClaimsDb schema and migration history.
- [ ] Deploy Claims.Api and Policy.Api after database stage succeeds.
- [ ] Run smoke tests and review monitoring.
- [ ] Demonstrate failure handling, approval gates and expand/contract strategy.
- [ ] Add multi-stage Dockerfiles for Claims.Api and Policy.Api; build and run both images locally.
- [ ] Publish versioned API images to ACR and record their build/commit traceability.
- [ ] Practise Kubernetes Pods, Deployments, Services, Namespaces, ConfigMaps, Secrets and probes.
- [ ] Create Helm chart and environment-specific values; practise upgrade and rollback.
- [ ] Configure AKS-to-ACR image pull access using secure identity integration.
- [ ] Deploy Claims.Api and Policy.Api to AKS using Helm.
- [ ] Add Azure DevOps AKS deployment stage after successful database deployment.
- [ ] Practise AKS rollout validation, pod logs, ImagePullBackOff and CrashLoopBackOff troubleshooting.
- [ ] Delete temporary AKS resources after the POC to control cost.
- [ ] Enable Application Insights / OpenTelemetry for both APIs and verify distributed trace propagation.
- [ ] Query API failures, latency and dependency duration in Log Analytics using KQL.
- [ ] Enable AKS Container Insights and inspect node/pod/container health and logs.
- [ ] Configure Azure Managed Prometheus and Azure Managed Grafana dashboards (if supported/available in the chosen subscription).
- [ ] Review Azure SQL Query Store and identify a slow query.
- [ ] Create and test actionable alerts for API errors/latency, pod restarts and resource saturation.
- [ ] Review Datadog, Dynatrace, New Relic, Elastic and Splunk at comparison level.

## 8. Interview preparation

After completing this POC, be prepared to explain:

1. How do you deploy database changes in a .NET microservices project?
2. How does EF Core track applied migrations?
3. What is the difference between `Database.Migrate()` and migration bundles?
4. Why should multiple API replicas not execute migrations at startup?
5. How do you automate database migrations using GitHub and Azure DevOps?
6. How do workload identity federation and managed identity reduce secret management?
7. How do you give a migration identity schema permissions without giving them to the API runtime identity?
8. What happens if a migration fails halfway through?
9. How do you coordinate database and API deployments?
10. How do you use expand-and-contract migrations for zero-downtime releases?
11. Why is database rollback different from application rollback?
12. How do you audit and trace a deployment to a Git commit and pipeline run?
13. What is the difference between a Docker image and a container?
14. Why use a multi-stage Dockerfile for a .NET API?
15. How do you tag and promote images through ACR?
16. What are Pods, Deployments and Services in Kubernetes?
17. How does AKS authenticate to ACR to pull images?
18. What is the role of Helm and its values files?
19. How do you perform a rolling deployment and rollback in AKS?
20. How do you troubleshoot ImagePullBackOff and CrashLoopBackOff?
21. What is the difference between Azure Container Apps and AKS, and when would you choose each?
22. What is the difference between logs, metrics and distributed traces?
23. How do Application Insights and OpenTelemetry help troubleshoot a .NET microservice?
24. What is a Log Analytics Workspace and how do you query it using KQL?
25. How do you monitor AKS nodes, pods, CPU, memory and restarts?
26. What are Prometheus and Grafana used for in Kubernetes monitoring?
27. How would you monitor Azure SQL query performance and correlate it with API latency?
28. Which observability platforms have you used or evaluated: Datadog, Dynatrace, New Relic, Elastic or Splunk?

## 9. Revised interview-focused effort – 7-hour crash plan

The full roadmap remains the reference for deeper implementation. To prepare efficiently for upcoming interviews, use this **time-boxed 7-hour path** first. It prioritises the ability to explain and demonstrate the end-to-end deployment flow rather than completing every optional production-hardening exercise.

| Session | Focus | Time | Minimum outcome |
|---|---|---:|---|
| 1 | EF Core migrations and automated Azure SQL deployment | 1.5 hrs | Explain migration generation, bundle/script, separate migration stage, identity/permissions, execution order and failure handling. Keep `AddClaimDeploymentNotes` pending for the pipeline. |
| 2 | GitHub + Azure DevOps CI/CD | 1 hr | Understand the YAML pipeline: restore, build, test, generate fresh migration artifacts, build/publish images, approvals and deployment stages. |
| 3 | Docker + ACR | 1 hr | Build and run one multi-stage .NET API image, explain image/container, tags and push/pull from ACR. |
| 4 | Kubernetes fundamentals | 1 hr | Explain Pod, Deployment, Service, Namespace, ConfigMap, Secret, probes, replicas and essential `kubectl` commands. |
| 5 | Helm + AKS deployment | 1 hr | Read a basic chart/values file; understand `helm upgrade`, rollout status, AKS-to-ACR permissions and rollback. Do not spend this session on advanced cluster setup. |
| 6 | Monitoring and observability | 1 hr | Explain logs, metrics and traces; practise one Application Insights trace and one KQL query. Know Container Insights, Prometheus/Grafana and commercial tools at comparison level. |
| 7 | End-to-end interview revision | 30 min | Explain the architecture from Git commit through database migration, image release, deployment, health checks and monitoring. Answer the interview questions in Section 8 aloud. |
| **Total** |  | **7 hrs** | **Interview-ready overview plus selected practical proof points** |

### What is intentionally deferred

To stay within seven hours, defer advanced AKS networking/cluster administration, complex Helm templating, full Prometheus/Grafana installation, ELK/Splunk setup, multiple environment automation, advanced rollback automation and exhaustive alert/dashboard creation. These remain useful follow-up learning, not prerequisites for the first interview-focused pass.

### Important scope note

Seven hours is a focused crash plan, **not a promise to complete and production-harden every phase**. Azure permissions, private SQL networking, agent configuration, provisioning AKS, image-pull issues or unexpected troubleshooting can extend the schedule. For interviews, clearly distinguish what you have personally implemented in the POC from what you understand conceptually. Do not claim production ownership of tools or deployments you have not operated.

Avoid provisioning paid resources until the relevant session requires them. AKS worker nodes and associated resources can incur charges; create a cluster only if needed for the practical exercise and delete temporary resources afterwards.

## 10. Immediate next step

Start with **Session 1 – EF Core migrations and automated Azure SQL deployment**. The immediate task is still **Phase 5 – Azure DevOps setup** after revising the migration flow, because the pending Claims migration must be deployed through the pipeline rather than manually.

1. Review the current state in Section 2, especially the pending `AddClaimDeploymentNotes` migration.
2. Be ready to explain how EF Core creates and tracks migrations, why a migration bundle/script is generated in CI, and why database deployment is a separate stage before API deployment.
3. Then open https://dev.azure.com/, check whether an organization already exists, create a private `InsuranceClaims` project only if needed, and connect the existing GitHub repository.
4. Confirm Azure DevOps can read the repository and its default branch.

Proceed one step at a time using the seven-hour schedule. Keep the detailed Phases 5–19 as reference material for later expansion. Do not execute `claims-deployment-v2.sql` manually; keep `AddClaimDeploymentNotes` pending for the first automated database deployment.
