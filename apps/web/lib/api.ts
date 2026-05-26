import type {
  CreatePropertyRequest,
  CreateTourHotspotRequest,
  CreateTourRoomRequest,
  CurrentSubscription,
  MediaFile,
  Property,
  PropertyImage,
  PublicProperty,
  PublicTour,
  StatsOverview,
  TourHotspot,
  TourRoom,
  UpdateTourHotspotRequest
} from "./types";

export type CurrentUser = {
  id: string;
  tenantId: string;
  fullName: string;
  email: string;
  role: string;
  tenantName: string;
  tenantSlug: string;
};

export type AuthResponse = {
  accessToken: string;
  expiresAt: string;
  user: CurrentUser;
};

type ApiEnvelope<T> = {
  success: boolean;
  data?: T;
  error?: {
    code: string;
    message: string;
  };
};

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly code?: string,
    public readonly status?: number
  ) {
    super(message);
  }
}

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ?? "http://localhost:5000";

export async function login(email: string, password: string): Promise<AuthResponse> {
  return request<AuthResponse>("/api/auth/login", {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({ email, password })
  });
}

export async function getCurrentUser(token: string): Promise<CurrentUser> {
  return request<CurrentUser>("/api/auth/me", {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });
}

export async function getProperties(token: string): Promise<Property[]> {
  return request<Property[]>("/api/properties", {
    headers: createAuthHeaders(token)
  });
}

export async function createProperty(
  token: string,
  property: CreatePropertyRequest
): Promise<Property> {
  return request<Property>("/api/properties", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...createAuthHeaders(token)
    },
    body: JSON.stringify(property)
  });
}

export async function publishProperty(token: string, propertyId: string): Promise<Property> {
  return request<Property>(`/api/properties/${propertyId}/publish`, {
    method: "PATCH",
    headers: createAuthHeaders(token)
  });
}

export async function unpublishProperty(token: string, propertyId: string): Promise<Property> {
  return request<Property>(`/api/properties/${propertyId}/unpublish`, {
    method: "PATCH",
    headers: createAuthHeaders(token)
  });
}

export async function deleteProperty(token: string, propertyId: string): Promise<void> {
  await request<void>(`/api/properties/${propertyId}`, {
    method: "DELETE",
    headers: createAuthHeaders(token)
  }, false);
}

export async function getStatsOverview(token: string): Promise<StatsOverview> {
  return request<StatsOverview>("/api/stats/overview", {
    headers: createAuthHeaders(token)
  });
}

export async function getCurrentSubscription(token: string): Promise<CurrentSubscription> {
  return request<CurrentSubscription>("/api/subscription/me", {
    headers: createAuthHeaders(token)
  });
}

export async function uploadMedia(
  token: string,
  file: File,
  fileType: string
): Promise<MediaFile> {
  const formData = new FormData();
  formData.append("file", file);
  formData.append("fileType", fileType);

  return request<MediaFile>("/api/media/upload", {
    method: "POST",
    headers: createAuthHeaders(token),
    body: formData
  });
}

export async function getPropertyImages(
  token: string,
  propertyId: string
): Promise<PropertyImage[]> {
  return request<PropertyImage[]>(`/api/properties/${propertyId}/images`, {
    headers: createAuthHeaders(token)
  });
}

export async function addPropertyImage(
  token: string,
  propertyId: string,
  mediaFileId: string,
  sortOrder?: number,
  isCover?: boolean
): Promise<PropertyImage> {
  return request<PropertyImage>(`/api/properties/${propertyId}/images`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...createAuthHeaders(token)
    },
    body: JSON.stringify({ mediaFileId, sortOrder, isCover })
  });
}

export async function setPropertyImageCover(
  token: string,
  propertyId: string,
  imageId: string
): Promise<void> {
  await request<void>(`/api/properties/${propertyId}/images/${imageId}/cover`, {
    method: "PATCH",
    headers: createAuthHeaders(token)
  }, false);
}

export async function deletePropertyImage(
  token: string,
  propertyId: string,
  imageId: string
): Promise<void> {
  await request<void>(`/api/properties/${propertyId}/images/${imageId}`, {
    method: "DELETE",
    headers: createAuthHeaders(token)
  }, false);
}

export async function getTourRooms(token: string, propertyId: string): Promise<TourRoom[]> {
  return request<TourRoom[]>(`/api/properties/${propertyId}/tour/rooms`, {
    headers: createAuthHeaders(token)
  });
}

export async function createTourRoom(
  token: string,
  propertyId: string,
  room: CreateTourRoomRequest
): Promise<TourRoom> {
  return request<TourRoom>(`/api/properties/${propertyId}/tour/rooms`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...createAuthHeaders(token)
    },
    body: JSON.stringify(room)
  });
}

export async function setTourRoomStart(
  token: string,
  propertyId: string,
  roomId: string
): Promise<void> {
  await request<void>(`/api/properties/${propertyId}/tour/rooms/${roomId}/start`, {
    method: "PATCH",
    headers: createAuthHeaders(token)
  }, false);
}

export async function deleteTourRoom(
  token: string,
  propertyId: string,
  roomId: string
): Promise<void> {
  await request<void>(`/api/properties/${propertyId}/tour/rooms/${roomId}`, {
    method: "DELETE",
    headers: createAuthHeaders(token)
  }, false);
}

export async function getTourHotspots(
  token: string,
  propertyId: string,
  roomId: string
): Promise<TourHotspot[]> {
  return request<TourHotspot[]>(
    `/api/properties/${propertyId}/tour/rooms/${roomId}/hotspots`,
    {
      headers: createAuthHeaders(token)
    }
  );
}

export async function createTourHotspot(
  token: string,
  propertyId: string,
  roomId: string,
  hotspot: CreateTourHotspotRequest
): Promise<TourHotspot> {
  return request<TourHotspot>(
    `/api/properties/${propertyId}/tour/rooms/${roomId}/hotspots`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        ...createAuthHeaders(token)
      },
      body: JSON.stringify(hotspot)
    }
  );
}

export async function updateTourHotspot(
  token: string,
  propertyId: string,
  roomId: string,
  hotspotId: string,
  hotspot: UpdateTourHotspotRequest
): Promise<TourHotspot> {
  return request<TourHotspot>(
    `/api/properties/${propertyId}/tour/rooms/${roomId}/hotspots/${hotspotId}`,
    {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        ...createAuthHeaders(token)
      },
      body: JSON.stringify(hotspot)
    }
  );
}

export async function deleteTourHotspot(
  token: string,
  propertyId: string,
  roomId: string,
  hotspotId: string
): Promise<void> {
  await request<void>(
    `/api/properties/${propertyId}/tour/rooms/${roomId}/hotspots/${hotspotId}`,
    {
      method: "DELETE",
      headers: createAuthHeaders(token)
    },
    false
  );
}

export async function getPublicProperty(
  tenantSlug: string,
  propertySlug: string
): Promise<PublicProperty> {
  return request<PublicProperty>(
    `/api/public/properties/${encodeURIComponent(tenantSlug)}/${encodeURIComponent(propertySlug)}`,
    {
      headers: {}
    }
  );
}

export async function getPublicTour(
  tenantSlug: string,
  propertySlug: string
): Promise<PublicTour> {
  return request<PublicTour>(
    `/api/public/properties/${encodeURIComponent(tenantSlug)}/${encodeURIComponent(propertySlug)}/tour`,
    {
      headers: {}
    }
  );
}

export async function trackPublicTourView(propertyId: string): Promise<void> {
  await request<void>(
    `/api/public/properties/${propertyId}/track-tour-view`,
    {
      method: "POST",
      headers: {}
    },
    false
  );
}

async function request<T>(
  path: string,
  init: RequestInit,
  requireData = true
): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      Accept: "application/json",
      ...init.headers
    }
  });

  const payload = (await response.json().catch(() => null)) as ApiEnvelope<T> | null;

  if (!response.ok || !payload?.success || (requireData && payload.data === undefined)) {
    throw new ApiError(
      payload?.error?.message ?? "تعذر الاتصال بالخادم. حاول مرة أخرى.",
      payload?.error?.code,
      response.status
    );
  }

  return payload.data as T;
}

function createAuthHeaders(token: string): Record<string, string> {
  return {
    Authorization: `Bearer ${token}`
  };
}
