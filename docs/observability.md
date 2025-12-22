# Correlation ID Strategy

This demo showcases request correlation across a Blazor client and a downstream
API using:

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

### Gateway (Edge, YARP)
- Acts as the **entry point** for user-driven interactions
- Responsible for **generating the business correlation ID** if none exists
- Forwards it to downstream APIs via HTTP headers

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
| Header name | `X-Correlation-ID` |
| Format | Opaque GUID (`Guid.NewGuid().ToString("N")`) |
| Length | 8-64 characters |
| PII | **Not allowed** |
| Cardinality | High (acceptable, but controlled)

---

## HTTP Flow (Web ==> Gateway ==> API)

1. EShop.Web issues a call to the Catalog API via the Gateway.
2. Gateway middleware:
   - Reads `X-Correlation-ID` if present
   - Otherwise generates one (`Guid.NewGuid().ToString("N")`)
   - Adds it to the inbound request headers forwarded by YARP
   - Echoes it on the Gateway response
3. API middleware:
   - Reads and validates the header
   - Generates one if missing/invalid
   - Adds it to:
     - response headers
     - logging scope
     - OpenTelemetry `Activity` as a tag

---

## OpenTelemetry Integration

The business correlation ID is **not** used for span parenting.

Instead, it is attached as an **Activity tag**:

`business.request_id`

This ensures:
- It is searchable in tracing backends
- It does not interfere with TraceId/SpanId semantics
- Trace structure remains standards-compliant
- It is attached only when Activity is recording to reduce cardinality impact

---

## Logging Strategy

Logs emitted during request processing include:

- `trace_id`
- `span_id`
- `request_id`

This allows:
- Navigating from logs ==> traces
- Navigating from traces ==> logs
- Cross-service log correlation using a stable identifier

## Validation

Both Gateway and API validate the incoming header before accepting it. Invalid
or missing values result in a new GUID-based correlation ID.

---

## Response Behavior

The Gateway and API always echo the correlation ID back to the caller:

`X-Correlation-ID: {value}`

This enables:
- Client-side debugging
- Support ticket correlation
- Easy reproduction of issues

---

## Downstream Integrations (Future Scope)

For async or messaging-based integrations (e.g. Service Bus):

- The correlation ID should be propagated as:
  - Message application property (`correlation_id`)
  - Or native broker correlation field where supported
- Consumers should extract and apply it to:
  - logging scope
  - Activity tags

This allows correlation across **multiple traces and async boundaries**.

---

## Summary

- **TraceId** = technical correlation (automatic, OpenTelemetry)
- **Correlation ID** = business/operational correlation (explicit, controlled)
- Generated at the **edge**
- Propagated downstream
- Visible in **logs, traces, headers, and integrations**
