namespace LibrarySystem;

public class LibraryItem
{
    private readonly int _loanPeriodDays;
    private readonly decimal _lateFeeMultiplier;

    public string CatalogNumber { get; }
    public string Title { get; }
    public decimal BaseLateFee { get; private set; }
    public bool IsWithdrawn { get; private set; }
    public bool IsLost { get; private set; }
    public bool IsOnLoan { get; internal set; }
    public int LoanPeriodDays { get { return _loanPeriodDays; } }

    protected LibraryItem(string catalogNumber, string title, decimal baseLateFee, int loanPeriodDays, decimal lateFeeMultiplier)
    {
        if (string.IsNullOrWhiteSpace(catalogNumber)) throw new ArgumentException("Catalog number is required.");
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.");
        if (baseLateFee <= 0m) throw new ArgumentException("Base late fee must be positive.");

        CatalogNumber = catalogNumber;
        Title = title;
        BaseLateFee = baseLateFee;
        _loanPeriodDays = loanPeriodDays;
        _lateFeeMultiplier = lateFeeMultiplier;
    }

    public decimal DailyLateFee()
    {
        return BaseLateFee * _lateFeeMultiplier;
    }

    public void ChangeBaseLateFee(decimal newLateFee)
    {
        if (newLateFee <= 0m) throw new ArgumentException("Late fee must be positive.");
        BaseLateFee = newLateFee;
    }

    public void Withdraw()
    {
        IsWithdrawn = true;
    }

    public void Restore()
    {
        IsWithdrawn = false;
    }

    internal void StartLoan()
    {
        if (IsWithdrawn) throw new InvalidOperationException("This item is withdrawn from circulation.");
        if (IsLost) throw new InvalidOperationException("This item was marked as lost.");
        if (IsOnLoan) throw new InvalidOperationException("This item is already on loan.");
        IsOnLoan = true;
    }

    internal void EndLoan()
    {
        IsOnLoan = false;
    }

    internal void MarkAsLost()
    {
        IsLost = true;
        IsOnLoan = false;
    }
}

public class Book : LibraryItem
{
    public Book(string catalogNumber, string title, decimal baseLateFee)
        : base(catalogNumber, title, baseLateFee, 21, 1m)
    {
    }
}

public class Dvd : LibraryItem
{
    public Dvd(string catalogNumber, string title, decimal baseLateFee)
        : base(catalogNumber, title, baseLateFee, 7, 2m)
    {
    }
}

public class Magazine : LibraryItem
{
    public Magazine(string catalogNumber, string title, decimal baseLateFee)
        : base(catalogNumber, title, baseLateFee, 3, 0.5m)
    {
    }
}
