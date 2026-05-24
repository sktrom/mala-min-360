# Codex Rules for Mala Min 360

## Product Context

Mala Min 360  مالا من 360 is a Syria-first real estate SaaS platform for real estate agencies.

The MVP focuses on:
- Arabic RTL experience
- Agency dashboard
- Public property pages
- 360 virtual tours
- WhatsApp-first sharing
- QR code sharing
- Manual subscription management

## Mandatory Architecture Rules

- Use a monorepo structure.
- Backend must be ASP.NET Core Web API.
- Frontend must be Next.js with TypeScript.
- Database must be PostgreSQL.
- Multi-tenancy must use TenantId on every tenant-owned table.
- Never trust TenantId from the frontend.
- TenantId must be resolved from the authenticated user context.
- Public property pages are the only unauthenticated public feature in the MVP.

## Mandatory Code Rules

- Do not push directly to main.
- Do not put business logic inside controllers.
- Use services or use-cases for business logic.
- Use DTOs for API requests and responses.
- Validate every write request.
- Use soft delete for tenant-owned business entities.
- Never store uploaded files in the database.
- Store only file metadata and storage URLs in the database.
- Never commit secrets, API keys, passwords, tokens, or .env files.
- Never add generated junk files.
- Keep code small, readable, and production-minded.

## MVP Boundaries

Allowed in MVP:
- Tenant registration and login foundation
- Property CRUD
- Public property page
- Media upload foundation
- 360 room/tour model
- Hotspots model
- WhatsApp click tracking
- QR code generation
- Manual subscription limits

Not allowed in MVP:
- Marketplace
- Native mobile app
- Online payments
- AI pricing
- Full CRM
- Matterport-style 3D scanning
- Complex maps
- Multi-language support beyond Arabic-first foundation

## Review Rules

Before accepting any PR, verify:
- No secrets were committed.
- Tenant isolation is respected.
- The implementation stays inside MVP scope.
- The code is easy to read and maintain.
- Documentation is updated when architecture changes.
