# Deployment Instructions

This document explains how to deploy the Cinema Booking web application to Azure using the provided Bicep files, and how to switch it from SQLite to the Azure SQL Database.

---

## Prerequisites

| Tool | Minimum version |
|------|----------------|
| [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) | 2.50+ |
| [Bicep CLI](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/install) | 0.22+ (bundled with Azure CLI 2.50+) |
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 |
| Azure subscription with Contributor rights | — |

---

## Step 1 – Log in to Azure

```bash
az login
az account set --subscription "<YOUR_SUBSCRIPTION_ID>"
```

---

## Step 2 – Create a Resource Group

```bash
az group create \
  --name rg-cinemabooking \
  --location uksouth
```

---

## Step 3 – Deploy the Azure SQL Database

```bash
az deployment group create \
  --resource-group rg-cinemabooking \
  --template-file infra_azure/sql.bicep \
  --parameters sqlAdminPassword="<STRONG_PASSWORD>" prefix="cinemabooking"
```

Note the **`sqlServerFqdn`** and **`sqlDatabaseName`** values from the deployment output.

---

## Step 4 – Run the Schema Script Against Azure SQL

Use the Azure Portal Query Editor, Azure Data Studio, or `sqlcmd`:

```bash
sqlcmd \
  -S <sqlServerFqdn> \
  -d CinemaBookingDb \
  -U cinemabooking_admin \
  -P "<STRONG_PASSWORD>" \
  -i infra_azure/azuresql_schema.sql
```

This creates all tables, stored procedures and seeds dummy data.

---

## Step 5 – Build & Publish the Web App

```bash
cd AccessApp
dotnet publish -c Release -o ../publish
```

---

## Step 6 – Deploy the App Service

```bash
# Build connection string from Step 3 outputs
SQL_FQDN="<sqlServerFqdn>"
SQL_CONN="Server=tcp:${SQL_FQDN},1433;Initial Catalog=CinemaBookingDb;Persist Security Info=False;User ID=cinemabooking_admin;******;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

az deployment group create \
  --resource-group rg-cinemabooking \
  --template-file infra_azure/webapp.bicep \
  --parameters sqlConnectionString="$SQL_CONN" prefix="cinemabooking"
```

Note the **`webAppName`** and **`webAppUrl`** from the output.

---

## Step 7 – Deploy the Application Code

```bash
cd publish
zip -r ../cinemabooking.zip .
cd ..

az webapp deploy \
  --resource-group rg-cinemabooking \
  --name <webAppName> \
  --src-path cinemabooking.zip \
  --type zip
```

The app will be live at the `webAppUrl` printed in Step 6.

---

## Azure Settings Reference

| Setting | Where to configure |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | App Service → Configuration → Application settings |
| `ConnectionStrings__DefaultConnection` | App Service → Configuration → Connection strings (type: SQLAzure) |
| SQL Server firewall | Azure Portal → SQL Server → Networking → Firewall rules |

---

## Switching from SQLite to Azure SQL (Coding Agent Prompt)

After you have tested the SQLite version locally and deployed the Azure SQL Database, run the following prompt inside this repository using the Copilot Coding Agent:

---

> **Prompt to give the Coding Agent:**
>
> Rebuild the `AccessApp` ASP.NET Razor Pages application so that it targets the Azure SQL Database instead of SQLite.
>
> Requirements:
> 1. Replace the `Microsoft.Data.Sqlite` NuGet package with `Microsoft.Data.SqlClient`.
> 2. Rewrite `AccessApp/Data/CinemaDb.cs` to use `SqlConnection` and `SqlCommand` instead of `SqliteConnection` and `SqliteCommand`.
> 3. Change all database interactions to call the stored procedures defined in `infra_azure/azuresql_schema.sql` using `CommandType.StoredProcedure` — do **not** write any inline T-SQL.
> 4. Update `appsettings.json` to use the Azure SQL connection string format (the value will be supplied via the App Service connection string setting at runtime — use the key `DefaultConnection`).
> 5. Remove the `InitialiseDatabase()` and `SeedData()` calls from `Program.cs` (schema/seed is managed by the SQL script).
> 6. Ensure the app still builds with `dotnet build -c Release` and no errors.
> 7. Do not change any Razor Pages (`.cshtml` / `.cshtml.cs` files) — only the data layer and project file need updating.
