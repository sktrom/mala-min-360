"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { Viewer } from "@photo-sphere-viewer/core";
import { MarkersPlugin, type MarkerConfig } from "@photo-sphere-viewer/markers-plugin";
import { ApiError, getPublicTour, trackPublicTourView } from "@/lib/api";
import type { PublicTour, PublicTourHotspot, PublicTourRoom } from "@/lib/types";
import { getMediaUrl } from "@/lib/url";

type PublicTourViewerProps = {
  tenantSlug: string;
  propertySlug: string;
};

type HotspotMarkerData = {
  hotspotId: string;
  type: PublicTourHotspot["type"];
  targetRoomId: string | null;
  label: string;
};

export function PublicTourViewer({ tenantSlug, propertySlug }: PublicTourViewerProps) {
  const [tour, setTour] = useState<PublicTour | null>(null);
  const [activeRoomId, setActiveRoomId] = useState<string | null>(null);
  const [activeInfo, setActiveInfo] = useState<string | null>(null);
  const [viewerError, setViewerError] = useState<string | null>(null);
  const [roomWarning, setRoomWarning] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const viewerContainerRef = useRef<HTMLDivElement | null>(null);
  const viewerRef = useRef<Viewer | null>(null);
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

  useEffect(() => {
    if (!activeRoom || !viewerContainerRef.current) {
      return;
    }

    setViewerError(null);
    setActiveInfo(null);
    setRoomWarning(null);

    let viewer: Viewer | null = null;

    try {
      viewer = new Viewer({
        container: viewerContainerRef.current,
        panorama: getMediaUrl(activeRoom.panoramaUrl),
        caption: activeRoom.name,
        description: activeRoom.name,
        defaultYaw: 0,
        defaultPitch: 0,
        defaultZoomLvl: 45,
        mousewheel: true,
        touchmoveTwoFingers: false,
        navbar: ["zoom", "move", "caption", "fullscreen"],
        loadingTxt: "جاري تحميل الصورة البانورامية...",
        plugins: [
          [
            MarkersPlugin,
            {
              markers: createMarkers(activeRoom.hotspots),
              defaultHoverScale: { amount: 1.08, duration: 120 }
            }
          ]
        ]
      });

      const markersPlugin = viewer.getPlugin<MarkersPlugin>(MarkersPlugin);

      markersPlugin.addEventListener("select-marker", (event) => {
        const markerData = event.marker.config.data as HotspotMarkerData | undefined;

        if (!markerData) {
          return;
        }

        handleMarkerSelect(markerData);
      });

      viewerRef.current = viewer;
    } catch {
      setViewerError("تعذر تشغيل عارض 360 على هذا الجهاز.");
    }

    return () => {
      viewerRef.current = null;
      viewer?.destroy();
    };
  }, [activeRoom?.id]);

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

  function handleMarkerSelect(markerData: HotspotMarkerData) {
    if (markerData.type === "Navigate") {
      if (markerData.targetRoomId && tour?.rooms.some((room) => room.id === markerData.targetRoomId)) {
        setActiveRoomId(markerData.targetRoomId);
        setActiveInfo(null);
        setRoomWarning(null);
      } else {
        setRoomWarning("الغرفة الهدف غير متاحة.");
      }

      return;
    }

    setActiveInfo(markerData.label);
    setRoomWarning(null);
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
          <div className="tour-panorama real-panorama-viewer" aria-label="عارض جولة 360">
            <div ref={viewerContainerRef} className="real-panorama-canvas" />
            {viewerError && (
              <div className="tour-viewer-fallback">
                <p>{viewerError}</p>
              </div>
            )}
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

          {roomWarning && <p className="form-error">{roomWarning}</p>}

          <p className="note">لأفضل تجربة، استخدم صورة بانورامية 2:1 بصيغة Equirectangular.</p>
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
                setRoomWarning(null);
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

function createMarkers(hotspots: PublicTourHotspot[]): MarkerConfig[] {
  return hotspots.map((hotspot) => ({
    id: hotspot.id,
    position: {
      yaw: `${hotspot.yaw}deg`,
      pitch: `${hotspot.pitch}deg`
    },
    html: createMarkerHtml(hotspot),
    className: hotspot.type === "Navigate" ? "psv-hotspot-nav" : "psv-hotspot-info",
    anchor: "center center",
    tooltip: {
      content: hotspot.label,
      position: "top center",
      trigger: "hover"
    },
    data: {
      hotspotId: hotspot.id,
      type: hotspot.type,
      targetRoomId: hotspot.targetRoomId,
      label: hotspot.label
    } satisfies HotspotMarkerData
  }));
}

function createMarkerHtml(hotspot: PublicTourHotspot): string {
  const icon = hotspot.type === "Navigate" ? "↗" : "i";
  const label = escapeHtml(hotspot.label);

  return `<span class="psv-hotspot-label"><span>${icon}</span>${label}</span>`;
}

function escapeHtml(value: string): string {
  return value
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#039;");
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

function toTourError(error: unknown): string {
  if (error instanceof ApiError && error.status === 404) {
    return "not-found";
  }

  return "generic";
}
