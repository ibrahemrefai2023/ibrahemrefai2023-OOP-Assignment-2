namespace LibrarySystem;

public enum LoanStatus
{
    Borrowed,
    Returned,
    Lost
}

public class Loan
{
    public string LoanId { get; }
    public DateOnly BorrowDate { get; }
    public DateOnly DueDate { get { return BorrowDate.AddDays(Item.LoanPeriodDays); } }
    public Member Member { get; }
    public LibraryItem Item { get; }
    public LoanStatus Status { get; private set; }
    public DateOnly? ReturnDate { get; private set; }
    public decimal LateFee
    {
        get
        {
            if (ReturnDate == null || ReturnDate.Value <= DueDate) return 0m;
            var lateDays = ReturnDate.Value.DayNumber - DueDate.DayNumber;
            var feeBeforeDiscount = lateDays * Item.DailyLateFee();
            return feeBeforeDiscount - feeBeforeDiscount * Member.LateFeeDiscountPercentage / 100m;
        }
    }

    internal Loan(string loanId, DateOnly borrowDate, Member member, LibraryItem item)
    {
        if (string.IsNullOrWhiteSpace(loanId)) throw new ArgumentException("Loan ID is required.");
        LoanId = loanId;
        BorrowDate = borrowDate;
        Member = member;
        Item = item;
        Status = LoanStatus.Borrowed;
    }

    internal void Return(DateOnly returnDate)
    {
        if (Status != LoanStatus.Borrowed) throw new InvalidOperationException("Only a borrowed loan can be returned.");
        if (returnDate < BorrowDate) throw new ArgumentException("Return date cannot be before borrow date.");

        ReturnDate = returnDate;
        Status = LoanStatus.Returned;
        Item.EndLoan();
    }

    internal void MarkAsLost()
    {
        if (Status != LoanStatus.Borrowed) throw new InvalidOperationException("Only a borrowed loan can be marked as lost.");
        Status = LoanStatus.Lost;
        Item.MarkAsLost();
    }
}
