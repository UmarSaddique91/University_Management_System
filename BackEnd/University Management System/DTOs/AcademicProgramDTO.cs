namespace University_Management_System.DTOs
{
    public class AcademicProgramDTO
    {
        public string ProgramName { get; set; }

        public string ProgramCode { get; set; }

        public int DurationInSemesters { get; set; }

        public int? DepartmentId { get; set; }
    }
}

