# CyShop

## Local Development Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started)

### Getting Started

1. Start the infrastructure services:

   ```bash
   docker compose -f Docker/docker-compose.yml up -d
   ```

2. Run the database migrator:

   ```bash
   dotnet run --project src/CyShop.DbMigrator
   ```

3. Run the Aspire AppHost:

   ```bash
   dotnet run --project src/CyShop.AppHost
   ```
