# Mala Min 360 Database Design

## Database Engine

PostgreSQL is the target database.

## Current Implementation

The current EF Core implementation includes the Tenants, Users, Properties, MediaFiles, PropertyImages, TourRooms, TourHotspots, PropertyStats, Plans, and Subscriptions tables.

More entities will be added step by step in later implementation steps.

Authentication foundation is implemented for development login testing.

Manual subscriptions are implemented for MVP billing control. Online payments, payment gateways, invoices, and automatic renewals are not implemented.

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

## MediaFiles (Implemented)

Represents uploaded media metadata.

MediaFiles are tenant-owned through TenantId.

Files are not stored in PostgreSQL. PostgreSQL stores metadata only.

Local development storage under storage/uploads is temporary; future production storage will use S3/R2-compatible object storage.

Fields:
- Id
- TenantId
- Url
- StorageKey
- FileType
- OriginalFileName
- MimeType
- SizeBytes
- Width
- Height
- ProcessingStatus
- CreatedAt
- UpdatedAt
- DeletedAt

FileType values:
- NormalImage
- Panorama360
- Logo
- Other

ProcessingStatus values:
- Pending
- Processing
- Completed
- Failed

## PropertyImages (Implemented)

Links normal images to properties.

PropertyImages links Properties to MediaFiles.

Delete operations soft delete the link only by setting DeletedAt.

MediaFile metadata and physical files are not deleted by PropertyImages operations.

Fields:
- Id
- TenantId
- PropertyId
- MediaFileId
- SortOrder
- IsCover
- CreatedAt
- UpdatedAt
- DeletedAt

## TourRooms (Implemented)

Represents one 360 panorama position inside a property.

TourRooms link Properties to Panorama360 MediaFiles.

Delete operations soft delete the room only by setting DeletedAt.

MediaFile metadata and physical files are not deleted by TourRoom operations.

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
- DeletedAt

## TourHotspots (Implemented)

Represents clickable points inside a 360 room.

Navigate hotspots require TargetRoomId.

Info hotspots do not require TargetRoomId.

Source and target rooms must belong to the same property and tenant.

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
- DeletedAt

Type values:
- Navigate
- Info

## PropertyStats (Implemented)

Aggregated daily property stats.

PropertyStats are tenant-owned through TenantId.

Stats are aggregated by UTC date using StatDate.

The tracking foundation does not store IP addresses, user agents, visitor fingerprints, or per-visitor events.

Fields:
- Id
- TenantId
- PropertyId
- StatDate
- Views
- TourViews
- WhatsAppClicks
- QrScans
- CreatedAt
- UpdatedAt

## Plans

Represents subscription plans.

Plans are implemented and define server-side limits.

Fields:
- Id
- Name
- Code
- MaxProperties
- MaxTours
- StorageLimitMb
- MonthlyPrice
- IsActive
- CreatedAt
- UpdatedAt

## Subscriptions

Represents manual subscriptions.

Subscriptions are implemented and manually managed.

Property creation enforces MaxProperties from the current tenant subscription.

Storage limits are reported through subscription usage but are not enforced yet.

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
- MediaFiles.FileType
- MediaFiles.ProcessingStatus
- PropertyImages.TenantId
- PropertyImages.PropertyId
- PropertyImages.MediaFileId
- PropertyImages.PropertyId + SortOrder
- PropertyImages.PropertyId + IsCover
- TourRooms.TenantId
- TourRooms.PropertyId
- TourRooms.PanoramaMediaId
- TourRooms.PropertyId + SortOrder
- TourRooms.PropertyId + IsStartRoom
- TourHotspots.TenantId
- TourHotspots.RoomId
- TourHotspots.TargetRoomId
- TourHotspots.Type
- PropertyStats.TenantId
- PropertyStats.PropertyId
- PropertyStats.StatDate
- PropertyStats.PropertyId + StatDate unique
- Plans.Code unique
- Subscriptions.TenantId
- Subscriptions.PlanId
- Subscriptions.Status
- Subscriptions.EndsAt

## Soft Delete Rule

Use DeletedAt for business entities such as:
- Properties
- MediaFiles if needed
- PropertyImages
- TourRooms if needed
- TourHotspots

Do not physically delete business data unless explicitly required.
