namespace SrpLab;

public sealed class LoanDesk
{
    public decimal RequestedAmount { get; }
    public int CreditScore { get; }
    public int EmploymentMonths { get; }
    public bool HasCollateral { get; }

    public LoanDesk(decimal requestedAmount, int creditScore, int employmentMonths, bool hasCollateral)
    {
        RequestedAmount = requestedAmount;
        CreditScore = creditScore;
        EmploymentMonths = employmentMonths;
        HasCollateral = hasCollateral;
    }

    public decimal RiskScore()
    {
        return new LoanRiskCalculator().Calculate(RequestedAmount, CreditScore, EmploymentMonths, HasCollateral);
    }

    public bool IsEligible()
    {
        return new LoanEligibilityPolicy().IsEligible(RiskScore(), CreditScore);
    }

    public IReadOnlyList<string> RequiredDocuments()
    {
        return new LoanDocumentChecklist().Create(RequestedAmount, EmploymentMonths, HasCollateral, IsEligible());
    }

    public string DecisionLetter(string applicantName)
    {
        return new LoanDecisionLetterWriter().Write(applicantName, RequestedAmount, RiskScore(), IsEligible(), RequiredDocuments());
    }

    public string UnderwriterCsvRow(string applicationId)
    {
        return new UnderwriterCsvExporter().Export(applicationId, CreditScore, EmploymentMonths, HasCollateral, RiskScore(), IsEligible());
    }
}

public class LoanRiskCalculator
{
    public decimal Calculate(decimal requestedAmount, int creditScore, int employmentMonths, bool hasCollateral)
    {
        decimal score = 100m;
        score -= Math.Max(0, 700 - creditScore) * 0.15m;
        if (employmentMonths < 6) score -= 20m;
        if (requestedAmount > 50_000m && !hasCollateral) score -= 25m;
        if (requestedAmount > 150_000m) score -= 10m;
        return Math.Clamp(score, 0m, 100m);
    }
}

public class LoanEligibilityPolicy
{
    public bool IsEligible(decimal riskScore, int creditScore)
    {
        return riskScore >= 55m && creditScore >= 580;
    }
}

public class LoanDocumentChecklist
{
    public IReadOnlyList<string> Create(decimal requestedAmount, int employmentMonths, bool hasCollateral, bool isEligible)
    {
        var documents = new List<string> { "National ID", "Proof of income (3 months)" };
        if (requestedAmount > 40_000m) documents.Add("Bank statements (6 months)");
        if (hasCollateral) documents.Add("Collateral ownership deed");
        if (employmentMonths < 12) documents.Add("Employer letter");
        if (!isEligible) documents.Add("Manual underwriter referral form");
        return documents;
    }
}

public class LoanDecisionLetterWriter
{
    public string Write(string name, decimal amount, decimal risk, bool approved, IReadOnlyList<string> documents)
    {
        if (approved)
        {
            return $"Dear {name},\nYour request for {amount:C} is pre-approved (risk {risk:0}).\nPlease upload: {string.Join("; ", documents)}.\n";
        }
        return $"Dear {name},\nWe are unable to approve {amount:C} at this time.\nReference risk={risk:0}. You may reapply after improving documentation.\n";
    }
}

public class UnderwriterCsvExporter
{
    public string Export(string applicationId, int creditScore, int employmentMonths, bool hasCollateral, decimal risk, bool approved)
    {
        return $"{applicationId},{creditScore},{employmentMonths},{(hasCollateral ? 1 : 0)},{risk:0.00},{(approved ? "Y" : "N")}";
    }
}
