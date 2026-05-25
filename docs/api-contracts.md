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

Implemented:
- GET /api/tenant/me

Not implemented yet:
- PUT /api/tenant/me

### GET /api/tenant/me

Requires:

```text
Authorization: Bearer <accessToken>
```

TenantId is resolved from the JWT claims. The frontend must not send TenantId in the body, query string, or headers for tenant-owned operations.

Success response:

```json
{
  "success": true,
  "data": {
    "id": "...",
    "name": "Demo Real Estate Agency",
    "slug": "demo-agency",
    "phone": null,
    "whatsAppNumber": null,
    "logoUrl": null,
    "address": null,
    "city": "Damascus",
    "status": "Trial"
  }
}
```

Tenant not found response:

```json
{
  "success": false,
  "error": {
    "code": "TENANT_NOT_FOUND",
    "message": "Tenant was not found."
  }
}
```

## Properties

Implemented:
- GET    /api/properties
- POST   /api/properties
- GET    /api/properties/{id}
- PUT    /api/properties/{id}
- DELETE /api/properties/{id}
- PATCH /api/properties/{id}/publish
- PATCH /api/properties/{id}/unpublish

All property endpoints require:

```text
Authorization: Bearer <accessToken>
```

Property operations are scoped by the current tenant. TenantId is resolved from JWT claims and must never be accepted from the frontend.

### POST /api/properties

Request:

```json
{
  "title": "Demo Apartment in Damascus",
  "description": "Test property for Mala Min 360.",
  "city": "Damascus",
  "areaName": "Malki",
  "addressText": "Near main street",
  "price": 75000,
  "currency": "USD",
  "listingType": "Sale",
  "propertyType": "Apartment",
  "bedrooms": 3,
  "bathrooms": 2,
  "floorNumber": 2,
  "areaSqm": 120
}
```

Success response:

```json
{
  "success": true,
  "data": {
    "id": "...",
    "title": "Demo Apartment in Damascus",
    "slug": "demo-apartment-in-damascus",
    "description": "Test property for Mala Min 360.",
    "city": "Damascus",
    "areaName": "Malki",
    "addressText": "Near main street",
    "price": 75000,
    "currency": "USD",
    "listingType": "Sale",
    "propertyType": "Apartment",
    "bedrooms": 3,
    "bathrooms": 2,
    "floorNumber": 2,
    "areaSqm": 120,
    "status": "Available",
    "isPublished": false,
    "createdAt": "...",
    "updatedAt": "..."
  }
}
```

### PUT /api/properties/{id}

Request includes the create fields plus:

```json
{
  "status": "Available",
  "isPublished": false
}
```

### DELETE /api/properties/{id}

Soft deletes the property by setting DeletedAt.

### PATCH /api/properties/{id}/publish

Marks the current tenant's non-deleted property as published.

### PATCH /api/properties/{id}/unpublish

Marks the current tenant's non-deleted property as unpublished.

## Media

Implemented:
- POST   /api/media/upload
- GET    /api/media
- GET    /api/media/{id}
- DELETE /api/media/{id}

All media endpoints require:

```text
Authorization: Bearer <accessToken>
```

Media operations are scoped by the current tenant. TenantId is resolved from JWT claims and must never be accepted from the frontend.

### POST /api/media/upload

Request:

```text
multipart/form-data
file=<uploaded image>
fileType=NormalImage
```

Allowed file types:
- NormalImage
- Panorama360
- Logo
- Other

Allowed MIME types:
- image/jpeg
- image/png
- image/webp

Success response:

```json
{
  "success": true,
  "data": {
    "id": "...",
    "url": "/uploads/...",
    "storageKey": "...",
    "fileType": "NormalImage",
    "originalFileName": "test-image.jpg",
    "mimeType": "image/jpeg",
    "sizeBytes": 12345,
    "width": null,
    "height": null,
    "processingStatus": "Completed",
    "createdAt": "..."
  }
}
```

The response does not expose TenantId.

### GET /api/media

Returns current tenant media only, excluding soft-deleted records.

### GET /api/media/{id}

Returns media only when it belongs to the current tenant and is not soft-deleted.

### DELETE /api/media/{id}

Soft deletes media metadata by setting DeletedAt. This step does not physically delete the local file.

## Property Images

Implemented:
- GET    /api/properties/{propertyId}/images
- POST   /api/properties/{propertyId}/images
- PUT    /api/properties/{propertyId}/images/reorder
- PATCH  /api/properties/{propertyId}/images/{imageId}/cover
- DELETE /api/properties/{propertyId}/images/{imageId}

All property image endpoints require:

```text
Authorization: Bearer <accessToken>
```

Property image operations are scoped by the current tenant. TenantId is resolved from JWT claims and must never be accepted from the frontend.

Only current tenant MediaFiles with fileType NormalImage can be linked as property images.

### POST /api/properties/{propertyId}/images

Request:

```json
{
  "mediaFileId": "...",
  "sortOrder": 0,
  "isCover": true
}
```

The first linked image is automatically set as cover.

Success response:

```json
{
  "success": true,
  "data": {
    "id": "...",
    "propertyId": "...",
    "mediaFileId": "...",
    "url": "/uploads/...",
    "originalFileName": "test-image.jpg",
    "mimeType": "image/jpeg",
    "sizeBytes": 12345,
    "width": null,
    "height": null,
    "sortOrder": 0,
    "isCover": true,
    "createdAt": "..."
  }
}
```

The response does not expose TenantId.

### PUT /api/properties/{propertyId}/images/reorder

Request:

```json
{
  "images": [
    { "propertyImageId": "...", "sortOrder": 0 },
    { "propertyImageId": "...", "sortOrder": 1 }
  ]
}
```

### PATCH /api/properties/{propertyId}/images/{imageId}/cover

Sets one property image as cover and unsets the other cover images for the same property.

### DELETE /api/properties/{propertyId}/images/{imageId}

Soft deletes the property image link only. It does not delete MediaFile metadata or the physical file.

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

Implemented:
- GET /api/public/properties/{tenantSlug}/{propertySlug}

Not implemented yet:
- GET  /api/public/properties/{tenantSlug}/{propertySlug}/tour
- POST /api/public/properties/{propertyId}/track-view
- POST /api/public/properties/{propertyId}/track-tour-view
- POST /api/public/properties/{propertyId}/track-whatsapp-click
- POST /api/public/properties/{propertyId}/track-qr-scan

### GET /api/public/properties/{tenantSlug}/{propertySlug}

Public endpoint. No Authorization header is required.

Returns only published, non-deleted properties for the tenant slug and property slug.

The response does not expose TenantId, DeletedAt, IsPublished, internal user data, or authentication data.

Success response:

```json
{
  "success": true,
  "data": {
    "id": "...",
    "title": "Public Demo Apartment",
    "slug": "public-demo-apartment",
    "description": "Public test property for Mala Min 360.",
    "city": "Damascus",
    "areaName": "Malki",
    "addressText": "Near main street",
    "price": 75000,
    "currency": "USD",
    "listingType": "Sale",
    "propertyType": "Apartment",
    "bedrooms": 3,
    "bathrooms": 2,
    "floorNumber": 2,
    "areaSqm": 120,
    "status": "Available",
    "createdAt": "...",
    "updatedAt": "...",
    "tenant": {
      "name": "Demo Real Estate Agency",
      "slug": "demo-agency",
      "phone": null,
      "whatsAppNumber": null,
      "logoUrl": null,
      "city": "Damascus"
    }
  }
}
```

Not found response:

```json
{
  "success": false,
  "error": {
    "code": "PUBLIC_PROPERTY_NOT_FOUND",
    "message": "Property was not found or is not published."
  }
}
```

<!-- Future endpoints:
GET  /api/public/properties/{tenantSlug}/{propertySlug}/tour
POST /api/public/properties/{propertyId}/track-view
POST /api/public/properties/{propertyId}/track-tour-view
POST /api/public/properties/{propertyId}/track-whatsapp-click
POST /api/public/properties/{propertyId}/track-qr-scan
-->

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
