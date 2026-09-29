using Microsoft.EntityFrameworkCore;
using University_Management_System.Data;
using University_Management_System.Models;

namespace University_Management_System.Data.Seed
{
    public static class AcademicDataSeeder
    {
        public static async Task SeedAcademicDataAsync(ApplicationDbContext context)
        {
            var departments = await context.Departments.ToListAsync();
            if (!departments.Any())
            {
                return;
            }

            // A. Ensure Every Department Has At Least One Academic Program
            var defaultProgramsByDept = new Dictionary<string, (string Name, string Code)>(StringComparer.OrdinalIgnoreCase)
            {
                { "Computer Science", ("BS Computer Science", "BSCS") },
                { "Mathematics", ("BS Mathematics", "BSM") },
                { "Physics", ("BS Physics", "BSPHY") },
                { "English", ("BS English", "BSENG") },
                { "Bussiness Administration", ("Bachelor of Business Administration", "BBA") },
                { "Business Administration", ("Bachelor of Business Administration", "BBA") },
                { "Electrical Engineering", ("BS Electrical Engineering", "BSEE") },
                { "Chemistry", ("BS Chemistry", "BSCHEM") },
                { "Biology", ("BS Biology", "BSBIO") },
                { "Ecnomoics", ("BS Economics", "BSECON") },
                { "Economics", ("BS Economics", "BSECON") }
            };

            foreach (var dept in departments)
            {
                var hasProgram = await context.AcademicPrograms.AnyAsync(p => p.DepartmentId == dept.Id);
                if (hasProgram)
                {
                    continue; // SKIP if department already has an academic program
                }

                string progName;
                string progCode;

                if (defaultProgramsByDept.TryGetValue(dept.DepartmentName ?? "", out var defProg))
                {
                    progName = defProg.Name;
                    progCode = defProg.Code;
                }
                else
                {
                    progName = $"BS {dept.DepartmentName}";
                    progCode = $"BS-{dept.Id}";
                }

                // Ensure unique code
                var codeExists = await context.AcademicPrograms.AnyAsync(p => p.ProgramCode == progCode);
                if (codeExists)
                {
                    progCode = $"{progCode}-{dept.Id}";
                }

                var newProgram = new AcademicProgram
                {
                    ProgramName = progName,
                    ProgramCode = progCode,
                    DurationInSemesters = 8,
                    DepartmentId = dept.Id
                };

                context.AcademicPrograms.Add(newProgram);
                await context.SaveChangesAsync();
            }

            // B. Ensure Every Academic Program Has At Least 6 Courses
            var curriculumCatalog = new Dictionary<string, List<(string Suffix, string Name)>>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "Computer Science", new List<(string Suffix, string Name)>
                    {
                        ("101", "Programming Fundamentals"),
                        ("102", "Object Oriented Programming"),
                        ("201", "Data Structures & Algorithms"),
                        ("202", "Database Systems"),
                        ("301", "Operating Systems"),
                        ("302", "Computer Networks")
                    }
                },
                {
                    "Software Engineering", new List<(string Suffix, string Name)>
                    {
                        ("101", "Introduction to Software Engineering"),
                        ("102", "Software Requirements Engineering"),
                        ("201", "Software Architecture & Design"),
                        ("202", "Software Quality Assurance"),
                        ("301", "Web Application Development"),
                        ("302", "Software Project Management")
                    }
                },
                {
                    "Mathematics", new List<(string Suffix, string Name)>
                    {
                        ("101", "Calculus & Analytical Geometry"),
                        ("102", "Multivariable Calculus"),
                        ("201", "Linear Algebra"),
                        ("202", "Differential Equations"),
                        ("301", "Discrete Mathematics"),
                        ("302", "Numerical Analysis")
                    }
                },
                {
                    "Physics", new List<(string Suffix, string Name)>
                    {
                        ("101", "Mechanics & Thermodynamics"),
                        ("102", "Waves & Oscillations"),
                        ("201", "Electricity & Magnetism"),
                        ("202", "Modern Physics"),
                        ("301", "Quantum Mechanics"),
                        ("302", "Electromagnetic Theory")
                    }
                },
                {
                    "English", new List<(string Suffix, string Name)>
                    {
                        ("101", "English Composition & Comprehension"),
                        ("102", "Introduction to Linguistics"),
                        ("201", "History of English Literature"),
                        ("202", "Academic Writing & Research"),
                        ("301", "Classical Poetry & Prose"),
                        ("302", "Short Fiction & Drama")
                    }
                },
                {
                    "Business Administration", new List<(string Suffix, string Name)>
                    {
                        ("101", "Principles of Management"),
                        ("102", "Financial Accounting"),
                        ("201", "Principles of Marketing"),
                        ("202", "Business Communication"),
                        ("301", "Human Resource Management"),
                        ("302", "Managerial Economics")
                    }
                },
                {
                    "Bussiness Administration", new List<(string Suffix, string Name)>
                    {
                        ("101", "Principles of Management"),
                        ("102", "Financial Accounting"),
                        ("201", "Principles of Marketing"),
                        ("202", "Business Communication"),
                        ("301", "Human Resource Management"),
                        ("302", "Managerial Economics")
                    }
                },
                {
                    "Electrical Engineering", new List<(string Suffix, string Name)>
                    {
                        ("101", "Linear Circuit Analysis"),
                        ("102", "Electronic Devices & Circuits"),
                        ("201", "Digital Logic Design"),
                        ("202", "Signals & Systems"),
                        ("301", "Electrical Machines"),
                        ("302", "Electromagnetic Fields")
                    }
                },
                {
                    "Chemistry", new List<(string Suffix, string Name)>
                    {
                        ("101", "General Chemistry"),
                        ("102", "Organic Chemistry"),
                        ("201", "Inorganic Chemistry"),
                        ("202", "Physical Chemistry"),
                        ("301", "Analytical Chemistry"),
                        ("302", "Biochemistry")
                    }
                },
                {
                    "Biology", new List<(string Suffix, string Name)>
                    {
                        ("101", "General Biology"),
                        ("102", "Cell Biology & Genetics"),
                        ("201", "Microbiology"),
                        ("202", "Molecular Biology"),
                        ("301", "Animal & Plant Physiology"),
                        ("302", "Ecology & Evolutionary Biology")
                    }
                },
                {
                    "Economics", new List<(string Suffix, string Name)>
                    {
                        ("101", "Principles of Microeconomics"),
                        ("102", "Principles of Macroeconomics"),
                        ("201", "Intermediate Microeconomics"),
                        ("202", "Intermediate Macroeconomics"),
                        ("301", "Introductory Econometrics"),
                        ("302", "Money, Banking & Financial Markets")
                    }
                },
                {
                    "Ecnomoics", new List<(string Suffix, string Name)>
                    {
                        ("101", "Principles of Microeconomics"),
                        ("102", "Principles of Macroeconomics"),
                        ("201", "Intermediate Microeconomics"),
                        ("202", "Intermediate Macroeconomics"),
                        ("301", "Introductory Econometrics"),
                        ("302", "Money, Banking & Financial Markets")
                    }
                }
            };

            var programs = await context.AcademicPrograms.Include(p => p.Department).ToListAsync();

            foreach (var prog in programs)
            {
                var mappedCount = await context.AcademicProgramCourses
                    .CountAsync(apc => apc.AcademicProgramId == prog.Id);

                if (mappedCount >= 6)
                {
                    continue; // KEEP untouched if program already has 6 or more courses
                }

                var neededCount = 6 - mappedCount;
                var deptName = prog.Department?.DepartmentName ?? "";
                var cleanProgCode = !string.IsNullOrWhiteSpace(prog.ProgramCode) ? prog.ProgramCode.Trim() : $"PROG-{prog.Id}";

                // Determine candidate course definitions
                List<(string Code, string Name)> candidates = new List<(string Code, string Name)>();

                if (curriculumCatalog.TryGetValue(deptName, out var catalogList))
                {
                    foreach (var item in catalogList)
                    {
                        candidates.Add(($"{cleanProgCode}-{item.Suffix}", item.Name));
                    }
                }

                // Fallback generator if catalog lacks entries
                for (int i = 1; candidates.Count < 6; i++)
                {
                    candidates.Add(($"{cleanProgCode}-{100 + i}", $"{prog.ProgramName} Core Course {i}"));
                }

                int addedForProgram = 0;
                foreach (var candidate in candidates)
                {
                    if (addedForProgram >= neededCount)
                    {
                        break;
                    }

                    // Check if Course already exists by CourseCode
                    var existingCourse = await context.Courses
                        .FirstOrDefaultAsync(c => c.CourseCode == candidate.Code);

                    if (existingCourse == null)
                    {
                        existingCourse = new Course
                        {
                            CourseCode = candidate.Code,
                            CourseName = candidate.Name,
                            CreditHours = 3,
                            Level = "Undergraduate",
                            DepartmentId = prog.DepartmentId,
                            Status = "Active"
                        };

                        context.Courses.Add(existingCourse);
                        await context.SaveChangesAsync();
                    }

                    // Check if already mapped to this AcademicProgram
                    var isMapped = await context.AcademicProgramCourses
                        .AnyAsync(apc => apc.AcademicProgramId == prog.Id && apc.CourseId == existingCourse.Id);

                    if (!isMapped)
                    {
                        context.AcademicProgramCourses.Add(new AcademicProgramCourse
                        {
                            AcademicProgramId = prog.Id,
                            CourseId = existingCourse.Id
                        });
                        await context.SaveChangesAsync();
                        addedForProgram++;
                    }
                }
            }

            // C. Ensure Every Academic Program Has Its Expected Semesters for Explicitly Active Academic Session
            var activeSession = await context.AcademicSessions.FirstOrDefaultAsync(s => s.IsActive);
            if (activeSession == null)
            {
                Console.WriteLine("Skipping semester seeding: No active AcademicSession found with IsActive = true.");
                return;
            }

            foreach (var prog in programs)
            {
                int targetSemesters = prog.DurationInSemesters.HasValue && prog.DurationInSemesters.Value > 0
                    ? prog.DurationInSemesters.Value
                    : 8;

                for (int semNum = 1; semNum <= targetSemesters; semNum++)
                {
                    var exists = await context.Semesters.AnyAsync(s =>
                        s.AcademicProgramId == prog.Id &&
                        s.AcademicSessionId == activeSession.Id &&
                        s.SemesterNumber == semNum);

                    if (!exists)
                    {
                        context.Semesters.Add(new Semester
                        {
                            SemesterNumber = semNum,
                            AcademicProgramId = prog.Id,
                            AcademicSessionId = activeSession.Id,
                            Status = "Active"
                        });
                    }
                }
            }
            await context.SaveChangesAsync();
        }
    }
}


