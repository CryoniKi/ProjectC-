namespace SplatoonLoadout.Models;

public enum Category
{
    Painters,
    JackOfAllTrades,
    AOE,
    Backlines
}

public class NameResolver
{
    public static string GetName(Category category) => category switch {
        Category.Painters => "Painters",
        Category.JackOfAllTrades => "Jack of all Trades",
        Category.AOE => "AOE",
        Category.Backlines => "Backlines",
        _ => throw new NotImplementedException($"Category {category} was not recognized")
    };
}