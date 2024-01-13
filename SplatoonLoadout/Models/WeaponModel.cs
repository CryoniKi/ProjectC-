namespace SplatoonLoadout.Models;

public class WeaponCollection {
    public Version Version { get; set; } = new Version(1, 0, 0);
    public List<WeaponModel> Weapons { get; set; } = [];
}

public class WeaponModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public Category Category { get; set; }
    public Trait[] Trait { get; set; } = [];
}