namespace EnrollmentSystem.Models
{
    public class Teacher
    {
        public string TeacherID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Department { get; set; }
        public string ContactNumber { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string ToFileString() =>
            $"{TeacherID}|{FirstName}|{LastName}|{Department}|{ContactNumber}";

        public static Teacher FromFileString(string line)
        {
            var p = line.Split('|');
            if (p.Length < 5) return null;
            return new Teacher
            {
                TeacherID     = p[0].Trim(),
                FirstName     = p[1].Trim(),
                LastName      = p[2].Trim(),
                Department    = p[3].Trim(),
                ContactNumber = p[4].Trim()
            };
        }

        public override string ToString() => $"{TeacherID} – {FullName}";
    }
}
