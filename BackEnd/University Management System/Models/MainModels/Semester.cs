namespace University_Management_System.Models.MainModels
{
    public class Semester
    {
        public int Id { get; set; }

        public int SemesterNumber { get; set; }

        // Foreign key of AcademicProfram
        public int AcademicProgramId { get; set; }
        public AcademicProgram AcademicProgram { get; set; }

        // Foreign key of Academinc Session
        public int AcademicSessionId { get; set; }
        public AcademicSession AcademicSession { get; set; }

        public string Status { get; set; } = "Active";
    }
}
