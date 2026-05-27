namespace EnrollmentSystem.Models
{
    public class User
    {
        public string UserID { get; set; }       // StudentID, TeacherID, or "ADMIN"
        public string Password { get; set; }
        public string Role { get; set; }         // "Student", "Teacher", "Admin"
        public string FullName { get; set; }

        public string ToFileString() =>
            $"{UserID}|{Password}|{Role}|{FullName}";

        public static User FromFileString(string line)
        {
            var p = line.Split('|');
            if (p.Length < 4) return null;
            return new User
            {
                UserID   = p[0].Trim(),
                Password = p[1].Trim(),
                Role     = p[2].Trim(),
                FullName = p[3].Trim()
            };
        }
    }
}
