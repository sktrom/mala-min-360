# Mala Min 360 Database Design

## Database Engine

PostgreSQL is the target database.

## Current Implementation

The initial EF Core implementation includes only the Tenants table.

More entities will be added step by step in later implementation steps.

## Multi-Tenancy Rule

Every tenant-owned table must include TenantId.

The API must scope tenant-owned queries by TenantId.

## Core Entities

## Tenants

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

## Users

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

## Properties

Represents a property listing.

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
