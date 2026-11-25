# Running MarketplaceApi

## Requirements

- .NET 10 SDK

## Start the API

Open a terminal in the folder containing `MarketplaceApi.sln` and run:

```powershell
dotnet run --project .\MarketplaceApi\MarketplaceApi.csproj --launch-profile https
```

The first run restores the required packages, creates the SQLite database, applies migrations, and adds demo data. Later runs keep the existing data.

Open Scalar in your browser to explore and test the API:

https://localhost:7235/scalar

If your browser does not trust the local HTTPS certificate, run:

```powershell
dotnet dev-certs https --trust
```

## Demo login

Use `POST /api/auth/login` with:

**Ola**

```json
{
  "email": "ola@brukt.no",
  "password": "MarketplaceDemo1!"
}
```

**Kari**

```json
{
  "email": "kari@brukt.no",
  "password": "MarketplaceDemo1!"
}
```

**Per**

```json
{
  "email": "per@brukt.no",
  "password": "MarketplaceDemo1!"
}
```

**Admin**

```json
{
  "email": "admin@brukt.no",
  "password": "MarketplaceDemo1!"
}
```

Use the returned `accessToken` as a bearer token in Scalar to access protected endpoints.

Press `Ctrl+C` in the terminal to stop the API.

## Run the tests

From the same folder as `MarketplaceApi.sln`, run:

```powershell
dotnet test .\MarketplaceApi.Tests\MarketplaceApi.Tests.csproj
```

The tests use an in-memory SQLite database, so they do not affect `marketplace.db`. The API does not need to be running.
