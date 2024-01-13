using MudBlazor;
using SplatoonLoadout.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SplatoonLoadout.Services;
public class WeaponService(CacheService cache, IHttpClientFactory factory)
{
    private const string WEAPON_URL = "https://raw.githubusercontent.com/CryoniKi/ProjectC-/main/SplatoonLoadout/AppResources/WeaponList.json";
    
    private readonly CacheService _cacheService = cache;
    private readonly IHttpClientFactory _httpClientFactory = factory;

    public async Task UpdateWhenRequired() {
        try {
            using var client = _httpClientFactory.CreateClient();
            var result = await client.GetFromJsonAsync<WeaponCollection>(WEAPON_URL);

            if(result is null) {
                //TODO: no internet or no connection.
                return;
            }

            if (result.Version > _cacheService.GetVersion()) {
                await _cacheService.WriteCache(result);
            }
        }
        catch {
            //TODO: no internet or no connection.
        }
    }

    public List<WeaponModel> GetWeapons() {
        return _cacheService.GetListFromCache();
    }
}
