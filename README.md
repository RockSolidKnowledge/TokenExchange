# OAuth Token Exchange (RFC 8693) for IdentityServer

An implementation of OAuth token exchange ([RFC 8693](https://www.rfc-editor.org/rfc/rfc8693.html)) for IdentityServer4 and Duende IdentityServer. This implementation provides the required abstractions for token exchange with extensibility points to implement your own authorization rules, with default implementation covering an API to API scenario.

This library includes:

- Implementation of IExtensionGrantValidator
- Token exchange request parsing
- Extensibility points for request validation (defaults to subject token validation)
- Extensibility points for claims parsing (defaults to actor claim generation)

To learn more about token exchange and why it is useful for microservices and API gateways, check out "[Delegation patterns for OAuth](https://www.scottbrady91.com/OAuth/Delegation-Patterns-for-OAuth-20)".

## Installation

This library is available as a nuget package for both IdentityServer4 and Duende IdentityServer.

- [Rsk.TokenExchange.DuendeIdentityServer](https://www.nuget.org/packages/Rsk.TokenExchange.DuendeIdentityServer/)
- [Rsk.TokenExchange.IdentityServer4](https://www.nuget.org/packages/Rsk.TokenExchange.IdentityServer4/)

Once you have installed the correct nuget package, you can enable the token exchange grant type by using the `AddTokenExchange` extension on `IdentityServerBuilder`.

```csharp
services.AddIdentityServer()
  // existing registrations
  .AddTokenExchange();
```

This will register the necessary extension grant validator and services, defaulting to the API to API token exchange implementation.

## Usage

To configure a client application to use token exchange, you’ll need to allow it to use the `urn:ietf:params:oauth:grant-type:token-exchange` grant type. It will also need the usual configuration for apps that talk to the token endpoint, such as a client secret and allowed scopes.

```csharp
new Client
{
    ClientId = "api1",
    ClientSecrets = new[] {new Secret("secret".Sha256())},
    AllowedGrantTypes = new[] {"urn:ietf:params:oauth:grant-type:token-exchange"},
    AllowedScopes = new[] {"api2"}
};
```

A typical token exchange request looks like this (unencoded):

```HTTP
POST /connect/token HTTP/1.1
 Host: demo.identityserver.com
 Content-Type: application/x-www-form-urlencoded

 grant_type=urn:ietf:params:oauth:grant-type:token-exchange
 &scope=api2
 &client_id=api1
 &client_secret=secret
 &subject_token=accVkjcJyb4BWCxGsndESCJQbdFMogUC5PbRDqceLTC
 &subject_token_type=urn:ietf:params:oauth:token-type:access_token
```

### Supporting the actor (act) claim

To support the actor claim in generated tokens, you will need to update your IdentityServerprofile service to include this claim type. This can be done using `IssuedClaims`, to save updating your scopes.

## Default implementation – API to API delegation

Our default implementation is designed for API gateways and microservices that receive an access token authorized by a user and need to call another API on behalf of that user.

Token exchange allows you to request a new token, still acting on behalf of the original user but scoped to a different audience. This enables you to maintain the principle of least privilege for your client applications and, thanks to the actor claim, build up an audit trail of the original requester.

### Request validation

The default implementation performs these validation steps:

1. Validates the incoming subject token, confirming IdentityServer has created it
2. Validates that the requesting client application is either an intended audience of the token or the original client who requested it.

You can override this by registering your own implementation of `ITokenExchangeRequestValidator`.

### Claims parsing

The default implementation for claim generation:

- looks for a single subject (sub) claim
- parses an actor (act) claim based on the original client ID (client_id) and any existing actor claims
- passes through all other claims to your profile service.

You can override this by registering your own implementation of `ITokenExchangeClaimsParser`.

### Example tokens

Original token payload, requested by "app" to talk to "api1":

```json
{
  "iss": "http://localhost",
  "sub": "123xyz",
  "client_id": "app",
  "aud": "api1",
  "scope": [
    "api1"
  ],
  "nbf": 1625215449,
  "exp": 1625219049,
  "iat": 1625215449,
}
```

Exchanged token payload, requested by "api1" to talk to "api2", on behalf of "app" and the original user:

```json
{
  "iss": "http://localhost",
  "sub": "123xyz",
  "client_id": "app",
  "aud": "api2",
  "scope": [
    "api2"
  ],
  "act": {
    "client_id": "api1"
  },
  "nbf": 1625215465,
  "exp": 1625219065,
  "iat": 1625215465,
}
```

## Future

We also have an implementation for multi-tenant token exchange, where a tenanted token can be swapped for a tenantless token. This can be useful for calling untenanted microservices. We will be open-sourcing this in the future.

If you have any feature requests, please reach out in the issue tracker; otherwise, get in contact at [identityserver.com](https://www.identityserver.com).
