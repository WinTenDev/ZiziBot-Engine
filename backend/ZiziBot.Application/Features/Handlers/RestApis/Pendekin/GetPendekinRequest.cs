using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Infrastructure.Database.Utils;

namespace ZiziBot.Application.Features.Handlers.RestApis.Pendekin;

public class GetPendekinRequest : ApiRequestBase<GetPendekinResponse>
{
    [FromRoute]
    public string ShortPath { get; set; }
}

public class GetPendekinRequestValidator : AbstractValidator<GetPendekinRequest>
{
    public GetPendekinRequestValidator()
    {
        RuleFor(x => x.ShortPath).NotNull().NotEmpty();
    }
}

public class GetPendekinResponse
{
    public string PendekinId { get; set; }
    public string OriginalUrl { get; set; }
    public string ShortUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class GetPendekinHandler(
    IHttpContextHelper httpContextHelper,
    DataFacade dataFacade
) : IApiRequestHandler<GetPendekinRequest, GetPendekinResponse>
{
    public async Task<ApiResponseBase<GetPendekinResponse>> Handle(GetPendekinRequest request, CancellationToken cancellationToken)
    {
        var response = ApiResponse.Create<GetPendekinResponse>();
        var pendekinMap = await dataFacade.MongoDb.PendekinMap.AsNoTracking()
            .WhereIf(!request.ShortPath.IsObjectId(), x => x.ShortPath == request.ShortPath)
            .WhereIf(request.ShortPath.IsObjectId(), x => x.Id == request.ShortPath.ToObjectId())
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (pendekinMap == null)
            return response.BadRequest("Pendekin not found");

        var pendekinConfig = await dataFacade.AppSetting.GetConfigSectionAsync<PendekinConfig>();

        if (pendekinConfig == null)
            return response.BadRequest("Pendekin not yet prepared");

        return response.Success("Get Pendekin successfully", new GetPendekinResponse() {
            PendekinId = pendekinMap.Id.ToString(),
            OriginalUrl = pendekinMap.OriginalUrl,
            ShortUrl = pendekinConfig.RouterBaseUrl.IsNotNullOrWhiteSpace() ? $"{pendekinConfig.RouterBaseUrl}/{pendekinMap.ShortPath}" : string.Empty,
            CreatedDate = pendekinMap.CreatedDate,
            UpdatedDate = pendekinMap.UpdatedDate
        });
    }
}
