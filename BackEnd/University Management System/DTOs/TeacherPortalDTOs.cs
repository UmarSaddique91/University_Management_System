using System.ComponentModel.DataAnnotations;

namespace University_Management_System.DTOs
{
    public class SaveCourseMarksDTO
    {
        public List<StudentMarksEntryDTO> Students { get; set; } = new();
    }

    public class StudentMarksEntryDTO
    {
        [Required]
        public int StudentId { get; set; }

        public double? MidMarks { get; set; }

        public double? FinalMarks { get; set; }

        public double? PracticalMarks { get; set; }
    }

    public class TakeAttendanceDTO
    {
        [Required]
        public DateTime ClassDate { get; set; }

        [Required]
        public List<StudentAttendanceEntryDTO> Records { get; set; } = new();
    }

    public class UpdateAttendanceSessionDTO
    {
        [Required]
        public List<StudentAttendanceEntryDTO> Records { get; set; } = new();
    }

    public class StudentAttendanceEntryDTO
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public string Status { get; set; } = "Present"; // Present or Absent
    }
}

