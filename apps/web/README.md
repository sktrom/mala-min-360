# Mala Min 360 Web

Next.js frontend foundation for Mala Min 360.

## Commands

```powershell
copy .env.example .env.local
npm install
npm run dev
npm run build
```

The backend API must be running at the URL in `.env.local`.

Default development API URL:

```text
http://localhost:5000
```

Local URL:

```text
http://localhost:3000
```

Current routes:
- `/`
- `/visitor`
- `/login`
- `/dashboard`
- `/properties`
- `/subscription`
- `/a/[tenantSlug]/[propertySlug]`
- `/a/[tenantSlug]/[propertySlug]/tour`

## Backend-backed pages

`/properties` connects to the backend property API and requires login.
`/subscription` connects to the backend subscription API and requires login.
`/visitor` connects to the public properties API and does not require login.
`/a/[tenantSlug]/[propertySlug]` connects to the public property API and does not require login.
`/a/[tenantSlug]/[propertySlug]/tour` connects to the public tour API and does not require login.

Supported MVP actions:
- list current tenant properties
- create property
- publish / unpublish
- soft delete
- upload and manage normal property images
- upload panorama images and manage 360 tour rooms
- create, edit, and delete manual 360 tour hotspots
- view public 360 tours with room navigation and hotspot navigation
- view current subscription, plan limits, and tenant usage

The backend must be running at `http://localhost:5000` unless `NEXT_PUBLIC_API_BASE_URL` is changed.
