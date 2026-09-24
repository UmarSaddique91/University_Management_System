using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using University_Management_System.Data;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Controllers.StudentPortalController
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

        #region Helper Methods

        private async Task<Student?> GetLoggedInStudentAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name;

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(userEmail))
            {
                return null;
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s =>
                    (!string.IsNullOrEmpty(userId) && s.UserId == userId) ||
                    (!string.IsNullOrEmpty(userEmail) && s.Email != null && s.Email.ToLower() == userEmail.ToLower())
                );

            return student;
        }

        private static string CalculateGradeLetter(double? obtained, double total)
        {
            if (obtained == null || total <= 0) return "—";
            var pct = (obtained.Value / total) * 100.0;
            if (pct >= 85) return "A";
            if (pct >= 80) return "A-";
            if (pct >= 75) return "B+";
            if (pct >= 70) return "B";
            if (pct >= 65) return "B-";
            if (pct >= 60) return "C+";
            if (pct >= 55) return "C";
            if (pct >= 50) return "D";
            return "F";
        }

        #endregion

        #region Module 1: My Courses & Dashboard Summary

        // GET: api/StudentPortal/my-courses
        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var student = await GetLoggedInStudentAsync();
            if (student == null)
            {
                return Unauthorized(new { message = "Student profile not found for this account." });
            }

            if (!string.IsNullOrEmpty(student.Status) && !student.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Your student account is inactive. Please contact the administrator." });
            }

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id && e.Status == "Active")
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Teacher)
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Semester)
                        .ThenInclude(s => s.AcademicProgram)
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Semester)
                        .ThenInclude(s => s.AcademicSession)
                .OrderBy(e => e.CourseOffering.Course.CourseName)
                .ToListAsync();

            var offeringIds = enrollments.Select(e => e.CourseOfferingId).ToList();

            // Total real classes conducted for enrolled offerings
            var totalClasses = await _context.AttendanceSessions
                .Where(s => offeringIds.Contains(s.CourseOfferingId))
                .CountAsync();

            if (totalClasses == 0 && offeringIds.Any())
            {
                totalClasses = await _context.Attendance
                    .Where(a => offeringIds.Contains(a.CourseOfferingId) && a.StudentId == student.Id)
                    .Select(a => new { a.CourseOfferingId, Date = a.AttendanceDate.Date })
                    .Distinct()
                    .CountAsync();
            }

            // Student attendance records across enrolled offerings
            var studentAttendance = await _context.Attendance
                .Where(a => offeringIds.Contains(a.CourseOfferingId) && a.StudentId == student.Id)
                .ToListAsync();

            var presentCount = studentAttendance.Count(a => a.Status.Equals("Present", StringComparison.OrdinalIgnoreCase));
            var totalRecords = studentAttendance.Count;
            double overallAttendance = totalRecords > 0
                ? Math.Round(((double)presentCount / totalRecords) * 100.0, 1)
                : 0.0;

            var coursesList = enrollments.Select(e => new
            {
                courseOfferingId = e.CourseOfferingId,
                courseCode = e.CourseOffering?.Course?.CourseCode ?? string.Empty,
                courseName = e.CourseOffering?.Course?.CourseName ?? string.Empty,
                teacherName = !string.IsNullOrWhiteSpace(e.CourseOffering?.Teacher?.TeacherName) ? e.CourseOffering.Teacher.TeacherName : "-",
                programName = e.CourseOffering?.Semester?.AcademicProgram?.ProgramName ?? string.Empty,
                semesterNumber = e.CourseOffering?.Semester?.SemesterNumber ?? 0,
                sessionName = e.CourseOffering?.Semester?.AcademicSession?.SessionName ?? string.Empty,
                hasPractical = e.CourseOffering?.Course != null && e.CourseOffering.Course.HasPractical
            }).ToList();

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
                    totalClasses,
                    overallAttendance
                },
                courses = coursesList
            });
        }

        #endregion

        #region Module 2: Results

        // GET: api/StudentPortal/results
        [HttpGet("results")]
        public async Task<IActionResult> GetResults()
        {
            var student = await GetLoggedInStudentAsync();
            if (student == null)
            {
                return Unauthorized(new { message = "Student profile not found for this account." });
            }

            if (!string.IsNullOrEmpty(student.Status) && !student.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Your student account is inactive. Please contact the administrator." });
            }

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id && e.Status == "Active")
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Teacher)
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Semester)
                        .ThenInclude(s => s.AcademicProgram)
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Semester)
                        .ThenInclude(s => s.AcademicSession)
                .OrderBy(e => e.CourseOffering.Course.CourseName)
                .ToListAsync();

            var offeringIds = enrollments.Select(e => e.CourseOfferingId).ToList();

            // Load exams for these offerings
            var exams = await _context.Exams
                .Where(e => offeringIds.Contains(e.CourseOfferingId))
                .ToListAsync();

            var examIds = exams.Select(e => e.Id).ToList();

            // Load student's grades
            var grades = await _context.Grades
                .Where(g => examIds.Contains(g.ExamId) && g.StudentId == student.Id)
                .ToListAsync();

            var results = enrollments.Select(e =>
            {
                var offering = e.CourseOffering;
                var hasPractical = offering.Course != null && offering.Course.HasPractical;

                var offeringExams = exams.Where(x => x.CourseOfferingId == offering.Id).ToList();
                var midExam = offeringExams.FirstOrDefault(x => x.ExamType.Equals("Mid", StringComparison.OrdinalIgnoreCase));
                var finalExam = offeringExams.FirstOrDefault(x => x.ExamType.Equals("Final", StringComparison.OrdinalIgnoreCase));
                var practicalExam = hasPractical
                    ? offeringExams.FirstOrDefault(x => x.ExamType.Equals("Practical", StringComparison.OrdinalIgnoreCase))
                    : null;

                var midGrade = midExam != null ? grades.FirstOrDefault(g => g.ExamId == midExam.Id) : null;
                var finalGrade = finalExam != null ? grades.FirstOrDefault(g => g.ExamId == finalExam.Id) : null;
                var practicalGrade = practicalExam != null ? grades.FirstOrDefault(g => g.ExamId == practicalExam.Id) : null;

                double? mid = midGrade != null ? midGrade.ObtainedMarks : null;
                double? fin = finalGrade != null ? finalGrade.ObtainedMarks : null;
                double? prac = practicalGrade != null ? practicalGrade.ObtainedMarks : null;

                bool isGraded = mid.HasValue || fin.HasValue || (hasPractical && prac.HasValue);
                double? totalObtained = isGraded ? (mid ?? 0) + (fin ?? 0) + (prac ?? 0) : null;

                int midTotal = midExam?.TotalMarks ?? 30;
                int finalTotal = finalExam?.TotalMarks ?? (hasPractical ? 50 : 70);
                int practicalTotal = practicalExam?.TotalMarks ?? (hasPractical ? 20 : 0);
                int totalMax = midTotal + finalTotal + (hasPractical ? practicalTotal : 0);

                string gradeLetter = isGraded ? CalculateGradeLetter(totalObtained, totalMax) : "—";

                return new
                {
                    courseOfferingId = offering.Id,
                    courseCode = offering.Course?.CourseCode ?? string.Empty,
                    courseName = offering.Course?.CourseName ?? string.Empty,
                    teacherName = !string.IsNullOrWhiteSpace(offering.Teacher?.TeacherName) ? offering.Teacher.TeacherName : "-",
                    programName = offering.Semester?.AcademicProgram?.ProgramName ?? string.Empty,
                    semesterNumber = offering.Semester?.SemesterNumber ?? 0,
                    sessionName = offering.Semester?.AcademicSession?.SessionName ?? string.Empty,
                    hasPractical,
                    midMarks = mid,
                    midTotalMarks = midTotal,
                    finalMarks = fin,
                    finalTotalMarks = finalTotal,
                    practicalMarks = prac,
                    practicalTotalMarks = practicalTotal,
                    totalObtainedMarks = totalObtained,
                    totalMaxMarks = totalMax,
                    gradeLetter,
                    isGraded
                };
            }).ToList();

            return Ok(results);
        }

        #endregion

        #region Module 3: Attendance

        // GET: api/StudentPortal/attendance
        [HttpGet("attendance")]
        public async Task<IActionResult> GetAttendanceSummary()
        {
            var student = await GetLoggedInStudentAsync();
            if (student == null)
            {
                return Unauthorized(new { message = "Student profile not found for this account." });
            }

            if (!string.IsNullOrEmpty(student.Status) && !student.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Your student account is inactive. Please contact the administrator." });
            }

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id && e.Status == "Active")
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Teacher)
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Semester)
                        .ThenInclude(s => s.AcademicSession)
                .OrderBy(e => e.CourseOffering.Course.CourseName)
                .ToListAsync();

            var offeringIds = enrollments.Select(e => e.CourseOfferingId).ToList();

            var sessions = await _context.AttendanceSessions
                .Where(s => offeringIds.Contains(s.CourseOfferingId))
                .ToListAsync();

            var attendanceRecords = await _context.Attendance
                .Where(a => offeringIds.Contains(a.CourseOfferingId) && a.StudentId == student.Id)
                .ToListAsync();

            var summaryList = enrollments.Select(e =>
            {
                var offering = e.CourseOffering;
                var offeringSessions = sessions.Where(s => s.CourseOfferingId == offering.Id).ToList();
                var offeringRecords = attendanceRecords.Where(a => a.CourseOfferingId == offering.Id).ToList();

                int present = offeringRecords.Count(a => a.Status.Equals("Present", StringComparison.OrdinalIgnoreCase));
                int absent = offeringRecords.Count(a => a.Status.Equals("Absent", StringComparison.OrdinalIgnoreCase));
                int totalClasses = offeringSessions.Count > 0 ? offeringSessions.Count : offeringRecords.Count;

                double pct = totalClasses > 0 ? Math.Round(((double)present / totalClasses) * 100.0, 1) : 0.0;

                return new
                {
                    courseOfferingId = offering.Id,
                    courseCode = offering.Course?.CourseCode ?? string.Empty,
                    courseName = offering.Course?.CourseName ?? string.Empty,
                    teacherName = !string.IsNullOrWhiteSpace(offering.Teacher?.TeacherName) ? offering.Teacher.TeacherName : "-",
                    semesterNumber = offering.Semester?.SemesterNumber ?? 0,
                    sessionName = offering.Semester?.AcademicSession?.SessionName ?? string.Empty,
                    totalClasses,
                    presentCount = present,
                    absentCount = absent,
                    attendancePercentage = pct
                };
            }).ToList();

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

            if (!string.IsNullOrEmpty(student.Status) && !student.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Your student account is inactive. Please contact the administrator." });
            }

            // Security: Verify student is actively enrolled in this course offering
            var enrollment = await _context.Enrollments
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Teacher)
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Semester)
                        .ThenInclude(s => s.AcademicProgram)
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Semester)
                        .ThenInclude(s => s.AcademicSession)
                .FirstOrDefaultAsync(e => e.StudentId == student.Id && e.CourseOfferingId == courseOfferingId && e.Status == "Active");

            if (enrollment == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden: You are not enrolled in this course offering." });
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

            var history = sessions.Select(s =>
            {
                var rec = records.FirstOrDefault(r => r.AttendanceSessionId == s.Id)
                          ?? records.FirstOrDefault(r => r.AttendanceDate.Date == s.ClassDate.Date);

                return new
                {
                    sessionId = s.Id,
                    classDate = s.ClassDate,
                    status = rec != null ? rec.Status : "Not Marked"
                };
            }).ToList();

            // If classes were recorded directly without session
            if (history.Count == 0 && records.Count > 0)
            {
                history = records
                    .OrderByDescending(r => r.AttendanceDate)
                    .Select(r => new
                    {
                        sessionId = r.AttendanceSessionId ?? r.Id,
                        classDate = r.AttendanceDate,
                        status = r.Status
                    }).ToList();
            }

            int present = records.Count(r => r.Status.Equals("Present", StringComparison.OrdinalIgnoreCase));
            int absent = records.Count(r => r.Status.Equals("Absent", StringComparison.OrdinalIgnoreCase));
            int totalClasses = history.Count;
            double pct = totalClasses > 0 ? Math.Round(((double)present / totalClasses) * 100.0, 1) : 0.0;

            return Ok(new
            {
                courseOfferingId = offering.Id,
                courseCode = offering.Course?.CourseCode ?? string.Empty,
                courseName = offering.Course?.CourseName ?? string.Empty,
                teacherName = !string.IsNullOrWhiteSpace(offering.Teacher?.TeacherName) ? offering.Teacher.TeacherName : "-",
                programName = offering.Semester?.AcademicProgram?.ProgramName ?? string.Empty,
                semesterNumber = offering.Semester?.SemesterNumber ?? 0,
                sessionName = offering.Semester?.AcademicSession?.SessionName ?? string.Empty,
                totalClasses,
                presentCount = present,
                absentCount = absent,
                attendancePercentage = pct,
                history
            });
        }

        #endregion
    }
}

