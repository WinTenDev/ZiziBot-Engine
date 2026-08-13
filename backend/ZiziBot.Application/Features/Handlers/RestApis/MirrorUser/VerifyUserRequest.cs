using FluentValidation;
using ZiziBot.Application.Common.Dtos;

namespace ZiziBot.Application.Features.Handlers.RestApis.MirrorUser;

public class VerifyUserRequest : ApiRequestBase<VerifyUserResponse, VerifyUserRequestBody>
{ }

public class VerifyUserRequestBody
{
    public long UserId { get; set; }
    public MirrorActivityType ActivityType { get; set; }
    public string Url { get; set; }
}

public class VerifyUserRequestValidator : AbstractValidator<VerifyUserRequest>
{
    public VerifyUserRequestValidator()
    {
        RuleFor(x => x.Body).SetValidator(new VerifyUserRequestBodyValidator());
    }
}

public class VerifyUserRequestBodyValidator : AbstractValidator<VerifyUserRequestBody>
{
    public VerifyUserRequestBodyValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.ActivityType).IsInEnum();
    }
}

public class VerifyUserResponse
{
    public long UserId { get; set; }
    public bool HasSubscription { get; set; }
    public DateTime? JoinDate { get; set; }
    public DateTime? ExpireDate { get; set; }
    public string TimeToExpiration { get; set; }
}

public class VerifyUserHandler(
    IHttpContextHelper httpContextHelper,
    DataFacade dataFacade
) : IApiRequestHandler<VerifyUserRequest, VerifyUserResponse>
{
    ApiResponseBase<VerifyUserResponse> Response { get; set; } = new();

    public async Task<ApiResponseBase<VerifyUserResponse>> Handle(VerifyUserRequest request, CancellationToken cancellationToken)
    {
        var mirrorUser = await dataFacade.MirrorUser.GetByUserId(request.Body.UserId);

        if (mirrorUser == null)
            return Response.BadRequest("Mirror User not found");

        if (mirrorUser.Status > EventStatus.Complete)
        {
            return Response.NotFound("Mirror User suspended");
        }

        await dataFacade.MirrorUser.SaveActivity(new MirrorActivityDto {
            UserId = mirrorUser.UserId,
            ActivityTypeId = request.Body.ActivityType,
            Url = request.Body.Url,
            TransactionId = httpContextHelper.UserInfo.TransactionId
        });

        return Response.Success("Mirror User verified successfully", new() {
            UserId = mirrorUser.UserId,
            HasSubscription = mirrorUser.ExpireDate > DateTime.UtcNow.AddHours(Env.DEFAULT_TIMEZONE),
            JoinDate = mirrorUser.CreatedDate.AddHours(Env.DEFAULT_TIMEZONE),
            ExpireDate = mirrorUser.ExpireDate.AddHours(Env.DEFAULT_TIMEZONE),
            TimeToExpiration = mirrorUser.ExpireDate.Subtract(DateTime.UtcNow.AddHours(Env.DEFAULT_TIMEZONE)).ForHuman(5, "en-us")
        });
    }
}