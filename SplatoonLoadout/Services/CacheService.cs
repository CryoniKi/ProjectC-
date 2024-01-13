using LiteDB;
using MudBlazor;
using SplatoonLoadout.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace SplatoonLoadout.Services;
public class CacheService(ILiteDatabase database, ISnackbar snackbar, IHttpClientFactory httpClientFactory)
{
    private readonly ISnackbar _snackbar = snackbar;
    private readonly ILiteDatabase _database = database;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public Version GetVersion() {
        return _database.GetCollection<Version>().FindAll().ToList().First();
    }

    public List<WeaponModel> GetListFromCache() {
        try {
            var weapons = _database.GetCollection<WeaponModel>().FindAll().ToList();
            
            if(weapons.Count <= 0) {
                _snackbar.Add("No local cache to serve", Severity.Error);
                return [];
            }

            return weapons;
        }
        catch {
            _snackbar.Add("Unable to serve local cache", Severity.Error);
            return [];
        }
    }

    private const string BASE_URL = "https://raw.githubusercontent.com/CryoniKi/ProjectC-/main/SplatoonLoadout/AppResources/Images/";
    public async Task WriteCache(WeaponCollection weapons) {
        var collection = _database.GetCollection<WeaponModel>();
        var versionCollection = _database.GetCollection<Version>();
        collection.DeleteAll();
        versionCollection.DeleteAll();

        collection.InsertBulk(weapons.Weapons, weapons.Weapons.Count);
        versionCollection.Insert(weapons.Version);

        await WriteImages(weapons);
    }

    private async Task WriteImages(WeaponCollection weapons) {
        var fs = _database.FileStorage;
        
        using var client = _httpClientFactory.CreateClient();
        foreach(var weapon in weapons.Weapons) {
            if (!fs.Exists(weapon.IconUrl)) {
                var stream = await client.GetStreamAsync(BASE_URL + weapon.IconUrl);
                fs.Upload(weapon.IconUrl, weapon.IconUrl, stream);
            }
        }
    }
}
