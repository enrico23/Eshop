# Authentication & Authorization Integration

This document describes how application components integrate with
the identity configuration defined in the Keycloak documentation.

It covers:
- Catalog API authentication and authorization
- Blazor UI authentication
- Access token forwarding

This document is **independent** of Keycloak setup mechanics and
assumes that the Keycloak realm already exists and conforms to the
documented contract.

---

## Prerequisites

Before applying this configuration:

- The Keycloak realm **must exist**
- Clients and scopes must match the documented Keycloak contract
- Required client roles must be present

Refer to:
- `docs/keycloak/keycloak-realm-contract.md`

---

## Keycloak Volume Reset Behavior (Important)

Keycloak realm and client configuration is restored from a repository-managed import file at container startup.

This results in **partial restoration** when the Keycloak data volume is deleted.

### Restored automatically
- Realm definition
- Clients and client roles
- Scopes and protocol configuration

### Not restored
- Users
- Client secrets used by consuming applications

### Operational impact
- After a Keycloak volume reset, client secrets are regenerated
- Applications using confidential clients will fail authentication with
  `invalid_request` / `Authentication failed`
- The Blazor UI is affected because it reads the client secret from **user secrets**

### Required manual steps after volume deletion
1. Open Keycloak Admin Console
2. Navigate to:  
   Realm → Clients → `blazor-ui` → **Credentials**
3. Copy or regenerate the client secret
4. Update Blazor user secrets:
   ```bash
   dotnet user-secrets set "Keycloak:ClientSecret" "<new-secret>"

## Architecture Overview

- **Blazor UI**
  - Interactive login using OpenID Connect
  - Authorization Code flow with PKCE
  - Issues access tokens scoped for the Catalog API

- **Catalog API**
  - JWT bearer authentication
  - Validates issuer, audience, lifetime, and signing keys
  - Enforces authorization via client roles

- **YARP Gateway**
  - No authentication or authorization
  - Pure request forwarding

---

## Catalog API Configuration

`Program.cs`

```csharp
builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer(
        serviceName: "keycloak",
        realm: "eshop",
        options =>
        {
            options.Audience = "catalogapi";

            if (builder.Environment.IsDevelopment())
            {
                options.Authority = "http://localhost:8080/realms/eshop";
                options.RequireHttpsMetadata = false;
            }

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };
        });

builder.Services.AddAuthorization();
```


## Blazor UI Configuration

`Program.cs`

```csharp
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddKeycloakOpenIdConnect(
        serviceName: "keycloak",
        realm: "eshop",
        options =>
        {
            options.ClientId = "blazor-ui";
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.ClientSecret = builder.Configuration["Keycloak:ClientSecret"];
            options.SaveTokens = true;

            if (builder.Environment.IsDevelopment())
            {
                options.Authority = "http://localhost:8080/realms/eshop";
                options.RequireHttpsMetadata = false;
            }

            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.Scope.Add("blazor_api_audience");
        });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

```

## Access Token Forwarding to the API

### HttpClient Registration

```csharp
builder.Services.AddTransient<AccessTokenHandler>();

builder.Services.AddHttpClient<ICatalogApiClient, CatalogApiClient>()
    .AddHttpMessageHandler<AccessTokenHandler>();

```

### Delegating Handler

```csharp
public sealed class AccessTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccessTokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _httpContextAccessor.HttpContext?
            .GetTokenAsync("access_token");

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}


