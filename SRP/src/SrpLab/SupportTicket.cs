namespace SrpLab;

public sealed class SupportTicket
{
    private readonly TicketPriorityCalculator _priorityCalculator = new TicketPriorityCalculator();
    private readonly SlaPolicy _slaPolicy = new SlaPolicy();

    public string Id { get; }
    public string Subject { get; private set; }
    public string Body { get; private set; }
    public DateTimeOffset OpenedAt { get; }
    public string Priority { get; private set; }

    public SupportTicket(string id, string subject, string body, DateTimeOffset openedAt)
    {
        Id = id;
        Subject = subject;
        Body = body;
        OpenedAt = openedAt;
        Priority = _priorityCalculator.Calculate(subject, body);
    }

    public void AppendCustomerMessage(string text)
    {
        Body += "\n---\n" + text;
        RecalculatePriorityFromText();
    }

    public void RecalculatePriorityFromText()
    {
        Priority = _priorityCalculator.Calculate(Subject, Body);
    }

    public DateTimeOffset SlaDeadline()
    {
        return _slaPolicy.GetDeadline(OpenedAt, Priority);
    }

    public bool IsBreached(DateTimeOffset now)
    {
        return now > SlaDeadline();
    }

    public string DraftPublicReply(string agentName)
    {
        return new PublicReplyWriter().Write(Id, Priority, agentName, SlaDeadline());
    }

    public string InternalEscalationBlurb()
    {
        return new EscalationMessageWriter().Write(Id, Priority, SlaDeadline());
    }
}

public class TicketPriorityCalculator
{
    public string Calculate(string subject, string body)
    {
        var text = (subject + " " + body).ToLowerInvariant();
        if (text.Contains("down") || text.Contains("outage") || text.Contains("cannot login")) return "P1";
        if (text.Contains("urgent") || text.Contains("asap") || text.Contains("blocked")) return "P2";
        return "P3";
    }
}

public class SlaPolicy
{
    public DateTimeOffset GetDeadline(DateTimeOffset openedAt, string priority)
    {
        var hours = priority == "P1" ? 4 : priority == "P2" ? 24 : 72;
        return openedAt.AddHours(hours);
    }
}

public class PublicReplyWriter
{
    public string Write(string ticketId, string priority, string agentName, DateTimeOffset deadline)
    {
        var opening = priority == "P1" ? "We are treating this as a critical incident." : "Thanks for reaching out.";
        return $"Hi,\n{opening}\nTicket {ticketId} is with {agentName}. Next update before {deadline:u}.\n";
    }
}

public class EscalationMessageWriter
{
    public string Write(string ticketId, string priority, DateTimeOffset deadline)
    {
        return $"ESCALATE {ticketId} priority={priority} breachAt={deadline:u} keywords-scanned=yes";
    }
}
