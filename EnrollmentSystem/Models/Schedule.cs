namespace EnrollmentSystem.Models
{
    public class Schedule
    {
        public string ScheduleID  { get; set; }
        public string CourseID    { get; set; }
        public string TeacherID   { get; set; }
        public string Day         { get; set; }   // e.g. "Monday", "Mon/Wed/Fri"
        public string TimeStart   { get; set; }   // e.g. "07:30"
        public string TimeEnd     { get; set; }   // e.g. "09:00"
        public string Room        { get; set; }
        public string Semester    { get; set; }   // e.g. "1st Sem 2025-2026"
        public string Section     { get; set; }   // e.g. "BSIT-1A"

        public string TimeDisplay  => $"{TimeStart} – {TimeEnd}";
        public string DayTimeRoom  => $"{Day}  {TimeStart}–{TimeEnd}  |  {Room}";

        public string ToFileString() =>
            $"{ScheduleID}|{CourseID}|{TeacherID}|{Day}|{TimeStart}|{TimeEnd}|{Room}|{Semester}|{Section}";

        public static Schedule FromFileString(string line)
        {
            var p = line.Split('|');
            if (p.Length < 9) return null;
            return new Schedule
            {
                ScheduleID = p[0].Trim(),
                CourseID   = p[1].Trim(),
                TeacherID  = p[2].Trim(),
                Day        = p[3].Trim(),
                TimeStart  = p[4].Trim(),
                TimeEnd    = p[5].Trim(),
                Room       = p[6].Trim(),
                Semester   = p[7].Trim(),
                Section    = p[8].Trim()
            };
        }
    }
}
