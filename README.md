# records

## Migrations
```bash
cd ./src/backend/Records/Persistence
dotnet ef migrations add InitialMigration --project .\Persistence.csproj --startup-project ..\WebAPI\WebAPI.csproj 
dotnet ef database update --project .\Persistence.csproj --startup-project ..\WebAPI\WebAPI.csproj 
```