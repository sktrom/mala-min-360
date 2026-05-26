"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { ApiError, getPublicTour, trackPublicTourView } from "@/lib/api";
import type { PublicTour, PublicTourHotspot, PublicTourRoom } from "@/lib/types";
import { getMediaUrl } from "@/lib/url";

type PublicTourViewerProps = {
  tenantSlug: string;
  propertySlug: string;
};

export function PublicTourViewer({ tenantSlug, propertySlug }: PublicTourViewerProps) {
  const [tour, setTour] = useState<PublicTour | null>(null);
  const [activeRoomId, setActiveRoomId] = useState<string | null>(null);
  const [activeInfo, setActiveInfo] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const trackedPropertyId = useRef<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    async function loadTour() {
      setIsLoading(true);
      setError(null);

      try {
        const loadedTour = normalizeTour(await getPublicTour(tenantSlug, propertySlug));

        if (!isMounted) {
          return;
        }

        const orderedRooms = getOrderedRooms(loadedTour.rooms);
        const startRoom =
          orderedRooms.find((room) => room.id === loadedTour.startRoomId) ?? orderedRooms[0] ?? null;

        setTour({ ...loadedTour, rooms: orderedRooms });
        setActiveRoomId(startRoom?.id ?? null);
      } catch (loadError) {
        if (!isMounted) {
          return;
        }

        setError(toTourError(loadError));
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    void loadTour();

    return () => {
      isMounted = false;
    };
  }, [tenantSlug, propertySlug]);

  useEffect(() => {
    if (!tour?.propertyId || trackedPropertyId.current === tour.propertyId) {
      return;
    }

    trackedPropertyId.current = tour.propertyId;
    void trackPublicTourView(tour.propertyId).catch(() => undefined);
  }, [tour?.propertyId]);

  const activeRoom = useMemo(() => {
    if (!tour) {
      return null;
    }

    return tour.rooms.find((room) => room.id === activeRoomId) ?? tour.rooms[0] ?? null;
  }, [tour, activeRoomId]);

  if (isLoading) {
    return (
      <section className="container public-tour-state">
        <p className="note">جاري تحميل جولة 360...</p>
      </section>
    );
  }

  if (error) {
    return (
      <section className="container public-unavailable panel">
        <p className="eyebrow">جولة 360</p>
        <h1>{error === "not-found" ? "الجولة غير متاحة" : "تعذر تحميل الجولة."}</h1>
        <p>
          {error === "not-found"
            ? "قد يكون العقار غير منشور أو لا يحتوي على جولة حالياً."
            : "تعذر تحميل الجولة."}
        </p>
      </section>
    );
  }

  if (!tour || tour.rooms.length === 0 || !activeRoom) {
    return (
      <section className="container public-unavailable panel">
        <p className="eyebrow">جولة 360</p>
        <h1>الجولة غير متاحة</h1>
        <p>لا توجد غرف 360 لهذا العقار حالياً.</p>
      </section>
    );
  }

  function handleHotspotClick(hotspot: PublicTourHotspot) {
    if (hotspot.type === "Navigate") {
      if (hotspot.targetRoomId && tour?.rooms.some((room) => room.id === hotspot.targetRoomId)) {
        setActiveRoomId(hotspot.targetRoomId);
        setActiveInfo(null);
      }

      return;
    }

    setActiveInfo(hotspot.label);
  }

  return (
    <main className="public-tour">
      <div className="container">
        <section className="public-tour-header">
          <div>
            <p className="eyebrow">{tour.tenantName}</p>
            <h1>{tour.propertyTitle}</h1>
            <p className="lead">{activeRoom.name}</p>
          </div>
        </section>

        <section className="tour-viewer-shell">
          <div className="tour-panorama" aria-label="عارض جولة 360">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={getMediaUrl(activeRoom.panoramaUrl)} alt={activeRoom.name} />
            {activeRoom.hotspots.map((hotspot) => (
              <button
                className={`tour-hotspot ${hotspot.type === "Navigate" ? "navigate" : "info"}`}
                type="button"
                key={hotspot.id}
                style={getHotspotPosition(hotspot)}
                onClick={() => handleHotspotClick(hotspot)}
              >
                {hotspot.type === "Navigate" ? "↗" : "i"} {hotspot.label}
              </button>
            ))}
          </div>

          {activeInfo && (
            <div className="tour-info-panel">
              <strong>معلومة</strong>
              <p>{activeInfo}</p>
              <button className="button secondary" type="button" onClick={() => setActiveInfo(null)}>
                إغلاق
              </button>
            </div>
          )}

          <p className="note">عرض 360 تجريبي — سيتم تحسين العارض لاحقاً.</p>
        </section>

        <nav className="tour-room-list" aria-label="غرف الجولة">
          {tour.rooms.map((room) => (
            <button
              className={room.id === activeRoom.id ? "active" : ""}
              type="button"
              key={room.id}
              onClick={() => {
                setActiveRoomId(room.id);
                setActiveInfo(null);
              }}
            >
              {room.name}
            </button>
          ))}
        </nav>
      </div>
    </main>
  );
}

function normalizeTour(tour: PublicTour): PublicTour {
  return {
    ...tour,
    rooms: (tour.rooms ?? []).map((room) => ({
      ...room,
      hotspots: room.hotspots ?? []
    }))
  };
}

function getOrderedRooms(rooms: PublicTourRoom[]): PublicTourRoom[] {
  return [...rooms].sort((first, second) => first.sortOrder - second.sortOrder);
}

function getHotspotPosition(hotspot: PublicTourHotspot): { left: string; top: string } {
  const x = clamp(((hotspot.yaw + 180) / 360) * 100, 5, 95);
  const y = clamp(((90 - hotspot.pitch) / 180) * 100, 5, 95);

  return {
    left: `${x}%`,
    top: `${y}%`
  };
}

function clamp(value: number, min: number, max: number): number {
  return Math.min(max, Math.max(min, value));
}

function toTourError(error: unknown): string {
  if (error instanceof ApiError && error.status === 404) {
    return "not-found";
  }

  return "generic";
}
