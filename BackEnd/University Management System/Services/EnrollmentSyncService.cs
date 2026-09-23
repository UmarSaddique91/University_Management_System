using Microsoft.EntityFrameworkCore;
using University_Management_System.Data;
using University_Management_System.Models.MainModels;

namespace University_Management_System.Services
{
    public class EnrollmentSyncService
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentSyncService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Synchronizes course enrollments for all students currently assigned to the given semester.
        /// Ensures each student is enrolled in all active course offerings for that semester,
        /// respecting the unique (StudentId, CourseOfferingId) constraint and canonical 'Active' status.
        /// </summary>
        public async Task<int> SynchronizeSemesterEnrollmentsAsync(int semesterId)
        {
            var students = await _context.Students
                .Where(s => s.CurrentSemesterId == semesterId)
                .ToListAsync();

            if (!students.Any())
            {
                return 0;
            }

            var activeOfferings = await _context.CourseOfferings
                .Where(co => co.SemesterId == semesterId && co.Status == "Active")
                .ToListAsync();

            if (!activeOfferings.Any())
            {
                return 0;
            }

            var studentIds = students.Select(s => s.Id).ToList();
            var offeringIds = activeOfferings.Select(o => o.Id).ToList();

            var existingEnrollments = await _context.Enrollments
                .Where(e => studentIds.Contains(e.StudentId) && offeringIds.Contains(e.CourseOfferingId))
                .Select(e => new { e.StudentId, e.CourseOfferingId })
                .ToListAsync();

            var existingSet = new HashSet<(int StudentId, int CourseOfferingId)>(
                existingEnrollments.Select(e => (e.StudentId, e.CourseOfferingId))
            );

            var newEnrollments = new List<Enrollment>();
            foreach (var student in students)
            {
                foreach (var offering in activeOfferings)
                {
                    if (!existingSet.Contains((student.Id, offering.Id)))
                    {
                        newEnrollments.Add(new Enrollment
                        {
                            StudentId = student.Id,
                            CourseOfferingId = offering.Id,
                            Status = "Active",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            if (newEnrollments.Any())
            {
                _context.Enrollments.AddRange(newEnrollments);
                await _context.SaveChangesAsync();
            }

            return newEnrollments.Count;
        }

        /// <summary>
        /// Synchronizes enrollments across all active semesters in the system.
        /// </summary>
        public async Task<int> SynchronizeAllActiveSemestersAsync()
        {
            var activeSemesters = await _context.Semesters
                .Where(s => s.Status == "Active")
                .Select(s => s.Id)
                .ToListAsync();

            int totalCreated = 0;
            foreach (var semesterId in activeSemesters)
            {
                totalCreated += await SynchronizeSemesterEnrollmentsAsync(semesterId);
            }

            return totalCreated;
        }
    }
}

