using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZiziBot.DataSource.Utils;

public class GetPendekinRequest : ApiRequestBase<GetPendekinResponse>
{
    [FromRoute] public string ShortPath { get; set; }
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
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class GetPendekinHandler(
    DataFacade dataFacade
) : IApiRequestHandler<GetPendekinRequest, GetPendekinResponse>
{
    public async Task<ApiResponseBase<GetPendekinResponse>> Handle(GetPendekinRequest request, CancellationToken cancellationToken)
    {
        var pendekinMap = await dataFacade.MongoEf.PendekinMap.AsNoTracking()
            .WhereIf(!request.ShortPath.IsObjectId(), x => x.ShortPath == request.ShortPath)
            .WhereIf(request.ShortPath.IsObjectId(), x => x.Id == request.ShortPath.ToObjectId())
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (pendekinMap == null)
            return ApiResponseBase.ReturnBadRequest<GetPendekinResponse>("Pendekin not found");

        return ApiResponseBase.ReturnSuccess("Get Pendekin successfully", new GetPendekinResponse() {
            PendekinId = pendekinMap.Id.ToString(),
            OriginalUrl = pendekinMap.OriginalUrl,
            CreatedDate = pendekinMap.CreatedDate,
            UpdatedDate = pendekinMap.UpdatedDate
        });
    }
}