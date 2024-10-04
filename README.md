# records

## Generate a JWT Key
Following [https://www.rfc-editor.org/rfc/rfc7518#section-3](https://www.rfc-editor.org/rfc/rfc7518#section-3).
```bash
openssl ecparam -name secp384r1 -genkey -noout -out jwt_private_key.pem
```

## Migrations
```bash
cd ./src/backend/Records/Persistence
dotnet ef migrations add InitialMigration --project .\Persistence.csproj --startup-project ..\WebAPI\WebAPI.csproj 
dotnet ef database update --project .\Persistence.csproj --startup-project ..\WebAPI\WebAPI.csproj 
```