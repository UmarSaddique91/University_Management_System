using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using University_Management_System.Data;
using University_Management_System.DTOs;
using University_Management_System.Models;

namespace University_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "TeacherAccess")]
    public class TeacherPortalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TeacherPortalController(ApplicationDbContext context)
        {
            _context = context;
        }

        

        // Extract Record from the login token
        private async Task<Teacher?> GetLoggedInTeacherAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name;

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(userEmail))
            {
                return null;
            }

            var teacher = await _context.Teachers.FirstOrDefaultAsync(t =>
                (!string.IsNullOrEmpty(userId) && t.UserId == userId) ||
                (!string.IsNullOrEmpty(userEmail) && t.Email != null && t.Email.ToLower() == userEmail.ToLower())
            );

            return teacher;
        }

        // Grade calculation (Marks to Letter Grade)
        private static string CalculateGradeLetter(double obtained, double total)
        {
            if (total <= 0) return "F";
            var percentage = (obtained / total) * 100.0;

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

        

        

        // GET: api/TeacherPortal/my-courses
        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var teacher = await GetLoggedInTeacherAsync();
            if (teacher == null)
            {
                return Unauthorized(new { message = "Teacher profile not found for this account." });
            }

            var offerings = await _context.CourseOfferings
                .Where(co => co.TeacherId == teacher.Id && co.Status == "Active")
                .Include(co => co.Course)
                .Include(co => co.Semester).ThenInclude(s => s.AcademicProgram)
                .Include(co => co.Semester).ThenInclude(s => s.AcademicSession)
                .OrderBy(co => co.Course.CourseName)
                .ToListAsync();

            var offeringIds = offerings.Select(co => co.Id).ToList();

            var totalDistinctStudents = await _context.Enrollments
                .Where(e => offeringIds.Contains(e.CourseOfferingId) && e.Status == "Active")
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync();

            var totalClassesConducted = await _context.AttendanceSessions
                .Where(s => offeringIds.Contains(s.CourseOfferingId))
                .CountAsync();

            // Fallback for direct attendance records without a session
            if (totalClassesConducted == 0 && offeringIds.Any())
            {
                totalClassesConducted = await _context.Attendance
                    .Where(a => offeringIds.Contains(a.CourseOfferingId))
                    .Select(a => new { a.CourseOfferingId, a.AttendanceDate.Date })
                    .Distinct()
                    .CountAsync();
            }

            var coursesList = new List<object>();
            foreach (var co in offerings)
            {
                var studentCount = await _context.Enrollments
                    .CountAsync(e => e.CourseOfferingId == co.Id && e.Status == "Active");

                coursesList.Add(new
                {
                    courseOfferingId = co.Id,
                    courseId = co.CourseId,
                    courseCode = co.Course?.CourseCode ?? string.Empty,
                    courseName = co.Course?.CourseName ?? string.Empty,
                    hasPractical = co.Course != null && co.Course.HasPractical,
                    programName = co.Semester?.AcademicProgram?.ProgramName ?? string.Empty,
                    semesterNumber = co.Semester?.SemesterNumber ?? 0,
                    sessionName = co.Semester?.AcademicSession?.SessionName ?? string.Empty,
                    studentCount = studentCount
                });
            }

            return Ok(new
            {
                teacher = new
                {
                    teacherId = teacher.Id,
                    teacherCode = teacher.TeacherId,
                    teacherName = teacher.TeacherName,
                    email = teacher.Email
                },
                summary = new
                {
                    assignedCourses = offerings.Count,
                    totalStudents = totalDistinctStudents,
                    classesConducted = totalClassesConducted
                },
                courses = coursesList
            });
        }

        

        //Course Stuent List

        [HttpGet("course/{courseOfferingId}/students")]
        public async Task<IActionResult> GetCourseStudents(int courseOfferingId)
        {
            var teacher = await GetLoggedInTeacherAsync();
            if (teacher == null) return Unauthorized(new { message = "Teacher profile not found." });

            var offering = await _context.CourseOfferings.FirstOrDefaultAsync(co => co.Id == courseOfferingId);
            if (offering == null) return NotFound(new { message = "Course offering not found." });
            if (offering.TeacherId != teacher.Id) return StatusCode(403, new { message = "You are not assigned to this course." });

            var students = await _context.Enrollments
                .Where(e => e.CourseOfferingId == courseOfferingId && e.Status == "Active")
                .Include(e => e.Student)
                .OrderBy(e => e.Student.RollNumber)
                .Select(e => new
                {
                    studentId = e.Student.Id,
                    rollNumber = e.Student.RollNumber ?? string.Empty,
                    studentName = e.Student.StudentName ?? string.Empty,
                    email = e.Student.Email ?? string.Empty
                })
                .ToListAsync();

            return Ok(students);
        }



        //Exam and Course



        [HttpGet("course/{courseOfferingId}/exams")]
        public async Task<IActionResult> GetCourseExams(int courseOfferingId)
        {
            var teacher = await GetLoggedInTeacherAsync();
            if (teacher == null) return Unauthorized(new { message = "Teacher profile not found." });

            var offering = await _context.CourseOfferings
                .Include(co => co.Course)
                .Include(co => co.Semester).ThenInclude(s => s.AcademicProgram)
                .Include(co => co.Semester).ThenInclude(s => s.AcademicSession)
                .FirstOrDefaultAsync(co => co.Id == courseOfferingId);

            if (offering == null) return NotFound(new { message = "Course offering not found." });
            if (offering.TeacherId != teacher.Id) return StatusCode(403, new { message = "You are not assigned to this course." });

            var hasPractical = offering.Course != null && offering.Course.HasPractical;

            var existingExams = await _context.Exams
                .Where(e => e.CourseOfferingId == courseOfferingId)
                .ToListAsync();

            // Ensure default exam components exist (Mid, Final, Practical)
            var midExam = existingExams.FirstOrDefault(e => e.ExamType.ToLower() == "mid");
            if (midExam == null)
            {
                midExam = new Exam
                {
                    CourseOfferingId = courseOfferingId,
                    ExamType = "Mid",
                    ExamDate = DateTime.UtcNow,
                    TotalMarks = 30
                };
                _context.Exams.Add(midExam);
            }

            var finalExam = existingExams.FirstOrDefault(e => e.ExamType.ToLower() == "final");
            int expectedFinalMarks = hasPractical ? 50 : 70;
            if (finalExam == null)
            {
                finalExam = new Exam
                {
                    CourseOfferingId = courseOfferingId,
                    ExamType = "Final",
                    ExamDate = DateTime.UtcNow,
                    TotalMarks = expectedFinalMarks
                };
                _context.Exams.Add(finalExam);
            }
            else
            {
                finalExam.TotalMarks = expectedFinalMarks;
            }

            Exam? practicalExam = null;
            if (hasPractical)
            {
                practicalExam = existingExams.FirstOrDefault(e => e.ExamType.ToLower() == "practical");
                if (practicalExam == null)
                {
                    practicalExam = new Exam
                    {
                        CourseOfferingId = courseOfferingId,
                        ExamType = "Practical",
                        ExamDate = DateTime.UtcNow,
                        TotalMarks = 20
                    };
                    _context.Exams.Add(practicalExam);
                }
            }

            await _context.SaveChangesAsync();

            var enrolledStudents = await _context.Enrollments
                .Where(e => e.CourseOfferingId == courseOfferingId && e.Status == "Active")
                .Include(e => e.Student)
                .OrderBy(e => e.Student.RollNumber)
                .Select(e => e.Student)
                .ToListAsync();

            var studentIds = enrolledStudents.Select(s => s.Id).ToList();

            var examIds = new List<int> { midExam.Id, finalExam.Id };
            if (practicalExam != null) examIds.Add(practicalExam.Id);

            var grades = await _context.Grades
                .Where(g => examIds.Contains(g.ExamId) && studentIds.Contains(g.StudentId))
                .ToListAsync();

            var studentsMarks = new List<object>();
            foreach (var student in enrolledStudents)
            {
                var midGrade = grades.FirstOrDefault(g => g.StudentId == student.Id && g.ExamId == midExam.Id);
                var finalGrade = grades.FirstOrDefault(g => g.StudentId == student.Id && g.ExamId == finalExam.Id);
                var practicalGrade = practicalExam != null
                    ? grades.FirstOrDefault(g => g.StudentId == student.Id && g.ExamId == practicalExam.Id)
                    : null;

                double? mid = midGrade?.ObtainedMarks;
                double? fin = finalGrade?.ObtainedMarks;
                double? prac = practicalGrade?.ObtainedMarks;
                double total = (mid ?? 0) + (fin ?? 0) + (prac ?? 0);

                studentsMarks.Add(new
                {
                    studentId = student.Id,
                    rollNumber = student.RollNumber ?? string.Empty,
                    studentName = student.StudentName ?? string.Empty,
                    midMarks = mid,
                    finalMarks = fin,
                    practicalMarks = prac,
                    totalMarks = total
                });
            }

            return Ok(new
            {
                courseOfferingId = offering.Id,
                courseCode = offering.Course?.CourseCode ?? string.Empty,
                courseName = offering.Course?.CourseName ?? string.Empty,
                hasPractical,
                programName = offering.Semester?.AcademicProgram?.ProgramName ?? string.Empty,
                semesterNumber = offering.Semester?.SemesterNumber ?? 0,
                sessionName = offering.Semester?.AcademicSession?.SessionName ?? string.Empty,
                midExamId = midExam.Id,
                midTotalMarks = midExam.TotalMarks,
                finalExamId = finalExam.Id,
                finalTotalMarks = finalExam.TotalMarks,
                practicalExamId = practicalExam?.Id,
                practicalTotalMarks = practicalExam?.TotalMarks ?? 0,
                students = studentsMarks
            });
        }

        // POST: api/TeacherPortal/course/{courseOfferingId}/save-marks
        [HttpPost("course/{courseOfferingId}/save-marks")]
        public async Task<IActionResult> SaveCourseMarks(int courseOfferingId, [FromBody] SaveCourseMarksDTO model)
        {
            var teacher = await GetLoggedInTeacherAsync();
            if (teacher == null) return Unauthorized(new { message = "Teacher profile not found." });

            var offering = await _context.CourseOfferings
                .Include(co => co.Course)
                .FirstOrDefaultAsync(co => co.Id == courseOfferingId);

            if (offering == null) return NotFound(new { message = "Course offering not found." });
            if (offering.TeacherId != teacher.Id) return StatusCode(403, new { message = "You are not assigned to this course." });

            var hasPractical = offering.Course != null && offering.Course.HasPractical;

            var exams = await _context.Exams
                .Where(e => e.CourseOfferingId == courseOfferingId)
                .ToListAsync();

            var midExam = exams.FirstOrDefault(e => e.ExamType.ToLower() == "mid");
            var finalExam = exams.FirstOrDefault(e => e.ExamType.ToLower() == "final");
            var practicalExam = hasPractical ? exams.FirstOrDefault(e => e.ExamType.ToLower() == "practical") : null;

            if (midExam == null || finalExam == null || (hasPractical && practicalExam == null))
            {
                return BadRequest(new { message = "Exams configuration not initialized. Please load exam details first." });
            }

            var enrolledStudentIds = await _context.Enrollments
                .Where(e => e.CourseOfferingId == courseOfferingId && e.Status == "Active")
                .Select(e => e.StudentId)
                .ToListAsync();

            var examIds = new List<int> { midExam.Id, finalExam.Id };
            if (practicalExam != null) examIds.Add(practicalExam.Id);

            var existingGrades = await _context.Grades
                .Where(g => examIds.Contains(g.ExamId))
                .ToListAsync();

            foreach (var item in model.Students)
            {
                if (!enrolledStudentIds.Contains(item.StudentId)) continue;

                // Mid marks save
                if (item.MidMarks.HasValue)
                {
                    if (item.MidMarks.Value < 0 || item.MidMarks.Value > midExam.TotalMarks)
                    {
                        return BadRequest(new { message = $"Mid marks for student ID {item.StudentId} must be between 0 and {midExam.TotalMarks}." });
                    }

                    var grade = existingGrades.FirstOrDefault(g => g.StudentId == item.StudentId && g.ExamId == midExam.Id);
                    if (grade == null)
                    {
                        grade = new Grade
                        {
                            StudentId = item.StudentId,
                            ExamId = midExam.Id,
                            ObtainedMarks = item.MidMarks.Value,
                            GradeLetter = CalculateGradeLetter(item.MidMarks.Value, midExam.TotalMarks)
                        };
                        _context.Grades.Add(grade);
                        existingGrades.Add(grade);
                    }
                    else
                    {
                        grade.ObtainedMarks = item.MidMarks.Value;
                        grade.GradeLetter = CalculateGradeLetter(item.MidMarks.Value, midExam.TotalMarks);
                    }
                }

                // Final marks save
                if (item.FinalMarks.HasValue)
                {
                    if (item.FinalMarks.Value < 0 || item.FinalMarks.Value > finalExam.TotalMarks)
                    {
                        return BadRequest(new { message = $"Final marks for student ID {item.StudentId} must be between 0 and {finalExam.TotalMarks}." });
                    }

                    var grade = existingGrades.FirstOrDefault(g => g.StudentId == item.StudentId && g.ExamId == finalExam.Id);
                    if (grade == null)
                    {
                        grade = new Grade
                        {
                            StudentId = item.StudentId,
                            ExamId = finalExam.Id,
                            ObtainedMarks = item.FinalMarks.Value,
                            GradeLetter = CalculateGradeLetter(item.FinalMarks.Value, finalExam.TotalMarks)
                        };
                        _context.Grades.Add(grade);
                        existingGrades.Add(grade);
                    }
                    else
                    {
                        grade.ObtainedMarks = item.FinalMarks.Value;
                        grade.GradeLetter = CalculateGradeLetter(item.FinalMarks.Value, finalExam.TotalMarks);
                    }
                }

                // Practical marks save
                if (hasPractical && practicalExam != null && item.PracticalMarks.HasValue)
                {
                    if (item.PracticalMarks.Value < 0 || item.PracticalMarks.Value > practicalExam.TotalMarks)
                    {
                        return BadRequest(new { message = $"Practical marks for student ID {item.StudentId} must be between 0 and {practicalExam.TotalMarks}." });
                    }

                    var grade = existingGrades.FirstOrDefault(g => g.StudentId == item.StudentId && g.ExamId == practicalExam.Id);
                    if (grade == null)
                    {
                        grade = new Grade
                        {
                            StudentId = item.StudentId,
                            ExamId = practicalExam.Id,
                            ObtainedMarks = item.PracticalMarks.Value,
                            GradeLetter = CalculateGradeLetter(item.PracticalMarks.Value, practicalExam.TotalMarks)
                        };
                        _context.Grades.Add(grade);
                        existingGrades.Add(grade);
                    }
                    else
                    {
                        grade.ObtainedMarks = item.PracticalMarks.Value;
                        grade.GradeLetter = CalculateGradeLetter(item.PracticalMarks.Value, practicalExam.TotalMarks);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Marks saved successfully." });
        }



        //Attendence Module
        [HttpGet("course/{courseOfferingId}/attendance-history")]
        public async Task<IActionResult> GetAttendanceHistory(int courseOfferingId)
        {
            var teacher = await GetLoggedInTeacherAsync();
            if (teacher == null) return Unauthorized(new { message = "Teacher profile not found." });

            var offering = await _context.CourseOfferings
                .Include(co => co.Course)
                .Include(co => co.Semester).ThenInclude(s => s.AcademicProgram)
                .Include(co => co.Semester).ThenInclude(s => s.AcademicSession)
                .FirstOrDefaultAsync(co => co.Id == courseOfferingId);

            if (offering == null) return NotFound(new { message = "Course offering not found." });
            if (offering.TeacherId != teacher.Id) return StatusCode(403, new { message = "You are not assigned to this course." });

            // Fetch attendance sessions for this offering
            var sessions = await _context.AttendanceSessions
                .Where(s => s.CourseOfferingId == courseOfferingId)
                .OrderByDescending(s => s.ClassDate)
                .ThenByDescending(s => s.Id)
                .ToListAsync();

            var sessionIds = sessions.Select(s => s.Id).ToList();

            var sessionRecords = await _context.Attendance
                .Where(a => a.AttendanceSessionId.HasValue && sessionIds.Contains(a.AttendanceSessionId.Value))
                .ToListAsync();

            var sessionList = new List<object>();
            foreach (var s in sessions)
            {
                var records = sessionRecords.Where(r => r.AttendanceSessionId == s.Id).ToList();
                int present = records.Count(r => r.Status.Equals("Present", StringComparison.OrdinalIgnoreCase));
                int absent = records.Count(r => r.Status.Equals("Absent", StringComparison.OrdinalIgnoreCase));

                sessionList.Add(new
                {
                    sessionId = s.Id,
                    classDate = s.ClassDate,
                    createdAt = s.CreatedAt,
                    totalStudents = records.Count,
                    presentCount = present,
                    absentCount = absent
                });
            }

            var enrolledStudents = await _context.Enrollments
                .Where(e => e.CourseOfferingId == courseOfferingId && e.Status == "Active")
                .Include(e => e.Student)
                .OrderBy(e => e.Student.RollNumber)
                .Select(e => e.Student)
                .ToListAsync();

            var allAttendance = await _context.Attendance
                .Where(a => a.CourseOfferingId == courseOfferingId)
                .ToListAsync();

            int totalClasses = sessions.Count;
            var studentPercentages = new List<object>();

            foreach (var student in enrolledStudents)
            {
                int attended = allAttendance.Count(a => a.StudentId == student.Id && a.Status.Equals("Present", StringComparison.OrdinalIgnoreCase));
                int total = totalClasses > 0 ? totalClasses : allAttendance.Where(a => a.StudentId == student.Id).Select(a => a.AttendanceDate.Date).Distinct().Count();
                double pct = total > 0 ? Math.Round(((double)attended / total) * 100.0, 1) : 0;

                studentPercentages.Add(new
                {
                    studentId = student.Id,
                    rollNumber = student.RollNumber ?? string.Empty,
                    studentName = student.StudentName ?? string.Empty,
                    totalClasses = total,
                    presentCount = attended,
                    attendancePercentage = pct
                });
            }

            return Ok(new
            {
                courseOfferingId = offering.Id,
                courseCode = offering.Course?.CourseCode ?? string.Empty,
                courseName = offering.Course?.CourseName ?? string.Empty,
                programName = offering.Semester?.AcademicProgram?.ProgramName ?? string.Empty,
                semesterNumber = offering.Semester?.SemesterNumber ?? 0,
                sessionName = offering.Semester?.AcademicSession?.SessionName ?? string.Empty,
                sessions = sessionList,
                studentPercentages
            });
        }

        // POST: api/TeacherPortal/course/{courseOfferingId}/take-attendance
        [HttpPost("course/{courseOfferingId}/take-attendance")]
        public async Task<IActionResult> TakeAttendance(int courseOfferingId, [FromBody] TakeAttendanceDTO model)
        {
            var teacher = await GetLoggedInTeacherAsync();
            if (teacher == null) return Unauthorized(new { message = "Teacher profile not found." });

            var offering = await _context.CourseOfferings.FirstOrDefaultAsync(co => co.Id == courseOfferingId);
            if (offering == null) return NotFound(new { message = "Course offering not found." });
            if (offering.TeacherId != teacher.Id) return StatusCode(403, new { message = "You are not assigned to this course." });

            var enrolledStudentIds = await _context.Enrollments
                .Where(e => e.CourseOfferingId == courseOfferingId && e.Status == "Active")
                .Select(e => e.StudentId)
                .ToListAsync();

            var session = new AttendanceSession
            {
                CourseOfferingId = courseOfferingId,
                ClassDate = model.ClassDate,
                CreatedAt = DateTime.UtcNow
            };
            _context.AttendanceSessions.Add(session);
            await _context.SaveChangesAsync();

            // Record attendance for each student
            foreach (var rec in model.Records)
            {
                if (!enrolledStudentIds.Contains(rec.StudentId)) continue;

                var status = rec.Status.Equals("Absent", StringComparison.OrdinalIgnoreCase) ? "Absent" : "Present";
                _context.Attendance.Add(new Attendance
                {
                    CourseOfferingId = courseOfferingId,
                    StudentId = rec.StudentId,
                    AttendanceDate = model.ClassDate,
                    Status = status,
                    AttendanceSessionId = session.Id
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Attendance marked successfully.",
                sessionId = session.Id
            });
        }

        // GET: api/TeacherPortal/attendance-session/{sessionId}
        [HttpGet("attendance-session/{sessionId}")]
        public async Task<IActionResult> GetAttendanceSession(int sessionId)
        {
            var teacher = await GetLoggedInTeacherAsync();
            if (teacher == null) return Unauthorized(new { message = "Teacher profile not found." });

            var session = await _context.AttendanceSessions
                .Include(s => s.CourseOffering).ThenInclude(co => co.Course)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return NotFound(new { message = "Attendance session not found." });
            if (session.CourseOffering.TeacherId != teacher.Id) return StatusCode(403, new { message = "You are not assigned to this course." });

            var records = await _context.Attendance
                .Where(a => a.AttendanceSessionId == sessionId)
                .Include(a => a.Student)
                .OrderBy(a => a.Student.RollNumber)
                .Select(a => new
                {
                    attendanceId = a.Id,
                    studentId = a.StudentId,
                    rollNumber = a.Student.RollNumber ?? string.Empty,
                    studentName = a.Student.StudentName ?? string.Empty,
                    status = a.Status
                })
                .ToListAsync();

            return Ok(new
            {
                sessionId = session.Id,
                courseOfferingId = session.CourseOfferingId,
                courseName = session.CourseOffering?.Course?.CourseName ?? string.Empty,
                classDate = session.ClassDate,
                records
            });
        }

        // PUT: api/TeacherPortal/attendance-session/{sessionId}
        [HttpPut("attendance-session/{sessionId}")]
        public async Task<IActionResult> UpdateAttendanceSession(int sessionId, [FromBody] UpdateAttendanceSessionDTO model)
        {
            var teacher = await GetLoggedInTeacherAsync();
            if (teacher == null) return Unauthorized(new { message = "Teacher profile not found." });

            var session = await _context.AttendanceSessions
                .Include(s => s.CourseOffering)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return NotFound(new { message = "Attendance session not found." });
            if (session.CourseOffering.TeacherId != teacher.Id) return StatusCode(403, new { message = "You are not assigned to this course." });

            var records = await _context.Attendance
                .Where(a => a.AttendanceSessionId == sessionId)
                .ToListAsync();

            foreach (var rec in model.Records)
            {
                var existing = records.FirstOrDefault(r => r.StudentId == rec.StudentId);
                var status = rec.Status.Equals("Absent", StringComparison.OrdinalIgnoreCase) ? "Absent" : "Present";

                if (existing != null)
                {
                    existing.Status = status;
                }
                else
                {
                    _context.Attendance.Add(new Attendance
                    {
                        CourseOfferingId = session.CourseOfferingId,
                        StudentId = rec.StudentId,
                        AttendanceDate = session.ClassDate,
                        Status = status,
                        AttendanceSessionId = sessionId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Attendance updated successfully." });
        }

    }
}


