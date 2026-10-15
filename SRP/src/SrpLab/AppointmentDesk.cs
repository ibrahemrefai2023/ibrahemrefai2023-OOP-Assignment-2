namespace SrpLab;

public sealed class AppointmentDesk
{
    private readonly HashSet<DateTimeOffset> _booked = new HashSet<DateTimeOffset>();
    private readonly BusinessHoursPolicy _hoursPolicy;
    private readonly AppointmentSlotFinder _slotFinder;

    public TimeOnly Open { get; }
    public TimeOnly Close { get; }
    public int SlotMinutes { get; }

    public AppointmentDesk(TimeOnly open, TimeOnly close, int slotMinutes)
    {
        Open = open;
        Close = close;
        SlotMinutes = slotMinutes;
        _hoursPolicy = new BusinessHoursPolicy(open, close, slotMinutes);
        _slotFinder = new AppointmentSlotFinder(_hoursPolicy, _booked, slotMinutes);
    }

    public bool IsWithinBusinessHours(DateTimeOffset when)
    {
        return _hoursPolicy.IsOpen(when);
    }

    public DateTimeOffset? FindNextSlot(DateTimeOffset from, int searchHours)
    {
        return _slotFinder.Find(from, searchHours);
    }

    public bool TryBook(DateTimeOffset slot)
    {
        if (!_hoursPolicy.IsOpen(slot) || _booked.Contains(slot)) return false;
        _booked.Add(slot);
        return true;
    }

    public string ToIcs(DateTimeOffset slot, string patientName, string clinician)
    {
        return new IcsCalendarWriter().Write(slot, SlotMinutes, patientName, clinician);
    }

    public string SmsReminder(DateTimeOffset slot, string clinicPhone)
    {
        return new SmsReminderWriter().Write(slot, clinicPhone);
    }
}

public class BusinessHoursPolicy
{
    private readonly TimeOnly _open;
    private readonly TimeOnly _close;
    private readonly int _slotMinutes;

    public BusinessHoursPolicy(TimeOnly open, TimeOnly close, int slotMinutes)
    {
        _open = open;
        _close = close;
        _slotMinutes = slotMinutes;
    }

    public bool IsOpen(DateTimeOffset when)
    {
        if (when.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday) return false;
        var time = TimeOnly.FromDateTime(when.DateTime);
        return time >= _open && time.AddMinutes(_slotMinutes) <= _close;
    }
}

public class AppointmentSlotFinder
{
    private readonly BusinessHoursPolicy _hoursPolicy;
    private readonly HashSet<DateTimeOffset> _booked;
    private readonly int _slotMinutes;

    public AppointmentSlotFinder(BusinessHoursPolicy hoursPolicy, HashSet<DateTimeOffset> booked, int slotMinutes)
    {
        _hoursPolicy = hoursPolicy;
        _booked = booked;
        _slotMinutes = slotMinutes;
    }

    public DateTimeOffset? Find(DateTimeOffset from, int searchHours)
    {
        var cursor = Align(from);
        var end = from.AddHours(searchHours);
        while (cursor < end)
        {
            if (_hoursPolicy.IsOpen(cursor) && !_booked.Contains(cursor)) return cursor;
            cursor = cursor.AddMinutes(_slotMinutes);
        }
        return null;
    }

    private DateTimeOffset Align(DateTimeOffset from)
    {
        var minutes = from.Minute - from.Minute % _slotMinutes;
        return new DateTimeOffset(from.Year, from.Month, from.Day, from.Hour, minutes, 0, from.Offset);
    }
}

public class IcsCalendarWriter
{
    public string Write(DateTimeOffset slot, int slotMinutes, string patientName, string clinician)
    {
        var end = slot.AddMinutes(slotMinutes);
        return "BEGIN:VCALENDAR\nVERSION:2.0\nBEGIN:VEVENT\n" +
               $"UID:{Guid.NewGuid()}\nDTSTART:{slot:yyyyMMdd'T'HHmmss'Z'}\nDTEND:{end:yyyyMMdd'T'HHmmss'Z'}\n" +
               $"SUMMARY:Visit {patientName} / {clinician}\nEND:VEVENT\nEND:VCALENDAR\n";
    }
}

public class SmsReminderWriter
{
    public string Write(DateTimeOffset slot, string clinicPhone)
    {
        return $"Reminder: appointment {slot:MMM dd HH:mm}. Call {clinicPhone} to reschedule.";
    }
}
