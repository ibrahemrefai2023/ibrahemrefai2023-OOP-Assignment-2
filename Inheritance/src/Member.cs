namespace LibrarySystem;

public class Member : Person
{
    private readonly List<Loan> _loans = new List<Loan>();

    public int MaximumLoans { get; }
    public decimal LateFeeDiscountPercentage { get; }
    public IReadOnlyList<Loan> Loans { get { return _loans; } }

    protected Member(string personId, string fullName, string phone, int maximumLoans, decimal lateFeeDiscountPercentage)
        : base(personId, fullName, phone)
    {
        MaximumLoans = maximumLoans;
        LateFeeDiscountPercentage = lateFeeDiscountPercentage;
    }

    public Loan BorrowItem(string loanId, LibraryItem item, DateOnly borrowDate)
    {
        if (item.IsWithdrawn) throw new InvalidOperationException("This item is withdrawn from circulation.");
        if (item.IsLost) throw new InvalidOperationException("This item was marked as lost.");
        if (item.IsOnLoan) throw new InvalidOperationException("This item is already on loan.");
        if (ActiveLoanCount() >= MaximumLoans) throw new InvalidOperationException("This member reached the maximum loan limit.");

        var loan = new Loan(loanId, borrowDate, this, item);
        item.StartLoan();
        _loans.Add(loan);
        return loan;
    }

    private int ActiveLoanCount()
    {
        var count = 0;
        foreach (var loan in _loans)
        {
            if (loan.Status == LoanStatus.Borrowed) count++;
        }
        return count;
    }
}

public class StudentMember : Member
{
    public StudentMember(string personId, string fullName, string phone)
        : base(personId, fullName, phone, 3, 0m)
    {
    }
}

public class PremiumMember : Member
{
    public decimal ReadingPoints
    {
        get
        {
            var points = 0m;
            foreach (var loan in Loans)
            {
                if (loan.Status == LoanStatus.Returned) points += 5m;
            }
            return points;
        }
    }

    public PremiumMember(string personId, string fullName, string phone, decimal lateFeeDiscountPercentage)
        : base(personId, fullName, phone, 10, lateFeeDiscountPercentage)
    {
        if (lateFeeDiscountPercentage < 0m || lateFeeDiscountPercentage > 100m)
        {
            throw new ArgumentException("Discount percentage must be between 0 and 100.");
        }
    }
}
