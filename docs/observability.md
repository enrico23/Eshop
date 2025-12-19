# Correlation ID Strategy

This demo showcases **enterprise-grade request correlation** across 
a Blazor client and a downstream API using:

- **OpenTelemetry distributed tracing** (technical correlation)
- A **business correlation ID** (operational/workflow correlation)

The goal is to ensure that every request can be reliably traced 
and correlated across services, logs, and integrations.

## Two Types of Correlation

### 1. Distributed Tracing (Technical Correlation)
- Implemented using **W3C Trace Context** (`traceparent`, `tracestate`)
- Managed automatically by **.NET Aspire + OpenTelemetry**
- Identified by a shared **TraceId** across:
  - Blazor host
  - API
  - Downstream dependencies

This provides **end-to-end trace visibility** and parent/child span relationships.

**TraceId is the primary technical correlation identifier.**

---

## Correlation ID Ownership Model

### Blazor Client (Edge)
- Acts as the **entry point** for user-driven interactions
- Responsible for **generating the business correlation ID** if none exists
- Sends it to downstream APIs via HTTP headers

### API
- Treats the correlation ID as **caller-owned**
- Validates and propagates it
- Generates a new one **only if missing or invalid**
- Uses it consistently for logging, tracing enrichment, and downstream calls

This aligns with common enterprise and platform engineering practices.

---

## Correlation ID Specification

| Aspect | Value |
|------|------|
| Header name | `X-Correlation-Id` |
| Format | Opaque GUID (`Guid.NewGuid().ToString("N")`) |
| Length | 32–64 characters |
| PII | **Not allowed** |
| Cardinality | High (acceptable, but controlled)
