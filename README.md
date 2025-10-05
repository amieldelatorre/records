# records

## Project Architecture
Trying to follow a clean architecture with a feature-based folder structure

## Generate a JWT Key
Following [https://www.rfc-editor.org/rfc/rfc7518#section-3](https://www.rfc-editor.org/rfc/rfc7518#section-3).
```bash
openssl ecparam -name secp384r1 -genkey -noout -out jwt_private_key.pem
```

## OpenTelemetry Setup
Useful environment variables
```bash
OTEL_EXPORTER_OTLP_ENDPOINT
OTEL_EXPORTER_OTLP_TRACES_ENDPOINT
OTEL_BSP_SCHEDULE_DELAY
OTEL_EXPORTER_OTLP_METRICS_ENDPOINT
OTEL_METRIC_EXPORT_INTERVAL
```

## Development Environment Setup

### JetBrains Rider Docker Configuration
1. Click on the configuration on the top right
2. Edit Configurations...
3. In the Run section, click on Modify, then click on Environment variables and add the needed values
5. In the Run section, click on Modify, then click on Run options and add the network of the PostgreSQL container with `--network <app-db-network>`


## Migrations

### Prerequisites
Needs environment variables to be set.

#### JetBrains Rider Terminal
1. Settings
2. Tools
3. Terminal
4. Environment Variables under the Project Settings section

### Performing Migrations Through the Terminal
```bash
cd ./src/backend/Persistence
dotnet ef migrations add InitialMigration --project Persistence.csproj --startup-project ../WebAPI/WebAPI.csproj 
dotnet ef database update --project ./Persistence.csproj --startup-project ../WebAPI/WebAPI.csproj 
```

### Creating a SQL Migrations Script
```bash
cd ./src/backend/Persistence
dotnet ef migrations script --project ./Persistence.csproj --startup-project ../WebAPI/WebAPI.csproj --idempotent --output migrations.sql

```

## Performance/Load Testing
```bash
# docker network mode host when running the API in a container on the same host
docker run --rm --network host -v ./scripts/k6/test_api_weightentry_get.js:/var/lib/test.js grafana/k6 run /var/lib/test.js # --env HOST=http://localhost:8080 --env URL_PATH=/api/v1/weightentry --env RPS_TARGET=1000
```

### Issues

#### Password Hashing Bottleneck
From profiling and testing it seems like the application becomes CPU bottlenecked when the User Create end point is called. After profiling, it seems to be caused by the [Pbkdf2PasswordHasher](./src/backend/Application/Features/PasswordFeatures/Pbkdf2PasswordHasher.cs).

In particular the `Rfc2898DeriveBytes.Pbkdf2` call is really expensive on the CPU.
