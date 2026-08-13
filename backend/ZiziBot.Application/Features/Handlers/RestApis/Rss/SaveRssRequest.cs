using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Features.Handlers.RestApis.Rss;

public class SaveRssRequest : ApiRequestBase<bool>
{
    [FromBody]
    public SaveRssRequestBody Body { get; set; }
}

public class SaveRssRequestBody
{
    public string Url { get; set; }
    public long ChatId { get; set; }
}

public class SaveRssValidation : AbstractValidator<SaveRssRequest>
{
    public SaveRssValidation()
    {
        RuleFor(x => x.Body.ChatId).NotEqual(0).WithMessage("ChatId is Required");
        RuleFor(x => x.Body.Url).NotEmpty().WithMessage("URL is required");
    }
}

public class SaveRssHandler(
    IHttpContextHelper httpContextHelper,
    DataFacade dataFacade
) : IApiRequestHandler<SaveRssRequest, bool>
{
    public async Task<ApiResponseBase<bool>> Handle(SaveRssRequest request, CancellationToken cancellationToken)
    {
        ApiResponseBase<bool> response = new();

        if (!httpContextHelper.UserInfo.ListChatId.Contains(request.Body.ChatId))
        {
            return response.BadRequest($"Kamu tidak mempunyai akses ke ChatId: {request.Body.ChatId}");
        }

        var rss = await dataFacade.MongoDb.RssSetting
            .Where(entity => entity.ChatId == request.Body.ChatId)
            .Where(entity => entity.RssUrl == request.Body.Url)
            .Where(entity => entity.Status == EventStatus.Complete)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (rss == null)
        {
            dataFacade.MongoDb.RssSetting.Add(new RssSettingEntity() {
                RssUrl = request.Body.Url,
                ChatId = request.Body.ChatId,
                Status = EventStatus.Complete
            });
        }

        await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

        return response.Success("RSS Berhasil disimpan", true);
    }
}
