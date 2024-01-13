using Microsoft.AspNetCore.Components;
using MudBlazor;
using SplatoonLoadout.Models;
using SplatoonLoadout.Services;

namespace SplatoonLoadout.Components.Pages;
public partial class Main
{
    [Inject] private WeaponService WeaponService { get; set; } = default!;

    private event EventHandler? OnUpdate;

    private readonly WeaponModel?[] _selected = new WeaponModel?[4];
    private string? _search = string.Empty;
    private Dictionary<Trait,bool> _chips = Enum.GetValues(typeof(Trait)).Cast<Trait>().ToDictionary(e => e, e => false);
    private List<WeaponModel> weapons = [];
    private List<Trait> _unallowedTraits = [];
    private Trait? _highlightTrait;

    #region initializers
    protected override async Task OnParametersSetAsync()
    {
        await base.OnInitializedAsync();
        weapons = WeaponService.GetWeapons();

        OnUpdate += UpdateAllowedTraits;
        OnUpdate += UpdateTraits;
    }

    #endregion

    #region Updates
    private void TrySetActive(WeaponModel weapon)
    {
        for (var i = 0; i < _selected.Length; i++) {
            if (_selected[i] is null) {
                _selected[i] = weapon;
                OnUpdate?.Invoke(this, EventArgs.Empty);
                return;
            }
        }

        return;
    }

    private void TrySetInactive(int index)
    {
        _selected[index] = null;
        OnUpdate?.Invoke(this, EventArgs.Empty);
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

    private void HighLightTag(Trait trait) => _highlightTrait = _highlightTrait == trait ? null : trait;
}
