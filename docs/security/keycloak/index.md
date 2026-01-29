# Keycloak Configuration

This directory contains all documentation related to **Keycloak configuration** for this repository.

Keycloak is used as the **identity provider** for:
- Interactive user authentication (Blazor UI)
- Access token issuance
- API authorization via JWT validation

The YARP gateway is intentionally **not secured** and does not participate in authentication or authorization.

---

## Document Overview

| Document | Purpose |
|--------|--------|
| `keycloak-setup-manual.md` | Step-by-step manual configuration using the Keycloak Admin Console |
| `keycloak-realm-contract.md` | Declarative definition of the expected Keycloak end state |
| `scripts/README.md` | Operational documentation for environment reset and cleanup scripts |

---

## Which Document Should I Read?

- **Manual setup or audit** → `keycloak-setup-manual.md`
- **Verify correctness or compare with realm export JSON** → `keycloak-realm-contract.md`
- **Reset or rehydrate an environment** → `scripts/README.md`

---

## Scope and Guarantees

These documents define the **authoritative Keycloak configuration** for this repository.

When discrepancies exist:
1. The **Setup Manual** and **Realm Contract** are authoritative
2. Realm export JSON and scripts must be aligned to them

Application runtime configuration (Blazor UI, APIs, Gateway) is documented elsewhere.