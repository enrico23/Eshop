# EShop Demo - Feature Backlog

This document tracks the features planned for the EShop demo. It keeps the
scope realistic while reflecting common enterprise requirements.

## Current baseline (already in repo)
- [x] Aspire AppHost orchestration (API + Web + DB + YARP)
- [x] Catalog API (Products/Categories) with paging/sorting and basic filtering
- [x] Admin screens for Products/Categories (MudBlazor + server-side grids)
- [x] Product image upload + Azure Blob Storage (Azurite local)

## Customer experience (replace the current Home)
- [ ] Customer home page: hero search + category strip + featured/new products card grid
- [ ] Shop/browse page (`/shop`): card grid with paging + sort + filters
  - [ ] Filter by category
  - [ ] Search by name/keywords
  - [ ] Price range filter
  - [ ] In-stock filter
- [ ] Product details page (`/products/{id}` or `/p/{slug}`)
  - [ ] Image gallery
  - [ ] Add to cart
  - [ ] Related products (same category)

## Cart & checkout (demo scope)
- [ ] Cart page with quantity updates + remove items
- [ ] Cart persistence
  - [ ] Anonymous: local storage
  - [ ] Authenticated: server-side cart (optional)
- [ ] Checkout (demo)
  - [ ] Address + contact form
  - [ ] "Place order" using a fake payment step
- [ ] Order confirmation screen

## Identity & authorization
- [ ] Add authentication (choose one)
  - [ ] ASP.NET Core Identity (local)
  - [ ] External OIDC (Azure AD / Keycloak) for enterprise realism
- [ ] Roles/Policies
  - [ ] `Admin` can access `/products`, `/categories`
  - [ ] Customer screens public / customer-only where needed
- [ ] Login/Logout buttons in app bar + user menu

## API enhancements (REST)
- [ ] Stronger REST filtering model for products
  - [ ] `categoryId`, `minPrice`, `maxPrice`, `inStock`, `status`, `q`
- [ ] Consistent API error responses (problem details)
- [ ] ETag / caching headers for images and product lists (where sensible)

## GraphQL (job-spec aligned)
Goal: make filtering and data shaping more elastic for UI screens.

### Option A - GraphQL as a BFF (recommended for demos)
- [ ] Add a GraphQL endpoint in `EShop.API` (HotChocolate)
- [ ] Query: `products(page, sort, filter)` returning `{ total, items }`
- [ ] Query: `categories` for navigation/filter strip
- [ ] Input types: `ProductFilterInput`, `SortInput`, `PageInput`
- [ ] Add DataLoader/batching to avoid N+1
- [ ] Add auth rules: customer-safe fields vs admin fields

### Production guardrails (required for "enterprise" credibility)
- [ ] Request timeout limits
- [ ] Depth/complexity limits (cost analysis)
- [ ] Persisted queries (or at least operation allowlist)
- [ ] Observability: log operation name + timings; metrics on errors

### Client integration
- [ ] GraphQL client in `EShop.Web`
  - [ ] Simple: `GraphQL.Client`
  - [ ] Typed: StrawberryShake (if worth it)

## UI/UX quality (enterprise polish)
- [ ] Dedicated customer layout (separate from admin navigation)
- [ ] Empty states, skeleton loaders, and consistent error banners
- [ ] Responsive design checks (mobile/tablet/desktop)
- [ ] Accessibility basics (labels, focus states, contrast)

## Reliability & observability
- [ ] OpenTelemetry tracing (API + Web) + Aspire dashboard visibility
- [ ] Structured logging with correlation IDs
- [ ] Health checks (API + dependencies)

## Testing
- [ ] API integration tests for filtering/paging
- [ ] Minimal UI/component tests for critical flows (optional)

## Nice-to-have stretch goals
- [ ] Product variants (size/color) and SKU matrix
- [ ] Admin bulk actions (activate/archive)
- [ ] Background jobs (e.g., image thumbnail generation)
- [ ] Recommendations/personalization (demo stub)
