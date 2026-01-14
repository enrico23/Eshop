# Keycloak Realm Contract

This document defines the **expected 
Keycloak end state** for this demo.

It is a **declarative reference**, independent of:
- Manual setup steps
- Automation scripts
- Realm export JSON format

Its purpose is to describe **what must exist**, not how it is created.

---

## Realm

| Property | Value |
|--------|------|
| Name | `eshop` |
| Enabled | true |

---

## Clients

### catalog-api

| Property | Value |
|--------|------|
| Protocol | OpenID Connect |
| Client authentication | Off |
| Authorization services | Off |
| Standard flow | Off |

#### Client Roles
- `catalog.read`
- `catalog.write`

---

### blazor-ui

| Property | Value |
|--------|------|
| Protocol | OpenID Connect |
| Client authentication | On (confidential) |
| Authorization flow | Authorization Code |
| PKCE | S256 |

#### Redirect URIs
- `https://localhost:7173/signin-oidc`
- `http://localhost:5174/signin-oidc`

#### Post Logout Redirect URIs
- `https://localhost:7173/`
- `http://localhost:5174/`

#### Default Scopes
- `openid`
- `profile`
- `email`
- `blazor_api_audience`

---

## Client Scopes

### blazor_api_audience

| Property | Value |
|--------|------|
| Type | Default |
| Protocol | OpenID Connect |

#### Protocol Mappers

**Audience Mapper**
- Audience: `catalog-api`

---

## Token Requirements

Access tokens issued to authenticated users **must contain**:

- Audience: `catalog-api`
- Claim:  
  `resource_access.catalogapi.roles[]`

---

## Authority Rules

If discrepancies exist between:
- This document
- Manual setup
- Realm export JSON
- Automation scripts

**This contract is authoritative.**
