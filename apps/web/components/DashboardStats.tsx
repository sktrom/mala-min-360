"use client";

import { useEffect, useState, useCallback } from "react";
import { getAccessToken } from "@/lib/auth-storage";
import { getStatsOverview } from "@/lib/api";
import { StatCard } from "./StatCard";
import type { StatsOverview } from "@/lib/types";

export function DashboardStats() {
  const [stats, setStats] = useState<StatsOverview | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchStats = useCallback(async () => {
    const token = getAccessToken();
    if (!token) {
      setError("يرجى تسجيل الدخول من جديد.");
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const data = await getStatsOverview(token);
      setStats(data);
    } catch (err: any) {
      setError("تعذر تحميل الإحصائيات.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchStats();
  }, [fetchStats]);

  if (loading && !stats) {
    return <p className="note">جاري تحميل الإحصائيات...</p>;
  }

  if (error) {
    return (
      <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
        <p className="form-error">{error}</p>
        <button
          className="button secondary"
          onClick={fetchStats}
          style={{ alignSelf: "flex-start" }}
        >
          تحديث
        </button>
      </div>
    );
  }

  // Safe mapping in case the backend returns different casing
  const totalViews = stats?.totalViews ?? (stats as any)?.TotalViews ?? 0;
  const totalTourViews = stats?.totalTourViews ?? (stats as any)?.TotalTourViews ?? 0;
  const totalWhatsAppClicks = stats?.totalWhatsAppClicks ?? (stats as any)?.TotalWhatsAppClicks ?? 0;
  const totalQrScans = stats?.totalQrScans ?? (stats as any)?.TotalQrScans ?? 0;

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
      <div style={{ display: "flex", justifyContent: "flex-end" }}>
        <button
          className="button secondary"
          onClick={fetchStats}
          disabled={loading}
          style={{ minHeight: "36px", padding: "6px 16px" }}
        >
          {loading ? "جاري التحميل..." : "تحديث"}
        </button>
      </div>
      <div className="grid dashboard-grid">
        <StatCard label="عدد المشاهدات" value={String(totalViews)} />
        <StatCard label="مشاهدات الجولة" value={String(totalTourViews)} />
        <StatCard label="ضغطات واتساب" value={String(totalWhatsAppClicks)} />
        <StatCard label="مسح QR" value={String(totalQrScans)} />
      </div>
    </div>
  );
}
