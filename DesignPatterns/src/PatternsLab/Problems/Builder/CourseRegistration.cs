namespace PatternsLab.Problems.Builder;

public sealed class CourseRegistration
{
    public string StudentEmail { get; }
    public string CourseCode { get; }
    public string AccessMode { get; }
    public string? GroupCode { get; }
    public string? DiscountCode { get; }
    public bool SendWhatsApp { get; }
    public bool SendEmailWelcome { get; }
    public string? MentorNote { get; }
    public DateOnly? PreferredStart { get; }

    private CourseRegistration(Builder builder)
    {
        StudentEmail = builder.StudentEmail;
        CourseCode = builder.CourseCode;
        AccessMode = builder.AccessMode;
        GroupCode = builder.GroupCode;
        DiscountCode = builder.DiscountCode;
        SendWhatsApp = builder.SendWhatsApp;
        SendEmailWelcome = builder.SendEmailWelcome;
        MentorNote = builder.MentorNote;
        PreferredStart = builder.PreferredStart;
    }

    public static Builder Start(string studentEmail, string courseCode)
    {
        return new Builder(studentEmail, courseCode);
    }

    public override string ToString()
    {
        return $"{StudentEmail} -> {CourseCode} [{AccessMode}] group={GroupCode ?? "-"} discount={DiscountCode ?? "-"} wa={SendWhatsApp} mail={SendEmailWelcome}";
    }

    public sealed class Builder
    {
        internal string StudentEmail { get; }
        internal string CourseCode { get; }
        internal string AccessMode { get; private set; } = "";
        internal string? GroupCode { get; private set; }
        internal string? DiscountCode { get; private set; }
        internal bool SendWhatsApp { get; private set; }
        internal bool SendEmailWelcome { get; private set; }
        internal string? MentorNote { get; private set; }
        internal DateOnly? PreferredStart { get; private set; }

        internal Builder(string studentEmail, string courseCode)
        {
            StudentEmail = studentEmail;
            CourseCode = courseCode;
        }

        public Builder LiveGroup(string groupCode)
        {
            AccessMode = "LiveGroup";
            GroupCode = groupCode;
            return this;
        }

        public Builder VideosOnly()
        {
            AccessMode = "VideosOnly";
            GroupCode = null;
            return this;
        }

        public Builder WithDiscount(string discountCode)
        {
            DiscountCode = discountCode;
            return this;
        }

        public Builder SendWhatsAppMessage()
        {
            SendWhatsApp = true;
            return this;
        }

        public Builder SendWelcomeEmail()
        {
            SendEmailWelcome = true;
            return this;
        }

        public Builder WithMentorNote(string note)
        {
            MentorNote = note;
            return this;
        }

        public Builder StartingOn(DateOnly date)
        {
            PreferredStart = date;
            return this;
        }

        public CourseRegistration Build()
        {
            if (string.IsNullOrWhiteSpace(StudentEmail))
            {
                throw new ArgumentException("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(CourseCode))
            {
                throw new ArgumentException("Course code is required.");
            }

            if (AccessMode != "LiveGroup" && AccessMode != "VideosOnly")
            {
                throw new InvalidOperationException("Choose LiveGroup or VideosOnly.");
            }

            if (AccessMode == "LiveGroup" && string.IsNullOrWhiteSpace(GroupCode))
            {
                throw new InvalidOperationException("LiveGroup requires a group code.");
            }

            if (AccessMode == "VideosOnly" && !string.IsNullOrWhiteSpace(GroupCode))
            {
                throw new InvalidOperationException("VideosOnly cannot have a group code.");
            }

            return new CourseRegistration(this);
        }
    }
}

public static class RegistrationCallSites
{
    public static CourseRegistration CreateLiveStudent()
    {
        return CourseRegistration.Start("sara@mail.com", "SEF-101")
            .LiveGroup("G1")
            .WithDiscount("EARLY10")
            .SendWhatsAppMessage()
            .SendWelcomeEmail()
            .WithMentorNote("Needs evening slot")
            .StartingOn(new DateOnly(2026, 10, 1))
            .Build();
    }

    public static CourseRegistration CreateVideosOnlyStudent()
    {
        return CourseRegistration.Start("ali@mail.com", "SEF-101")
            .VideosOnly()
            .SendWelcomeEmail()
            .Build();
    }
}
