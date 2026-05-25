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
