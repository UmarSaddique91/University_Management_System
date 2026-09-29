namespace University_Management_System.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string? DepartmentName { get; set; }


        public ICollection<Teacher>? Teachers { get; set; }
        public ICollection<Course>? Courses { get; set; }

    }
}

