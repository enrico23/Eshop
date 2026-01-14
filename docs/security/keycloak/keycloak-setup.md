# Keycloak Setup Manual

This document describes the **manual process** 
to create and configure the Keycloak identity 
resources required by this demo.

The procedure is intended to be:

- Deterministic
- Reproducible
- Suitable for comparison with a Keycloak realm export (`realm.json`)

---

## Scope

This document covers **Keycloak configuration only**:

- Realm
- Clients
- Client roles
- Client scopes
- Protocol mappers
- Test Users
- Retrieve `blazor-ui` client secret

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

## 2. Create the API client: `catalog-api`

This client represents the **resource server** (Catalog API). 
We will create **client roles** on it.

### 2.1 Create client

1. Left menu → **Clients** → **Create client**
2. Set:
   - **Client type**: OpenID Connect
   - **Client ID**: `catalogapi`
3. Click **Next**

### 2.2 Configure Capabilities

Set:

- **Client authentication**: **Off**  
  (API does not log in; it only validates JWTs)
- **Authorization**: **Off**  
  (unless you intend to use Keycloak Authorization Services; not required here)
- **Standard flow**: **Off**
- **Direct access grants**: **Off**
- **Service accounts**: **Off**

Click **Save**.

> Note  
> The API does not “log in” to Keycloak; it only validates JWTs. So you don’t need secrets, flows, etc.

### 2.3 Create client roles (required by your API policies)

1. Clients → **catalog-api** → tab **Roles**
2. Create role: `catalog.read`
3. Create role: `catalog.write`

---

## 3. Create Client Scope and “Audience” mapper

This ensures the Blazor UI access token includes the correct audience for the API.

1. Left menu → **Client scopes** → **Create client scope**
2. Set:
   - **Name**: `blazor_api_audience`
   - **Type**: `Default`
   - **Protocol**: OpenID Connect
3. Click **Save**

### 3.1 Add Audience Mapper

1. Open client scope `blazor_api_audience`
2. Go to **Protocol mappers** → **Add mapper**
3. Mapper type: **Audience**
4. Audience: `catalogapi`
5. Save

---

## 4. Create the Blazor UI client: `blazor-ui`

This is the interactive login client.

### 4.1 Create client

1. Clients → **Create client**
2. Set:
   - **Client type**: OpenID Connect
   - **Client ID**: `blazor-ui`
3. Click **Next**

### 4.2 Configure capabilities

Set:

- **Client authentication**: **On** (confidential)
- **Standard flow**: **On**
- **Direct access grants**: **Off**
- **Service accounts**: **Off**

Click **Save**.

### 4.3 Configure login settings (redirects)

In client **blazor-ui** → **Settings**, configure:

**Valid redirect URIs**
- `https://localhost:7173/signin-oidc`
- `http://localhost:5174/signin-oidc` (only if you actually use HTTP)

**Valid post logout redirect URIs**
- `https://localhost:7173/`
- `http://localhost:5174/`

**Web origins**
- `https://localhost:7173`
- `http://localhost:5174`

Click **Save**.

### 4.4 Ensure PKCE is accepted

Most modern Keycloak setups accept PKCE 
by default for Standard Flow. If you 
see PKCE-related errors, check:

- Client → **Advanced / Login settings** → **PKCE method** (wording varies by Keycloak version)

Use: **S256** (the app uses S256)

### 4.5 Assign client scope mapper

1. In client **blazor-ui**, go to **Client scopes** tab
2. Click **Add client scope**
3. Add `blazor_api_audience` to **Default client scopes**
4. Click **Save**

---

## 5. Create a test user and assign the API roles

This step ensures the token issued to the user contains:

`resource_access.catalogapi.roles = [ "catalog.read", ... ]`

### 5.1 Create a user

1. Realm `eshop` → **Users**
2. Click **Create new user**
3. Set:
   - **Username**: `alice` (or any name)
   - **Enabled**: ON
4. Click **Create**

### 5.2 Set a password

1. Open the user
2. Go to **Credentials**
3. Set a password
4. Set **Temporary = OFF** (so you can log in without forced reset)
5. Click **Save**


### 5.3 Assign `catalog-api` client roles to the user

1. Open user → **Role mapping**
2. In “Assign role”:
   - Switch from **Realm roles** to **Client roles**
   - Select client: `catalogapi`
3. Assign:
   - `catalog.read`
   - (optionally) `catalog.write`
4. Click **Save**

---

## 6. Retrieve Blazor UI client secret (user-secrets)

Before leaving Keycloak, retrieve 
the client secret for **blazor-ui**:

1. Realm `eshop` → Clients → **blazor-ui** → **Credentials** tab
2. Copy **Client Secret**
