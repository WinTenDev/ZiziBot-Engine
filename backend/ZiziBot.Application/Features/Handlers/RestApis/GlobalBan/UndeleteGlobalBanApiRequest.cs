using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ZiziBot.Application.Features.Handlers.RestApis.GlobalBan;

public class UndeleteGlobalBanApiRequest : ApiRequestBase<bool>
{
    public long UserId { get; set; }
}

public class UndeleteGlobalBanApiValidator : AbstractValidator<UndeleteGlobalBanApiRequest>
{
    public UndeleteGlobalBanApiValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}

public class UndeleteGlobalBanApiHandler(
    DataFacade dataFacade
) : IApiRequestHandler<UndeleteGlobalBanApiRequest, bool>
{
    public async Task<ApiResponseBase<bool>> Handle(UndeleteGlobalBanApiRequest request, CancellationToken cancellationToken)
    {
        var response = new ApiResponseBase<bool>();

        var globalBan = await dataFacade.MongoDb.GlobalBan
            .Where(entity => entity.UserId == request.UserId)
            .Where(entity => entity.Status == EventStatus.Complete)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (globalBan == null)
        {
            return response.BadRequest("Global ban not found.", false);
        }

        globalBan.Status = EventStatus.Complete;

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return response.Success("Global ban undeleted.", true);
    }
}