namespace SrpLab;

public sealed class KitchenTicket
{
    private readonly List<KitchenItem> _items = new List<KitchenItem>();
    private readonly AllergenScanner _allergenScanner = new AllergenScanner();

    public void AddItem(string item, IEnumerable<string> ingredients, int prepMinutes)
    {
        _items.Add(new KitchenItem(item, ingredients.Select(x => x.Trim().ToLowerInvariant()).ToList(), prepMinutes));
    }

    public IReadOnlyList<string> DetectAllergens()
    {
        return _allergenScanner.Detect(_items);
    }

    public int EstimatedReadyMinutes(int openStations)
    {
        return new KitchenEtaCalculator().Calculate(_items, openStations, DetectAllergens().Count > 0);
    }

    public string RenderThermalTicket(int orderNumber)
    {
        return new ThermalTicketRenderer().Render(orderNumber, _items, EstimatedReadyMinutes(2), DetectAllergens());
    }

    public string ExpoLaneHint()
    {
        return new ExpoLaneDecider().Choose(DetectAllergens().Count > 0, EstimatedReadyMinutes(2));
    }
}

public class KitchenItem
{
    public string Name { get; }
    public List<string> Ingredients { get; }
    public int PrepMinutes { get; }

    public KitchenItem(string name, List<string> ingredients, int prepMinutes)
    {
        Name = name;
        Ingredients = ingredients;
        PrepMinutes = prepMinutes;
    }
}

public class AllergenScanner
{
    public IReadOnlyList<string> Detect(IEnumerable<KitchenItem> items)
    {
        var allergens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            foreach (var ingredient in item.Ingredients)
            {
                if (ingredient.Contains("milk") || ingredient.Contains("cheese") || ingredient.Contains("butter")) allergens.Add("dairy");
                if (ingredient.Contains("wheat") || ingredient.Contains("flour") || ingredient.Contains("bread")) allergens.Add("gluten");
                if (ingredient.Contains("peanut") || ingredient.Contains("almond") || ingredient.Contains("cashew")) allergens.Add("nuts");
                if (ingredient.Contains("shrimp") || ingredient.Contains("prawn") || ingredient.Contains("crab")) allergens.Add("shellfish");
            }
        }
        return allergens.OrderBy(x => x).ToList();
    }
}

public class KitchenEtaCalculator
{
    public int Calculate(IReadOnlyList<KitchenItem> items, int openStations, bool hasAllergens)
    {
        if (openStations <= 0) openStations = 1;
        var totalMinutes = items.Sum(x => x.PrepMinutes);
        var estimated = (int)Math.Ceiling(totalMinutes / (double)openStations);
        if (hasAllergens) estimated += 3;
        var longest = items.Count == 0 ? 0 : items.Max(x => x.PrepMinutes);
        return Math.Max(estimated, longest);
    }
}

public class ThermalTicketRenderer
{
    public string Render(int orderNumber, IEnumerable<KitchenItem> items, int eta, IReadOnlyList<string> allergens)
    {
        var line = new string('=', 32);
        var body = string.Join('\n', items.Select(x => $"* {x.Name.ToUpperInvariant()} ({x.PrepMinutes}m)"));
        var allergyLine = allergens.Count == 0 ? "ALLERGENS: none" : "ALLERGENS: " + string.Join(',', allergens);
        return $"{line}\nORDER #{orderNumber}\nETA {eta} MIN\n{body}\n{allergyLine}\n{line}\n";
    }
}

public class ExpoLaneDecider
{
    public string Choose(bool hasAllergens, int eta)
    {
        if (hasAllergens) return "LANE-ALLERGY";
        return eta > 20 ? "LANE-SLOW" : "LANE-FAST";
    }
}
