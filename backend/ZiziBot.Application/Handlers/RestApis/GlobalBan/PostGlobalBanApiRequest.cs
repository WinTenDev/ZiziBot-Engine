using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MongoFramework.Linq;

namespace ZiziBot.Application.Handlers.RestApis.GlobalBan;

public class PostGlobalBanApiRequest : ApiRequestBase<bool>
{
    [FromBody]
    public PostGlobalBanApiModel Model { get; set; }
}

public class PostGlobalBanApiModel
{
    public long UserId { get; set; }
    public string? Reason { get; set; }
}

public class PostGlobalBanApiValidator : AbstractValidator<PostGlobalBanApiRequest>
{
    public PostGlobalBanApiValidator()
    {
        RuleFor(x => x.Model.UserId).GreaterThan(0).WithMessage("UserId must be greater than 0");
    }
}

public class PostGlobalBanApiHandler : IRequestHandler<PostGlobalBanApiRequest, ApiResponseBase<bool>>
{
    private readonly MongoDbContextBase _mongoDbContext;

    public PostGlobalBanApiHandler(MongoDbContextBase mongoDbContext)
    {
        _mongoDbContext = mongoDbContext;
    }

    public async Task<ApiResponseBase<bool>> Handle(PostGlobalBanApiRequest request, CancellationToken cancellationToken)
    {
        var response = new ApiResponseBase<bool>();

        var globalBan = await _mongoDbContext.GlobalBan.FirstOrDefaultAsync(entity =>
                entity.UserId == request.Model.UserId &&
                entity.Status == (int)EventStatus.Complete,
            cancellationToken: cancellationToken);

        if (globalBan != null)
        {
            globalBan.Reason = request.Model.Reason;
        }
        else
        {
            _mongoDbContext.GlobalBan.Add(new GlobalBanEntity()
            {
                UserId = request.Model.UserId,
                Reason = request.Model.Reason,
                Status = (int)EventStatus.Complete
            });
        }

        await _mongoDbContext.SaveChangesAsync(cancellationToken);

        return response.Success("Global ban saved.", true);
    }
}