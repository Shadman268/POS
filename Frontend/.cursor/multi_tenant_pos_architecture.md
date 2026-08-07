# Multi-Tenant POS Architecture Specification

## 1. Purpose

This document defines the architecture for a cloud-based, multi-tenant Point of Sale platform for phamacy, supermarkets and retail shops.

The initial deployment will support approximately two shops and must be able to scale gradually to 100 or more shops without creating separate codebases or deployments for each customer.

The system must support:

- Multiple independent shop organizations
- Multiple branches per shop
- Multiple POS terminals per branch
- Tenant-specific users, products, pricing, inventory, sales, settings, branding, and subscription features
- Shared application code and infrastructure
- Secure tenant data isolation
- Low initial hosting cost
- A clear scale-up path
- Cloud deployment with automated CI/CD
- Future offline POS synchronization

---

## 2. Primary Architecture Decision

The platform will use a shared multi-tenant SaaS architecture.

The system will have:

- One Angular frontend codebase
- One ASP.NET Core backend codebase
- One production frontend deployment
- One production backend deployment
- One shared production database
- One set of shared database tables
- A `TenantId` column on every tenant-owned record
- A `BranchId` column where branch-level separation is required

A new shop will be created through application data and configuration. It will not require a new source-code repository, frontend deployment, backend deployment, or database.

Core rule:

```text
Deploy per environment.
Configure per tenant.
Do not deploy per shop.
```

---

## 3. Technology Stack

### Frontend

- Angular
- TypeScript
- Angular signals or RxJS for application state
- Progressive Web App support where useful
- Cloudflare Pages for hosting
- Cloudflare DNS and SSL

### Backend

- ASP.NET Core Web API
- C#
- Entity Framework Core
- JWT or secure cookie-based authentication
- FluentValidation or equivalent request validation
- Docker container
- Azure Container Apps Consumption for hosting

### Database

- Azure SQL Database
- Shared database and shared schema
- EF Core migrations for schema deployment
- Tenant-aware indexes
- Automated Azure backups

### Storage

- Azure Blob Storage for:
  - Product images
  - Tenant logos
  - Invoice exports
  - Report files
  - Import files
  - Optional archived documents

### Source Control and CI/CD

- GitHub private repository
- GitHub Actions
- Cloudflare Pages GitHub integration for frontend deployment
- GitHub Actions for backend Docker build and deployment
- GitHub Container Registry or Azure Container Registry for backend images


## 6. Tenant Model

A tenant represents one customer organization or shop company.

A tenant may have one or more branches.

```text
Tenant / Company
  |
  +-- Branch 1
  |     +-- Terminal 1
  |     +-- Terminal 2
  |
  +-- Branch 2
        +-- Terminal 1
```

## 7. Tenant Identification

Start with one common frontend URL:

```text
https://pos.example.com
```

The login screen should request:

- Shop code
- Username or email
- Password

The backend will resolve the tenant using the shop code and authenticate the user inside that tenant.

Later, optional tenant subdomains may be added:

```text
rahim.example.com
freshmart.example.com
```

The subdomain may be used to load public branding, but it must not be treated as the final security boundary.

After authentication, the backend must use the authenticated user's tenant identity from the JWT or server-side session.

---

## 8. Authentication and Authorization

### JWT Claims

A token should contain claims similar to:

```json
{
  "sub": "user-id",
  "tenantId": "tenant-id",
  "branchId": "branch-id",
  "role": "Cashier",
  "terminalId": "terminal-id"
}
```

### Security Rules

- The frontend must never be trusted to provide the authoritative `TenantId`.
- The backend must resolve `TenantId` from the authenticated principal.
- Every query and write operation must be tenant-scoped.
- Branch-restricted users must only access permitted branches.
- Feature access must be validated by the backend, not only hidden in the UI.
- Platform administrators must use separate elevated authorization policies.

### Roles

Suggested default roles:

- Admin(me)
- cashier
- manager of the shop

## 9. Database Tenancy Strategy

Use one shared Azure SQL database with shared tables.

Every tenant-owned table must contain `TenantId`.

Tables with branch-specific data must also contain `BranchId`.

Example:

```text
Products
- Id
- TenantId
- Name
- SKU
- Barcode
- SellingPrice
- CostPrice

Sales
- Id
- TenantId
- BranchId
- TerminalId
- InvoiceNumber
- CustomerId
- Subtotal
- DiscountAmount
- TaxAmount
- TotalAmount
- PaymentStatus
- CreatedAtUtc
```


### Unique Constraints

Uniqueness must normally be scoped by tenant.

Examples:

```text
Unique: TenantId + ShopCode
Unique: TenantId + SKU
Unique: TenantId + Barcode, when required
Unique: TenantId + BranchId + InvoiceNumber
Unique: TenantId + UserEmail
```

### Recommended Indexes

```text
(TenantId, Id)
(TenantId, CreatedAtUtc)
(TenantId, SKU)
(TenantId, Barcode)
(TenantId, BranchId)
(TenantId, BranchId, CreatedAtUtc)
(TenantId, BranchId, InvoiceNumber)
(TenantId, ProductId)
(TenantId, CustomerId)
```

All high-volume queries should begin with tenant filtering.

---

## 10. Core Database Modules

Suggested main tables:

### Platform and Tenancy

```text
Tenants
TenantDomains
TenantSettings
SubscriptionPlans
TenantSubscriptions
Features
TenantFeatures
Branches
PosTerminals
```

### Identity and Access

```text
Users
Roles
Permissions
UserRoles
RolePermissions
UserBranches
RefreshTokens
LoginAuditLogs
```

### Product Catalog

```text
Products
ProductCategories
Brands
UnitsOfMeasure
ProductBarcodes
ProductPrices
ProductImages
```

