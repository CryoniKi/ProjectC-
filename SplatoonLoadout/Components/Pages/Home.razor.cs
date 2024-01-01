using Microsoft.AspNetCore.Components;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using MudBlazor;
using SplatoonLoadout.Models;
using SplatoonLoadout.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace SplatoonLoadout.Components.Pages;
public partial class Home
{
    private const string WEAPON_URL = "https://raw.githubusercontent.com/CryoniKi/ProjectC-/main/SplatoonLoadout/AppResources/WeaponList.json";
    [Inject] private UpdateChecker UpdateChecker { get; set; } = default!;
    [Inject] private IHttpClientFactory HttpFactory { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private event EventHandler? _onUpdate;
    
    private Dictionary<Trait,bool> _chips = Enum.GetValues(typeof(Trait)).Cast<Trait>().ToDictionary(e => e, e => false);
    private readonly WeaponModel?[] _selected = new WeaponModel?[4];
    private List<WeaponModel> weapons = [];
    private List<Trait> _unallowedTraits = [];
    private Trait? _highlightTrait;

    #region initializers
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var (newVersion, latestTag) = await UpdateChecker.CheckForUpdate();
        if (newVersion) {
            NavigationManager.NavigateTo($"RequireUpdate/{latestTag}");
        }

        _onUpdate += UpdateAllowedTraits;
        _onUpdate += UpdateTraits;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender) {
            weapons = await GetWeapons();
            StateHasChanged();
        }
    }

    #endregion

    #region Updates
    private void TrySetActive(WeaponModel weapon)
    {
        for (var i = 0; i < _selected.Length; i++) {
            if (_selected[i] is null) {
                _selected[i] = weapon;
                _onUpdate?.Invoke(this, EventArgs.Empty);
                return;
            }
        }

        return;
    }

    private void TrySetInactive(int index)
    {
        _selected[index] = null;
        _onUpdate?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    private void UpdateAllowedTraits(object? sender, EventArgs args)
    {
        _unallowedTraits = CalculateAllowedTags().ToList();

        IEnumerable<Trait> CalculateAllowedTags()
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

            if (traitCounts[(int)Trait.Backline] >= 1)
                yield return Trait.Backline;
        }
    }

    private void UpdateTraits(object? sender, EventArgs args)
    {
        var selectedTraits = _selected.Where(e => e is not null).SelectMany(e => e.Trait).Distinct().ToArray();
        _chips = Enum.GetValues(typeof(Trait)).Cast<Trait>().ToDictionary(e => e, e => selectedTraits.Contains(e));
    }

    private void HighLightTag(Trait trait)
    {
        if(_highlightTrait == trait) {
            _highlightTrait = null;
        }
        else {
            _highlightTrait = trait;
        }
    }

    private async Task<List<WeaponModel>> GetWeapons()
    {
        try {
            using var client = HttpFactory.CreateClient();
            var result = await client.GetFromJsonAsync<List<WeaponModel>>(WEAPON_URL);
            if(result is null) {
                return GetListFromCache();
            }
            else {
                if (!Directory.Exists("cache")) Directory.CreateDirectory("cache");
                File.WriteAllText("cache/WeaponList.json", JsonSerializer.Serialize(result, new JsonSerializerOptions() { WriteIndented = true }));
            }

            return result;
        }
        catch {
            Snackbar.Add("Unable to get list from github", Severity.Warning);
            return GetListFromCache();
        }
    }

    private List<WeaponModel> GetListFromCache()
    {
        try {
            if (File.Exists("cache/WeaponList.json")) {
                return JsonSerializer.Deserialize<List<WeaponModel>>(File.ReadAllText("cache/WeaponList.json")) ?? throw new Exception("Result was null");
            }
            else {
                Snackbar.Add("No local cache to serve", Severity.Error);
                return [];
            }
        }
        catch {
            Snackbar.Add("Unable to serve local cache", Severity.Error);
            return [];
        }
        
    }
}
