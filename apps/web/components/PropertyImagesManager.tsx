"use client";

import { ChangeEvent, useEffect, useMemo, useState } from "react";
import {
  addPropertyImage,
  ApiError,
  deletePropertyImage,
  getPropertyImages,
  setPropertyImageCover,
  uploadMedia
} from "@/lib/api";
import { getAccessToken } from "@/lib/auth-storage";
import type { PropertyImage } from "@/lib/types";
import { getMediaUrl } from "@/lib/url";

type PropertyImagesManagerProps = {
  propertyId: string;
  propertyTitle?: string;
};

const supportedMimeTypes = ["image/jpeg", "image/png", "image/webp"];

export function PropertyImagesManager({
  propertyId,
  propertyTitle
}: PropertyImagesManagerProps) {
  const [images, setImages] = useState<PropertyImage[]>([]);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isUploading, setIsUploading] = useState(false);
  const [actionImageId, setActionImageId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const nextSortOrder = useMemo(() => {
    if (images.length === 0) {
      return 0;
    }

    return Math.max(...images.map((image) => image.sortOrder)) + 1;
  }, [images]);

  useEffect(() => {
    void loadImages();
  }, [propertyId]);

  async function loadImages() {
    const token = getAccessToken();

    if (!token) {
      setError("يرجى تسجيل الدخول من جديد.");
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const loadedImages = await getPropertyImages(token, propertyId);
      setImages([...loadedImages].sort(compareImages));
    } catch {
      setError("تعذر تحميل الصور.");
    } finally {
      setIsLoading(false);
    }
  }

  async function handleUpload() {
    const token = getAccessToken();

    if (!token) {
      setError("يرجى تسجيل الدخول من جديد.");
      return;
    }

    if (!selectedFile) {
      setError("اختر صورة أولاً.");
      return;
    }

    if (!supportedMimeTypes.includes(selectedFile.type)) {
      setError("نوع الصورة غير مدعوم.");
      return;
    }

    setIsUploading(true);
    setError(null);
    setSuccess(null);

    try {
      const uploadedMedia = await uploadMedia(token, selectedFile, "NormalImage");
      await addPropertyImage(
        token,
        propertyId,
        uploadedMedia.id,
        nextSortOrder,
        images.length === 0
      );
      setSelectedFile(null);
      setSuccess("تم رفع الصورة وربطها بالعقار.");
      await loadImages();
    } catch (uploadError) {
      setError(toImageError(uploadError));
    } finally {
      setIsUploading(false);
    }
  }

  async function handleSetCover(image: PropertyImage) {
    const token = getAccessToken();

    if (!token) {
      setError("يرجى تسجيل الدخول من جديد.");
      return;
    }

    setActionImageId(image.id);
    setError(null);
    setSuccess(null);

    try {
      await setPropertyImageCover(token, propertyId, image.id);
      setSuccess("تم تعيين صورة الغلاف.");
      await loadImages();
    } catch (coverError) {
      setError(toImageError(coverError));
    } finally {
      setActionImageId(null);
    }
  }

  async function handleDelete(image: PropertyImage) {
    const token = getAccessToken();

    if (!token) {
      setError("يرجى تسجيل الدخول من جديد.");
      return;
    }

    const confirmed = window.confirm("هل تريد حذف رابط هذه الصورة من العقار؟");

    if (!confirmed) {
      return;
    }

    setActionImageId(image.id);
    setError(null);
    setSuccess(null);

    try {
      await deletePropertyImage(token, propertyId, image.id);
      setSuccess("تم حذف رابط الصورة.");
      await loadImages();
    } catch (deleteError) {
      setError(toImageError(deleteError));
    } finally {
      setActionImageId(null);
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
          <p className="eyebrow">صور العقار</p>
          <h3>{propertyTitle ? `إدارة صور ${propertyTitle}` : "إدارة الصور"}</h3>
        </div>
      </div>

      <div className="image-upload-row">
        <label className="field image-file-field">
          اختر صورة
          <input
            type="file"
            accept="image/jpeg,image/png,image/webp"
            onChange={handleFileChange}
          />
        </label>
        <button
          className="button primary"
          type="button"
          onClick={() => void handleUpload()}
          disabled={isUploading}
        >
          {isUploading ? "جاري الرفع..." : "رفع صورة"}
        </button>
      </div>

      {error && <p className="form-error">{error}</p>}
      {success && <p className="form-success">{success}</p>}

      {isLoading ? (
        <p className="note">جاري تحميل الصور...</p>
      ) : images.length === 0 ? (
        <p className="empty-state">لا توجد صور لهذا العقار بعد.</p>
      ) : (
        <div className="property-images-grid">
          {images.map((image) => (
            <article className="property-image-card" key={image.id}>
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img src={getMediaUrl(image.url)} alt={image.originalFileName} />
              <div className="property-image-body">
                <div className="badge-row">
                  {image.isCover && <span className="badge published">الغلاف</span>}
                  <span className="badge">{formatFileSize(image.sizeBytes)}</span>
                </div>
                <strong>{image.originalFileName}</strong>
                <p>{image.mimeType}</p>
                <div className="property-actions">
                  <button
                    className="button secondary"
                    type="button"
                    disabled={image.isCover || actionImageId === image.id}
                    onClick={() => void handleSetCover(image)}
                  >
                    تعيين كغلاف
                  </button>
                  <button
                    className="button danger"
                    type="button"
                    disabled={actionImageId === image.id}
                    onClick={() => void handleDelete(image)}
                  >
                    حذف الصورة
                  </button>
                </div>
              </div>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}

function toImageError(error: unknown): string {
  if (error instanceof ApiError) {
    const code = error.code?.toUpperCase() ?? "";
    const message = error.message.toLowerCase();

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

function compareImages(first: PropertyImage, second: PropertyImage): number {
  return (
    first.sortOrder - second.sortOrder ||
    new Date(first.createdAt).getTime() - new Date(second.createdAt).getTime()
  );
}
