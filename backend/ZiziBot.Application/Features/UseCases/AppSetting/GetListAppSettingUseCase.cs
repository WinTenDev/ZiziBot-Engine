using Microsoft.EntityFrameworkCore;

namespace ZiziBot.Application.Features.UseCases.AppSetting;

public class GetListAppSettingRequest
{
    public AvailabilityStatus AvailabilityStatus { get; set; }
    public ConfigRoot? Root { get; set; }
    public string? Search { get; set; }
}

public class GetListAppSettingResponse
{
    public required string Root { get; set; }
    public required string Name { get; set; }
    public string? InitialValue { get; set; }
    public string? DefaultValue { get; set; }
    public string? Value { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class GetListAppSettingUseCase(AppSettingRepository appSettingRepository, DataFacade dataFacade)
{
    public async Task<ApiResponseBase<List<GetListAppSettingResponse>>> Handle(GetListAppSettingRequest request)
    {
        var response = ApiResponse.Create<List<GetListAppSettingResponse>>();

        var appSettingsEntities = await dataFacade.MongoDb.AppSettings.AsNoTracking()
            .WhereIf(request.AvailabilityStatus == AvailabilityStatus.Active, x => x.Status == EventStatus.Complete)
            .WhereIf(request.AvailabilityStatus == AvailabilityStatus.Deleted, x => x.Status == EventStatus.Deleted)
            .WhereIf(request.AvailabilityStatus == AvailabilityStatus.Inactive, x => x.Status == EventStatus.Inactive)
            .WhereIf(request.Root != null, x => x.Root == request.Root.ToString())
            .WhereIf(!string.IsNullOrWhiteSpace(request.Search), x => x.Name.Contains(request.Search ?? string.Empty) || x.Value.Contains(request.Search ?? string.Empty))
            .Select(x => new GetListAppSettingResponse {
                Root = x.Root ?? string.Empty,
                Name = x.Name,
                InitialValue = x.InitialValue,
                DefaultValue = x.DefaultValue,
                Value = x.Value,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate,
            }).ToListAsync();

        return response.Success("Get List App Setting Success", appSettingsEntities)
            .SetMetadata(totalItem: appSettingsEntities.Count);
    }
}