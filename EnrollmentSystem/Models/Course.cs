namespace EnrollmentSystem.Models
{
    public class Course
    {
        public string CourseID { get; set; }
        public string CourseName { get; set; }
        public string Department { get; set; }

        public string ToFileString() =>
            $"{CourseID}|{CourseName}|{Department}";

        public static Course FromFileString(string line)
        {
            var parts = line.Split('|');
            if (parts.Length < 3) return null;
            return new Course
            {
                CourseID = parts[0].Trim(),
                CourseName = parts[1].Trim(),
                Department = parts[2].Trim()
            };
        }

        public override string ToString() => CourseName;
    }
}
