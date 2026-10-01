using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using University_Management_System.Data;
using University_Management_System.Models;

namespace University_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "StudentAccess")]
    public class StudentPortalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentPortalController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region 1. Helper Methods

        // Resolve student profile from current user claims
        private async Task<Student?> GetLoggedInStudentAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name;

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(userEmail))
            {
                return null;
            }

            var student = await _context.Students.FirstOrDefaultAsync(s =>
                (!string.IsNullOrEmpty(userId) && s.UserId == userId) ||
                (!string.IsNullOrEmpty(userEmail) && s.Email != null && s.Email.ToLower() == userEmail.ToLower())
            );

            return student;
        }

        // Marks to Grade Letter calculation
        private static string CalculateGradeLetter(double? obtained, double total)
        {
            if (obtained == null || total <= 0) return "—";
            var percentage = (obtained.Value / total) * 100.0;

            if (percentage >= 85) return "A";
            if (percentage >= 80) return "A-";
            if (percentage >= 75) return "B+";
            if (percentage >= 70) return "B";
            if (percentage >= 65) return "B-";
            if (percentage >= 60) return "C+";
            if (percentage >= 55) return "C";
            if (percentage >= 50) return "D";
            return "F";
        }

        #endregion

        #region 2. Module: My Courses & Dashboard

        // GET: api/StudentPortal/my-courses
        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var student = await GetLoggedInStudentAsync();
            if (student == null)
            {
                return Unauthorized(new { message = "Student profile not found for this account." });
            }

            // Fetch active enrolled courses
            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id && e.Status == "Active")
                .Include(e => e.CourseOffering).ThenInclude(co => co.Course)
                .Include(e => e.CourseOffering).ThenInclude(co => co.Teacher)
                .Include(e => e.CourseOffering).ThenInclude(co => co.Semester).ThenInclude(s => s.AcademicProgram)
                .Include(e => e.CourseOffering).ThenInclude(co => co.Semester).ThenInclude(s => s.AcademicSession)
                .OrderBy(e => e.CourseOffering.Course.CourseName)
                .ToListAsync();

            var offeringIds = enrollments.Select(e => e.CourseOfferingId).ToList();

            // Total classes conducted for these courses
            var totalClasses = await _context.AttendanceSessions
                .Where(s => offeringIds.Contains(s.CourseOfferingId))
                .CountAsync();

            if (totalClasses == 0 && offeringIds.Any())
            {
                totalClasses = await _context.Attendance
                    .Where(a => offeringIds.Contains(a.CourseOfferingId) && a.StudentId == student.Id)
                    .Select(a => new { a.CourseOfferingId, a.AttendanceDate.Date })
                    .Distinct()
                    .CountAsync();
            }

            // Student attendance records
            var studentAttendance = await _context.Attendance
                .Where(a => offeringIds.Contains(a.CourseOfferingId) && a.StudentId == student.Id)
                .ToListAsync();

            int presentCount = studentAttendance.Count(a => a.Status.Equals("Present", StringComparison.OrdinalIgnoreCase));
            int totalRecords = studentAttendance.Count;
            double overallAttendance = totalRecords > 0
                ? Math.Round(((double)presentCount / totalRecords) * 100.0, 1)
                : 0.0;

            var coursesList = new List<object>();
            foreach (var e in enrollments)
            {
                var co = e.CourseOffering;
                var teacherName = !string.IsNullOrWhiteSpace(co?.Teacher?.TeacherName) ? co.Teacher.TeacherName : "-";

                coursesList.Add(new
                {
                    courseOfferingId = e.CourseOfferingId,
                    courseCode = co?.Course?.CourseCode ?? string.Empty,
                    courseName = co?.Course?.CourseName ?? string.Empty,
                    teacherName = teacherName,
                    programName = co?.Semester?.AcademicProgram?.ProgramName ?? string.Empty,
                    semesterNumber = co?.Semester?.SemesterNumber ?? 0,
                    sessionName = co?.Semester?.AcademicSession?.SessionName ?? string.Empty,
                    hasPractical = co?.Course != null && co.Course.HasPractical
                });
            }

            return Ok(new
            {
                student = new
                {
                    studentId = student.Id,
                    studentName = student.StudentName,
                    rollNumber = student.RollNumber,
                    email = student.Email
                },
                summary = new
                {
                    enrolledCourses = enrollments.Count,
                    totalClasses = totalClasses,
                    overallAttendance = overallAttendance
                },
                courses = coursesList
            });
        }

        #endregion

        #region 3. Module: Results

        // GET: api/StudentPortal/results
        [HttpGet("results")]
        public async Task<IActionResult> GetResults()
        {
            var student = await GetLoggedInStudentAsync();
            if (student == null)
            {
                return Unauthorized(new { message = "Student profile not found for this account." });
            }

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id && e.Status == "Active")
                .Include(e => e.CourseOffering).ThenInclude(co => co.Course)
                .Include(e => e.CourseOffering).ThenInclude(co => co.Teacher)
                .Include(e => e.CourseOffering).ThenInclude(co => co.Semester).ThenInclude(s => s.AcademicProgram)
                .Include(e => e.CourseOffering).ThenInclude(co => co.Semester).ThenInclude(s => s.AcademicSession)
                .OrderBy(e => e.CourseOffering.Course.CourseName)
                .ToListAsync();

            var offeringIds = enrollments.Select(e => e.CourseOfferingId).ToList();

            var exams = await _context.Exams
                .Where(e => offeringIds.Contains(e.CourseOfferingId))
                .ToListAsync();

            var examIds = exams.Select(e => e.Id).ToList();

            var grades = await _context.Grades
                .Where(g => examIds.Contains(g.ExamId) && g.StudentId == student.Id)
                .ToListAsync();

            var results = new List<object>();

            foreach (var e in enrollments)
            {
                var offering = e.CourseOffering;
                var hasPractical = offering.Course != null && offering.Course.HasPractical;

                var offeringExams = exams.Where(x => x.CourseOfferingId == offering.Id).ToList();
                var midExam = offeringExams.FirstOrDefault(x => x.ExamType.ToLower() == "mid");
                var finalExam = offeringExams.FirstOrDefault(x => x.ExamType.ToLower() == "final");
                var practicalExam = hasPractical ? offeringExams.FirstOrDefault(x => x.ExamType.ToLower() == "practical") : null;

                var midGrade = midExam != null ? grades.FirstOrDefault(g => g.ExamId == midExam.Id) : null;
                var finalGrade = finalExam != null ? grades.FirstOrDefault(g => g.ExamId == finalExam.Id) : null;
                var practicalGrade = practicalExam != null ? grades.FirstOrDefault(g => g.ExamId == practicalExam.Id) : null;

                double? mid = midGrade?.ObtainedMarks;
                double? fin = finalGrade?.ObtainedMarks;
                double? prac = practicalGrade?.ObtainedMarks;

                bool isGraded = mid.HasValue || fin.HasValue || (hasPractical && prac.HasValue);
                double? totalObtained = isGraded ? (mid ?? 0) + (fin ?? 0) + (prac ?? 0) : null;

                int midTotal = midExam?.TotalMarks ?? 30;
                int finalTotal = finalExam?.TotalMarks ?? (hasPractical ? 50 : 70);
                int practicalTotal = practicalExam?.TotalMarks ?? (hasPractical ? 20 : 0);
                int totalMax = midTotal + finalTotal + (hasPractical ? practicalTotal : 0);

                string gradeLetter = isGraded ? CalculateGradeLetter(totalObtained, totalMax) : "â€”";
                var teacherName = !string.IsNullOrWhiteSpace(offering.Teacher?.TeacherName) ? offering.Teacher.TeacherName : "-";

                results.Add(new
                {
                    courseOfferingId = offering.Id,
                    courseCode = offering.Course?.CourseCode ?? string.Empty,
                    courseName = offering.Course?.CourseName ?? string.Empty,
                    teacherName = teacherName,
                    programName = offering.Semester?.AcademicProgram?.ProgramName ?? string.Empty,
                    semesterNumber = offering.Semester?.SemesterNumber ?? 0,
                    sessionName = offering.Semester?.AcademicSession?.SessionName ?? string.Empty,
                    hasPractical = hasPractical,
                    midMarks = mid,
                    midTotalMarks = midTotal,
                    finalMarks = fin,
                    finalTotalMarks = finalTotal,
                    practicalMarks = prac,
                    practicalTotalMarks = practicalTotal,
                    totalObtainedMarks = totalObtained,
                    totalMaxMarks = totalMax,
                    gradeLetter = gradeLetter,
                    isGraded = isGraded
                });
            }

            return Ok(results);
        }

        #endregion

        #region 4. Module: Attendance

        // GET: api/StudentPortal/attendance
        [HttpGet("attendance")]
        public async Task<IActionResult> GetAttendanceSummary()
        {
            var student = await GetLoggedInStudentAsync();
            if (student == null)
            {
                return Unauthorized(new { message = "Student profile not found for this account." });
            }

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id && e.Status == "Active")
                .Include(e => e.CourseOffering).ThenInclude(co => co.Course)
                .Include(e => e.CourseOffering).ThenInclude(co => co.Teacher)
                .Include(e => e.CourseOffering).ThenInclude(co => co.Semester).ThenInclude(s => s.AcademicSession)
                .OrderBy(e => e.CourseOffering.Course.CourseName)
                .ToListAsync();

            var offeringIds = enrollments.Select(e => e.CourseOfferingId).ToList();

            var sessions = await _context.AttendanceSessions
                .Where(s => offeringIds.Contains(s.CourseOfferingId))
                .ToListAsync();

            var attendanceRecords = await _context.Attendance
                .Where(a => offeringIds.Contains(a.CourseOfferingId) && a.StudentId == student.Id)
                .ToListAsync();

            var summaryList = new List<object>();

            foreach (var e in enrollments)
            {
                var offering = e.CourseOffering;
                var offeringSessions = sessions.Where(s => s.CourseOfferingId == offering.Id).ToList();
                var offeringRecords = attendanceRecords.Where(a => a.CourseOfferingId == offering.Id).ToList();

                int present = offeringRecords.Count(a => a.Status.Equals("Present", StringComparison.OrdinalIgnoreCase));
                int absent = offeringRecords.Count(a => a.Status.Equals("Absent", StringComparison.OrdinalIgnoreCase));
                int totalClasses = offeringSessions.Count > 0 ? offeringSessions.Count : offeringRecords.Count;
                double pct = totalClasses > 0 ? Math.Round(((double)present / totalClasses) * 100.0, 1) : 0.0;

                var teacherName = !string.IsNullOrWhiteSpace(offering.Teacher?.TeacherName) ? offering.Teacher.TeacherName : "-";

                summaryList.Add(new
                {
                    courseOfferingId = offering.Id,
                    courseCode = offering.Course?.CourseCode ?? string.Empty,
                    courseName = offering.Course?.CourseName ?? string.Empty,
                    teacherName = teacherName,
                    semesterNumber = offering.Semester?.SemesterNumber ?? 0,
                    sessionName = offering.Semester?.AcademicSession?.SessionName ?? string.Empty,
                    totalClasses = totalClasses,
                    presentCount = present,
                    absentCount = absent,
                    attendancePercentage = pct
                });
            }

            return Ok(summaryList);
        }

        // GET: api/StudentPortal/course/{courseOfferingId}/attendance
        [HttpGet("course/{courseOfferingId}/attendance")]
        public async Task<IActionResult> GetCourseAttendanceDetails(int courseOfferingId)
        {
            var student = await GetLoggedInStudentAsync();
            if (student == null)
            {
                return Unauthorized(new { message = "Student profile not found for this account." });
            }

            // Verify active enrollment
            var enrollment = await _context.Enrollments
                .Include(e => e.CourseOffering).ThenInclude(co => co.Course)
                .Include(e => e.CourseOffering).ThenInclude(co => co.Teacher)
                .Include(e => e.CourseOffering).ThenInclude(co => co.Semester).ThenInclude(s => s.AcademicProgram)
                .Include(e => e.CourseOffering).ThenInclude(co => co.Semester).ThenInclude(s => s.AcademicSession)
                .FirstOrDefaultAsync(e => e.StudentId == student.Id && e.CourseOfferingId == courseOfferingId && e.Status == "Active");

            if (enrollment == null)
            {
                return StatusCode(403, new { message = "Forbidden: You are not enrolled in this course offering." });
            }

            var offering = enrollment.CourseOffering;

            var sessions = await _context.AttendanceSessions
                .Where(s => s.CourseOfferingId == courseOfferingId)
                .OrderByDescending(s => s.ClassDate)
                .ThenByDescending(s => s.Id)
                .ToListAsync();

            var records = await _context.Attendance
                .Where(a => a.CourseOfferingId == courseOfferingId && a.StudentId == student.Id)
                .ToListAsync();

            var history = new List<object>();
            foreach (var s in sessions)
            {
                var rec = records.FirstOrDefault(r => r.AttendanceSessionId == s.Id)
                          ?? records.FirstOrDefault(r => r.AttendanceDate.Date == s.ClassDate.Date);

                history.Add(new
                {
                    sessionId = s.Id,
                    classDate = s.ClassDate,
                    status = rec != null ? rec.Status : "Not Marked"
                });
            }

            if (history.Count == 0 && records.Count > 0)
            {
                foreach (var r in records.OrderByDescending(x => x.AttendanceDate))
                {
                    history.Add(new
                    {
                        sessionId = r.AttendanceSessionId ?? r.Id,
                        classDate = r.AttendanceDate,
                        status = r.Status
                    });
                }
            }

            int present = records.Count(r => r.Status.Equals("Present", StringComparison.OrdinalIgnoreCase));
            int absent = records.Count(r => r.Status.Equals("Absent", StringComparison.OrdinalIgnoreCase));
            int totalClasses = history.Count;
            double pct = totalClasses > 0 ? Math.Round(((double)present / totalClasses) * 100.0, 1) : 0.0;

            var teacherName = !string.IsNullOrWhiteSpace(offering.Teacher?.TeacherName) ? offering.Teacher.TeacherName : "-";

            return Ok(new
            {
                courseOfferingId = offering.Id,
                courseCode = offering.Course?.CourseCode ?? string.Empty,
                courseName = offering.Course?.CourseName ?? string.Empty,
                teacherName = teacherName,
                programName = offering.Semester?.AcademicProgram?.ProgramName ?? string.Empty,
                semesterNumber = offering.Semester?.SemesterNumber ?? 0,
                sessionName = offering.Semester?.AcademicSession?.SessionName ?? string.Empty,
                totalClasses = totalClasses,
                presentCount = present,
                absentCount = absent,
                attendancePercentage = pct,
                history = history
            });
        }

        #endregion
    }
}


