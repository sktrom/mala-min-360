# Mala Min 360 Database Design

## Database Engine

PostgreSQL is the target database.

## Current Implementation

The current EF Core implementation includes the Tenants, Users, and Properties tables.

More entities will be added step by step in later implementation steps.

Authentication foundation is implemented for development login testing.

Development startup seeds one demo tenant and owner user when running in Development:
- Tenant: Demo Real Estate Agency
- Tenant slug: demo-agency
- User email: owner@demo.local

User passwords are stored only as PasswordHash values. Plain text passwords are not stored in the database.

## Multi-Tenancy Rule

Every tenant-owned table must include TenantId.

The API must scope tenant-owned queries by TenantId.

TenantId is resolved from JWT claims through ITenantContext and is never accepted from the frontend.

## Core Entities

## Tenants (Implemented)

Represents a real estate agency.

Fields:
- Id
- Name
- Slug
- Phone
- WhatsAppNumber
- LogoUrl
- Address
- City
- Status
- CreatedAt
- UpdatedAt

Status values:
- Trial
- Active
- Suspended
- Expired

## Users (Implemented)

Represents platform users.

Fields:
- Id
- TenantId
- FullName
- Email
- Phone
- PasswordHash
- Role
- IsActive
- LastLoginAt
- CreatedAt
- UpdatedAt

Roles:
- SuperAdmin
- TenantOwner
- Manager
- Agent

## Properties (Implemented)

Represents a property listing.

Properties are tenant-owned through TenantId.

Delete operations use soft delete by setting DeletedAt.

Fields:
- Id
- TenantId
- Title
- Slug
- Description
- City
- AreaName
- AddressText
- Price
- Currency
- ListingType
- PropertyType
- Bedrooms
- Bathrooms
- FloorNumber
- AreaSqm
- Status
- IsPublished
- CreatedAt
- UpdatedAt
- DeletedAt

ListingType values:
- Sale
- Rent

PropertyType values:
- Apartment
- Villa
- House
- Shop
- Office
- Land

Status values:
- Available
- Reserved
- Sold
- Rented

## MediaFiles

Represents uploaded media metadata.

Fields:
- Id
- TenantId
- Url
- StorageKey
- FileType
- MimeType
- SizeBytes
- Width
- Height
- ProcessingStatus
- CreatedAt

FileType values:
- NormalImage
- Panorama360
- Logo

ProcessingStatus values:
- Pending
- Processing
- Completed
- Failed

## PropertyImages

Links normal images to properties.

Fields:
- Id
- TenantId
- PropertyId
- MediaFileId
- SortOrder
- IsCover
- CreatedAt

## TourRooms

Represents one 360 panorama position inside a property.

Fields:
- Id
- TenantId
- PropertyId
- Name
- PanoramaMediaId
- SortOrder
- IsStartRoom
- CreatedAt
- UpdatedAt

## TourHotspots

Represents clickable points inside a 360 room.

Fields:
- Id
- TenantId
- RoomId
- TargetRoomId
- Type
- Label
- Yaw
- Pitch
- CreatedAt
- UpdatedAt

Type values:
- Navigate
- Info

## PropertyStats

Aggregated daily stats.

Fields:
- Id
- TenantId
- PropertyId
- StatDate
- Views
- TourViews
- WhatsAppClicks
- QrScans

## Plans

Represents subscription plans.

Fields:
- Id
- Name
- MaxProperties
- MaxTours
- StorageLimitMb
- MonthlyPrice
- IsActive

## Subscriptions

Represents manual subscriptions.

Fields:
- Id
- TenantId
- PlanId
- Status
- StartsAt
- EndsAt
- CreatedAt
- UpdatedAt

Status values:
- Trial
- Active
- Suspended
- Expired

## Required Indexes

Recommended indexes:
- Tenants.Slug unique
- Users.Email unique
- Properties.TenantId
- Properties.Slug
- Properties.TenantId + Slug unique
- MediaFiles.TenantId
- TourRooms.PropertyId
- TourHotspots.RoomId
- PropertyStats.PropertyId + StatDate unique

## Soft Delete Rule

Use DeletedAt for business entities such as:
- Properties
- MediaFiles if needed
- TourRooms if needed

Do not physically delete business data unless explicitly required.
