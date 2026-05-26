export type Property = {
  id: string;
  title: string;
  slug: string;
  description: string | null;
  city: string;
  areaName: string;
  addressText: string | null;
  price: number;
  currency: string;
  listingType: string;
  propertyType: string;
  bedrooms: number | null;
  bathrooms: number | null;
  floorNumber: number | null;
  areaSqm: number;
  status: string;
  isPublished: boolean;
  createdAt: string;
  updatedAt: string;
};

export type CreatePropertyRequest = {
  title: string;
  description?: string | null;
  city: string;
  areaName: string;
  addressText?: string | null;
  price: number;
  currency: string;
  listingType: string;
  propertyType: string;
  bedrooms?: number | null;
  bathrooms?: number | null;
  floorNumber?: number | null;
  areaSqm: number;
};

export type StatsOverview = {
  totalViews: number;
  totalTourViews: number;
  totalWhatsAppClicks: number;
  totalQrScans: number;
};

export type Plan = {
  id: string;
  name: string;
  code: string;
  maxProperties: number;
  maxTours: number;
  storageLimitMb: number;
  monthlyPrice: number;
  isActive: boolean;
};

export type CurrentSubscription = {
  status: string;
  startsAt: string;
  endsAt: string;
  plan: Plan;
  currentProperties: number;
  currentTours: number;
  currentStorageMb: number;
};

export type MediaFile = {
  id: string;
  url: string;
  storageKey: string;
  fileType: string;
  originalFileName: string;
  mimeType: string;
  sizeBytes: number;
  width: number | null;
  height: number | null;
  processingStatus: string;
  createdAt: string;
};

export type PropertyImage = {
  id: string;
  propertyId: string;
  mediaFileId: string;
  url: string;
  originalFileName: string;
  mimeType: string;
  sizeBytes: number;
  width: number | null;
  height: number | null;
  sortOrder: number;
  isCover: boolean;
  createdAt: string;
};

export type TourRoom = {
  id: string;
  propertyId: string;
  name: string;
  panoramaMediaId: string;
  panoramaUrl: string;
  originalFileName: string;
  mimeType: string;
  sizeBytes: number;
  width: number | null;
  height: number | null;
  sortOrder: number;
  isStartRoom: boolean;
  createdAt: string;
  updatedAt: string;
};

export type CreateTourRoomRequest = {
  name: string;
  panoramaMediaId: string;
  sortOrder?: number | null;
  isStartRoom?: boolean | null;
};

export type TourHotspotType = "Navigate" | "Info";

export type TourHotspot = {
  id: string;
  roomId: string;
  targetRoomId: string | null;
  type: TourHotspotType;
  label: string;
  yaw: number;
  pitch: number;
  createdAt: string;
  updatedAt: string;
};

export type CreateTourHotspotRequest = {
  targetRoomId?: string | null;
  type: TourHotspotType;
  label: string;
  yaw: number;
  pitch: number;
};

export type UpdateTourHotspotRequest = {
  targetRoomId?: string | null;
  type: TourHotspotType;
  label: string;
  yaw: number;
  pitch: number;
};

export type PublicTenantSummary = {
  name: string;
  slug: string;
  phone: string | null;
  whatsAppNumber: string | null;
  logoUrl: string | null;
  city: string | null;
};

export type PublicPropertyImage = {
  url: string;
  sortOrder: number;
  isCover: boolean;
};

export type PublicProperty = {
  id: string;
  title: string;
  slug: string;
  description: string | null;
  city: string;
  areaName: string;
  addressText: string | null;
  price: number;
  currency: string;
  listingType: string;
  propertyType: string;
  bedrooms: number | null;
  bathrooms: number | null;
  floorNumber: number | null;
  areaSqm: number;
  status: string;
  createdAt: string;
  updatedAt: string;
  tenant: PublicTenantSummary;
  coverImageUrl?: string | null;
  images?: PublicPropertyImage[];
};

export type PublicTourHotspot = {
  id: string;
  roomId: string;
  targetRoomId: string | null;
  type: "Navigate" | "Info";
  label: string;
  yaw: number;
  pitch: number;
};

export type PublicTourRoom = {
  id: string;
  name: string;
  panoramaUrl: string;
  originalFileName: string;
  mimeType: string;
  sizeBytes: number;
  width: number | null;
  height: number | null;
  sortOrder: number;
  isStartRoom: boolean;
  hotspots: PublicTourHotspot[];
};

export type PublicTour = {
  propertyId: string;
  propertyTitle: string;
  propertySlug: string;
  tenantName: string;
  tenantSlug: string;
  startRoomId: string | null;
  rooms: PublicTourRoom[];
};

export type PublicPropertyCard = {
  id: string;
  title: string;
  slug: string;
  city: string;
  areaName: string;
  price: number;
  currency: string;
  listingType: string;
  propertyType: string;
  bedrooms: number | null;
  bathrooms: number | null;
  areaSqm: number;
  tenantName: string;
  tenantSlug: string;
  coverImageUrl: string | null;
};
