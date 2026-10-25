# InsuranceClaims Configuration

The APIs contain only non-secret settings in `appsettings*.json`. Required secrets must come from environment variables, .NET user-secrets, or Azure Key Vault.

## Local Environment Variables

Set these in the terminal that starts each API. Both APIs must use the same JWT signing key for the local token-forwarding scenario.

```powershell
# Claims.Api
$env:ConnectionStrings__ClaimsDb = 'Server=.\SQLEXPRESS;Database=ClaimsDb;Trusted_Connection=True;TrustServerCertificate=True'
$env:Jwt__SigningKey = '<random-key-with-at-least-32-characters>'
dotnet run --project InsuranceClaims/Claims.Api

# Policy.Api (in a separate terminal)
$env:ConnectionStrings__PolicyDb = 'Server=.\SQLEXPRESS;Database=PolicyDb;Trusted_Connection=True;TrustServerCertificate=True'
$env:Jwt__SigningKey = '<the-same-random-key>'
dotnet run --project InsuranceClaims/Policy.Api
```

Environment-variable keys use `__` in place of the configuration `:` separator.

## Development User-Secrets

Each API has its own `UserSecretsId`. From the workspace root:

```powershell
dotnet user-secrets set --project InsuranceClaims/Claims.Api 'ConnectionStrings:ClaimsDb' 'Server=.\SQLEXPRESS;Database=ClaimsDb;Trusted_Connection=True;TrustServerCertificate=True'
dotnet user-secrets set --project InsuranceClaims/Claims.Api 'Jwt:SigningKey' '<random-key-with-at-least-32-characters>'

dotnet user-secrets set --project InsuranceClaims/Policy.Api 'ConnectionStrings:PolicyDb' 'Server=.\SQLEXPRESS;Database=PolicyDb;Trusted_Connection=True;TrustServerCertificate=True'
dotnet user-secrets set --project InsuranceClaims/Policy.Api 'Jwt:SigningKey' '<the-same-random-key>'
```

User-secrets load automatically only in the `Development` environment. They are stored outside the repository and are not encrypted, so they are a development convenience rather than a production secret store.

## Optional Azure Key Vault

Key Vault is loaded only when `KeyVault:Uri` is present. Set it with an environment variable:

```powershell
$env:KeyVault__Uri = 'https://<vault-name>.vault.azure.net/'
```

Use these Key Vault secret names; the provider maps `--` to `:`:

| Secret name                   | Configuration key            |
| ----------------------------- | ---------------------------- |
| `ConnectionStrings--ClaimsDb` | `ConnectionStrings:ClaimsDb` |
| `ConnectionStrings--PolicyDb` | `ConnectionStrings:PolicyDb` |
| `Jwt--SigningKey`             | `Jwt:SigningKey`             |

`DefaultAzureCredential` uses local developer credentials such as Azure CLI credentials during development. In Azure, enable a managed identity for the host and grant it the **Key Vault Secrets User** role scoped to the vault. No Key Vault is created by this project.
