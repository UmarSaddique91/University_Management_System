namespace University_Management_System.Models.MainModels
{
    public class Setting
    {
        public int Id { get; set; }
        //University INFO
        public string? UniversityName { get; set; }
        public string? UniversityLogo { get; set; }
        public string? ContactaEmail { get; set; }
        public string? PrimaryAddress { get; set; }

        //System Configuration

        public string? DefaultLanguage { get; set; }
        public string? DefaultTimeZone { get; set; }
        public string? DateFormat { get; set; }

        //Active Modules

        public bool TeacherModule { get; set; }
        public bool CourseModule { get; set; }
        public bool AssignmentModule { get; set; }
        public bool PaymentModule { get; set; }

        //SecurityAccess

        public string? PasswordComplexity { get; set; }
        public int MinPasswordLength { get; set; }
        public bool Authentication { get; set; }

    }
}
