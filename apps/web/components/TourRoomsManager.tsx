"use client";

import { ChangeEvent, FormEvent, useEffect, useMemo, useState } from "react";
import {
  ApiError,
  createTourRoom,
  deleteTourRoom,
  getTourRooms,
  setTourRoomStart,
  uploadMedia
} from "@/lib/api";
import { getAccessToken } from "@/lib/auth-storage";
import type { TourRoom } from "@/lib/types";
import { getMediaUrl } from "@/lib/url";
import { TourHotspotsManager } from "./TourHotspotsManager";

type TourRoomsManagerProps = {
  propertyId: string;
  propertyTitle?: string;
};

const supportedMimeTypes = ["image/jpeg", "image/png", "image/webp"];

export function TourRoomsManager({ propertyId, propertyTitle }: TourRoomsManagerProps) {
  const [rooms, setRooms] = useState<TourRoom[]>([]);
  const [roomName, setRoomName] = useState("");
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [fileInputKey, setFileInputKey] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [actionRoomId, setActionRoomId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const nextSortOrder = useMemo(() => {
    if (rooms.length === 0) {
      return 0;
    }

    return Math.max(...rooms.map((room) => room.sortOrder)) + 1;
  }, [rooms]);

  useEffect(() => {
    void loadRooms();
  }, [propertyId]);

  async function loadRooms() {
    const token = getAccessToken();

    if (!token) {
      setError("يرجى تسجيل الدخول من جديد.");
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const loadedRooms = await getTourRooms(token, propertyId);
      setRooms([...loadedRooms].sort(compareRooms));
    } catch {
      setError("تعذر تحميل غرف الجولة.");
    } finally {
      setIsLoading(false);
    }
  }

  async function handleCreateRoom(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const token = getAccessToken();

    if (!token) {
      setError("يرجى تسجيل الدخول من جديد.");
      return;
    }

    if (roomName.trim().length === 0) {
      setError("أدخل اسم الغرفة.");
      return;
    }

    if (!selectedFile) {
      setError("اختر صورة 360 أولاً.");
      return;
    }

    if (!supportedMimeTypes.includes(selectedFile.type)) {
      setError("نوع الصورة غير مدعوم.");
      return;
    }

    setIsSubmitting(true);
    setError(null);
    setSuccess(null);

    try {
      const uploadedMedia = await uploadMedia(token, selectedFile, "Panorama360");
      await createTourRoom(token, propertyId, {
        name: roomName.trim(),
        panoramaMediaId: uploadedMedia.id,
        sortOrder: nextSortOrder,
        isStartRoom: rooms.length === 0
      });
      setRoomName("");
      setSelectedFile(null);
      setFileInputKey((current) => current + 1);
      setSuccess("تمت إضافة غرفة 360.");
      await loadRooms();
    } catch (createError) {
      setError(toTourRoomError(createError));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleSetStart(room: TourRoom) {
    const token = getAccessToken();

    if (!token) {
      setError("يرجى تسجيل الدخول من جديد.");
      return;
    }

    setActionRoomId(room.id);
    setError(null);
    setSuccess(null);

    try {
      await setTourRoomStart(token, propertyId, room.id);
      setSuccess("تم تعيين غرفة البداية.");
      await loadRooms();
    } catch (startError) {
      setError(toTourRoomError(startError));
    } finally {
      setActionRoomId(null);
    }
  }

  async function handleDelete(room: TourRoom) {
    const token = getAccessToken();

    if (!token) {
      setError("يرجى تسجيل الدخول من جديد.");
      return;
    }

    const confirmed = window.confirm("هل تريد حذف هذه الغرفة من الجولة؟");

    if (!confirmed) {
      return;
    }

    setActionRoomId(room.id);
    setError(null);
    setSuccess(null);

    try {
      await deleteTourRoom(token, propertyId, room.id);
      setSuccess("تم حذف الغرفة من الجولة.");
      await loadRooms();
    } catch (deleteError) {
      setError(toTourRoomError(deleteError));
    } finally {
      setActionRoomId(null);
    }
  }

  function handleFileChange(event: ChangeEvent<HTMLInputElement>) {
    setSelectedFile(event.target.files?.[0] ?? null);
    setError(null);
    setSuccess(null);
  }

  return (
    <section className="property-images-manager">
      <div className="section-heading compact">
        <div>
          <p className="eyebrow">جولة 360</p>
          <h3>{propertyTitle ? `غرف جولة ${propertyTitle}` : "غرف الجولة"}</h3>
        </div>
      </div>

      <form className="tour-room-form" onSubmit={handleCreateRoom}>
        <label className="field">
          اسم الغرفة
          <input
            value={roomName}
            onChange={(event) => setRoomName(event.target.value)}
            maxLength={150}
            required
          />
        </label>
        <label className="field image-file-field">
          صورة 360
          <input
            key={fileInputKey}
            type="file"
            accept="image/jpeg,image/png,image/webp"
            onChange={handleFileChange}
          />
        </label>
        <div className="form-actions">
          <button className="button primary" type="submit" disabled={isSubmitting}>
            {isSubmitting ? "جاري الإضافة..." : "إضافة غرفة 360"}
          </button>
        </div>
      </form>

      {error && <p className="form-error">{error}</p>}
      {success && <p className="form-success">{success}</p>}

      {isLoading ? (
        <p className="note">جاري تحميل غرف الجولة...</p>
      ) : rooms.length === 0 ? (
        <p className="empty-state">لا توجد غرف 360 لهذا العقار بعد.</p>
      ) : (
        <>
          <div className="property-images-grid">
            {rooms.map((room) => (
              <article className="property-image-card" key={room.id}>
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img src={getMediaUrl(room.panoramaUrl)} alt={room.name} />
                <div className="property-image-body">
                  <div className="badge-row">
                    {room.isStartRoom && <span className="badge published">غرفة البداية</span>}
                    <span className="badge">ترتيب {room.sortOrder}</span>
                  </div>
                  <strong>{room.name}</strong>
                  <p>{room.originalFileName}</p>
                  <p>{room.mimeType} · {formatFileSize(room.sizeBytes)}</p>
                  <div className="property-actions">
                    <button
                      className="button secondary"
                      type="button"
                      disabled={room.isStartRoom || actionRoomId === room.id}
                      onClick={() => void handleSetStart(room)}
                    >
                      تعيين كبداية
                    </button>
                    <button
                      className="button danger"
                      type="button"
                      disabled={actionRoomId === room.id}
                      onClick={() => void handleDelete(room)}
                    >
                      حذف الغرفة
                    </button>
                  </div>
                </div>
              </article>
            ))}
          </div>
          <TourHotspotsManager propertyId={propertyId} rooms={rooms} />
        </>
      )}
    </section>
  );
}

function toTourRoomError(error: unknown): string {
  if (error instanceof ApiError) {
    const code = error.code?.toUpperCase() ?? "";
    const message = error.message.toLowerCase();

    if (code === "PLAN_LIMIT_EXCEEDED") {
      return "تم تجاوز حد الجولات في الباقة الحالية.";
    }

    if (code.includes("PANORAMA") || message.includes("panorama")) {
      return "يجب رفع صورة من نوع Panorama360.";
    }

    if (code.includes("FILE_TYPE") || code.includes("MIME") || message.includes("type")) {
      return "نوع الصورة غير مدعوم.";
    }

    if (code.includes("SIZE") || message.includes("size") || error.status === 413) {
      return "حجم الصورة أكبر من الحد المسموح.";
    }
  }

  return "تعذر تنفيذ العملية. حاول مرة أخرى.";
}

function formatFileSize(sizeBytes: number): string {
  if (sizeBytes < 1024 * 1024) {
    return `${Math.max(1, Math.round(sizeBytes / 1024))} KB`;
  }

  return `${(sizeBytes / 1024 / 1024).toFixed(1)} MB`;
}

function compareRooms(first: TourRoom, second: TourRoom): number {
  return (
    first.sortOrder - second.sortOrder ||
    new Date(first.createdAt).getTime() - new Date(second.createdAt).getTime()
  );
}
