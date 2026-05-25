namespace MalaMin.Api.Application.PropertyImages;

public sealed record AddPropertyImageRequest(
    Guid MediaFileId,
    int? SortOrder,
    bool? IsCover);
