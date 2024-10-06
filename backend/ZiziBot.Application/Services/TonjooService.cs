using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;

namespace ZiziBot.Application.Services;

public class TonjooService(ILogger<TonjooService> logger, AdditionalRepository additionalRepository)
{
    public async Task<string> GetAwbInfoMerged(string courier, string awb)
    {
        var htmlMessage = HtmlMessage.Empty;

        logger.LogInformation("Getting AWB info from DB. Courier: {Courier}, Awb: {Awb}", courier, awb);
        var storedAwb = await additionalRepository.GetStoredAwb(courier, awb);
        var detail = storedAwb?.Detail;

        if (storedAwb == null)
        {
            logger.LogDebug("Getting AWB info from API. Courier: {Courier}, Awb: {Awb}", courier, awb);

            var awbInfoRaw = await GetAwbInfoRaw(courier, awb);

            if (awbInfoRaw.Data == null)
            {
                htmlMessage.TextBr("Sepertinya no resi tidak ada");
                return htmlMessage.ToString();
            }

            detail = awbInfoRaw.Data?.AwbDetail;

            if (detail?.Status.Contains("delivered", StringComparison.InvariantCultureIgnoreCase) ?? false)
            {
                logger.LogDebug("Storing AWB info to DB. Courier: {Courier}, Awb: {Awb}", courier, awb);
                await additionalRepository.SaveAwbInfo(detail);
            }
        }

        logger.LogInformation("Merging AWB info. Courier: {Courier}, Awb: {Awb}", courier, awb);
        htmlMessage
            .Bold("📦 Ringkasan").Br()
            .Bold("Kurir: ").CodeBr(detail.Kurir.FirstOrDefault())
            .Bold("Resi: ").CodeBr(detail.Code)
            .Bold("Service: ").TextBr(detail.Service)
            .Bold("Status: ").TextBr(detail.Status)
            .Bold("Tanggal: ").Text(detail.DateShipment?.ToString("yyyy-MM-dd HH:mm")).Text(" -> ").TextBr(detail.DateReceived?.ToString("yyyy-MM-dd HH:mm"))
            .Br()
            .Bold("📜 Detail").Br()
            .Bold("Pengirim: ").TextBr(detail.Shipper.Name)
            .Bold("🎯 ").TextBr(detail.Shipper.Address)
            .Bold("Penerima: ").TextBr(detail.Consignee.Name)
            .Bold("🎯 ").TextBr(detail.Consignee.Address)
            .Br()
            .Bold("🕰 Riwayat").Br();

        detail.History.ForEach(history => {
            var date = history.Time.ToString("yyyy-MM-dd HH:mm");
            htmlMessage
                .Bold(date).Br()
                .TextBr("└ " + history.Desc)
                .Br();
        });

        htmlMessage
            .Bold("Ditenagai oleh: ").Url("https://tonjoostudio.com", "tonjoostudio.com");

        return htmlMessage.ToString();
    }

    public async Task<CheckAwb> GetAwbInfoRaw(string courier, string awb)
    {
        var flurlResponse = await UrlConst.API_TONJOO_ONGKIR.AppendPathSegment("/front/resi")
            .PostJsonAsync(new
            {
                kurir = courier,
                resi = awb
            });

        var checkAwb = await flurlResponse.GetJsonAsync<CheckAwb>();

        return checkAwb;
    }
}