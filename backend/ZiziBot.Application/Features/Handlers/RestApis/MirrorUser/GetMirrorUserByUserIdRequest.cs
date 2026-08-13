using FluentValidation;

namespace ZiziBot.Application.Features.Handlers.RestApis.MirrorUser;

public class GetMirrorUserByUserIdRequestDto : ApiRequestBase<GetMirrorUserDto>
{
    public long UserId { get; set; }
}

public class GetMirrorUserByUserIdValidation : AbstractValidator<GetMirrorUserByUserIdRequestDto>
{
    public GetMirrorUserByUserIdValidation()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("UserId is required");
    }
}

public class GetMirrorUserDto
{
    public long UserId { get; set; }
    public bool HasSubscription { get; set; }
    public DateTime? SubscriptionUntil { get; set; }
    public DateTime? MemberSince { get; set; }
}

public class GetMirrorUserByUserIdRequestHandler(
    DataFacade dataFacade
)
    : IApiRequestHandler<GetMirrorUserByUserIdRequestDto, GetMirrorUserDto>
{
    readonly ApiResponseBase<GetMirrorUserDto> _response = new();

    public async Task<ApiResponseBase<GetMirrorUserDto>> Handle(GetMirrorUserByUserIdRequestDto request, CancellationToken cancellationToken)
    {
        var user = await dataFacade.MirrorUser.GetByUserId(request.UserId);

        if (user == null)
        {
            return _response.NotFound("Mirror User not found");
        }

        if (user.Status > EventStatus.Complete)
        {
            return _response.NotFound("Mirror User suspended");
        }

        return _response.Success("Mirror User found", new() {
            UserId = request.UserId,
            HasSubscription = user.ExpireDate > DateTime.UtcNow.AddHours(Env.DEFAULT_TIMEZONE),
            SubscriptionUntil = user.ExpireDate.AddHours(Env.DEFAULT_TIMEZONE),
            MemberSince = user.CreatedDate.AddHours(Env.DEFAULT_TIMEZONE)
        });
    }
}
