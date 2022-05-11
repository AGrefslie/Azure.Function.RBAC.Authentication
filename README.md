# Azure.Function.RBAC.Authentication

Methods for authentication AzureFunction http-triggers (Or any other contexts where a httprequestmessage is available).


## Authentication

Add nuget
- Azure.Function.RBAC.Authentication

Requierd section in appsetings 
```json
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "Tenant id",
    "ClientId": "Client id",
    "RoleClaimType": "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
  }
```

If Azure function use the following
Double underscore (__) creates config hierarchy
```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "AzureAd__Instance":"https://login.microsoftonline.com/",
        "AzureAd__TenantId": "Tenant id",
        "AzureAd__ClientId": "Client id",
        "AzureAd__RoleClaimType": "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    }
}
```
### Startup
Add using in startup file
```csharp
using Azure.Function.RBAC.Authentication;
```
Add authentication to services
```csharp
services.AddJwtTokenValidator(Configuration); //IConfiguration
```

### Setup
Create a class that spessifies valid roles.
```csharp
public class AuthPolicy
{
    public static string[] ValidRoles = { "Role" };
}
```

### Function
Inject the interface IAuthentication through the constructor
```csharp
using Azure.Function.RBAC.Authentication;

private readonly IAuthentication _authentication;

public MyClass(IAuthentication authentication)
{
    _authentication = authentication;
}
```

This interface has to ways of validating the token, AuthenticateTokenWithRolesAsync and AuthenticateAsync. Both methods return true
or false depending if the token is valid or not. Both methods are validating the following:

- Valid Audience
- Valid Issuer
- Signing Keys
- Token Lifetime

To validate token with roles, add the following to the first line of your function.

```csharp
public async Task<HttpResponseMessage> MyEndpoint(
[HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestMessage request, CancellationToken cancellationToken, ILogger logg)
{
    if (!await _authentication.AuthenticateTokenWithRolesAsync(request, AuthPolicy.ValidRoles, cancellationToken))
    {
        return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("Authorization header not set or access token not valid.") };
    }
}
```

To validate a token where role based accesscontrol is not required.
```csharp
public async Task<HttpResponseMessage> MyEndpoint(
[HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestMessage request, CancellationToken cancellationToken, ILogger logg)
{
    if (!await _authentication.AuthenticateAsync(request, cancellationToken))
    {
        return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("Authorization header not set or access token not valid.") };
    }
}
```