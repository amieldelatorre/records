# records

## Project Architecture
Trying to follow a clean architecture with a feature-based folder structure

## Generate a JWT Key
Following [https://www.rfc-editor.org/rfc/rfc7518#section-3](https://www.rfc-editor.org/rfc/rfc7518#section-3).
```bash
openssl ecparam -name secp384r1 -genkey -noout -out jwt_private_key.pem
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