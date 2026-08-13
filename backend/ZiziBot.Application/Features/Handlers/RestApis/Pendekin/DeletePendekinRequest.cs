using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Infrastructure.Database.Utils;

namespace ZiziBot.Application.Features.Handlers.RestApis.Pendekin;

public class DeletePendekinRequest : ApiRequestBase<DeletePendekinResponse>
{
    [FromRoute]
    public string ShortPath { get; set; }
}

public class DeletePendekinRequestValidator : AbstractValidator<DeletePendekinRequest>
{
    public DeletePendekinRequestValidator()
    {
        RuleFor(x => x.ShortPath).NotNull().NotEmpty();
    }
}

public class DeletePendekinResponse
{
}

public class DeletePendekinHandler(
    DataFacade dataFacade
) : IApiRequestHandler<DeletePendekinRequest, DeletePendekinResponse>
{
    public async Task<ApiResponseBase<DeletePendekinResponse>> Handle(DeletePendekinRequest request, CancellationToken cancellationToken)
    {
        var response = ApiResponse.Create<DeletePendekinResponse>();

        var pendekinMap = await dataFacade.MongoDb.PendekinMap
            .WhereIf(request.ShortPath.IsObjectId(), x => x.Id == request.ShortPath.ToObjectId())
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (pendekinMap == null)
            return response.BadRequest("Pendekin not found");

        pendekinMap.Status = EventStatus.Deleted;

        await dataFacade.SaveChangesAsync();

        return response.Success("Delete Pendekin successfully");
    }
}
