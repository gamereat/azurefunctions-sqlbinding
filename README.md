# Overview

This project is created to provide a simple 
Microsoft SQL Server binding for Azure Functions.

# Supported features

The following commands are supported as of now.

## Input binding

The input binding can be used as like the following
piece of code.

```csharp
[FunctionName(nameof(GetSingleRecord))]
public static IActionResult GetSingleRecord(
	[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(GetSingleRecord))]
	HttpRequest req,
	[SqlServer(Query = "SELECT TOP 1 Id, Name, Description FROM MyTable")]
	SqlServerModel model)
{
	return new OkObjectResult(model.Record.Id);
}

[FunctionName(nameof(GetCollectionFromSqlServer))]
public static IActionResult GetCollectionFromSqlServer(
	[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(GetCollectionFromSqlServer))]
	HttpRequest req,
	[SqlServer(Query = "SELECT TOP 100 Id, Name, Description FROM MyTable")]
	IEnumerable<SqlServerModel> collection)
{
	return new OkObjectResult(collection.Select(m => m.Record.Id));
}
```

In order for this to work you need a `local.settings.json`
file with a proper connectionstring. If not specified
the default name `SqlServerConnectionString` will be used.

```json
{
	"IsEncrypted": false,
	"Values": {
		"AzureWebJobsStorage": "UseDevelopmentStorage=true",
		"FUNCTIONS_WORKER_RUNTIME": "dotnet"
	},
	"ConnectionStrings": {
		"SqlServerConnectionString": "Server=(localdb)\\myInstance;Database=MySqlBindingDatabase;Trusted_Connection=True",
		"providerName": "System.Data.SqlClient"
	}
}
```

### Using the Managed Identity

It's also possible to use the Managed Identity of your Function App.  
To do so, you need to create an implementation of the interface `AzureFunctions.SqlBinding.ISqlBindingTokenProvider` and register it in the service collection.

The implementation can look like the following sample.

```csharp
[assembly: FunctionsStartup(typeof(Startup))]
namespace FunctionApp
{
	public class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{
			builder.Services.AddTransient<ISqlBindingTokenProvider, SqlBindingTokenProvider>();
		}
	}
}

internal class SqlBindingTokenProvider : ISqlBindingTokenProvider
{
	public async Task<string> GetToken(CancellationToken cancellationToken)
	{
		var azureServiceTokenProvider = new AzureServiceTokenProvider();
		var accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://database.windows.net/", null, cancellationToken);
		return accessToken;
	}
}
```

If registered, the implementation of the `ISqlBindingTokenProvider` will be retrieved in the binding via the injected `IServiceProvider`. 
If no implementation has been registered, the functionality for retrieving an access token will be ommitted.
You can find a sample of the usage in the sample application of this repository.