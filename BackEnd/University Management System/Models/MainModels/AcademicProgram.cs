namespace University_Management_System.Models.MainModels
{
    public class AcademicProgram
    {
        public int Id { get; set; }

        public string? ProgramName { get; set; }

        public string? ProgramCode { get; set; }

        public int? DurationInSemesters { get; set; }


        //Foreign Key
        public int? DepartmentId { get; set; }

        public Department? Department { get; set; }
    }
}
