namespace SrpLab;

public sealed class WarehousePickList
{
    private readonly List<PickNeed> _lines = new List<PickNeed>();

    public void AddNeed(string sku, string aisle, int bin, int qtyNeeded, int qtyOnHand)
    {
        _lines.Add(new PickNeed(sku, aisle, bin, qtyNeeded, qtyOnHand));
    }

    public IReadOnlyList<(string Sku, int Allocated)> Allocate()
    {
        return new StockAllocator().Allocate(_lines);
    }

    public IReadOnlyList<(string Aisle, int Bin, string Sku, int Qty)> WalkingOrder()
    {
        return new WarehousePathPlanner().Create(_lines);
    }

    public string PickerScript()
    {
        return new PickerScriptWriter().Write(WalkingOrder(), Allocate(), _lines);
    }

    public string WmsXmlBatch(string batchId)
    {
        return new WmsXmlExporter().Export(batchId, Allocate());
    }
}

public class PickNeed
{
    public string Sku { get; }
    public string Aisle { get; }
    public int Bin { get; }
    public int QtyNeeded { get; }
    public int QtyOnHand { get; }

    public PickNeed(string sku, string aisle, int bin, int qtyNeeded, int qtyOnHand)
    {
        Sku = sku;
        Aisle = aisle;
        Bin = bin;
        QtyNeeded = qtyNeeded;
        QtyOnHand = qtyOnHand;
    }
}

public class StockAllocator
{
    public IReadOnlyList<(string Sku, int Allocated)> Allocate(IEnumerable<PickNeed> needs)
    {
        return needs.Select(x => (x.Sku, Math.Min(x.QtyNeeded, x.QtyOnHand))).ToList();
    }
}

public class WarehousePathPlanner
{
    public IReadOnlyList<(string Aisle, int Bin, string Sku, int Qty)> Create(IEnumerable<PickNeed> needs)
    {
        return needs.OrderBy(x => x.Aisle)
            .ThenBy(x => x.Bin)
            .Select(x => (x.Aisle, x.Bin, x.Sku, Math.Min(x.QtyNeeded, x.QtyOnHand)))
            .Where(x => x.Item4 > 0)
            .ToList();
    }
}

public class PickerScriptWriter
{
    public string Write(IReadOnlyList<(string Aisle, int Bin, string Sku, int Qty)> walkingOrder, IReadOnlyList<(string Sku, int Allocated)> allocation, IEnumerable<PickNeed> needs)
    {
        var steps = walkingOrder.Select((x, index) => $"{index + 1}. Go aisle {x.Aisle} bin {x.Bin}: pick {x.Qty} × {x.Sku}");
        var shortages = allocation.Where(x => x.Allocated < needs.First(n => n.Sku == x.Sku).QtyNeeded).Select(x => x.Sku);
        var warning = shortages.Any() ? "SHORTAGES: " + string.Join(", ", shortages) : "SHORTAGES: none";
        return string.Join('\n', steps) + "\n" + warning;
    }
}

public class WmsXmlExporter
{
    public string Export(string batchId, IEnumerable<(string Sku, int Allocated)> allocation)
    {
        var lines = allocation.Select(x => $"<line sku=\"{x.Sku}\" qty=\"{x.Allocated}\" />");
        return $"<batch id=\"{batchId}\">{string.Join("", lines)}</batch>";
    }
}
