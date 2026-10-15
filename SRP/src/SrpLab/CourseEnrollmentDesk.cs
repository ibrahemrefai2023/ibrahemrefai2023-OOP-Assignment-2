namespace SrpLab;

public sealed class CourseEnrollmentDesk
{
    private readonly EnrollmentRoster _roster;

    public int Capacity { get; }
    public decimal Tuition { get; }
    public string CourseCode { get; }

    public CourseEnrollmentDesk(string courseCode, int capacity, decimal tuition)
    {
        CourseCode = courseCode;
        Capacity = capacity;
        Tuition = tuition;
        _roster = new EnrollmentRoster(capacity);
    }

    public string Register(string studentEmail)
    {
        return _roster.Register(studentEmail);
    }

    public int WaitlistPosition(string studentEmail)
    {
        return _roster.WaitlistPosition(studentEmail);
    }

    public string WelcomePacketMarkdown(string studentEmail, string studentName)
    {
        return new WelcomePacketWriter().Write(CourseCode, studentEmail, studentName, _roster.IsSeated(studentEmail), WaitlistPosition(studentEmail));
    }

    public string TuitionInvoiceLine(string studentEmail)
    {
        return new TuitionInvoiceWriter().Write(CourseCode, Tuition, _roster.IsSeated(studentEmail));
    }

    public void PromoteFromWaitlist(int seats)
    {
        _roster.Promote(seats);
    }
}

public class EnrollmentRoster
{
    private readonly HashSet<string> _seated = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _waitlist = new List<string>();
    private readonly int _capacity;

    public EnrollmentRoster(int capacity)
    {
        _capacity = capacity;
    }

    public string Register(string studentEmail)
    {
        if (string.IsNullOrWhiteSpace(studentEmail)) throw new ArgumentException("Email is required.");
        var email = studentEmail.Trim();
        if (_seated.Contains(email) || _waitlist.Contains(email, StringComparer.OrdinalIgnoreCase)) return "ALREADY_REGISTERED";
        if (_seated.Count < _capacity)
        {
            _seated.Add(email);
            return "SEATED";
        }
        _waitlist.Add(email);
        return $"WAITLIST:{_waitlist.Count}";
    }

    public int WaitlistPosition(string email)
    {
        var index = _waitlist.FindIndex(x => x.Equals(email, StringComparison.OrdinalIgnoreCase));
        return index < 0 ? -1 : index + 1;
    }

    public bool IsSeated(string email)
    {
        return _seated.Contains(email);
    }

    public void Promote(int seats)
    {
        while (seats > 0 && _waitlist.Count > 0 && _seated.Count < _capacity)
        {
            _seated.Add(_waitlist[0]);
            _waitlist.RemoveAt(0);
            seats--;
        }
    }
}

public class WelcomePacketWriter
{
    public string Write(string courseCode, string email, string name, bool seated, int position)
    {
        var status = seated ? "confirmed seat" : $"waitlist #{position}";
        return $"# Welcome to {courseCode}\nHi {name},\nYour status: **{status}**.\nBring a laptop. Discord onboarding link: https://example.invalid/{courseCode.ToLowerInvariant()}\n";
    }
}

public class TuitionInvoiceWriter
{
    public string Write(string courseCode, decimal tuition, bool seated)
    {
        if (!seated) return $"{courseCode},WAITLIST,0.00";
        var vat = Math.Round(tuition * 0.14m, 2);
        return $"{courseCode},TUITION,{tuition:0.00},VAT,{vat:0.00},TOTAL,{(tuition + vat):0.00}";
    }
}
