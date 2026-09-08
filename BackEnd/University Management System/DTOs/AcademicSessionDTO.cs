namespace University_Management_System.DTOs
{
    public class AcademicSessionDTO
    {
        public string SessionName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
