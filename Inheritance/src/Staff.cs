namespace LibrarySystem;

public class Staff : Person
{
    private readonly decimal _responsibilityAllowance;

    public DateOnly HireDate { get; }
    public decimal MonthlySalary { get; private set; }
    public decimal MonthlyPay { get { return MonthlySalary + _responsibilityAllowance; } }

    protected Staff(string personId, string fullName, string phone, DateOnly hireDate, decimal monthlySalary, decimal responsibilityAllowance)
        : base(personId, fullName, phone)
    {
        if (monthlySalary < 0m) throw new ArgumentException("Salary cannot be negative.");
        HireDate = hireDate;
        MonthlySalary = monthlySalary;
        _responsibilityAllowance = responsibilityAllowance;
    }

    public void GiveRaise(decimal percentage)
    {
        if (percentage <= 0m) throw new ArgumentException("Raise percentage must be positive.");
        MonthlySalary += MonthlySalary * percentage / 100m;
    }
}

public class Librarian : Staff
{
    public Librarian(string personId, string fullName, string phone, DateOnly hireDate, decimal monthlySalary)
        : base(personId, fullName, phone, hireDate, monthlySalary, 0m)
    {
    }

    public void ProcessReturn(Loan loan, DateOnly returnDate)
    {
        loan.Return(returnDate);
    }

    public void MarkItemAsLost(Loan loan)
    {
        loan.MarkAsLost();
    }
}

public class Shelver : Staff
{
    public string Section { get; private set; }

    public Shelver(string personId, string fullName, string phone, DateOnly hireDate, decimal monthlySalary, string section)
        : base(personId, fullName, phone, hireDate, monthlySalary, 0m)
    {
        Section = section;
    }

    public void Reassign(string section)
    {
        if (string.IsNullOrWhiteSpace(section)) throw new ArgumentException("Section is required.");
        Section = section;
    }
}

public class HeadLibrarian : Staff
{
    public HeadLibrarian(string personId, string fullName, string phone, DateOnly hireDate, decimal monthlySalary)
        : base(personId, fullName, phone, hireDate, monthlySalary, 400m)
    {
    }

    public void ChangeLateFee(LibraryItem item, decimal newLateFee)
    {
        item.ChangeBaseLateFee(newLateFee);
    }

    public void WithdrawItem(LibraryItem item)
    {
        item.Withdraw();
    }

    public void RestoreItem(LibraryItem item)
    {
        item.Restore();
    }
}
