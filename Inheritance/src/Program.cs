using LibrarySystem;

Console.WriteLine("Library System Demo");
Console.WriteLine("===================");

// The following lines must NOT compile. They show protected constructors and protected state.
// var person = new Person("P0", "Plain Person", "0100");
// var member = new Member("M0", "Plain Member", "0100", 3, 0m);
// var staff = new Staff("S0", "Plain Staff", "0100", new DateOnly(2020, 1, 1), 5000m, 0m);
// var item = new LibraryItem("I0", "Plain Item", 1m, 7, 1m);

var student = new StudentMember("M-1", "Ali Student", "01000000001");
// student.FullName = "Changed"; // must NOT compile
// student.Loans.Add(null); // must NOT compile

var librarian = new Librarian("S-1", "Sara Librarian", "01000000002", new DateOnly(2023, 1, 1), 8000m);
var headLibrarian = new HeadLibrarian("S-2", "Mona Head", "01000000003", new DateOnly(2020, 1, 1), 12000m);
var shelver = new Shelver("S-3", "Omar Shelver", "01000000004", new DateOnly(2024, 1, 1), 6000m, "Fiction");

var staffMembers = new List<Staff> { librarian, headLibrarian, shelver };
Console.WriteLine("\nMonthly pay:");
foreach (var staffMember in staffMembers)
{
    Console.WriteLine($"{staffMember.FullName}: {staffMember.MonthlyPay}");
}

var book = new Book("B-1", "Clean Code", 2m);
var dvd = new Dvd("D-1", "Inception", 3m);
var magazine = new Magazine("G-1", "Science Monthly", 1m);
// book.IsOnLoan = false; // must NOT compile

var items = new List<LibraryItem> { book, dvd, magazine };
Console.WriteLine("\nItem rules:");
foreach (var libraryItem in items)
{
    Console.WriteLine($"{libraryItem.Title}: {libraryItem.LoanPeriodDays} days, late fee {libraryItem.DailyLateFee()} per day");
}

Console.WriteLine("\nValidation examples:");
headLibrarian.WithdrawItem(book);
try
{
    student.BorrowItem("L-1", book, new DateOnly(2026, 10, 1));
}
catch (Exception ex)
{
    Console.WriteLine($"Borrow withdrawn item: {ex.Message}");
}
headLibrarian.RestoreItem(book);

var normalLoan = student.BorrowItem("L-2", book, new DateOnly(2026, 10, 1));
try
{
    student.BorrowItem("L-3", book, new DateOnly(2026, 10, 1));
}
catch (Exception ex)
{
    Console.WriteLine($"Borrow item already on loan: {ex.Message}");
}
librarian.ProcessReturn(normalLoan, new DateOnly(2026, 10, 3));

student.BorrowItem("L-4", new Book("B-2", "Book Two", 1m), new DateOnly(2026, 10, 1));
student.BorrowItem("L-5", new Book("B-3", "Book Three", 1m), new DateOnly(2026, 10, 1));
student.BorrowItem("L-6", new Book("B-4", "Book Four", 1m), new DateOnly(2026, 10, 1));
try
{
    student.BorrowItem("L-7", new Book("B-5", "Book Five", 1m), new DateOnly(2026, 10, 1));
}
catch (Exception ex)
{
    Console.WriteLine($"Student borrows a fourth active item: {ex.Message}");
}

var premium = new PremiumMember("M-2", "Nora Premium", "01000000005", 10m);
var premiumDvd = new Dvd("D-2", "Interstellar", 2m);
var lateLoan = premium.BorrowItem("L-8", premiumDvd, new DateOnly(2026, 10, 1));
librarian.ProcessReturn(lateLoan, new DateOnly(2026, 10, 13));
Console.WriteLine("\nPremium return:");
Console.WriteLine($"Due date: {lateLoan.DueDate}");
Console.WriteLine($"Late fee: {lateLoan.LateFee}");
Console.WriteLine($"Reading points: {premium.ReadingPoints}");

try
{
    librarian.ProcessReturn(lateLoan, new DateOnly(2026, 10, 13));
}
catch (Exception ex)
{
    Console.WriteLine($"Return the same loan twice: {ex.Message}");
}

try
{
    librarian.MarkItemAsLost(lateLoan);
}
catch (Exception ex)
{
    Console.WriteLine($"Mark a returned loan as lost: {ex.Message}");
}

try
{
    librarian.GiveRaise(0m);
}
catch (Exception ex)
{
    Console.WriteLine($"Zero raise: {ex.Message}");
}

try
{
    headLibrarian.ChangeLateFee(book, 0m);
}
catch (Exception ex)
{
    Console.WriteLine($"Zero late fee: {ex.Message}");
}
Console.ReadLine();