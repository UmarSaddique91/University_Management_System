namespace University_Management_System.DTOs
{
    public class DashboardSummaryDTO
    {
        public int TotalStudents { get; set; }
        public int StudentThisMonth { get; set; }
        public double StudentGrowthPercentage { get; set; }


        public int TotalTeachers { get; set; }
        public int TeacherThisMonth { get; set; }
        public double TeacherGrowthPercentage { get; set; }




        public int TotalCourses { get; set; }
        public int TotalDepartments { get; set; }


        public int ActiveEnrollments { get; set; }
        public int EnrollmentThisMonth { get; set; }
        public double EnrollmentGrowthPercentage { get; set; }
       
    }
}
