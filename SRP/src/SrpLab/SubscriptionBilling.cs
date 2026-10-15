namespace SrpLab;

public sealed class SubscriptionBilling
{
    private readonly InvoiceNumberGenerator _invoiceNumbers = new InvoiceNumberGenerator();

    public string CustomerId { get; }
    public decimal MonthlyPrice { get; }
    public DateOnly PeriodStart { get; }
    public DateOnly PeriodEnd { get; }
    public int FailedPayments { get; private set; }

    public SubscriptionBilling(string customerId, decimal monthlyPrice, DateOnly periodStart, DateOnly periodEnd)
    {
        CustomerId = customerId;
        MonthlyPrice = monthlyPrice;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
    }

    public decimal Prorate(DateOnly activeFrom)
    {
        return new ProrationCalculator().Calculate(MonthlyPrice, PeriodStart, PeriodEnd, activeFrom);
    }

    public string NextInvoiceNumber()
    {
        return _invoiceNumbers.Next(PeriodStart);
    }

    public void RegisterFailedPayment()
    {
        FailedPayments++;
    }

    public string DunningEmail(string customerName, DateOnly asOf)
    {
        return new DunningEmailWriter().Write(customerName, Prorate(PeriodStart), NextInvoiceNumber(), FailedPayments, asOf);
    }

    public string LedgerJournalLine(DateOnly activeFrom)
    {
        return new LedgerJournalWriter().Write(CustomerId, NextInvoiceNumber(), Prorate(activeFrom));
    }
}

public class ProrationCalculator
{
    public decimal Calculate(decimal monthlyPrice, DateOnly periodStart, DateOnly periodEnd, DateOnly activeFrom)
    {
        if (activeFrom <= periodStart) return monthlyPrice;
        if (activeFrom >= periodEnd) return 0m;
        var totalDays = periodEnd.DayNumber - periodStart.DayNumber;
        if (totalDays <= 0) return monthlyPrice;
        var usedDays = periodEnd.DayNumber - activeFrom.DayNumber;
        return Math.Round(monthlyPrice * usedDays / totalDays, 2);
    }
}

public class InvoiceNumberGenerator
{
    private static int _sequence = 1000;

    public string Next(DateOnly periodStart)
    {
        _sequence++;
        return $"INV-{periodStart:yyyyMM}-{_sequence:D5}";
    }
}

public class DunningEmailWriter
{
    public string Write(string name, decimal amount, string invoiceNumber, int failedPayments, DateOnly asOf)
    {
        var severity = failedPayments <= 1 ? "friendly reminder" : failedPayments == 2 ? "second notice" : "final notice before suspension";
        return $"Subject: {severity} {invoiceNumber}\nHi {name},\nBalance {amount:C} as of {asOf:o} ({failedPayments} failures).\n";
    }
}

public class LedgerJournalWriter
{
    public string Write(string customerId, string invoiceNumber, decimal amount)
    {
        return $"{customerId},{invoiceNumber},{amount:0.00},AR-SUB";
    }
}
