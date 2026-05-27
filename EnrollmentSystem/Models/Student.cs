namespace EnrollmentSystem.Models
{
    public class Student
    {
        public string StudentID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Course { get; set; }
        public string YearLevel { get; set; }
        public string ContactNumber { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string ToFileString() =>
            $"{StudentID}|{FirstName}|{LastName}|{Gender}|{Course}|{YearLevel}|{ContactNumber}";

        public static Student FromFileString(string line)
        {
            var parts = line.Split('|');
            if (parts.Length < 7) return null;
            return new Student
            {
                StudentID = parts[0].Trim(),
                FirstName = parts[1].Trim(),
                LastName = parts[2].Trim(),
                Gender = parts[3].Trim(),
                Course = parts[4].Trim(),
                YearLevel = parts[5].Trim(),
                ContactNumber = parts[6].Trim()
            };
        }
    }
}
