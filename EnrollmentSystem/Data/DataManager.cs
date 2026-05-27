using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnrollmentSystem.Models;

namespace EnrollmentSystem.Data
{
    public static class DataManager
    {
        private static readonly string StudentsFile    = "students.txt";
        private static readonly string CoursesFile     = "courses.txt";
        private static readonly string EnrollmentsFile = "enrollments.txt";
        private static readonly string UsersFile       = "users.txt";
        private static readonly string GradesFile      = "grades.txt";
        private static readonly string TeachersFile    = "teachers.txt";
        private static readonly string SchedulesFile   = "schedules.txt";

        public static List<Student>    Students    { get; private set; } = new List<Student>();
        public static List<Course>     Courses     { get; private set; } = new List<Course>();
        public static List<Enrollment> Enrollments { get; private set; } = new List<Enrollment>();
        public static List<User>       Users       { get; private set; } = new List<User>();
        public static List<Grade>      Grades      { get; private set; } = new List<Grade>();
        public static List<Teacher>    Teachers    { get; private set; } = new List<Teacher>();
        public static List<Schedule>   Schedules   { get; private set; } = new List<Schedule>();

        public static User CurrentUser { get; set; }

        

        public static void Initialize()
        {
            foreach (var f in new[] { StudentsFile, CoursesFile, EnrollmentsFile, UsersFile, GradesFile, TeachersFile, SchedulesFile })
                if (!File.Exists(f)) File.WriteAllText(f, string.Empty);
            LoadAll();
            SeedDefaultAdmin();
        }

        private static void SeedDefaultAdmin()
        {
            if (!Users.Any(u => u.Role == "Admin"))
            {
                Users.Add(new User { UserID = "ADMIN", Password = "admin123", Role = "Admin", FullName = "System Administrator" });
                SaveUsers();
            }
        }

        public static void LoadAll()
        {
            Students = File.ReadAllLines(StudentsFile)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(Student.FromFileString).Where(s => s != null).ToList();

            Courses = File.ReadAllLines(CoursesFile)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(Course.FromFileString).Where(c => c != null).ToList();

            Enrollments = File.ReadAllLines(EnrollmentsFile)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(Enrollment.FromFileString).Where(e => e != null).ToList();

            Users = File.ReadAllLines(UsersFile)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(User.FromFileString).Where(u => u != null).ToList();

            Grades = File.ReadAllLines(GradesFile)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(Grade.FromFileString).Where(g => g != null).ToList();

            Teachers = File.ReadAllLines(TeachersFile)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(Teacher.FromFileString).Where(t => t != null).ToList();

            Schedules = File.ReadAllLines(SchedulesFile)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(Schedule.FromFileString).Where(s => s != null).ToList();
        }

        

        public static void SaveStudents()    => File.WriteAllLines(StudentsFile,    Students.Select(s => s.ToFileString()));
        public static void SaveCourses()     => File.WriteAllLines(CoursesFile,     Courses.Select(c => c.ToFileString()));
        public static void SaveEnrollments() => File.WriteAllLines(EnrollmentsFile, Enrollments.Select(e => e.ToFileString()));
        public static void SaveUsers()       => File.WriteAllLines(UsersFile,       Users.Select(u => u.ToFileString()));
        public static void SaveGrades()      => File.WriteAllLines(GradesFile,      Grades.Select(g => g.ToFileString()));
        public static void SaveTeachers()    => File.WriteAllLines(TeachersFile,    Teachers.Select(t => t.ToFileString()));
        public static void SaveSchedules()   => File.WriteAllLines(SchedulesFile,   Schedules.Select(s => s.ToFileString()));

        

        public static User Login(string userID, string password)
        {
            return Users.FirstOrDefault(u =>
                u.UserID.Equals(userID.Trim(), StringComparison.OrdinalIgnoreCase) &&
                u.Password == password.Trim());
        }

        

        public static bool AddStudent(Student student)
        {
            if (Students.Any(s => s.StudentID == student.StudentID)) return false;
            Students.Add(student); SaveStudents();
            if (!Users.Any(u => u.UserID == student.StudentID))
            {
                Users.Add(new User { UserID = student.StudentID, Password = student.StudentID, Role = "Student", FullName = student.FullName });
                SaveUsers();
            }
            return true;
        }

        public static bool UpdateStudent(Student updated)
        {
            var idx = Students.FindIndex(s => s.StudentID == updated.StudentID);
            if (idx < 0) return false;
            Students[idx] = updated; SaveStudents();
            var user = Users.FirstOrDefault(u => u.UserID == updated.StudentID);
            if (user != null) { user.FullName = updated.FullName; SaveUsers(); }
            return true;
        }

        public static bool DeleteStudent(string studentID)
        {
            int r = Students.RemoveAll(s => s.StudentID == studentID);
            if (r == 0) return false;
            Users.RemoveAll(u => u.UserID == studentID && u.Role == "Student");
            SaveStudents(); SaveUsers(); return true;
        }

        public static List<Student> SearchStudents(string text) =>
            Students.Where(s =>
                s.StudentID.Contains(text) ||
                s.FirstName.ToLower().Contains(text.ToLower()) ||
                s.LastName.ToLower().Contains(text.ToLower()) ||
                s.Course.ToLower().Contains(text.ToLower()))
            .OrderBy(s => s.LastName).ToList();

        public static List<Student> FilterStudents(string course = null, string yearLevel = null)
        {
            var q = Students.AsEnumerable();
            if (!string.IsNullOrEmpty(course))    q = q.Where(s => s.Course == course);
            if (!string.IsNullOrEmpty(yearLevel)) q = q.Where(s => s.YearLevel == yearLevel);
            return q.OrderBy(s => s.LastName).ToList();
        }

        

        public static bool AddCourse(Course course)
        {
            if (Courses.Any(c => c.CourseID == course.CourseID)) return false;
            Courses.Add(course); SaveCourses(); return true;
        }

        public static bool UpdateCourse(Course updated)
        {
            var idx = Courses.FindIndex(c => c.CourseID == updated.CourseID);
            if (idx < 0) return false;
            Courses[idx] = updated; SaveCourses(); return true;
        }

        public static bool DeleteCourse(string courseID)
        {
            if (Enrollments.Any(e => e.CourseID == courseID)) return false;
            int r = Courses.RemoveAll(c => c.CourseID == courseID);
            if (r == 0) return false;
            SaveCourses(); return true;
        }

        

        public static bool AddTeacher(Teacher teacher)
        {
            if (Teachers.Any(t => t.TeacherID == teacher.TeacherID)) return false;
            Teachers.Add(teacher); SaveTeachers();
            if (!Users.Any(u => u.UserID == teacher.TeacherID))
            {
                Users.Add(new User { UserID = teacher.TeacherID, Password = teacher.TeacherID, Role = "Teacher", FullName = teacher.FullName });
                SaveUsers();
            }
            return true;
        }

        public static bool UpdateTeacher(Teacher updated)
        {
            var idx = Teachers.FindIndex(t => t.TeacherID == updated.TeacherID);
            if (idx < 0) return false;
            Teachers[idx] = updated; SaveTeachers();
            var user = Users.FirstOrDefault(u => u.UserID == updated.TeacherID);
            if (user != null) { user.FullName = updated.FullName; SaveUsers(); }
            return true;
        }

        public static bool DeleteTeacher(string teacherID)
        {
            int r = Teachers.RemoveAll(t => t.TeacherID == teacherID);
            if (r == 0) return false;
            Users.RemoveAll(u => u.UserID == teacherID && u.Role == "Teacher");
            SaveTeachers(); SaveUsers(); return true;
        }

        

        public static bool AddEnrollment(Enrollment enrollment)
        {
            if (Enrollments.Any(e => e.EnrollmentID == enrollment.EnrollmentID)) return false;
            Enrollments.Add(enrollment); SaveEnrollments(); return true;
        }

        public static bool UpdateEnrollment(Enrollment updated)
        {
            var idx = Enrollments.FindIndex(e => e.EnrollmentID == updated.EnrollmentID);
            if (idx < 0) return false;
            Enrollments[idx] = updated; SaveEnrollments(); return true;
        }

        public static bool DeleteEnrollment(string enrollmentID)
        {
            int r = Enrollments.RemoveAll(e => e.EnrollmentID == enrollmentID);
            if (r == 0) return false;
            SaveEnrollments(); return true;
        }

        public static List<Enrollment> SearchEnrollments(string text) =>
            Enrollments.Where(e =>
                e.EnrollmentID.Contains(text) ||
                e.StudentID.Contains(text) ||
                e.CourseID.Contains(text) ||
                e.Status.ToLower().Contains(text.ToLower()))
            .ToList();

        

        public static bool AddGrade(Grade grade)
        {
            if (Grades.Any(g => g.GradeID == grade.GradeID)) return false;
            Grades.Add(grade); SaveGrades(); return true;
        }

        public static bool UpdateGrade(Grade updated)
        {
            var idx = Grades.FindIndex(g => g.GradeID == updated.GradeID);
            if (idx < 0) return false;
            Grades[idx] = updated; SaveGrades(); return true;
        }

        public static bool DeleteGrade(string gradeID)
        {
            int r = Grades.RemoveAll(g => g.GradeID == gradeID);
            if (r == 0) return false;
            SaveGrades(); return true;
        }

        public static List<Grade> GetGradesForStudent(string studentID) =>
            Grades.Where(g => g.StudentID == studentID).ToList();

        public static List<Grade> GetGradesForTeacher(string teacherID) =>
            Grades.Where(g => g.TeacherID == teacherID).ToList();

        

        public static bool AddSchedule(Schedule schedule)
        {
            if (Schedules.Any(s => s.ScheduleID == schedule.ScheduleID)) return false;
            Schedules.Add(schedule); SaveSchedules(); return true;
        }

        public static bool UpdateSchedule(Schedule updated)
        {
            var idx = Schedules.FindIndex(s => s.ScheduleID == updated.ScheduleID);
            if (idx < 0) return false;
            Schedules[idx] = updated; SaveSchedules(); return true;
        }

        public static bool DeleteSchedule(string scheduleID)
        {
            int r = Schedules.RemoveAll(s => s.ScheduleID == scheduleID);
            if (r == 0) return false;
            SaveSchedules(); return true;
        }

        public static List<Schedule> GetSchedulesForTeacher(string teacherID) =>
            Schedules.Where(s => s.TeacherID == teacherID)
                     .OrderBy(s => s.Day).ThenBy(s => s.TimeStart).ToList();

        public static List<Schedule> GetSchedulesForStudent(string studentID)
        {
            
            var enrolledCourseIDs = Enrollments
                .Where(e => e.StudentID == studentID && e.Status == "Enrolled")
                .Select(e => e.CourseID)
                .ToList();

            return Schedules
                .Where(s => enrolledCourseIDs.Contains(s.CourseID))
                .OrderBy(s => s.Day).ThenBy(s => s.TimeStart)
                .ToList();
        }

        public static List<Schedule> SearchSchedules(string text) =>
            Schedules.Where(s =>
                s.ScheduleID.Contains(text) ||
                s.CourseID.Contains(text) ||
                s.TeacherID.Contains(text) ||
                s.Room.ToLower().Contains(text.ToLower()) ||
                s.Section.ToLower().Contains(text.ToLower()) ||
                s.Day.ToLower().Contains(text.ToLower()))
            .ToList();

        

        public static string GenerateStudentID()
        {
            int year = DateTime.Now.Year;
            if (!Students.Any()) return $"{year}-001";
            var nums = Students
                .Where(s => s.StudentID.StartsWith(year.ToString()))
                .Select(s => { var p = s.StudentID.Split('-'); return p.Length > 1 && int.TryParse(p[1], out int n) ? n : 0; });
            return $"{year}-{((nums.Any() ? nums.Max() : 0) + 1):D3}";
        }

        public static string GenerateCourseID()
        {
            if (!Courses.Any()) return "C001";
            var nums = Courses.Select(c => c.CourseID.StartsWith("C") && int.TryParse(c.CourseID.Substring(1), out int n) ? n : 0);
            return $"C{((nums.Any() ? nums.Max() : 0) + 1):D3}";
        }

        public static string GenerateEnrollmentID()
        {
            if (!Enrollments.Any()) return "E001";
            var nums = Enrollments.Select(e => e.EnrollmentID.StartsWith("E") && int.TryParse(e.EnrollmentID.Substring(1), out int n) ? n : 0);
            return $"E{((nums.Any() ? nums.Max() : 0) + 1):D3}";
        }

        public static string GenerateTeacherID()
        {
            if (!Teachers.Any()) return "T001";
            var nums = Teachers.Select(t => t.TeacherID.StartsWith("T") && int.TryParse(t.TeacherID.Substring(1), out int n) ? n : 0);
            return $"T{((nums.Any() ? nums.Max() : 0) + 1):D3}";
        }

        public static string GenerateGradeID()
        {
            if (!Grades.Any()) return "G001";
            var nums = Grades.Select(g => g.GradeID.StartsWith("G") && int.TryParse(g.GradeID.Substring(1), out int n) ? n : 0);
            return $"G{((nums.Any() ? nums.Max() : 0) + 1):D3}";
        }

        public static string GenerateScheduleID()
        {
            if (!Schedules.Any()) return "SCH001";
            var nums = Schedules.Select(s =>
                s.ScheduleID.StartsWith("SCH") && int.TryParse(s.ScheduleID.Substring(3), out int n) ? n : 0);
            return $"SCH{((nums.Any() ? nums.Max() : 0) + 1):D3}";
        }

       

        public static Student  GetStudent(string id)  => Students.FirstOrDefault(s => s.StudentID == id);
        public static Course   GetCourse(string id)   => Courses.FirstOrDefault(c => c.CourseID == id);
        public static Teacher  GetTeacher(string id)  => Teachers.FirstOrDefault(t => t.TeacherID == id);
        public static Schedule GetSchedule(string id) => Schedules.FirstOrDefault(s => s.ScheduleID == id);
    }
}
