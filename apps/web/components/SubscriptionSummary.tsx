"use client";

import { useCallback, useEffect, useState } from "react";
import { getCurrentSubscription } from "@/lib/api";
import { getAccessToken } from "@/lib/auth-storage";
import type { CurrentSubscription } from "@/lib/types";

const statusLabels: Record<string, string> = {
  Trial: "تجربة",
  Active: "فعّال",
  Suspended: "معلّق",
  Expired: "منتهي"
};

export function SubscriptionSummary() {
  const [subscription, setSubscription] = useState<CurrentSubscription | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadSubscription = useCallback(async () => {
    const token = getAccessToken();

    if (!token) {
      setError("يرجى تسجيل الدخول من جديد.");
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const data = await getCurrentSubscription(token);
      setSubscription(data);
    } catch {
      setError("تعذر تحميل بيانات الاشتراك.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadSubscription();
  }, [loadSubscription]);

  if (loading && !subscription) {
    return <p className="note">جاري تحميل بيانات الاشتراك...</p>;
  }

  if (error) {
    return (
      <div className="subscription-summary">
        <p className="form-error">{error}</p>
        <button className="button secondary" type="button" onClick={loadSubscription}>
          تحديث
        </button>
      </div>
    );
  }

  const plan = subscription?.plan;
  const status = subscription?.status ?? "";
  const currentProperties = subscription?.currentProperties ?? 0;
  const maxProperties = plan?.maxProperties ?? 0;
  const currentTours = subscription?.currentTours ?? 0;
  const maxTours = plan?.maxTours ?? 0;
  const currentStorageMb = subscription?.currentStorageMb ?? 0;
  const storageLimitMb = plan?.storageLimitMb ?? 0;
  const statusLabel = statusLabels[status] ?? (status || "غير متوفر");

  return (
    <section className="subscription-summary panel">
      <div className="subscription-header">
        <div>
          <p className="eyebrow">الاشتراك الحالي</p>
          <h2>الباقة الحالية</h2>
        </div>
        <button
          className="button secondary"
          type="button"
          onClick={loadSubscription}
          disabled={loading}
        >
          {loading ? "جاري التحميل..." : "تحديث"}
        </button>
      </div>

      <div className="subscription-grid">
        <DetailItem label="اسم الباقة" value={plan?.name ?? "غير متوفر"} />
        <DetailItem label="رمز الباقة" value={plan?.code ?? "غير متوفر"} />
        <DetailItem label="حالة الاشتراك" value={statusLabel} />
        <DetailItem label="تاريخ البداية" value={formatDate(subscription?.startsAt)} />
        <DetailItem label="تاريخ الانتهاء" value={formatDate(subscription?.endsAt)} />
        <DetailItem label="السعر الشهري" value={`${formatNumber(plan?.monthlyPrice ?? 0)} USD`} />
      </div>

      <div className="usage-list">
        <UsageItem
          label="العقارات المستخدمة"
          current={currentProperties}
          limit={maxProperties}
        />
        <UsageItem
          label="الجولات المستخدمة"
          current={currentTours}
          limit={maxTours}
        />
        <UsageItem
          label="التخزين المستخدم"
          current={currentStorageMb}
          limit={storageLimitMb}
          unit="MB"
        />
      </div>
    </section>
  );
}

function DetailItem({ label, value }: { label: string; value: string }) {
  return (
    <div className="subscription-detail">
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  );
}

function UsageItem({
  label,
  current,
  limit,
  unit
}: {
  label: string;
  current: number;
  limit: number;
  unit?: string;
}) {
  const percentage = getUsagePercentage(current, limit);

  return (
    <div className="usage-item">
      <div className="usage-meta">
        <span>{label}</span>
        <strong>{formatUsage(current, limit, unit)}</strong>
      </div>
      <div className="progress-track" aria-hidden="true">
        <span className="progress-value" style={{ width: `${percentage}%` }} />
      </div>
    </div>
  );
}

function getUsagePercentage(current: number, limit: number): number {
  if (limit <= 0) {
    return 0;
  }

  return Math.min(100, Math.round((current / limit) * 100));
}

function formatUsage(current: number, limit: number, unit = ""): string {
  const suffix = unit ? ` ${unit}` : "";

  if (limit <= 0) {
    return `${formatNumber(current)}${suffix} / غير محدد`;
  }

  return `${formatNumber(current)}${suffix} / ${formatNumber(limit)}${suffix}`;
}

function formatDate(value?: string): string {
  if (!value) {
    return "غير متوفر";
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return "غير متوفر";
  }

  return new Intl.DateTimeFormat("ar-SY", {
    dateStyle: "medium"
  }).format(date);
}

function formatNumber(value: number): string {
  return new Intl.NumberFormat("en-US", {
    maximumFractionDigits: 2
  }).format(value);
}
