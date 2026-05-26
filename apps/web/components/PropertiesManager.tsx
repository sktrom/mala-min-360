"use client";

import { FormEvent, useEffect, useState } from "react";
import {
  ApiError,
  createProperty,
  deleteProperty,
  getProperties,
  publishProperty,
  unpublishProperty
} from "@/lib/api";
import { getAccessToken } from "@/lib/auth-storage";
import type { CreatePropertyRequest, Property } from "@/lib/types";
import { PropertyImagesManager } from "./PropertyImagesManager";
import { TourRoomsManager } from "./TourRoomsManager";

type ActivePanel = {
  propertyId: string;
  panel: "images" | "tour";
} | null;

type PropertyFormState = {
  title: string;
  description: string;
  city: string;
  areaName: string;
  addressText: string;
  price: string;
  currency: string;
  listingType: string;
  propertyType: string;
  bedrooms: string;
  bathrooms: string;
  floorNumber: string;
  areaSqm: string;
};

const initialForm: PropertyFormState = {
  title: "",
  description: "",
  city: "دمشق",
  areaName: "",
  addressText: "",
  price: "",
  currency: "USD",
  listingType: "Sale",
  propertyType: "Apartment",
  bedrooms: "",
  bathrooms: "",
  floorNumber: "",
  areaSqm: ""
};

const statusLabels: Record<string, string> = {
  Available: "متاح",
  Reserved: "محجوز",
  Sold: "مباع",
  Rented: "مؤجر"
};

const listingTypeLabels: Record<string, string> = {
  Sale: "بيع",
  Rent: "إيجار"
};

const propertyTypeLabels: Record<string, string> = {
  Apartment: "شقة",
  Villa: "فيلا",
  House: "منزل",
  Shop: "محل",
  Office: "مكتب",
  Land: "أرض"
};

export function PropertiesManager() {
  const [properties, setProperties] = useState<Property[]>([]);
  const [form, setForm] = useState<PropertyFormState>(initialForm);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [actionPropertyId, setActionPropertyId] = useState<string | null>(null);
  const [activePanel, setActivePanel] = useState<ActivePanel>(null);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  useEffect(() => {
    void loadProperties();
  }, []);

  async function loadProperties() {
    const token = getAccessToken();

    if (!token) {
      setError("يرجى تسجيل الدخول من جديد.");
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const loadedProperties = await getProperties(token);
      setProperties(loadedProperties);
    } catch (loadError) {
      setError(toArabicError(loadError));
    } finally {
      setIsLoading(false);
    }
  }

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const token = getAccessToken();

    if (!token) {
      setError("يرجى تسجيل الدخول من جديد.");
      return;
    }

    setIsSubmitting(true);
    setError(null);
    setSuccess(null);

    try {
      await createProperty(token, createRequestFromForm(form));
      setForm(initialForm);
      setSuccess("تم إنشاء العقار بنجاح.");
      await loadProperties();
    } catch (createError) {
      setError(toArabicError(createError));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleTogglePublish(property: Property) {
    const token = getAccessToken();

    if (!token) {
      setError("يرجى تسجيل الدخول من جديد.");
      return;
    }

    setActionPropertyId(property.id);
    setError(null);
    setSuccess(null);

    try {
      if (property.isPublished) {
        await unpublishProperty(token, property.id);
        setSuccess("تم إلغاء نشر العقار.");
      } else {
        await publishProperty(token, property.id);
        setSuccess("تم نشر العقار.");
      }

      await loadProperties();
    } catch (publishError) {
      setError(toArabicError(publishError));
    } finally {
      setActionPropertyId(null);
    }
  }

  async function handleDelete(property: Property) {
    const token = getAccessToken();

    if (!token) {
      setError("يرجى تسجيل الدخول من جديد.");
      return;
    }

    const confirmed = window.confirm(`هل تريد حذف العقار "${property.title}"؟`);

    if (!confirmed) {
      return;
    }

    setActionPropertyId(property.id);
    setError(null);
    setSuccess(null);

    try {
      await deleteProperty(token, property.id);
      setSuccess("تم حذف العقار.");
      await loadProperties();
    } catch (deleteError) {
      setError(toArabicError(deleteError));
    } finally {
      setActionPropertyId(null);
    }
  }

  return (
    <div className="properties-workspace">
      <section className="panel property-form-panel">
        <div className="section-heading compact">
          <div>
            <p className="eyebrow">إضافة عقار</p>
            <h2>بيانات العقار الأساسية</h2>
          </div>
        </div>

        <form className="property-form" onSubmit={handleCreate}>
          <label className="field">
            العنوان
            <input
              value={form.title}
              onChange={(event) => updateForm("title", event.target.value)}
              required
              maxLength={250}
            />
          </label>

          <label className="field">
            المدينة
            <input
              value={form.city}
              onChange={(event) => updateForm("city", event.target.value)}
              required
              maxLength={100}
            />
          </label>

          <label className="field">
            المنطقة
            <input
              value={form.areaName}
              onChange={(event) => updateForm("areaName", event.target.value)}
              required
              maxLength={150}
            />
          </label>

          <label className="field">
            السعر
            <input
              type="number"
              min="0"
              step="0.01"
              value={form.price}
              onChange={(event) => updateForm("price", event.target.value)}
              required
            />
          </label>

          <label className="field">
            العملة
            <input
              value={form.currency}
              onChange={(event) => updateForm("currency", event.target.value)}
              required
              maxLength={10}
            />
          </label>

          <label className="field">
            نوع الإعلان
            <select
              value={form.listingType}
              onChange={(event) => updateForm("listingType", event.target.value)}
            >
              <option value="Sale">بيع</option>
              <option value="Rent">إيجار</option>
            </select>
          </label>

          <label className="field">
            نوع العقار
            <select
              value={form.propertyType}
              onChange={(event) => updateForm("propertyType", event.target.value)}
            >
              <option value="Apartment">شقة</option>
              <option value="Villa">فيلا</option>
              <option value="House">منزل</option>
              <option value="Shop">محل</option>
              <option value="Office">مكتب</option>
              <option value="Land">أرض</option>
            </select>
          </label>

          <label className="field">
            المساحة
            <input
              type="number"
              min="1"
              value={form.areaSqm}
              onChange={(event) => updateForm("areaSqm", event.target.value)}
              required
            />
          </label>

          <label className="field">
            عدد الغرف
            <input
              type="number"
              min="0"
              value={form.bedrooms}
              onChange={(event) => updateForm("bedrooms", event.target.value)}
            />
          </label>

          <label className="field">
            عدد الحمامات
            <input
              type="number"
              min="0"
              value={form.bathrooms}
              onChange={(event) => updateForm("bathrooms", event.target.value)}
            />
          </label>

          <label className="field">
            الطابق
            <input
              type="number"
              min="0"
              value={form.floorNumber}
              onChange={(event) => updateForm("floorNumber", event.target.value)}
            />
          </label>

          <label className="field">
            العنوان التفصيلي
            <input
              value={form.addressText}
              onChange={(event) => updateForm("addressText", event.target.value)}
            />
          </label>

          <label className="field full-width">
            الوصف
            <textarea
              value={form.description}
              onChange={(event) => updateForm("description", event.target.value)}
              rows={4}
            />
          </label>

          <div className="form-actions">
            <button className="button primary" type="submit" disabled={isSubmitting}>
              {isSubmitting ? "جاري الحفظ..." : "إنشاء عقار"}
            </button>
          </div>
        </form>
      </section>

      {error && <p className="form-error">{error}</p>}
      {success && <p className="form-success">{success}</p>}

      <section>
        <div className="section-heading compact">
          <div>
            <p className="eyebrow">القائمة</p>
            <h2>العقارات الحالية</h2>
          </div>
        </div>

        {isLoading ? (
          <p className="note">جاري تحميل العقارات...</p>
        ) : properties.length === 0 ? (
          <p className="empty-state">لا توجد عقارات بعد</p>
        ) : (
          <div className="grid properties-list">
            {properties.map((property) => (
              <article className="property-card" key={property.id}>
                <div className="property-thumb" aria-hidden="true" />
                <div className="property-card-body">
                  <div className="badge-row">
                    <span className={property.isPublished ? "badge published" : "badge draft"}>
                      {property.isPublished ? "منشور" : "غير منشور"}
                    </span>
                    <span className="badge">{statusLabels[property.status] ?? property.status}</span>
                  </div>
                  <h3>{property.title}</h3>
                  <p>
                    {property.city} · {property.areaName} · {property.areaSqm} م²
                  </p>
                  <p>
                    {listingTypeLabels[property.listingType] ?? property.listingType} ·{" "}
                    {propertyTypeLabels[property.propertyType] ?? property.propertyType}
                  </p>
                  <strong>
                    {formatPrice(property.price)} {property.currency}
                  </strong>
                  <div className="property-actions">
                    <button
                      className="button secondary"
                      type="button"
                      disabled={actionPropertyId === property.id}
                      onClick={() => void handleTogglePublish(property)}
                    >
                      {property.isPublished ? "إلغاء النشر" : "نشر"}
                    </button>
                    <button
                      className="button secondary"
                      type="button"
                      onClick={() => togglePanel(property.id, "images")}
                    >
                      {activePanel?.propertyId === property.id && activePanel.panel === "images"
                        ? "إغلاق الصور"
                        : "إدارة الصور"}
                    </button>
                    <button
                      className="button secondary"
                      type="button"
                      onClick={() => togglePanel(property.id, "tour")}
                    >
                      {activePanel?.propertyId === property.id && activePanel.panel === "tour"
                        ? "إغلاق الجولة"
                        : "إدارة جولة 360"}
                    </button>
                    <button
                      className="button danger"
                      type="button"
                      disabled={actionPropertyId === property.id}
                      onClick={() => void handleDelete(property)}
                    >
                      حذف
                    </button>
                  </div>
                  {activePanel?.propertyId === property.id && activePanel.panel === "images" && (
                    <PropertyImagesManager
                      propertyId={property.id}
                      propertyTitle={property.title}
                    />
                  )}
                  {activePanel?.propertyId === property.id && activePanel.panel === "tour" && (
                    <TourRoomsManager
                      propertyId={property.id}
                      propertyTitle={property.title}
                    />
                  )}
                </div>
              </article>
            ))}
          </div>
        )}
      </section>
    </div>
  );

  function updateForm(field: keyof PropertyFormState, value: string) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  function togglePanel(propertyId: string, panel: "images" | "tour") {
    setActivePanel((current) =>
      current?.propertyId === propertyId && current.panel === panel ? null : { propertyId, panel }
    );
  }
}

function createRequestFromForm(form: PropertyFormState): CreatePropertyRequest {
  return {
    title: form.title.trim(),
    description: normalizeOptionalText(form.description),
    city: form.city.trim(),
    areaName: form.areaName.trim(),
    addressText: normalizeOptionalText(form.addressText),
    price: Number(form.price),
    currency: form.currency.trim(),
    listingType: form.listingType,
    propertyType: form.propertyType,
    bedrooms: normalizeOptionalNumber(form.bedrooms),
    bathrooms: normalizeOptionalNumber(form.bathrooms),
    floorNumber: normalizeOptionalNumber(form.floorNumber),
    areaSqm: Number(form.areaSqm)
  };
}

function normalizeOptionalText(value: string): string | null {
  const trimmed = value.trim();
  return trimmed.length === 0 ? null : trimmed;
}

function normalizeOptionalNumber(value: string): number | null {
  return value.trim().length === 0 ? null : Number(value);
}

function toArabicError(error: unknown): string {
  if (error instanceof ApiError && error.code === "PLAN_LIMIT_EXCEEDED") {
    return "تم تجاوز حد العقارات في الباقة الحالية.";
  }

  if (error instanceof Error) {
    return error.message;
  }

  return "حدث خطأ غير متوقع. حاول مرة أخرى.";
}

function formatPrice(value: number): string {
  return new Intl.NumberFormat("en-US", {
    maximumFractionDigits: 2
  }).format(value);
}
