using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SplatoonLoadout;
public class WeaponModel
{
    public string Name { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public Category Category { get; set; }
    public Trait[] Trait { get; set; } = [];
}

public enum Trait
{
    Support,
    PushingSpecial,
    LethalBomb,
    Range,
    Paint,
    Frontline,
    Backline
}

public enum Category
{
    FrontlineSpeed,
    FrontlineControl,
    MidlineControl,
    MidlineAOE,
    NoRangeBackline,
    Backline,
    Range
}

public class NameResolver
{
    public static string GetName(Category category) => category switch {
            Category.FrontlineSpeed => "Frontline Speed",
            Category.FrontlineControl => "Frontline Control",
            Category.MidlineControl => "Midline Control",
            Category.MidlineAOE => "Midline AOE",
            Category.NoRangeBackline => "No range backline",
            Category.Backline => "Backline",
            Category.Range => "Range",
            _ => throw new NotImplementedException($"Category {category} was not recognized")
        };
}