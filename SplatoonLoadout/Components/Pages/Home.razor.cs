using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SplatoonLoadout.Components.Pages;
public partial class Home
{
    private readonly HttpClient _httpClient = new();
    private readonly WeaponModel?[] _selected = new WeaponModel?[4];
    private List<WeaponModel> weapons = [];
    private List<Trait> _unallowedTraits = [];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        weapons = await GetWeapons();
    }

    private void TrySetActive(WeaponModel weapon)
    {
        for (var i = 0; i < _selected.Length; i++) {
            if (_selected[i] is null) {
                _selected[i] = weapon;
                _unallowedTraits = CalculateAllowedTags();
                StateHasChanged();
                return;
            }
        }

        //TODO: Gracefully handle too many items;
        return;
    }

    private void TrySetInactive(int index)
    {
        _selected[index] = null;
        _unallowedTraits = CalculateAllowedTags();
    }

    private List<Trait> CalculateAllowedTags()
    {
        int support = 0, pushingSpecial = 0, frontline = 0;

        foreach(var weapon in _selected) {
            if(weapon is null) { continue; }
            if (weapon.Trait.Contains(Trait.Support)) { support++; }
            if (weapon.Trait.Contains(Trait.PushingSpecial)) { pushingSpecial++; }
            if (weapon.Trait.Contains(Trait.Frontline)) { frontline++; }
        }

        List<Trait> unallowedTraits = [ ];
        if(support >= 1) { unallowedTraits.Add(Trait.Support); }
        if(pushingSpecial >= 3) { unallowedTraits.Add(Trait.PushingSpecial); }
        if(frontline >= 2) { unallowedTraits.Add(Trait.Frontline); }

        return unallowedTraits;
    }

    private async Task<List<WeaponModel>> GetWeapons()
    {
        string url = "https://raw.githubusercontent.com/AsyncException/SplatoonLoadout/main/SplatoonLoadout/AppResources/WeaponList.json";
        var returnValue = await _httpClient.GetAsync(url) ?? throw new Exception("No internet bozo");

        var results = await returnValue.Content.ReadFromJsonAsync<List<WeaponModel>>();
        return results;
    }
}
