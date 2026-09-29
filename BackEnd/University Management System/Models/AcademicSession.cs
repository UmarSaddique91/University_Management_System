namespace University_Management_System.Models
{
    public class AcademicSession
    {
        public int Id { get; set; }

        public string SessionName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }
    }
}

