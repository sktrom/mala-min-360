# Mala Min 360 API Contracts

## API Style

Backend APIs are under:

/api

Public APIs are under:

/api/public

Admin APIs are under:

/api/admin

## Auth

Implemented:
- POST /api/auth/login
- GET  /api/auth/me

Not implemented yet:
- Public registration
- Refresh tokens
- Logout
- Forgot password
- Email verification

### POST /api/auth/login

Request:

```json
{
  "email": "owner@demo.local",
  "password": "Demo12345!"
}
```

Success response:

```json
{
  "success": true,
  "data": {
    "accessToken": "...",
    "expiresAt": "...",
    "user": {
      "id": "...",
      "tenantId": "...",
      "fullName": "Demo Owner",
      "email": "owner@demo.local",
      "role": "TenantOwner",
      "tenantName": "Demo Real Estate Agency",
      "tenantSlug": "demo-agency"
    }
  }
}
```

Invalid credentials response:

```json
{
  "success": false,
  "error": {
    "code": "INVALID_CREDENTIALS",
    "message": "Invalid email or password."
  }
}
```

### GET /api/auth/me

Requires:

```text
Authorization: Bearer <accessToken>
```

Success response:

```json
{
  "success": true,
  "data": {
    "id": "...",
    "tenantId": "...",
    "fullName": "Demo Owner",
    "email": "owner@demo.local",
    "role": "TenantOwner",
    "tenantName": "Demo Real Estate Agency",
    "tenantSlug": "demo-agency"
  }
}
```

## Tenant

GET /api/tenant/me
PUT /api/tenant/me

## Properties

GET    /api/properties
POST   /api/properties
GET    /api/properties/{id}
PUT    /api/properties/{id}
DELETE /api/properties/{id}
PATCH  /api/properties/{id}/publish
PATCH  /api/properties/{id}/unpublish

## Media

POST   /api/media/upload
GET    /api/media
GET    /api/media/{id}
DELETE /api/media/{id}

## Property Images

GET    /api/properties/{propertyId}/images
POST   /api/properties/{propertyId}/images
PUT    /api/properties/{propertyId}/images/reorder
DELETE /api/properties/{propertyId}/images/{imageId}
PATCH  /api/properties/{propertyId}/images/{imageId}/cover

## Tour Rooms

GET    /api/properties/{propertyId}/tour
POST   /api/properties/{propertyId}/tour/rooms
PUT    /api/tour/rooms/{roomId}
DELETE /api/tour/rooms/{roomId}
PATCH  /api/tour/rooms/{roomId}/start

## Tour Hotspots

POST   /api/tour/rooms/{roomId}/hotspots
PUT    /api/tour/hotspots/{hotspotId}
DELETE /api/tour/hotspots/{hotspotId}

## Stats

GET /api/stats/overview
GET /api/stats/properties/{propertyId}

## Public Property Page

GET  /api/public/properties/{tenantSlug}/{propertySlug}
GET  /api/public/properties/{tenantSlug}/{propertySlug}/tour
POST /api/public/properties/{propertyId}/track-view
POST /api/public/properties/{propertyId}/track-tour-view
POST /api/public/properties/{propertyId}/track-whatsapp-click
POST /api/public/properties/{propertyId}/track-qr-scan

## Admin

GET   /api/admin/tenants
GET   /api/admin/tenants/{id}
PATCH /api/admin/tenants/{id}/status

GET   /api/admin/plans
POST  /api/admin/plans
PUT   /api/admin/plans/{id}

GET   /api/admin/subscriptions
POST  /api/admin/subscriptions
PATCH /api/admin/subscriptions/{id}/status

## Response Rules

Successful response:

{
  "success": true,
  "data": {}
}

Error response:

{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Readable error message"
  }
}

## Validation Rules

Every write endpoint must validate:
- Required fields
- Field length
- Numeric ranges
- Enum values
- Tenant access
- Ownership of related records

## Tenant Security Rule

Tenant-owned endpoints must never accept TenantId from the client as trusted input.

The backend must resolve TenantId from the authenticated user.
