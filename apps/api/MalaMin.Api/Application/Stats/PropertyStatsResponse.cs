namespace MalaMin.Api.Application.Stats;

public sealed record PropertyStatsResponse(
    DateOnly StatDate,
    int Views,
    int TourViews,
    int WhatsAppClicks,
    int QrScans);
