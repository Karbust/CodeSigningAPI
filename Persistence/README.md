# Persistence Project

## Generate Migrations

```bash
dotnet ef --startup-project ../CodeSigningAPI/ migrations add InitialMigration
```

## Update Database

```bash
dotnet ef --startup-project ../CodeSigningAPI/ database update
```
