using Microsoft.EntityFrameworkCore;

namespace ZiziBot.Application.Features.UseCases.ApiKey;

public class GetListApiKeyRequest
{
    public AvailabilityStatus AvailabilityStatus { get; set; }
    public ApiKeyVendor? Vendor { get; set; }
    public string? SearchQuery { get; set; }
}

public class GetListApiKeyResponse
{
    public required string ObjectId { get; set; }
    public int VendorId { get; set; }
    public required string VendorName { get; set; }
    public required string ApiKey { get; set; }
    public DateTime? LastUsedDate { get; set; }
    public int? Usage { get; set; }
    public int? Remaining { get; set; }
    public int? Limit { get; set; }
    public string? LimitUnit { get; set; }
    public DateTimeOffset? ResetUsageDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class GetListApiKeyUseCase(DataFacade dataFacade)
{
    private readonly ApiResponseBase<List<GetListApiKeyResponse>> response = new();

    public async Task<ApiResponseBase<List<GetListApiKeyResponse>>> Handle(GetListApiKeyRequest request)
    {
        var query = await dataFacade.MongoDb.ApiKey
            .WhereIf(request.AvailabilityStatus == AvailabilityStatus.Active, x => x.Status == EventStatus.Complete)
            .WhereIf(request.AvailabilityStatus == AvailabilityStatus.Deleted, x => x.Status == EventStatus.Deleted)
            .WhereIf(request.AvailabilityStatus == AvailabilityStatus.Inactive, x => x.Status == EventStatus.Inactive)
            .WhereIf(request.Vendor != null, x => x.Name == request.Vendor)
            .ToListAsync();

        var apiKeys = query.Select(x => new GetListApiKeyResponse() {
            ObjectId = x.Id.ToString(),
            VendorId = (int)x.Name,
            VendorName = x.Name.ToString(),
            ApiKey = x.ApiKey,
            LastUsedDate = x.LastUsedDate,
            Usage = x.Usage,
            Remaining = x.Remaining,
            Limit = x.Limit,
            LimitUnit = x.LimitUnit,
            ResetUsageDate = x.ResetUsageDate,
            CreatedDate = x.CreatedDate,
            UpdatedDate = x.UpdatedDate
        }).ToList();

        return response.Success("Get list Api Key successfully", apiKeys);
    }
}