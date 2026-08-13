using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Infrastructure.Database.MongoDb;

namespace ZiziBot.Application.Features.UseCases.Rss;

public class GetRssHistoryRequest
{
    public long ChatId { get; set; }
    public int? ThreadId { get; set; }
    public string? RssUrl { get; set; }
    public HistoryType? HistoryType { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    internal bool IsPaginationRequested => Page.HasValue && PageSize.HasValue;
}

public class GetRssHistoryValidator : AbstractValidator<GetRssHistoryRequest>
{
    public GetRssHistoryValidator()
    {
        RuleFor(x => x.ChatId).NotEqual(0).WithMessage("ChatId is required");
    }
}

public class GetRssHistoryResponse
{
    public long ChatId { get; set; }
    public int? ThreadId { get; set; }
    public string? RssUrl { get; set; }
    public required string? ErrorMessage { get; set; }
    public DateTime PublishDate { get; set; }
    public required string? ArticleUrl { get; set; }
    public string? ArticleTitle { get; set; }
    public string? ArticleAuthor { get; set; }
    public HistoryType HistoryType { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class GetRssHistoryUseCase(MongoDbContext mongoDbContext)
{
    public async Task<ApiResponseBase<List<GetRssHistoryResponse>>> Handle(GetRssHistoryRequest request)
    {
        var response = ApiResponse.Create<List<GetRssHistoryResponse>>();

        // Default values for pagination if not provided
        var effectivePage = request.Page ?? 1;
        var effectivePageSize = request.PageSize ?? -1;

        var query = mongoDbContext.RssHistory.AsNoTracking()
            .Where(x => x.ChatId == request.ChatId)
            .WhereIf(request.ThreadId.HasValue, e => e.ThreadId == request.ThreadId)
            .WhereIf(!string.IsNullOrWhiteSpace(request.RssUrl), e => e.RssUrl == request.RssUrl)
            .WhereIf(request.HistoryType.HasValue, e => e.HistoryType == request.HistoryType);

        var totalItems = await query.CountAsync();

        if (request.IsPaginationRequested)
        {
            query = query.Skip((effectivePage - 1) * effectivePageSize).Take(effectivePageSize);
        }

        var listData = await query.OrderByDescending(x => x.CreatedDate).ToListAsync();

        var data = listData.Select(e => new GetRssHistoryResponse
        {
            ChatId = e.ChatId,
            ThreadId = e.ThreadId,
            RssUrl = e.RssUrl,
            ErrorMessage = e.ErrorMessage,
            ArticleUrl = e.Url,
            ArticleTitle = e.Title,
            ArticleAuthor = e.Author,
            PublishDate = e.PublishDate,
            CreatedDate = e.CreatedDate,
            HistoryType = e.HistoryType ?? HistoryType.Unknown
        }).ToList();

        return response.Success("OK", data).SetMetadata(pageSize: effectivePageSize, totalItem: totalItems, pageNumber: effectivePage, actualPageSize: listData.Count);
    }
}
