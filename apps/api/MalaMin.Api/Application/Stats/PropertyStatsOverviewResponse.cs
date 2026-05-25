namespace MalaMin.Api.Application.Stats;

public sealed record PropertyStatsOverviewResponse(
    Guid PropertyId,
    string PropertyTitle,
    int TotalViews,
    int TotalTourViews,
    int TotalWhatsAppClicks,
    int TotalQrScans,
    List<PropertyStatsResponse> DailyStats);
