namespace SrpLab;

public sealed class GradeBook
{
    private readonly GradeRecords _records = new GradeRecords();
    private readonly GradeCalculator _calculator = new GradeCalculator();
    private readonly LetterGradePolicy _letterPolicy = new LetterGradePolicy();

    public void Record(string studentId, decimal score)
    {
        _records.Add(studentId, score);
    }

    public decimal Average(string studentId)
    {
        return _calculator.GetAverage(_records.GetScores(studentId));
    }

    public string Letter(string studentId)
    {
        return _letterPolicy.GetLetter(Average(studentId));
    }

    public bool MeetsHonorRoll(string studentId)
    {
        return new HonorRollPolicy().Meets(Average(studentId), Letter(studentId));
    }

    public string TranscriptPlain(string studentId, string fullName)
    {
        return new TranscriptWriter().Write(studentId, fullName, Average(studentId), Letter(studentId), MeetsHonorRoll(studentId));
    }

    public string ExportCsv()
    {
        return new GradeCsvExporter().Export(_records.StudentIds, this);
    }
}

public class GradeRecords
{
    private readonly Dictionary<string, List<decimal>> _scores = new Dictionary<string, List<decimal>>(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<string> StudentIds => _scores.Keys;

    public void Add(string studentId, decimal score)
    {
        if (score is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(score));
        if (!_scores.ContainsKey(studentId)) _scores[studentId] = new List<decimal>();
        _scores[studentId].Add(score);
    }

    public IReadOnlyList<decimal> GetScores(string studentId)
    {
        return _scores.ContainsKey(studentId) ? _scores[studentId] : new List<decimal>();
    }
}

public class GradeCalculator
{
    public decimal GetAverage(IReadOnlyList<decimal> scores)
    {
        if (scores.Count == 0) return 0m;
        return Math.Round(scores.Average(), 2);
    }
}

public class LetterGradePolicy
{
    public string GetLetter(decimal average)
    {
        if (average >= 90) return "A";
        if (average >= 80) return "B";
        if (average >= 70) return "C";
        if (average >= 60) return "D";
        return "F";
    }
}

public class HonorRollPolicy
{
    public bool Meets(decimal average, string letter)
    {
        return average >= 85 && (letter == "A" || letter == "B");
    }
}

public class TranscriptWriter
{
    public string Write(string studentId, string name, decimal average, string letter, bool honor)
    {
        return $"TRANSCRIPT\nStudent: {name} ({studentId})\nAverage: {average}\nLetter: {letter}\nHonor: {honor}\n";
    }
}

public class GradeCsvExporter
{
    public string Export(IEnumerable<string> studentIds, GradeBook gradeBook)
    {
        var rows = new List<string> { "studentId,average,letter,honor" };
        foreach (var id in studentIds.OrderBy(x => x))
        {
            rows.Add($"{id},{gradeBook.Average(id)},{gradeBook.Letter(id)},{(gradeBook.MeetsHonorRoll(id) ? 1 : 0)}");
        }
        return string.Join('\n', rows);
    }
}
