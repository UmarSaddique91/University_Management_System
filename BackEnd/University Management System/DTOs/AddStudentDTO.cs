namespace University_Management_System.DTOs
{
    public class AddStudentDTO
    {
        public string UserId { get; set; }

        public string StudentName { get; set; }

        public string RollNumber { get; set; }

        public int? DepartmentId { get; set; }

        public string? Program { get; set; }

        public int Semester { get; set; }

        public string Status { get; set; }
    }
}
