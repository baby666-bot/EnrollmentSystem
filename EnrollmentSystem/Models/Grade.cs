namespace EnrollmentSystem.Models
{
    public class Grade
    {
        public string GradeID { get; set; }
        public string EnrollmentID { get; set; }
        public string StudentID { get; set; }
        public string CourseID { get; set; }
        public string TeacherID { get; set; }
        public string SubjectGrade { get; set; }   // e.g. "1.25", "INC", "DROPPED"
        public string Remarks { get; set; }         // "Passed", "Failed", "Incomplete"
        public string DateRecorded { get; set; }

        public string ToFileString() =>
            $"{GradeID}|{EnrollmentID}|{StudentID}|{CourseID}|{TeacherID}|{SubjectGrade}|{Remarks}|{DateRecorded}";

        public static Grade FromFileString(string line)
        {
            var p = line.Split('|');
            if (p.Length < 8) return null;
            return new Grade
            {
                GradeID      = p[0].Trim(),
                EnrollmentID = p[1].Trim(),
                StudentID    = p[2].Trim(),
                CourseID     = p[3].Trim(),
                TeacherID    = p[4].Trim(),
                SubjectGrade = p[5].Trim(),
                Remarks      = p[6].Trim(),
                DateRecorded = p[7].Trim()
            };
        }
    }
}
