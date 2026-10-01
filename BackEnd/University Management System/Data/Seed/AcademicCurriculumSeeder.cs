using Microsoft.EntityFrameworkCore;
using University_Management_System.Data;
using University_Management_System.Models;

namespace University_Management_System.Data.Seed
{
    public static class AcademicCurriculumSeeder
    {
        public class CourseSpec
        {
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public int DepartmentId { get; set; }
            public bool HasPractical { get; set; } = false;
            public int CreditHours { get; set; } = 3;
        }

        public static async Task SeedCurriculumAsync(ApplicationDbContext context)
        {
            var departments = await context.Departments.ToListAsync();
            var programs = await context.AcademicPrograms.ToListAsync();
            var semesters = await context.Semesters.ToListAsync();

            if (!departments.Any() || !programs.Any() || !semesters.Any())
            {
                return;
            }

            // Define comprehensive 6-course curriculum for all 9 programs across all 8 semesters
            var curriculum = GetCurriculumMap();

            // Cache existing courses by CourseCode (case-insensitive)
            var existingCourses = await context.Courses.ToListAsync();
            var courseMap = new Dictionary<string, Course>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in existingCourses)
            {
                if (!string.IsNullOrWhiteSpace(c.CourseCode) && !courseMap.ContainsKey(c.CourseCode))
                {
                    courseMap[c.CourseCode] = c;
                }
            }

            // 1. Ensure all courses exist in Courses table
            var newCourses = new List<Course>();
            foreach (var progKvp in curriculum)
            {
                foreach (var semKvp in progKvp.Value)
                {
                    foreach (var spec in semKvp.Value)
                    {
                        if (!courseMap.ContainsKey(spec.Code))
                        {
                            var course = new Course
                            {
                                CourseCode = spec.Code,
                                CourseName = spec.Name,
                                CreditHours = spec.CreditHours,
                                DepartmentId = spec.DepartmentId,
                                HasPractical = spec.HasPractical,
                                Level = "Undergraduate",
                                Status = "Active"
                            };
                            newCourses.Add(course);
                            courseMap[spec.Code] = course;
                        }
                    }
                }
            }

            if (newCourses.Any())
            {
                context.Courses.AddRange(newCourses);
                await context.SaveChangesAsync();
            }

            // Refresh course map with IDs
            foreach (var c in await context.Courses.ToListAsync())
            {
                if (!string.IsNullOrWhiteSpace(c.CourseCode))
                {
                    courseMap[c.CourseCode] = c;
                }
            }

            // 2. Ensure AcademicProgramCourses mappings
            var existingMappings = await context.AcademicProgramCourses.ToListAsync();
            var mappingSet = new HashSet<(int ProgramId, int CourseId)>(
                existingMappings.Select(m => (m.AcademicProgramId, m.CourseId))
            );

            var newMappings = new List<AcademicProgramCourse>();
            foreach (var prog in programs)
            {
                var cleanCode = prog.ProgramCode?.Trim() ?? "";
                if (!curriculum.TryGetValue(cleanCode, out var semMap))
                {
                    continue;
                }

                foreach (var semKvp in semMap)
                {
                    foreach (var spec in semKvp.Value)
                    {
                        if (courseMap.TryGetValue(spec.Code, out var c))
                        {
                            if (!mappingSet.Contains((prog.Id, c.Id)))
                            {
                                var apc = new AcademicProgramCourse
                                {
                                    AcademicProgramId = prog.Id,
                                    CourseId = c.Id
                                };
                                newMappings.Add(apc);
                                mappingSet.Add((prog.Id, c.Id));
                            }
                        }
                    }
                }
            }

            if (newMappings.Any())
            {
                context.AcademicProgramCourses.AddRange(newMappings);
                await context.SaveChangesAsync();
            }

            // 3. Ensure CourseOfferings for every semester of every academic program (6 per semester)
            var existingOfferings = await context.CourseOfferings.ToListAsync();
            var offeringMap = new Dictionary<(int SemesterId, int CourseId), CourseOffering>();
            foreach (var off in existingOfferings)
            {
                offeringMap[(off.SemesterId, off.CourseId)] = off;
            }

            var newOfferings = new List<CourseOffering>();
            foreach (var prog in programs)
            {
                var cleanCode = prog.ProgramCode?.Trim() ?? "";
                if (!curriculum.TryGetValue(cleanCode, out var semMap))
                {
                    continue;
                }

                var progSemesters = semesters.Where(s => s.AcademicProgramId == prog.Id).ToList();

                foreach (var sem in progSemesters)
                {
                    if (!semMap.TryGetValue(sem.SemesterNumber, out var specs))
                    {
                        continue;
                    }

                    // Sirf pehle 2 courses per semester seed karo
                    int addedThisSem = 0;
                    foreach (var spec in specs)
                    {
                        if (addedThisSem >= 2) break; // 2 se zyada nahi

                        if (courseMap.TryGetValue(spec.Code, out var c))
                        {
                            if (!offeringMap.ContainsKey((sem.Id, c.Id)))
                            {
                                var off = new CourseOffering
                                {
                                    SemesterId = sem.Id,
                                    CourseId = c.Id,
                                    TeacherId = null, // Unassigned by default
                                    Status = "Active"
                                };
                                newOfferings.Add(off);
                                offeringMap[(sem.Id, c.Id)] = off;
                            }
                            addedThisSem++;
                        }
                    }
                }
            }

            if (newOfferings.Any())
            {
                context.CourseOfferings.AddRange(newOfferings);
                await context.SaveChangesAsync();
            }

            // 4. Data Fix: Clean up invalid enrollments for Jamel Ahmad (Id: 17)
            var jamel = await context.Students.FirstOrDefaultAsync(s => s.Id == 17 || s.RollNumber == "BSPHY-001");
            if (jamel != null && jamel.CurrentSemesterId.HasValue)
            {
                // Remove enrollments that are NOT in Jamel's current semester (e.g. Offering 18 from Semester 3)
                var invalidEnrollments = await context.Enrollments
                    .Include(e => e.CourseOffering)
                    .Where(e => e.StudentId == jamel.Id && e.CourseOffering.SemesterId != jamel.CurrentSemesterId.Value)
                    .ToListAsync();

                if (invalidEnrollments.Any())
                {
                    context.Enrollments.RemoveRange(invalidEnrollments);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static Dictionary<string, Dictionary<int, List<CourseSpec>>> GetCurriculumMap()
        {
            var map = new Dictionary<string, Dictionary<int, List<CourseSpec>>>(StringComparer.OrdinalIgnoreCase);

            // ==========================================
            // 1. BS COMPUTER SCIENCE (BSCS) - Dept 1
            // ==========================================
            map["BSCS"] = new Dictionary<int, List<CourseSpec>>
            {
                [1] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "CS-101", Name = "Intro to Information & Communication Tech", DepartmentId = 1, HasPractical = true, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-101", Name = "Programming Fundamentals", DepartmentId = 1, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "MATH-101", Name = "Calculus & Analytical Geometry", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-101", Name = "English Composition & Comprehension", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "ISL-101", Name = "Islamic Studies / Ethics", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "PHY-103", Name = "Applied Physics for Computing", DepartmentId = 3, HasPractical = true, CreditHours = 3 }
                },
                [2] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSCS-102", Name = "Object Oriented Programming", DepartmentId = 1, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSCS-103", Name = "Digital Logic Design", DepartmentId = 1, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "MATH-102", Name = "Linear Algebra", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-102", Name = "Communication Skills", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "PAK-101", Name = "Pakistan Studies", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "BSCS-104", Name = "Discrete Structures", DepartmentId = 1, CreditHours = 3 }
                },
                [3] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSCS-201", Name = "Data Structures & Algorithms", DepartmentId = 1, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSCS-202", Name = "Computer Organization & Assembly Language", DepartmentId = 1, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "MATH-202", Name = "Multivariable Calculus", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-201", Name = "Technical & Business Writing", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "STAT-201", Name = "Probability & Statistics", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-203", Name = "Software Engineering Principles", DepartmentId = 1, CreditHours = 3 }
                },
                [4] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSCS-204", Name = "Operating Systems", DepartmentId = 1, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSCS-205", Name = "Database Systems", DepartmentId = 1, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSCS-206", Name = "Design & Analysis of Algorithms", DepartmentId = 1, CreditHours = 3 },
                    new CourseSpec { Code = "MATH-201", Name = "Differential Equations", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-207", Name = "Computer Networks", DepartmentId = 1, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "MGT-101", Name = "Principles of Management", DepartmentId = 5, CreditHours = 3 }
                },
                [5] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSCS-301", Name = "Theory of Automata", DepartmentId = 1, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-302", Name = "Artificial Intelligence", DepartmentId = 1, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSCS-303", Name = "Web Technologies", DepartmentId = 1, HasPractical = true, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-304", Name = "Software Architecture & Design", DepartmentId = 1, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-305", Name = "Numerical Computing", DepartmentId = 1, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-306", Name = "Information Security", DepartmentId = 1, CreditHours = 3 }
                },
                [6] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSCS-307", Name = "Compiler Construction", DepartmentId = 1, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-308", Name = "Mobile Application Development", DepartmentId = 1, HasPractical = true, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-309", Name = "Data Science Fundamentals", DepartmentId = 1, HasPractical = true, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-310", Name = "Cloud Computing", DepartmentId = 1, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-311", Name = "Software Quality Assurance", DepartmentId = 1, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-312", Name = "Distributed Systems", DepartmentId = 1, CreditHours = 3 }
                },
                [7] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSCS-401", Name = "Final Year Project I", DepartmentId = 1, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-402", Name = "Machine Learning", DepartmentId = 1, HasPractical = true, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-403", Name = "Internet of Things (IoT)", DepartmentId = 1, HasPractical = true, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-404", Name = "Human Computer Interaction", DepartmentId = 1, CreditHours = 3 },
                    new CourseSpec { Code = "MGT-201", Name = "Entrepreneurship & Innovation", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-405", Name = "Professional Practices in Computing", DepartmentId = 1, CreditHours = 3 }
                },
                [8] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSCS-406", Name = "Final Year Project II", DepartmentId = 1, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-407", Name = "Deep Learning & Neural Networks", DepartmentId = 1, HasPractical = true, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-408", Name = "Cyber Security & Ethical Hacking", DepartmentId = 1, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-409", Name = "Big Data Analytics", DepartmentId = 1, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-410", Name = "Natural Language Processing", DepartmentId = 1, CreditHours = 3 },
                    new CourseSpec { Code = "BSCS-411", Name = "Blockchain Technologies", DepartmentId = 1, CreditHours = 3 }
                }
            };

            // ==========================================
            // 2. BS MATHEMATICS (BSM) - Dept 2
            // ==========================================
            map["BSM"] = new Dictionary<int, List<CourseSpec>>
            {
                [1] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "MATH-101", Name = "Calculus & Analytical Geometry", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "CS-101", Name = "Intro to Information & Communication Tech", DepartmentId = 1, HasPractical = true, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-101", Name = "English Composition & Comprehension", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "ISL-101", Name = "Islamic Studies / Ethics", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "PHY-101", Name = "Mechanics & Thermodynamics", DepartmentId = 3, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSM-101", Name = "Elements of Set Theory & Mathematical Logic", DepartmentId = 2, CreditHours = 3 }
                },
                [2] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "MATH-102", Name = "Linear Algebra", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-102", Name = "Calculus II", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-102", Name = "Communication Skills", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "PAK-101", Name = "Pakistan Studies", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "PHY-102", Name = "Heat & Thermodynamics", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-103", Name = "Discrete Mathematics", DepartmentId = 2, CreditHours = 3 }
                },
                [3] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "MATH-201", Name = "Differential Equations", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "MATH-202", Name = "Multivariable Calculus", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-201", Name = "Technical & Business Writing", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "STAT-201", Name = "Probability & Statistics", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-201", Name = "Number Theory", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-202", Name = "Mathematical Statistics I", DepartmentId = 2, CreditHours = 3 }
                },
                [4] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSM-203", Name = "Real Analysis I", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-204", Name = "Group Theory I", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-205", Name = "Vector & Tensor Analysis", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-206", Name = "Partial Differential Equations", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "MGT-101", Name = "Principles of Management", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-207", Name = "Numerical Methods I", DepartmentId = 2, HasPractical = true, CreditHours = 3 }
                },
                [5] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSM-301", Name = "Real Analysis II", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-302", Name = "Group Theory II", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-303", Name = "Complex Analysis", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-304", Name = "Topology", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-305", Name = "Differential Geometry", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-306", Name = "Classical Mechanics for Mathematicians", DepartmentId = 2, CreditHours = 3 }
                },
                [6] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSM-307", Name = "Functional Analysis", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-308", Name = "Ring and Field Theory", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-309", Name = "Numerical Analysis II", DepartmentId = 2, HasPractical = true, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-310", Name = "Fluid Mechanics I", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-311", Name = "Mathematical Modeling", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-312", Name = "Optimization Techniques", DepartmentId = 2, CreditHours = 3 }
                },
                [7] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSM-401", Name = "Measure Theory", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-402", Name = "Fluid Mechanics II", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-403", Name = "Operations Research", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-404", Name = "Graph Theory", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "MGT-201", Name = "Entrepreneurship & Innovation", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-405", Name = "Mathematics Project I", DepartmentId = 2, CreditHours = 3 }
                },
                [8] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSM-406", Name = "Advanced Functional Analysis", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-407", Name = "Financial Mathematics", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-408", Name = "Cryptography", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-409", Name = "Fractional Calculus", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-410", Name = "Mathematics Project II", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSM-411", Name = "Seminar on Applied Mathematics", DepartmentId = 2, CreditHours = 3 }
                }
            };

            // ==========================================
            // 3. BS PHYSICS (BSPHY) - Dept 3
            // ==========================================
            map["BSPHY"] = new Dictionary<int, List<CourseSpec>>
            {
                [1] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "PHY-101", Name = "Mechanics & Thermodynamics", DepartmentId = 3, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "MATH-101", Name = "Calculus & Analytical Geometry", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "CS-101", Name = "Intro to Information & Communication Tech", DepartmentId = 1, HasPractical = true, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-101", Name = "English Composition & Comprehension", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "ISL-101", Name = "Islamic Studies / Ethics", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "CHEM-101", Name = "General Chemistry I", DepartmentId = 7, HasPractical = true, CreditHours = 4 }
                },
                [2] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSPHY-101", Name = "Electricity & Magnetism I", DepartmentId = 3, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "MATH-102", Name = "Linear Algebra", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-102", Name = "Communication Skills", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "PAK-101", Name = "Pakistan Studies", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "CHEM-102", Name = "General Chemistry II", DepartmentId = 7, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSPHY-103", Name = "Heat and Thermodynamics", DepartmentId = 3, CreditHours = 3 }
                },
                [3] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSPHY-102", Name = "Waves & Oscillations", DepartmentId = 3, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSPHY-203", Name = "Optics & Spectroscopy", DepartmentId = 3, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "MATH-201", Name = "Differential Equations", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-201", Name = "Technical & Business Writing", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-201", Name = "Electricity & Magnetism II", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-205", Name = "Computer Programming for Physicists", DepartmentId = 1, HasPractical = true, CreditHours = 3 }
                },
                [4] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSPHY-202", Name = "Modern Physics", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-204", Name = "Classical Mechanics", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "MATH-202", Name = "Multivariable Calculus", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-206", Name = "Electronics I", DepartmentId = 3, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "STAT-201", Name = "Probability & Statistics", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-207", Name = "Mathematical Methods of Physics I", DepartmentId = 3, CreditHours = 3 }
                },
                [5] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSPHY-301", Name = "Quantum Mechanics I", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-302", Name = "Mathematical Methods of Physics II", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-303", Name = "Electrodynamics I", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-304", Name = "Thermal & Statistical Physics", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-305", Name = "Electronics II", DepartmentId = 3, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSPHY-306", Name = "Atomic & Molecular Physics", DepartmentId = 3, CreditHours = 3 }
                },
                [6] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSPHY-307", Name = "Quantum Mechanics II", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-308", Name = "Electrodynamics II", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-309", Name = "Solid State Physics I", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-310", Name = "Nuclear Physics I", DepartmentId = 3, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSPHY-311", Name = "Computational Physics", DepartmentId = 3, HasPractical = true, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-312", Name = "Advanced Physics Laboratory", DepartmentId = 3, HasPractical = true, CreditHours = 3 }
                },
                [7] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSPHY-401", Name = "Solid State Physics II", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-402", Name = "Nuclear Physics II", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-403", Name = "Laser & Fiber Optics", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-404", Name = "Plasma Physics", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "MGT-201", Name = "Entrepreneurship & Innovation", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-405", Name = "Research Project I", DepartmentId = 3, CreditHours = 3 }
                },
                [8] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSPHY-406", Name = "Particle Physics", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-407", Name = "Relativity & Cosmology", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-408", Name = "Materials Science", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-409", Name = "Environmental Physics", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-410", Name = "Research Project II", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSPHY-411", Name = "Seminar on Contemporary Physics", DepartmentId = 3, CreditHours = 3 }
                }
            };

            // ==========================================
            // 4. BS ENGLISH (BSENG) - Dept 4
            // ==========================================
            map["BSENG"] = new Dictionary<int, List<CourseSpec>>
            {
                [1] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "ENG-101", Name = "English Composition & Comprehension", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "ISL-101", Name = "Islamic Studies / Ethics", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "CS-101", Name = "Intro to Information & Communication Tech", DepartmentId = 1, HasPractical = true, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-101", Name = "Introduction to Literature", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-102", Name = "History of English Literature I", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-103", Name = "English Grammar & Syntax", DepartmentId = 4, CreditHours = 3 }
                },
                [2] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "ENG-102", Name = "Communication Skills", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "PAK-101", Name = "Pakistan Studies", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "BSENG-104", Name = "History of English Literature II", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-105", Name = "Introduction to Linguistics", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-106", Name = "Classical Poetry", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-107", Name = "Phonetics & Phonology", DepartmentId = 4, CreditHours = 3 }
                },
                [3] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "ENG-201", Name = "Technical & Business Writing", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-201", Name = "Classical Drama", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-202", Name = "Morphology & Syntax", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-203", Name = "18th & 19th Century Poetry", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-204", Name = "Romantic Literature", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "MGT-101", Name = "Principles of Management", DepartmentId = 5, CreditHours = 3 }
                },
                [4] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSENG-205", Name = "19th Century Novel", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-206", Name = "Semantics & Pragmatics", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-207", Name = "Literary Criticism", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-208", Name = "Victorian Literature", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-209", Name = "Sociolinguistics", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "STAT-201", Name = "Probability & Statistics", DepartmentId = 2, CreditHours = 3 }
                },
                [5] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSENG-301", Name = "20th Century Poetry", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-302", Name = "Modern Drama", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-303", Name = "Psycholinguistics", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-304", Name = "American Literature", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-305", Name = "Discourse Analysis", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-306", Name = "English for Specific Purposes", DepartmentId = 4, CreditHours = 3 }
                },
                [6] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSENG-307", Name = "20th Century Novel", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-308", Name = "Postcolonial Literature", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-309", Name = "Second Language Acquisition", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-310", Name = "Stylistics", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-311", Name = "Creative Writing", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-312", Name = "Pakistani Literature in English", DepartmentId = 4, CreditHours = 3 }
                },
                [7] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSENG-401", Name = "Modern Literary Theory", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-402", Name = "World Literature in Translation", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-403", Name = "Corpus Linguistics", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-404", Name = "Translation Studies", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "MGT-201", Name = "Entrepreneurship & Innovation", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-405", Name = "English Research Project I", DepartmentId = 4, CreditHours = 3 }
                },
                [8] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSENG-406", Name = "Contemporary Critical Theories", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-407", Name = "Computational Linguistics", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-408", Name = "South Asian Literature", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-409", Name = "Media & Cultural Studies", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-410", Name = "English Research Project II", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "BSENG-411", Name = "Seminar in English Studies", DepartmentId = 4, CreditHours = 3 }
                }
            };

            // ==========================================
            // 5. BBA - Dept 5
            // ==========================================
            map["BBA"] = new Dictionary<int, List<CourseSpec>>
            {
                [1] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "MGT-101", Name = "Principles of Management", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-101", Name = "Financial Accounting I", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "CS-101", Name = "Intro to Information & Communication Tech", DepartmentId = 1, HasPractical = true, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-101", Name = "English Composition & Comprehension", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "ISL-101", Name = "Islamic Studies / Ethics", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "MATH-103", Name = "Business Mathematics", DepartmentId = 2, CreditHours = 3 }
                },
                [2] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BBA-102", Name = "Principles of Marketing", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-103", Name = "Financial Accounting II", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-102", Name = "Communication Skills", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "PAK-101", Name = "Pakistan Studies", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "ECON-101", Name = "Principles of Microeconomics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-104", Name = "Business Law", DepartmentId = 5, CreditHours = 3 }
                },
                [3] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BBA-201", Name = "Cost Accounting", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-202", Name = "Human Resource Management", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-201", Name = "Technical & Business Writing", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "ECON-102", Name = "Principles of Macroeconomics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "STAT-201", Name = "Probability & Statistics", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-203", Name = "Organizational Behavior", DepartmentId = 5, CreditHours = 3 }
                },
                [4] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BBA-204", Name = "Financial Management", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-205", Name = "Marketing Management", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-206", Name = "Management Information Systems", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-207", Name = "Business Ethics & CSR", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-208", Name = "Operations Management", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-209", Name = "Consumer Behavior", DepartmentId = 5, CreditHours = 3 }
                },
                [5] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BBA-301", Name = "Corporate Finance", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-302", Name = "Strategic Management", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-303", Name = "Business Research Methods", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-304", Name = "Supply Chain Management", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-305", Name = "Services Marketing", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-306", Name = "Money & Banking", DepartmentId = 5, CreditHours = 3 }
                },
                [6] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BBA-307", Name = "International Business", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-308", Name = "Project Management", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-309", Name = "Total Quality Management", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-310", Name = "Digital Marketing", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-311", Name = "Investment & Portfolio Management", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-312", Name = "Brand Management", DepartmentId = 5, CreditHours = 3 }
                },
                [7] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "MGT-201", Name = "Entrepreneurship & Innovation", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-401", Name = "Strategic Marketing", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-402", Name = "International Finance", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-403", Name = "Leadership & Team Building", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-404", Name = "Business Analytics", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-405", Name = "BBA Project I", DepartmentId = 5, CreditHours = 3 }
                },
                [8] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BBA-406", Name = "Corporate Governance", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-407", Name = "Risk Management & Insurance", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-408", Name = "Change Management", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-409", Name = "E-Commerce & Online Business", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-410", Name = "BBA Project II", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-411", Name = "Seminar in Business Policy", DepartmentId = 5, CreditHours = 3 }
                }
            };

            // ==========================================
            // 6. BS ELECTRICAL ENGINEERING (BSEE) - Dept 6
            // ==========================================
            map["BSEE"] = new Dictionary<int, List<CourseSpec>>
            {
                [1] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "MATH-101", Name = "Calculus & Analytical Geometry", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-101", Name = "English Composition & Comprehension", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "ISL-101", Name = "Islamic Studies / Ethics", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "PHY-101", Name = "Mechanics & Thermodynamics", DepartmentId = 3, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "CS-101", Name = "Intro to Information & Communication Tech", DepartmentId = 1, HasPractical = true, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-101", Name = "Linear Circuit Analysis", DepartmentId = 6, HasPractical = true, CreditHours = 4 }
                },
                [2] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "MATH-102", Name = "Linear Algebra", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-102", Name = "Communication Skills", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "PAK-101", Name = "Pakistan Studies", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "BSEE-102", Name = "Electronic Devices & Circuits", DepartmentId = 6, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSCS-101", Name = "Programming Fundamentals", DepartmentId = 1, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSEE-103", Name = "Workshop Practice", DepartmentId = 6, HasPractical = true, CreditHours = 2 }
                },
                [3] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "MATH-201", Name = "Differential Equations", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-201", Name = "Electrical Network Analysis", DepartmentId = 6, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSEE-202", Name = "Digital Logic Design for Engineers", DepartmentId = 6, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "ENG-201", Name = "Technical & Business Writing", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "STAT-201", Name = "Probability & Statistics", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-203", Name = "Instrumentation & Measurements", DepartmentId = 6, HasPractical = true, CreditHours = 3 }
                },
                [4] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "MATH-202", Name = "Multivariable Calculus", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-204", Name = "Signals & Systems", DepartmentId = 6, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-205", Name = "Electrical Machines", DepartmentId = 6, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSEE-206", Name = "Electromagnetic Field Theory", DepartmentId = 6, CreditHours = 3 },
                    new CourseSpec { Code = "MGT-101", Name = "Principles of Management", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-207", Name = "Microprocessor & Interfacing", DepartmentId = 6, HasPractical = true, CreditHours = 4 }
                },
                [5] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSEE-301", Name = "Control Systems", DepartmentId = 6, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSEE-302", Name = "Power Distribution & Utilization", DepartmentId = 6, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-303", Name = "Communication Systems", DepartmentId = 6, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSEE-304", Name = "Power Electronics", DepartmentId = 6, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSEE-305", Name = "Numerical Analysis for Engineers", DepartmentId = 6, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-306", Name = "Electronic Circuit Design", DepartmentId = 6, CreditHours = 3 }
                },
                [6] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSEE-307", Name = "Power System Analysis", DepartmentId = 6, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-308", Name = "Digital Signal Processing", DepartmentId = 6, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSEE-309", Name = "Power Generation & Transmission", DepartmentId = 6, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-310", Name = "High Voltage Engineering", DepartmentId = 6, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-311", Name = "Embedded Systems Design", DepartmentId = 6, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSEE-312", Name = "RF & Microwave Engineering", DepartmentId = 6, CreditHours = 3 }
                },
                [7] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSEE-401", Name = "Senior Design Project I", DepartmentId = 6, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-402", Name = "Power System Protection", DepartmentId = 6, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSEE-403", Name = "Renewable Energy Systems", DepartmentId = 6, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-404", Name = "Industrial Automation", DepartmentId = 6, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "MGT-201", Name = "Entrepreneurship & Innovation", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-405", Name = "Engineering Economics & Management", DepartmentId = 6, CreditHours = 3 }
                },
                [8] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSEE-406", Name = "Senior Design Project II", DepartmentId = 6, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-407", Name = "Smart Grid Technologies", DepartmentId = 6, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-408", Name = "Wireless Communications", DepartmentId = 6, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-409", Name = "Power System Operation & Control", DepartmentId = 6, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-410", Name = "Biomedical Electronics", DepartmentId = 6, CreditHours = 3 },
                    new CourseSpec { Code = "BSEE-411", Name = "Engineering Ethics & Safety", DepartmentId = 6, CreditHours = 3 }
                }
            };

            // ==========================================
            // 7. BS CHEMISTRY (BSCHEM) - Dept 7
            // ==========================================
            map["BSCHEM"] = new Dictionary<int, List<CourseSpec>>
            {
                [1] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "CHEM-101", Name = "General Chemistry I", DepartmentId = 7, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "MATH-101", Name = "Calculus & Analytical Geometry", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-101", Name = "English Composition & Comprehension", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "ISL-101", Name = "Islamic Studies / Ethics", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "PHY-101", Name = "Mechanics & Thermodynamics", DepartmentId = 3, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "CS-101", Name = "Intro to Information & Communication Tech", DepartmentId = 1, HasPractical = true, CreditHours = 3 }
                },
                [2] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "CHEM-102", Name = "General Chemistry II", DepartmentId = 7, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "MATH-102", Name = "Linear Algebra", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-102", Name = "Communication Skills", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "PAK-101", Name = "Pakistan Studies", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "PHY-102", Name = "Heat & Thermodynamics", DepartmentId = 3, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-103", Name = "Environmental Chemistry", DepartmentId = 7, CreditHours = 3 }
                },
                [3] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSCHEM-201", Name = "Inorganic Chemistry I", DepartmentId = 7, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSCHEM-202", Name = "Organic Chemistry I", DepartmentId = 7, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "ENG-201", Name = "Technical & Business Writing", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "STAT-201", Name = "Probability & Statistics", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "MATH-201", Name = "Differential Equations", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-203", Name = "Analytical Chemistry I", DepartmentId = 7, HasPractical = true, CreditHours = 4 }
                },
                [4] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSCHEM-204", Name = "Physical Chemistry I", DepartmentId = 7, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSCHEM-205", Name = "Inorganic Chemistry II", DepartmentId = 7, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSCHEM-206", Name = "Organic Chemistry II", DepartmentId = 7, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSCHEM-207", Name = "Analytical Chemistry II", DepartmentId = 7, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "MGT-101", Name = "Principles of Management", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-208", Name = "Chemical Separation Techniques", DepartmentId = 7, CreditHours = 3 }
                },
                [5] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSCHEM-301", Name = "Physical Chemistry II", DepartmentId = 7, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSCHEM-302", Name = "Quantum Chemistry", DepartmentId = 7, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-303", Name = "Organometallic Chemistry", DepartmentId = 7, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-304", Name = "Heterocyclic Chemistry", DepartmentId = 7, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-305", Name = "Spectroscopy & Structure Determination", DepartmentId = 7, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-306", Name = "Biochemistry I", DepartmentId = 7, HasPractical = true, CreditHours = 4 }
                },
                [6] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSCHEM-307", Name = "Polymer Chemistry", DepartmentId = 7, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-308", Name = "Coordination Chemistry", DepartmentId = 7, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-309", Name = "Reaction Kinetics & Catalysis", DepartmentId = 7, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-310", Name = "Industrial Chemistry", DepartmentId = 7, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSCHEM-311", Name = "Electrochemistry", DepartmentId = 7, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-312", Name = "Advanced Laboratory Techniques", DepartmentId = 7, HasPractical = true, CreditHours = 3 }
                },
                [7] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSCHEM-401", Name = "Nuclear & Radiochemistry", DepartmentId = 7, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-402", Name = "Nanochemistry", DepartmentId = 7, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-403", Name = "Medicinal Chemistry", DepartmentId = 7, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-404", Name = "Chemical Toxicology", DepartmentId = 7, CreditHours = 3 },
                    new CourseSpec { Code = "MGT-201", Name = "Entrepreneurship & Innovation", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-405", Name = "Chemistry Project I", DepartmentId = 7, CreditHours = 3 }
                },
                [8] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSCHEM-406", Name = "Natural Products Chemistry", DepartmentId = 7, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-407", Name = "Computational Chemistry", DepartmentId = 7, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-408", Name = "Green & Sustainable Chemistry", DepartmentId = 7, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-409", Name = "Surface Chemistry & Colloids", DepartmentId = 7, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-410", Name = "Chemistry Project II", DepartmentId = 7, CreditHours = 3 },
                    new CourseSpec { Code = "BSCHEM-411", Name = "Seminar in Chemical Sciences", DepartmentId = 7, CreditHours = 3 }
                }
            };

            // ==========================================
            // 8. BS BIOLOGY (BSBIO) - Dept 8
            // ==========================================
            map["BSBIO"] = new Dictionary<int, List<CourseSpec>>
            {
                [1] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BIO-101", Name = "General Biology", DepartmentId = 8, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "CHEM-101", Name = "General Chemistry I", DepartmentId = 7, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "ENG-101", Name = "English Composition & Comprehension", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "ISL-101", Name = "Islamic Studies / Ethics", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "CS-101", Name = "Intro to Information & Communication Tech", DepartmentId = 1, HasPractical = true, CreditHours = 3 },
                    new CourseSpec { Code = "MATH-101", Name = "Calculus & Analytical Geometry", DepartmentId = 2, CreditHours = 3 }
                },
                [2] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSBIO-102", Name = "Cell Biology & Genetics", DepartmentId = 8, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "CHEM-102", Name = "General Chemistry II", DepartmentId = 7, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "ENG-102", Name = "Communication Skills", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "PAK-101", Name = "Pakistan Studies", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "BSBIO-103", Name = "Diversity of Plants", DepartmentId = 8, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSBIO-104", Name = "Diversity of Animals", DepartmentId = 8, HasPractical = true, CreditHours = 4 }
                },
                [3] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSBIO-201", Name = "Microbiology", DepartmentId = 8, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSBIO-202", Name = "Plant Physiology & Ecology", DepartmentId = 8, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "ENG-201", Name = "Technical & Business Writing", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "STAT-201", Name = "Probability & Statistics", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSBIO-203", Name = "Animal Physiology", DepartmentId = 8, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "MGT-101", Name = "Principles of Management", DepartmentId = 5, CreditHours = 3 }
                },
                [4] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSBIO-204", Name = "Molecular Biology", DepartmentId = 8, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSBIO-205", Name = "Biochemistry for Biologists", DepartmentId = 8, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSBIO-206", Name = "Evolution & Systematics", DepartmentId = 8, CreditHours = 3 },
                    new CourseSpec { Code = "BSBIO-207", Name = "Genetics & Genomics", DepartmentId = 8, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSBIO-208", Name = "Ecology & Conservation Biology", DepartmentId = 8, CreditHours = 3 },
                    new CourseSpec { Code = "BSBIO-209", Name = "Developmental Biology", DepartmentId = 8, CreditHours = 3 }
                },
                [5] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSBIO-301", Name = "Animal & Plant Physiology", DepartmentId = 8, CreditHours = 3 },
                    new CourseSpec { Code = "BSBIO-302", Name = "Biotechnology Principles", DepartmentId = 8, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSBIO-303", Name = "Immunology", DepartmentId = 8, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSBIO-304", Name = "Entomology", DepartmentId = 8, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSBIO-305", Name = "Bioinformatics Tools", DepartmentId = 8, HasPractical = true, CreditHours = 3 },
                    new CourseSpec { Code = "BSBIO-306", Name = "Parasitology", DepartmentId = 8, HasPractical = true, CreditHours = 4 }
                },
                [6] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSBIO-307", Name = "Recombinant DNA Technology", DepartmentId = 8, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSBIO-308", Name = "Environmental Biology", DepartmentId = 8, CreditHours = 3 },
                    new CourseSpec { Code = "BSBIO-309", Name = "Pharmacology Basics", DepartmentId = 8, CreditHours = 3 },
                    new CourseSpec { Code = "BSBIO-310", Name = "Virology & Bacteriology", DepartmentId = 8, CreditHours = 3 },
                    new CourseSpec { Code = "BSBIO-311", Name = "Plant Tissue Culture", DepartmentId = 8, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSBIO-312", Name = "Wildlife Management & Conservation", DepartmentId = 8, CreditHours = 3 }
                },
                [7] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSBIO-401", Name = "Medical Microbiology", DepartmentId = 8, CreditHours = 3 },
                    new CourseSpec { Code = "BSBIO-402", Name = "Agricultural Biotechnology", DepartmentId = 8, CreditHours = 3 },
                    new CourseSpec { Code = "BSBIO-403", Name = "Molecular Genetics", DepartmentId = 8, CreditHours = 3 },
                    new CourseSpec { Code = "BSBIO-404", Name = "Bioethics & Biosafety", DepartmentId = 8, CreditHours = 3 },
                    new CourseSpec { Code = "MGT-201", Name = "Entrepreneurship & Innovation", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BSBIO-405", Name = "Biology Research Project I", DepartmentId = 8, CreditHours = 3 }
                },
                [8] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSBIO-406", Name = "Forensic Biology", DepartmentId = 8, CreditHours = 3 },
                    new CourseSpec { Code = "BSBIO-407", Name = "Stem Cell Biology", DepartmentId = 8, CreditHours = 3 },
                    new CourseSpec { Code = "BSBIO-408", Name = "Industrial Fermentation", DepartmentId = 8, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSBIO-409", Name = "Microbial Genetics", DepartmentId = 8, CreditHours = 3 },
                    new CourseSpec { Code = "BSBIO-410", Name = "Biology Research Project II", DepartmentId = 8, CreditHours = 3 },
                    new CourseSpec { Code = "BSBIO-411", Name = "Seminar in Biological Sciences", DepartmentId = 8, CreditHours = 3 }
                }
            };

            // ==========================================
            // 9. BS ECONOMICS (BSECON) - Dept 9
            // ==========================================
            map["BSECON"] = new Dictionary<int, List<CourseSpec>>
            {
                [1] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "ECON-101", Name = "Principles of Microeconomics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "MATH-101", Name = "Calculus & Analytical Geometry", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-101", Name = "English Composition & Comprehension", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "ISL-101", Name = "Islamic Studies / Ethics", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "CS-101", Name = "Intro to Information & Communication Tech", DepartmentId = 1, HasPractical = true, CreditHours = 3 },
                    new CourseSpec { Code = "MGT-101", Name = "Principles of Management", DepartmentId = 5, CreditHours = 3 }
                },
                [2] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "ECON-102", Name = "Principles of Macroeconomics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "MATH-102", Name = "Linear Algebra", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-102", Name = "Communication Skills", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "PAK-101", Name = "Pakistan Studies", DepartmentId = 4, CreditHours = 2 },
                    new CourseSpec { Code = "BBA-101", Name = "Financial Accounting I", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-103", Name = "Economic History of Pakistan", DepartmentId = 9, CreditHours = 3 }
                },
                [3] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSECON-201", Name = "Intermediate Microeconomics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-202", Name = "Mathematical Economics I", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "ENG-201", Name = "Technical & Business Writing", DepartmentId = 4, CreditHours = 3 },
                    new CourseSpec { Code = "STAT-201", Name = "Probability & Statistics", DepartmentId = 2, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-203", Name = "Development Economics I", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-104", Name = "Business Law", DepartmentId = 5, CreditHours = 3 }
                },
                [4] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSECON-204", Name = "Intermediate Macroeconomics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-205", Name = "Mathematical Economics II", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-206", Name = "Introductory Econometrics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-207", Name = "Development Economics II", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-208", Name = "Agricultural Economics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BBA-204", Name = "Financial Management", DepartmentId = 5, CreditHours = 3 }
                },
                [5] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSECON-301", Name = "Advanced Microeconomics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-302", Name = "Applied Econometrics", DepartmentId = 9, HasPractical = true, CreditHours = 4 },
                    new CourseSpec { Code = "BSECON-303", Name = "Money, Banking & Financial Markets", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-304", Name = "Public Finance & Fiscal Policy", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-305", Name = "Labor Economics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-306", Name = "Islamic Economics & Finance", DepartmentId = 9, CreditHours = 3 }
                },
                [6] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSECON-307", Name = "Advanced Macroeconomics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-308", Name = "International Trade & Finance", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-309", Name = "Monetary Economics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-310", Name = "Environmental & Resource Economics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-311", Name = "Institutional Economics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-312", Name = "Game Theory for Economists", DepartmentId = 9, CreditHours = 3 }
                },
                [7] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSECON-401", Name = "Time Series Econometrics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-402", Name = "Microfinance & Poverty Alleviation", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-403", Name = "Urban & Regional Economics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-404", Name = "Behavioral Economics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "MGT-201", Name = "Entrepreneurship & Innovation", DepartmentId = 5, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-405", Name = "Economics Project I", DepartmentId = 9, CreditHours = 3 }
                },
                [8] = new List<CourseSpec>
                {
                    new CourseSpec { Code = "BSECON-406", Name = "Financial Econometrics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-407", Name = "Health Economics", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-408", Name = "Comparative Economic Systems", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-409", Name = "Project Appraisal & Evaluation", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-410", Name = "Economics Project II", DepartmentId = 9, CreditHours = 3 },
                    new CourseSpec { Code = "BSECON-411", Name = "Seminar on Pakistan Economy", DepartmentId = 9, CreditHours = 3 }
                }
            };

            return map;
        }
    }
}


