using System.Text;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using MongoFramework.Linq;
using MoreLinq;
using ZiziBot.DataSource.MongoDb.Entities;

namespace ZiziBot.Application.Services;

public class BinderByteService(
    ILogger<BinderByteService> logger,
    AppSettingRepository appSettingRepository,
    MongoDbContextBase mongoDbContext,
    CacheService cacheService)
{
    private BinderByteConfig? _binderByteConfig = new();

    public async Task<string> CekResiMergedAsync(string courier, string awb)
    {
        var sb = new StringBuilder();

        _binderByteConfig = await appSettingRepository.GetConfigSectionAsync<BinderByteConfig>();
        if (_binderByteConfig?.ApiKey == null || _binderByteConfig?.BaseUrl == null ||
            _binderByteConfig.IsEnabled == false)
        {
            sb.Append("Cek Resi sepertinya belum dipersiapkan");
            return sb.ToString();
        }

        logger.LogInformation("Getting AWB info from DB. Courier: {Courier}, Awb: {Awb}", courier, awb);
        var storedAwb = await GetStoredAwb(awb);
        var awbInfo = storedAwb?.AwbInfo;

        if (storedAwb?.AwbInfo == null)
        {
            logger.LogInformation("Getting AWB info from API. Courier: {Courier}, Awb: {Awb}", courier, awb);
            var result = await CekResiRawAsync(courier, awb);

            if (result.Status != 200)
            {
                var message = result.Message;

                sb.AppendLine("<b>Kesalahan: </b>400")
                    .Append("<b>Pesan: </b>");

                if (message.Contains("data not found", StringComparison.CurrentCultureIgnoreCase))
                {
                    sb.Append("Sepertinya no resi tidak ada");
                }
                else
                {
                    sb.Append("Sesuatu telah terjadi silakan hubungi Administrator");
                    logger.LogError("BinderByte CekResi failed. {@V}", result);
                }

                return sb.ToString();
            }

            awbInfo = result.AwbInfo;
        }

        var summary = awbInfo.Summary;
        var detail = awbInfo.Detail;
        var histories = awbInfo.History;

        sb.AppendLine("<b>📦 Ringkasan</b>")
            .Append("Kurir: ").AppendLine(summary.Courier)
            .Append("Status: ").AppendLine(summary.Status)
            .Append("Tanggal: ").AppendLine(summary.Date)
            .Append("Deskripsi: ").AppendLine(summary.Desc)
            .Append("Berat: ").AppendLine(summary.Weight)
            .AppendLine();

        sb.AppendLine("<b>📜 Detail</b>")
            .Append("Origin: ").AppendLine(detail.Origin)
            .Append("Tujuan: ").AppendLine(detail.Destination)
            .Append("Shipper: ").AppendLine(detail.Shipper)
            .Append("Penerima: ").AppendLine(detail.Receiver)
            .AppendLine();

        sb.AppendLine("<b>🕰 Riwayat</b>");

        histories
            .OrderBy(x => x.Date)
            .ForEach((history, index) => {
                    sb.Append(index + 1).Append(". ").AppendLine(history.Date.ToString())
                        .Append("└ ").AppendLine(history.Desc)
                        .AppendLine();
                }
            );

        var mergedResult = sb.ToString().Trim();

        return mergedResult;
    }

    public async Task<BinderByteCheckAwbEntity?> GetStoredAwb(string awb)
    {
        var collection = await mongoDbContext.BinderByteCheckAwb
            .FirstOrDefaultAsync(resi => resi.AwbInfo.Summary.Awb == awb);

        return collection;
    }

    public async Task SaveAwbInfo(AwbInfo data)
    {
        mongoDbContext.BinderByteCheckAwb.Add(new BinderByteCheckAwbEntity() {
            Awb = data.Summary.Awb,
            Courier = data.Summary.Courier,
            AwbInfo = data,
            Status = (int)EventStatus.Complete
        });

        await mongoDbContext.SaveChangesAsync();
    }

    public async Task<ApiResponse> CekResiRawAsync(string courier, string awb)
    {
        var result = await cacheService.GetOrSetAsync(
            cacheKey: $"cek-resi/{courier}/{awb}",
            staleAfter: "1s",
            expireAfter: "1d",
            action: async () => {
                var response = await CekResiRawCoreAsync(courier, awb);

                var status = response.AwbInfo?.Summary?.Status;

                if (status?.Contains("delivered", StringComparison.CurrentCultureIgnoreCase) ?? false)
                {
                    await SaveAwbInfo(response.AwbInfo);
                }

                return response;
            }
        );

        return result;
    }

    public async Task<ApiResponse> CekResiRawCoreAsync(string courier, string awb)
    {
        var res = await UrlConst.API_BINDERBYTE
            .AppendPathSegment("track")
            .SetQueryParam("api_key", _binderByteConfig?.ApiKey)
            .SetQueryParam("courier", courier)
            .SetQueryParam("awb", awb)
            .AllowHttpStatus("400")
            .GetJsonAsync<ApiResponse>();

        return res;
    }
}