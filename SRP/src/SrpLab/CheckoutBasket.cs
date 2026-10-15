namespace SrpLab;

public sealed class CheckoutBasket
{
    private readonly List<BasketLine> _lines = new List<BasketLine>();
    private readonly BasketPricingService _pricing = new BasketPricingService();
    private string? _couponText;
    private bool _giftWrap;

    public void AddLine(string sku, decimal price, int qty)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        _lines.Add(new BasketLine(sku, price, qty));
    }

    public void ApplyCouponText(string? couponText)
    {
        _couponText = couponText;
    }

    public void EnableGiftWrap()
    {
        _giftWrap = true;
    }

    public decimal SubTotal()
    {
        return _pricing.GetSubTotal(_lines);
    }

    public decimal DiscountAmount()
    {
        return _pricing.GetDiscount(SubTotal(), _couponText);
    }

    public decimal GrandTotal()
    {
        return _pricing.GetGrandTotal(SubTotal(), DiscountAmount(), _giftWrap);
    }

    public string GiftMessageCard(string fromName)
    {
        return new GiftMessageWriter().Write(fromName, _lines, GrandTotal());
    }

    public string AuthorizePaymentStub(string cardLast4)
    {
        return new PaymentAuthorizer().Authorize(GrandTotal(), cardLast4, _lines.Count);
    }
}

public class BasketLine
{
    public string Sku { get; }
    public decimal Price { get; }
    public int Qty { get; }

    public BasketLine(string sku, decimal price, int qty)
    {
        Sku = sku;
        Price = price;
        Qty = qty;
    }
}

public class CouponParser
{
    public decimal GetDiscount(decimal subTotal, string? couponText)
    {
        if (string.IsNullOrWhiteSpace(couponText)) return 0m;
        var text = couponText.Trim().ToUpperInvariant();
        if (text.StartsWith("SAVE") && int.TryParse(text[4..], out var percent) && percent is > 0 and <= 50)
        {
            return Math.Round(subTotal * percent / 100m, 2);
        }
        if (text == "WELCOME10") return Math.Min(10m, subTotal);
        return 0m;
    }
}

public class BasketPricingService
{
    private readonly CouponParser _couponParser = new CouponParser();

    public decimal GetSubTotal(IEnumerable<BasketLine> lines)
    {
        return lines.Sum(x => x.Price * x.Qty);
    }

    public decimal GetDiscount(decimal subTotal, string? couponText)
    {
        return _couponParser.GetDiscount(subTotal, couponText);
    }

    public decimal GetGrandTotal(decimal subTotal, decimal discount, bool giftWrap)
    {
        var total = subTotal - discount;
        if (giftWrap) total += 4.99m;
        return Math.Max(0m, total);
    }
}

public class GiftMessageWriter
{
    public string Write(string fromName, IEnumerable<BasketLine> lines, decimal total)
    {
        var items = string.Join(", ", lines.Select(x => x.Sku));
        return $"Dear friend,\nA gift from {fromName} awaits ({items}).\nTotal surprise value: {total:C}\n";
    }
}

public class PaymentAuthorizer
{
    public string Authorize(decimal total, string cardLast4, int lineCount)
    {
        var payload = $"{total:0.00}|{cardLast4}|{lineCount}";
        return $"AUTH-{Math.Abs(payload.GetHashCode()):X8}";
    }
}
