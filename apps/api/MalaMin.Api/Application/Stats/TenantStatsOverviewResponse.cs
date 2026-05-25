namespace MalaMin.Api.Application.Stats;

public sealed record TenantStatsOverviewResponse(
    int TotalViews,
    int TotalTourViews,
    int TotalWhatsAppClicks,
    int TotalQrScans);
