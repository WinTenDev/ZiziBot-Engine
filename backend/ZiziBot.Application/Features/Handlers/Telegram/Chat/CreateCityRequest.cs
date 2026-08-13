using Microsoft.EntityFrameworkCore;
using ZiziBot.Application.Common.Types;
using ZiziBot.Application.Infrastructure.Database.MongoDb.Entities;

namespace ZiziBot.Application.Features.Handlers.Telegram.Chat;

public class AddCityBotRequest : BotRequestBase
{
    public int CityId { get; set; }
    public string? CityName { get; set; }
}

internal class AddCityHandler(
    DataFacade dataFacade,
    ServiceFacade serviceFacade
) : IBotRequestHandler<AddCityBotRequest>
{
    public async Task<BotResponseBase> Handle(AddCityBotRequest request, CancellationToken cancellationToken)
    {
        var htmlMessage = HtmlMessage.Empty;
        serviceFacade.TelegramService.SetupResponse(request);

        var cityInfoAll = await serviceFacade.FathimahRestService.GetAllCityAsync();
        var cityInfo = cityInfoAll.Cities
            .WhereIf(request.CityId > 0, kota => kota.Id == request.CityId)
            .WhereIf(request.CityName.IsNotNullOrEmpty(), kota => kota.Lokasi.Like(request.CityName))
            .FirstOrDefault();

        if (cityInfo == null)
        {
            return await serviceFacade.TelegramService.SendMessageText("Kota tidak ditemukan");
        }

        var city = await dataFacade.MongoDb.BangHasan_ShalatCity
            .Where(entity => entity.ChatId == request.ChatIdentifier)
            .Where(entity => entity.CityId == cityInfo.Id)
            .Where(entity => entity.Status == EventStatus.Complete)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        var cityMsg = HtmlMessage.Empty
            .Bold("ID Kota: ").CodeBr(cityInfo.Id.ToString())
            .Bold("Kota: ").CodeBr(cityInfo.Lokasi);

        if (city != null)
        {
            htmlMessage.Text("Kota sudah ditambahkan")
                .Br()
                .Append(cityMsg);
        }
        else
        {
            dataFacade.MongoDb.BangHasan_ShalatCity.Add(new BangHasan_ShalatCityEntity() {
                ChatId = request.ChatIdentifier,
                UserId = request.UserId,
                CityId = cityInfo.Id,
                CityName = cityInfo.Lokasi,
                Status = EventStatus.Complete
            });

            await dataFacade.MongoDb.SaveChangesAsync(cancellationToken);

            htmlMessage.Text("Kota berhasil disimpan")
                .Br()
                .Append(cityMsg);
        }

        return await serviceFacade.TelegramService.SendMessageText(htmlMessage.ToString());
    }
}
