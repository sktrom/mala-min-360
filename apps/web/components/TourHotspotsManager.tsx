"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import {
  ApiError,
  createTourHotspot,
  deleteTourHotspot,
  getTourHotspots,
  updateTourHotspot
} from "@/lib/api";
import { getAccessToken } from "@/lib/auth-storage";
import type {
  CreateTourHotspotRequest,
  TourHotspot,
  TourHotspotType,
  TourRoom,
  UpdateTourHotspotRequest
} from "@/lib/types";

type TourHotspotsManagerProps = {
  propertyId: string;
  rooms: TourRoom[];
};

type HotspotFormState = {
  type: TourHotspotType;
  label: string;
  targetRoomId: string;
  yaw: string;
  pitch: string;
};

const initialForm: HotspotFormState = {
  type: "Navigate",
  label: "",
  targetRoomId: "",
  yaw: "0",
  pitch: "0"
};

const hotspotTypeLabels: Record<TourHotspotType, string> = {
  Navigate: "انتقال",
  Info: "معلومة"
};

export function TourHotspotsManager({ propertyId, rooms }: TourHotspotsManagerProps) {
  const [selectedRoomId, setSelectedRoomId] = useState("");
  const [hotspots, setHotspots] = useState<TourHotspot[]>([]);
  const [form, setForm] = useState<HotspotFormState>(initialForm);
  const [editingHotspotId, setEditingHotspotId] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [actionHotspotId, setActionHotspotId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const selectedRoom = rooms.find((room) => room.id === selectedRoomId) ?? null;
  const targetRooms = useMemo(
    () => rooms.filter((room) => room.id !== selectedRoomId),
    [rooms, selectedRoomId]
  );

  useEffect(() => {
    if (rooms.length === 0) {
      setSelectedRoomId("");
      setHotspots([]);
      return;
    }

    setSelectedRoomId((current) =>
      rooms.some((room) => room.id === current) ? current : rooms[0].id
    );
  }, [rooms]);

  useEffect(() => {
    if (!selectedRoomId) {
      return;
    }

    resetForm();
    void loadHotspots(selectedRoomId);
  }, [selectedRoomId]);

  if (rooms.length === 0) {
    return (
      <section className="tour-hotspots-manager">
        <p className="empty-state">أضف غرف 360 أولاً قبل إنشاء نقاط التفاعل.</p>
      </section>
    );
  }

  async function loadHotspots(roomId = selectedRoomId) {
    const token = getAccessToken();

    if (!token) {
      setError("يرجى تسجيل الدخول من جديد.");
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const loadedHotspots = await getTourHotspots(token, propertyId, roomId);
      setHotspots(
        [...loadedHotspots].sort(
          (first, second) =>
            new Date(first.createdAt).getTime() - new Date(second.createdAt).getTime()
        )
      );
    } catch {
      setError("تعذر تحميل نقاط الجولة.");
    } finally {
      setIsLoading(false);
    }
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const token = getAccessToken();

    if (!token) {
      setError("يرجى تسجيل الدخول من جديد.");
      return;
    }

    const request = buildRequestFromForm(form);
    const validationError = validateHotspotRequest(request);

    if (validationError) {
      setError(validationError);
      return;
    }

    setIsSubmitting(true);
    setError(null);
    setSuccess(null);

    try {
      if (editingHotspotId) {
        await updateTourHotspot(token, propertyId, selectedRoomId, editingHotspotId, request);
        setSuccess("تم تعديل نقطة التفاعل.");
      } else {
        await createTourHotspot(token, propertyId, selectedRoomId, request);
        setSuccess("تمت إضافة نقطة التفاعل.");
      }

      resetForm();
      await loadHotspots();
    } catch (submitError) {
      setError(toHotspotError(submitError));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDelete(hotspot: TourHotspot) {
    const token = getAccessToken();

    if (!token) {
      setError("يرجى تسجيل الدخول من جديد.");
      return;
    }

    const confirmed = window.confirm("هل تريد حذف نقطة التفاعل؟");

    if (!confirmed) {
      return;
    }

    setActionHotspotId(hotspot.id);
    setError(null);
    setSuccess(null);

    try {
      await deleteTourHotspot(token, propertyId, selectedRoomId, hotspot.id);
      setSuccess("تم حذف نقطة التفاعل.");
      await loadHotspots();
    } catch (deleteError) {
      setError(toHotspotError(deleteError));
    } finally {
      setActionHotspotId(null);
    }
  }

  function startEdit(hotspot: TourHotspot) {
    setEditingHotspotId(hotspot.id);
    setForm({
      type: hotspot.type,
      label: hotspot.label,
      targetRoomId: hotspot.targetRoomId ?? "",
      yaw: String(hotspot.yaw),
      pitch: String(hotspot.pitch)
    });
    setError(null);
    setSuccess(null);
  }

  function resetForm() {
    setEditingHotspotId(null);
    setForm({
      ...initialForm,
      targetRoomId: targetRooms[0]?.id ?? ""
    });
  }

  function updateForm(field: keyof HotspotFormState, value: string) {
    setForm((current) => {
      if (field === "type") {
        const type = value as TourHotspotType;
        return {
          ...current,
          type,
          targetRoomId: type === "Info" ? "" : current.targetRoomId
        };
      }

      return { ...current, [field]: value };
    });
  }

  return (
    <section className="tour-hotspots-manager">
      <div className="section-heading compact">
        <div>
          <p className="eyebrow">نقاط الجولة</p>
          <h3>نقاط التفاعل</h3>
        </div>
      </div>

      <label className="field room-selector">
        اختر الغرفة
        <select
          value={selectedRoomId}
          onChange={(event) => setSelectedRoomId(event.target.value)}
        >
          {rooms.map((room) => (
            <option value={room.id} key={room.id}>
              {room.name}
            </option>
          ))}
        </select>
      </label>

      <p className="note">
        حالياً يتم إدخال Yaw و Pitch يدوياً، وسيتم لاحقاً دعم التحديد البصري داخل عارض 360.
      </p>

      <form className="hotspot-form" onSubmit={handleSubmit}>
        <label className="field">
          نوع النقطة
          <select
            value={form.type}
            onChange={(event) =>
              updateForm("type", event.target.value as TourHotspotType)
            }
          >
            <option value="Navigate">انتقال</option>
            <option value="Info">معلومة</option>
          </select>
        </label>

        <label className="field">
          النص
          <input
            value={form.label}
            onChange={(event) => updateForm("label", event.target.value)}
            maxLength={150}
            required
          />
        </label>

        {form.type === "Navigate" && (
          <label className="field">
            الغرفة الهدف
            <select
              value={form.targetRoomId}
              onChange={(event) => updateForm("targetRoomId", event.target.value)}
              required
            >
              <option value="">اختر الغرفة الهدف</option>
              {targetRooms.map((room) => (
                <option value={room.id} key={room.id}>
                  {room.name}
                </option>
              ))}
            </select>
          </label>
        )}

        <label className="field">
          Yaw
          <input
            type="number"
            min="-180"
            max="180"
            step="0.01"
            value={form.yaw}
            onChange={(event) => updateForm("yaw", event.target.value)}
            required
          />
        </label>

        <label className="field">
          Pitch
          <input
            type="number"
            min="-90"
            max="90"
            step="0.01"
            value={form.pitch}
            onChange={(event) => updateForm("pitch", event.target.value)}
            required
          />
        </label>

        <div className="form-actions hotspot-actions">
          <button className="button primary" type="submit" disabled={isSubmitting}>
            {editingHotspotId ? "حفظ التعديل" : "إضافة نقطة"}
          </button>
          {editingHotspotId && (
            <button className="button secondary" type="button" onClick={resetForm}>
              إلغاء
            </button>
          )}
        </div>
      </form>

      {error && <p className="form-error">{error}</p>}
      {success && <p className="form-success">{success}</p>}

      {isLoading ? (
        <p className="note">جاري تحميل نقاط الجولة...</p>
      ) : hotspots.length === 0 ? (
        <p className="empty-state">لا توجد نقاط تفاعل لهذه الغرفة بعد.</p>
      ) : (
        <div className="hotspots-grid">
          {hotspots.map((hotspot) => (
            <article className="hotspot-card" key={hotspot.id}>
              <div className="badge-row">
                <span className="badge">
                  {hotspotTypeLabels[hotspot.type] ?? hotspot.type}
                </span>
                <span className="badge">
                  Yaw {formatCoordinate(hotspot.yaw)} / Pitch {formatCoordinate(hotspot.pitch)}
                </span>
              </div>
              <strong>{hotspot.label}</strong>
              {hotspot.type === "Navigate" && (
                <p>الهدف: {getRoomName(rooms, hotspot.targetRoomId)}</p>
              )}
              <div className="property-actions">
                <button
                  className="button secondary"
                  type="button"
                  disabled={actionHotspotId === hotspot.id}
                  onClick={() => startEdit(hotspot)}
                >
                  تعديل
                </button>
                <button
                  className="button danger"
                  type="button"
                  disabled={actionHotspotId === hotspot.id}
                  onClick={() => void handleDelete(hotspot)}
                >
                  حذف
                </button>
              </div>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}

function buildRequestFromForm(
  form: HotspotFormState
): CreateTourHotspotRequest | UpdateTourHotspotRequest {
  return {
    type: form.type,
    label: form.label.trim(),
    targetRoomId: form.type === "Navigate" ? form.targetRoomId || null : null,
    yaw: Number(form.yaw),
    pitch: Number(form.pitch)
  };
}

function validateHotspotRequest(
  request: CreateTourHotspotRequest | UpdateTourHotspotRequest
): string | null {
  if (request.label.length === 0 || request.label.length > 150) {
    return "تحقق من بيانات نقطة التفاعل.";
  }

  if (request.type !== "Navigate" && request.type !== "Info") {
    return "تحقق من بيانات نقطة التفاعل.";
  }

  if (request.type === "Navigate" && !request.targetRoomId) {
    return "الغرفة الهدف غير صالحة.";
  }

  if (request.type === "Info" && request.targetRoomId) {
    return "تحقق من بيانات نقطة التفاعل.";
  }

  if (!Number.isFinite(request.yaw) || request.yaw < -180 || request.yaw > 180) {
    return "تحقق من بيانات نقطة التفاعل.";
  }

  if (!Number.isFinite(request.pitch) || request.pitch < -90 || request.pitch > 90) {
    return "تحقق من بيانات نقطة التفاعل.";
  }

  return null;
}

function toHotspotError(error: unknown): string {
  if (error instanceof ApiError) {
    const code = error.code?.toUpperCase() ?? "";
    const message = error.message.toLowerCase();

    if (code.includes("TARGET") || message.includes("target")) {
      return "الغرفة الهدف غير صالحة.";
    }

    if (error.status === 400 || code.includes("VALIDATION")) {
      return "تحقق من بيانات نقطة التفاعل.";
    }
  }

  return "تعذر تنفيذ العملية. حاول مرة أخرى.";
}

function getRoomName(rooms: TourRoom[], roomId: string | null): string {
  if (!roomId) {
    return "غير محدد";
  }

  return rooms.find((room) => room.id === roomId)?.name ?? "غير محدد";
}

function formatCoordinate(value: number): string {
  return new Intl.NumberFormat("en-US", {
    maximumFractionDigits: 2
  }).format(value);
}
