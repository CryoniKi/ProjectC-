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
    private const string WEAPON_URL = "https://raw.githubusercontent.com/CryoniKi/ProjectC-/main/SplatoonLoadout/AppResources/WeaponList.json";

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
                _unallowedTraits = CalculateAllowedTags().ToList();
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
        _unallowedTraits = CalculateAllowedTags().ToList();
    }

    private IEnumerable<Trait> CalculateAllowedTags()
    {
        int[] traitCounts = new int[Enum.GetValues(typeof(Trait)).Length];

        foreach (var weapon in _selected.Where(e => e != null))
            foreach (var trait in weapon!.Trait)
                traitCounts[(int)trait]++;

        if (traitCounts[(int)Trait.Support] >= 1)
            yield return Trait.Support;

        if (traitCounts[(int)Trait.PushingSpecial] >= 3)
            yield return Trait.PushingSpecial;

        if (traitCounts[(int)Trait.Frontline] >= 3)
            yield return Trait.Frontline;

        if (traitCounts[(int)Trait.Backline] >= 1) {
            yield return Trait.Backline;
        }
    }

    private async Task<List<WeaponModel>> GetWeapons()
    {
        try {
            HttpResponseMessage responseMessage = await _httpClient.GetAsync(WEAPON_URL);

            //return an empty list on null reference
            List<WeaponModel> weaponList = (await responseMessage.Content.ReadFromJsonAsync<List<WeaponModel>>()) ?? [];
            return weaponList;
        }
        catch {
            //return empty on error in the chain.
            //TODO: add a more robust error checking.
            return [];
        }
    }
}
