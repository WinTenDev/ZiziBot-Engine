using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;
using MongoFramework.Linq;

namespace ZiziBot.Application.Handlers.RestApis.Group;

public class SaveWelcomeMessageRequest : ApiRequestBase<object>
{
    [FromBody]
    public SaveWelcomeMessageRequestModel Model { get; set; }
}

public class SaveWelcomeMessageValidation : AbstractValidator<SaveWelcomeMessageRequest>
{
    public SaveWelcomeMessageValidation()
    {
        RuleFor(x => x.Model.ChatId).NotEqual(0);
        RuleFor(x => x.Model.Text).NotEmpty().WithMessage("Text is required");
    }
}

public class SaveWelcomeMessageRequestModel
{
    public string? Id { get; set; }
    public long ChatId { get; set; }
    public string Text { get; set; }
    public string? RawButton { get; set; }
    public string? Media { get; set; }
    public int DataType { get; set; } = -1;

    [BindNever]
    public ObjectId ObjectId => Id != null ? new ObjectId(Id) : ObjectId.Empty;
}

public class SaveWelcomeMessageHandler : IRequestHandler<SaveWelcomeMessageRequest, ApiResponseBase<object>>
{
    private readonly GroupDbContext _groupDbContext;

    public SaveWelcomeMessageHandler(GroupDbContext groupDbContext)
    {
        _groupDbContext = groupDbContext;
    }

    public async Task<ApiResponseBase<object>> Handle(SaveWelcomeMessageRequest request, CancellationToken cancellationToken)
    {
        var response = new ApiResponseBase<object>();

        if (!request.AdminChatId.Contains(request.Model.ChatId))
        {
            return response.BadRequest("You are not admin of this group");
        }

        var findWelcome = await _groupDbContext.WelcomeMessage
            .FirstOrDefaultAsync(x => x.Id == request.Model.ObjectId, cancellationToken);

        if (findWelcome == null)
        {

            var welcomeMessage = await _groupDbContext.WelcomeMessage
                .FirstOrDefaultAsync(x => x.ChatId == request.Model.ChatId, cancellationToken);

            _groupDbContext.WelcomeMessage.Add(new WelcomeMessageEntity
            {
                ChatId = request.Model.ChatId,
                Text = request.Model.Text,
                RawButton = request.Model.RawButton,
                Media = request.Model.Media,
                DataType = request.Model.DataType,
                Status = welcomeMessage == null ? (int)EventStatus.Complete : (int)EventStatus.Inactive,
                TransactionId = request.TransactionId
            });
        }
        else
        {
            findWelcome.Text = request.Model.Text;
            findWelcome.RawButton = request.Model.RawButton;
            findWelcome.Media = request.Model.Media;
            findWelcome.DataType = request.Model.DataType;
            findWelcome.TransactionId = request.TransactionId;
            findWelcome.Status = (int)EventStatus.InProgress;
        }

        await _groupDbContext.SaveChangesAsync(cancellationToken);

        return response.Success("Save Welcome Message successfully", true);
    }
}