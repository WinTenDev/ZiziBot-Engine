namespace ZiziBot.Application.Services.Facades;

public class ServiceFacade(
    IAppMediator mediator,
    ICacheService cacheService,
    MediatorService mediatorService,
    JobService jobService,
    TelegramService telegramService,
    AntiSpamService antiSpamService,
    TonjooService tonjooService,
    BinderByteService binderByteService,
    FathimahRestService fathimahRestService,
    MirrorPaymentRestService mirrorPaymentRestService,
    OcrSpaceRestService ocrSpaceRestService,
    SubdlRestService subdlRestService,
    UupDumpService uupDumpService,
    WebhookService webhookService
)
{
    public IAppMediator Mediator => mediator;
    public ICacheService CacheService => cacheService;
    public MediatorService MediatorService => mediatorService;
    public JobService JobService => jobService;
    public TelegramService TelegramService => telegramService;
    public AntiSpamService AntiSpamService => antiSpamService;
    public TonjooService TonjooService => tonjooService;
    public BinderByteService BinderByteService => binderByteService;
    public FathimahRestService FathimahRestService => fathimahRestService;
    public MirrorPaymentRestService MirrorPaymentRestService => mirrorPaymentRestService;
    public OcrSpaceRestService OcrSpaceRestService => ocrSpaceRestService;
    public SubdlRestService SubdlRestService => subdlRestService;
    public UupDumpService UupDumpService => uupDumpService;
    public WebhookService WebhookService => webhookService;
}
