using FluentValidation;
using MongoFramework.Linq;

namespace ZiziBot.Application.Handlers.RestApis.GlobalBan;

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
) : IRequestHandler<UndeleteGlobalBanApiRequest, ApiResponseBase<bool>>
{
    public async Task<ApiResponseBase<bool>> Handle(UndeleteGlobalBanApiRequest request, CancellationToken cancellationToken)
    {
        var response = new ApiResponseBase<bool>();

        var globalBan = await dataFacade.MongoDb.GlobalBan
            .FirstOrDefaultAsync(entity =>
                    entity.UserId == request.UserId,
                cancellationToken: cancellationToken);

        if (globalBan == null)
        {
            return response.BadRequest("Global ban not found.", false);
        }

        globalBan.Status = (int)EventStatus.Complete;

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return response.Success("Global ban undeleted.", true);
    }
}