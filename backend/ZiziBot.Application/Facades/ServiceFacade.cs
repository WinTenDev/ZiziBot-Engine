using Hangfire;
using ZiziBot.Interfaces;

namespace ZiziBot.Application.Facades;

public class ServiceFacade(
    IMediator mediator,
    IRecurringJobManager recurringJobManager,
    ICacheService cacheService,
    MediatorService mediatorService,
    TelegramService telegramService,
    AntiSpamService antiSpamService,
    SudoService sudoService,
    TonjooService tonjooService,
    BinderByteService binderByteService,
    FathimahApiService fathimahApiService,
    MirrorPaymentService mirrorPaymentService,
    NoteService noteService,
    OcrSpaceService ocrSpaceService,
    SubdlService subdlService,
    UupDumpService uupDumpService,
    WebhookService webhookService
)
{
    public IMediator Mediator { get; } = mediator;
    public IRecurringJobManager RecurringJobManager { get; } = recurringJobManager;
    public ICacheService CacheService { get; } = cacheService;
    public MediatorService MediatorService { get; } = mediatorService;
    public TelegramService TelegramService { get; } = telegramService;
    public AntiSpamService AntiSpamService { get; } = antiSpamService;
    public SudoService SudoService { get; } = sudoService;
    public TonjooService TonjooService { get; } = tonjooService;
    public BinderByteService BinderByteService { get; } = binderByteService;
    public FathimahApiService FathimahApiService { get; } = fathimahApiService;
    public MirrorPaymentService MirrorPaymentService { get; } = mirrorPaymentService;
    public NoteService NoteService { get; } = noteService;
    public OcrSpaceService OcrSpaceService { get; } = ocrSpaceService;
    public SubdlService SubdlService { get; } = subdlService;
    public UupDumpService UupDumpService { get; } = uupDumpService;
    public WebhookService WebhookService { get; } = webhookService;
}