# Mala Min 360 Architecture

Mala Min 360  مالا من 360 is a Syria-first real estate SaaS platform for real estate agencies.

## Goal

The platform helps agencies create professional property pages with:
- Normal images
- 360 virtual tours
- WhatsApp sharing
- QR codes
- Basic engagement tracking
- Manual subscription management

## Monorepo Structure

mala-min-360/
  apps/
    api/     ASP.NET Core Web API
    web/     Next.js frontend
  packages/
    shared/
  docs/
  infra/

## Frontend

Technology:
- Next.js
- TypeScript
- Arabic RTL first
- Mobile-first public pages

Responsibilities:
- Agency dashboard
- Property management UI
- Public property pages
- 360 tour builder UI
- Login UI
- Admin UI later

## Backend

Technology:
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL

Responsibilities:
- Authentication
- Tenant management
- Property management
- Media metadata management
- 360 tour management
- Hotspot management
- Stats tracking
- Manual subscriptions

## Database

Database:
- PostgreSQL

Model:
- Single database
- Shared tables
- TenantId on tenant-owned records

## Storage

MVP:
- Local storage during early development

Future:
- S3-compatible object storage

Rules:
- Never store files directly inside PostgreSQL.
- Store only metadata and URLs in the database.

## Multi-Tenancy

Every tenant-owned entity must include TenantId.

TenantId must be resolved from the authenticated user context.

The frontend must never be trusted to provide TenantId.

## Public Access

Only these features are public:
- Public property page
- Public 360 tour view
- Public tracking endpoints

Everything else requires authentication.

## Security

Rules:
- No secrets in Git
- Validate all write requests
- Sanitize user input
- Validate uploaded files
- Use soft delete for business entities
- Never expose stack traces to users

## Performance

Public pages must be:
- Lightweight
- Mobile-first
- Optimized for weak internet connections

360 tours must:
- Load current room first
- Avoid loading all panorama images at once
- Use compressed images
- Use thumbnails when possible
