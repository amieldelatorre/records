# records

## Project Architecture
Trying to follow a clean architecture with a feature-based folder structure

## Generate a JWT Key
Following [https://www.rfc-editor.org/rfc/rfc7518#section-3](https://www.rfc-editor.org/rfc/rfc7518#section-3).
```bash
openssl ecparam -name secp384r1 -genkey -noout -out jwt_private_key.pem
```

## Migrations

### Prerequisites
Needs environment variables to be set.

#### Jetbrains Rider Terminal
1. Settings
2. Tools
3. Terminal
4. Environment Variables under the Project Settings section

### Commands
```bash
cd ./src/backend/Records/Persistence
dotnet ef migrations add InitialMigration --project Persistence.csproj --startup-project ../WebAPI/WebAPI.csproj 
dotnet ef database update --project ./Persistence.csproj --startup-project ../WebAPI/WebAPI.csproj 
```