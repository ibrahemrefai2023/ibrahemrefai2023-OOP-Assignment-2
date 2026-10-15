namespace SrpLab;

public sealed class WardBoard
{
    private readonly Dictionary<int, WardBed> _beds = new Dictionary<int, WardBed>();
    private readonly AcuityCalculator _acuityCalculator = new AcuityCalculator();
    private readonly PagerLog _pagerLog = new PagerLog();

    public void AssignBed(int bed, string patientId, int heartRate, int spo2)
    {
        if (bed <= 0) throw new ArgumentOutOfRangeException(nameof(bed));
        if (string.IsNullOrWhiteSpace(patientId)) throw new ArgumentException("Patient is required.");

        var acuity = _acuityCalculator.Calculate(heartRate, spo2);
        _beds[bed] = new WardBed(bed, patientId.Trim().ToUpperInvariant(), acuity);
        _pagerLog.AddIfNeeded(bed, acuity);
    }

    public int ScoreAcuity(int heartRate, int spo2)
    {
        return _acuityCalculator.Calculate(heartRate, spo2);
    }

    public string BuildHandoffNote(int bed)
    {
        if (!_beds.ContainsKey(bed)) return $"Bed {bed}: empty";
        return new HandoffNoteWriter().Write(_beds[bed]);
    }

    public IReadOnlyList<string> DrainPagerLog()
    {
        return _pagerLog.Drain();
    }

    public string ExportCensusCsv()
    {
        return new CensusCsvExporter().Export(_beds.Values);
    }
}

public class WardBed
{
    public int Number { get; }
    public string PatientId { get; }
    public int Acuity { get; }

    public WardBed(int number, string patientId, int acuity)
    {
        Number = number;
        PatientId = patientId;
        Acuity = acuity;
    }
}

public class AcuityCalculator
{
    public int Calculate(int heartRate, int spo2)
    {
        var score = 0;
        if (heartRate > 120 || heartRate < 45) score += 4;
        else if (heartRate > 100) score += 2;
        if (spo2 < 90) score += 5;
        else if (spo2 < 94) score += 2;
        return Math.Min(score, 10);
    }
}

public class PagerLog
{
    private readonly List<string> _entries = new List<string>();

    public void AddIfNeeded(int bed, int acuity)
    {
        if (acuity >= 8) _entries.Add($"CODE-YELLOW bed={bed} at {DateTime.UtcNow:HH:mm}");
    }

    public IReadOnlyList<string> Drain()
    {
        var result = _entries.ToList();
        _entries.Clear();
        return result;
    }
}

public class HandoffNoteWriter
{
    public string Write(WardBed bed)
    {
        var tone = bed.Acuity >= 8 ? "ESCALATE" : bed.Acuity >= 4 ? "WATCH" : "STABLE";
        return $"[HANDOFF {DateTime.UtcNow:yyyy-MM-dd}] Bed {bed.Number} · {bed.PatientId} · acuity={bed.Acuity} · {tone}";
    }
}

public class CensusCsvExporter
{
    public string Export(IEnumerable<WardBed> beds)
    {
        var lines = new List<string> { "bed,patient,acuity" };
        foreach (var bed in beds.OrderBy(x => x.Number))
        {
            lines.Add($"{bed.Number},{bed.PatientId},{bed.Acuity}");
        }
        return string.Join('\n', lines);
    }
}
