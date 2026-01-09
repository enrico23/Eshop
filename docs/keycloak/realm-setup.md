# Keycloak Setup Manual

This document describes the **manual process** to create and configure the Keycloak identity resources required by this repository.

It is intended to be **deterministic**, reproducible, and suitable for:

* initial setup
* recovery after environment reset
* comparison with realm export JSON

The scope of this document is **Keycloak configuration only**:

* Realm
* Clients
* Client roles
* Client scopes

User seeding and application configuration are intentionally out of scope.

---

## 1. Create the Realm

1. Open the **Keycloak Admin Console**.
2. In the top-left realm selector, click **Create realm**.
3. Set:

   * **Realm name**: `eshop`
4. Click **Create**.

**Result**

* Realm `eshop` exists and is selected.

---

## 2. Create the UI Client (`blazorui`)

This client is used for **interactive user login** via OpenID Connect (Authorization Code flow).

### 2.1 Create the client

1. Navigate to **Clients** → **Create client**.
2. Set:

   * **Client type**: OpenID Connect
   * **Client ID**: `blazorui`
3. Click **Next**.

### 2.2 Configure client capabilities

Set the following:

* **Client authentication**: ON (confidential client)
* **Standard flow**: ON
* **Direct access grants**: OFF
* **Service accounts**: OFF

Click **Save**.

### 2.3 Configure redirect and origin settings

In the **Settings** tab of the `blazorui` client:

**Valid redirect URIs**

```
https://localhost:7173/signin-oidc
```

(For local debugging only, a wildcard such as `https://localhost:7173/*` may be used temporarily.)

**Web origins**

```
https://localhost:7173
```

Click **Save**.

### 2.4 Client secret

1. Open the **Credentials** tab.
2. Copy the generated **Client Secret**.

This value must be supplied to the Blazor application configuration.

**Result**

* `blazorui` can authenticate users interactively.

---

## 3. Create the API Client (`catalogapi`)

This client represents the protected API. It does **not** perform interactive login.

### 3.1 Create the client

1. Navigate to **Clients** → **Create client**.
2. Set:

   * **Client type**: OpenID Connect
   * **Client ID**: `catalogapi`
3. Click **Next**.

### 3.2 Configure client capabilities

Set **all** of the following to OFF:

* Client authentication
* Standard flow
* Direct access grants
* Service accounts

Click **Save**.

**Result**

* `catalogapi` exists as a resource server identifier only.

---

## 4. Create Client Roles for `catalogapi`

These roles are consumed by the API authorization policies.

1. Navigate to **Clients** → **catalogapi**.
2. Open the **Roles** tab.
3. Create the following roles:

* `catalog.read`
* `catalog.write`

**Result**

* Roles exist under `catalogapi`.

---

## 5. Create Client Scope for API Audience

The API validates access tokens with:

```
aud = catalogapi
```

To ensure tokens issued via `blazorui` include this audience, a client scope with an **Audience mapper** is required.

### 5.1 Create the client scope

1. Navigate to **Client scopes** → **Create client scope**.
2. Set:

   * **Name**: `catalog-audience`
   * **Type**: Default
   * **Protocol**: OpenID Connect
3. Click **Create**.

### 5.2 Add Audience protocol mapper

1. Open the `catalog-audience` client scope.
2. Navigate to **Protocol mappers** → **Add mapper** → **By configuration**.
3. Select **Audience**.
4. Set:

   * **Name**: `aud-catalogapi`
   * **Included Client Audience**: `catalogapi`
   * **Add to access token**: ON
5. Save.

---

## 6. Assign Client Scope to `blazorui`

1. Navigate to **Clients** → `blazorui`.
2. Open **Client scope assignments**.
3. Add `catalog-audience` to **Default client scopes**.
4. Save.

**Result**

* Access tokens issued via `blazorui` include `catalogapi` in the `aud` claim.

---

## 7. Expected Token Shape (Validation Reference)

After successful login through `blazorui`, the **access token** must contain:

### Audience

```json
"aud": ["blazorui", "catalogapi"]
```

### Client roles

```json
"resource_access": {
  "catalogapi": {
    "roles": ["catalog.read"]
  }
}
```

If both are present, API authentication and authorization will succeed.

---

## 8. Notes on Export / Import

* Realm export may include:

  * realm configuration
  * clients
  * client scopes
  * roles
* Users are often **not** included and should be seeded separately.
* Client secrets must be validated after import.

---

## 9. Summary

| Item         | Value                           |
| ------------ | ------------------------------- |
| Realm        | `eshop`                         |
| UI client    | `blazorui`                      |
| API client   | `catalogapi`                    |
| Client roles | `catalog.read`, `catalog.write` |
| Client scope | `catalog-audience`              |
| Audience     | `catalogapi`                    |

This document defines the authoritative Keycloak configuration for this repository.
