using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ZiziBot.DataSource.MongoEf.Entities;

public class CreatePendekinRequest : ApiPostRequestBase<CreatePendekinRequestBody, CreatePendekinResponse>
{ }

public class CreatePendekinRequestBody
{
    public string OriginalUrl { get; set; }
    public string? ShortPath { get; set; }
}

public class CreatePendekinValidation : AbstractValidator<CreatePendekinRequest>
{
    public CreatePendekinValidation()
    {
        RuleFor(x => x.Body).NotNull();

        When(x => x.Body != null, () => {
            RuleFor(x => x.Body.OriginalUrl).NotEmpty();
        });
    }
}

public class CreatePendekinResponse
{
    public string OriginalUrl { get; set; }
    public string ShortPath { get; set; }
}

public class CreatePendekinHandler(
    DataFacade dataFacade
) : IApiRequestHandler<CreatePendekinRequest, CreatePendekinResponse>
{
    public async Task<ApiResponseBase<CreatePendekinResponse>> Handle(CreatePendekinRequest request, CancellationToken cancellationToken)
    {
        var shortPath = request.Body.ShortPath ?? StringUtil.GetNanoId();

        var pendekinMap = await dataFacade.MongoEf.PendekinMap.FirstOrDefaultAsync(x => x.ShortPath == request.Body.ShortPath);

        if (pendekinMap != null)
            return ApiResponseBase.ReturnBadRequest<CreatePendekinResponse>("Pendekin Path is already exist");

        dataFacade.MongoEf.PendekinMap.Add(new PendekinMapEntity() {
            OriginalUrl = request.Body.OriginalUrl,
            ShortPath = shortPath,
            Status = EventStatus.Complete,
            TransactionId = request.TransactionId
        });

        await dataFacade.MongoEf.SaveChangesAsync(cancellationToken);

        return ApiResponseBase.ReturnSuccess("Create Pendekin successfully", new CreatePendekinResponse() {
            OriginalUrl = request.Body.OriginalUrl,
            ShortPath = shortPath
        });
    }
}