﻿using Flurl;
using Flurl.Http;
using ZiziBot.Types.Vendor.Subdl;

namespace ZiziBot.Services;

public class SubdlService(ICacheService cacheService)
{
    public async Task<Popular> FetchPopular()
    {
        var url = UrlConst.API_SUBDL_BASE.AppendPathSegment("popular");
        var cache = await cacheService.GetOrSetAsync(
            cacheKey: $"vendor/{url.ToString().ForCacheKey()}",
            action: async () => {
                var response = await url.GetJsonAsync<Popular>();

                return response;
            });

        return cache;
    }

    public async Task<Popular> Search(string query)
    {
        var url = UrlConst.API_SUBDL_BASE
            .AppendPathSegment("auto")
            .SetQueryParam("query", query);

        var cache = await cacheService.GetOrSetAsync(
            cacheKey: $"vendor/{url.ToString().ForCacheKey()}",
            action: async () => {
                var response = await url.GetJsonAsync<Popular>();

                return response;
            });

        return cache;
    }
}