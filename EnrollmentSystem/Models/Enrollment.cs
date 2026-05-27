using System;

namespace EnrollmentSystem.Models
{
    public class Enrollment
    {
        public string EnrollmentID { get; set; }
        public string StudentID { get; set; }
        public string CourseID { get; set; }
        public string EnrollmentDate { get; set; }
        public string Status { get; set; }

        public string ToFileString() =>
            $"{EnrollmentID}|{StudentID}|{CourseID}|{EnrollmentDate}|{Status}";

        public static Enrollment FromFileString(string line)
        {
            var parts = line.Split('|');
            if (parts.Length < 5) return null;
            return new Enrollment
            {
                EnrollmentID = parts[0].Trim(),
                StudentID = parts[1].Trim(),
                CourseID = parts[2].Trim(),
                EnrollmentDate = parts[3].Trim(),
                Status = parts[4].Trim()
            };
        }
    }
}
