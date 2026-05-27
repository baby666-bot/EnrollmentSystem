using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EnrollmentSystem.Data;
using EnrollmentSystem.Models;

namespace EnrollmentSystem.Forms
{
    public class StudentDashboard : Form
    {
        private TabControl tabs;
        private string studentID => DataManager.CurrentUser.UserID;

        public StudentDashboard()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            var user = DataManager.CurrentUser;
            this.Text = $"Student Portal — {user.FullName}";
            this.Size = new Size(1000, 660);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9f);

            
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top, Height = 72,
                BackColor = Color.FromArgb(41, 128, 185)
            };
            pnlHeader.Controls.Add(new Label
            {
                Text = $"🎓  Student Portal",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(20, 10)
            });
            pnlHeader.Controls.Add(new Label
            {
                Text = $"{user.FullName}   |   ID: {user.UserID}   |   Role: Student",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(195, 225, 255),
                AutoSize = true, Location = new Point(22, 44)
            });

            var btnLogout = new Button
            {
                Text = "🚪 Logout", Size = new Size(95, 32),
                Location = new Point(880, 20),
                BackColor = Color.FromArgb(192, 57, 43), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) =>
            {
                DataManager.CurrentUser = null;
                new LoginForm().Show();
                this.Close();
            };
            pnlHeader.Controls.Add(btnLogout);

            
            tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10f),
                Padding = new Point(18, 6)
            };

            
            this.Controls.Add(tabs);
            this.Controls.Add(pnlHeader);
        }

        private void LoadData()
        {
            DataManager.LoadAll();
            tabs.TabPages.Clear();

            tabs.TabPages.Add(BuildProfileTab());
            tabs.TabPages.Add(BuildScheduleTab());
            tabs.TabPages.Add(BuildEnrollmentsTab());
            tabs.TabPages.Add(BuildGradesTab());
        }

        

        private TabPage BuildProfileTab()
        {
            var tab = new TabPage("👤  My Profile") { BackColor = Color.White };
            var info = DataManager.GetStudent(studentID);

            var card = new Panel
            {
                Size = new Size(520, 340),
                Location = new Point(50, 30),
                BackColor = Color.White
            };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(220, 228, 235));
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                using var b = new SolidBrush(Color.FromArgb(41, 128, 185));
                e.Graphics.FillRectangle(b, 0, 0, 6, card.Height);
            };

            var titleBar = new Panel { Location = new Point(0, 0), Size = new Size(520, 46), BackColor = Color.FromArgb(41, 128, 185) };
            titleBar.Controls.Add(new Label
            {
                Text = "🎓  Student Profile",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(14, 12)
            });
            card.Controls.Add(titleBar);

            int y = 58;
            void Field(string label, string value)
            {
                card.Controls.Add(new Label
                {
                    Text = label,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(120, 130, 150),
                    AutoSize = true, Location = new Point(20, y)
                });
                card.Controls.Add(new Label
                {
                    Text = value ?? "—",
                    Font = new Font("Segoe UI", 10.5f),
                    ForeColor = Color.FromArgb(30, 50, 80),
                    AutoSize = true, Location = new Point(210, y)
                });
                y += 40;
            }

            if (info != null)
            {
                Field("Student ID",      info.StudentID);
                Field("Full Name",       info.FullName);
                Field("Gender",          info.Gender);
                Field("Course",          info.Course);
                Field("Year Level",      info.YearLevel);
                Field("Contact Number",  info.ContactNumber);
            }
            else
            {
                card.Controls.Add(new Label
                {
                    Text = "Student profile not found. Contact the administrator.",
                    ForeColor = Color.Gray, AutoSize = true, Location = new Point(20, 60)
                });
            }

            tab.Controls.Add(card);
            return tab;
        }

        

        private TabPage BuildScheduleTab()
        {
            var tab = new TabPage("🗓️  My Schedule") { BackColor = Color.White };

            var pnlInfo = new Panel
            {
                Dock = DockStyle.Top, Height = 46,
                BackColor = Color.FromArgb(232, 241, 252)
            };
            pnlInfo.Controls.Add(new Label
            {
                Text = "ℹ  Schedules are based on your active enrollments (Status = Enrolled).",
                AutoSize = true, Location = new Point(14, 14),
                ForeColor = Color.FromArgb(30, 90, 160),
                Font = new Font("Segoe UI", 9f)
            });
            tab.Controls.Add(pnlInfo);

            var dgv = MakeGrid(
                new[] { "Sched ID", "Course", "Teacher", "Section", "Day", "Time", "Room", "Semester" },
                Color.FromArgb(41, 128, 185));

            var schedules = DataManager.GetSchedulesForStudent(studentID);
            foreach (var s in schedules)
            {
                var course  = DataManager.GetCourse(s.CourseID);
                var teacher = DataManager.GetTeacher(s.TeacherID);
                dgv.Rows.Add(
                    s.ScheduleID,
                    course?.CourseName  ?? s.CourseID,
                    teacher?.FullName   ?? s.TeacherID,
                    s.Section,
                    s.Day,
                    s.TimeDisplay,
                    s.Room,
                    s.Semester
                );
            }

            if (schedules.Count == 0)
            {
                var lbl = new Label
                {
                    Text = "No schedule found. You may not be enrolled in any courses yet,\nor your teacher has not been assigned a schedule.",
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Location = new Point(20, 60)
                };
                tab.Controls.Add(lbl);
            }

            tab.Controls.Add(dgv);
            return tab;
        }

       

        private TabPage BuildEnrollmentsTab()
        {
            var tab = new TabPage("📋  My Enrollments") { BackColor = Color.White };

            var dgv = MakeGrid(
                new[] { "Enrollment ID", "Course", "Date Enrolled", "Status" },
                Color.FromArgb(142, 68, 173));

            var myEnrollments = DataManager.Enrollments
                .Where(e => e.StudentID == studentID)
                .OrderByDescending(e => e.EnrollmentDate)
                .ToList();

            foreach (var e in myEnrollments)
            {
                var course = DataManager.GetCourse(e.CourseID);
                int row = dgv.Rows.Add(
                    e.EnrollmentID,
                    course?.CourseName ?? e.CourseID,
                    e.EnrollmentDate,
                    e.Status);

                var cell = dgv.Rows[row].Cells[3];
                cell.Style.ForeColor = e.Status switch
                {
                    "Enrolled"  => Color.FromArgb(39, 174, 96),
                    "Dropped"   => Color.FromArgb(192, 57, 43),
                    "Completed" => Color.FromArgb(41, 128, 185),
                    "Pending"   => Color.FromArgb(230, 126, 34),
                    _           => Color.Gray
                };
                cell.Style.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            }

            tab.Controls.Add(dgv);
            return tab;
        }

        

        private TabPage BuildGradesTab()
        {
            var tab = new TabPage("📊  My Grades") { BackColor = Color.White };

            var dgv = MakeGrid(
                new[] { "Grade ID", "Course", "Grade", "Remarks", "Teacher", "Date" },
                Color.FromArgb(23, 115, 115));

            var myGrades = DataManager.GetGradesForStudent(studentID)
                .OrderByDescending(g => g.DateRecorded)
                .ToList();

            foreach (var g in myGrades)
            {
                var course  = DataManager.GetCourse(g.CourseID);
                var teacher = DataManager.GetTeacher(g.TeacherID);

                int row = dgv.Rows.Add(
                    g.GradeID,
                    course?.CourseName ?? g.CourseID,
                    g.SubjectGrade,
                    g.Remarks,
                    teacher?.FullName ?? g.TeacherID,
                    g.DateRecorded);

                
                var cell = dgv.Rows[row].Cells[3];
                cell.Style.ForeColor = g.Remarks switch
                {
                    "Passed"     => Color.FromArgb(39, 174, 96),
                    "Failed"     => Color.FromArgb(192, 57, 43),
                    "Incomplete" => Color.FromArgb(230, 126, 34),
                    "Dropped"    => Color.FromArgb(149, 165, 166),
                    _            => Color.FromArgb(41, 128, 185)
                };
                cell.Style.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            }

            
            if (myGrades.Any())
            {
                var pnlSummary = new Panel
                {
                    Dock = DockStyle.Bottom, Height = 38,
                    BackColor = Color.FromArgb(232, 248, 248)
                };
                double avg = myGrades
                    .Where(g => double.TryParse(g.SubjectGrade, out _))
                    .Select(g => double.Parse(g.SubjectGrade))
                    .DefaultIfEmpty(0)
                    .Average();

                int passed = myGrades.Count(g => g.Remarks == "Passed");
                int failed = myGrades.Count(g => g.Remarks == "Failed");

                pnlSummary.Controls.Add(new Label
                {
                    Text = $"📊  Total Grades: {myGrades.Count}   |   " +
                           $"Passed: {passed}   |   Failed: {failed}   |   " +
                           $"Average: {(avg > 0 ? avg.ToString("F2") : "N/A")}",
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(23, 100, 100),
                    AutoSize = true, Location = new Point(14, 10)
                });
                tab.Controls.Add(pnlSummary);
            }

            tab.Controls.Add(dgv);
            return tab;
        }

        

        private DataGridView MakeGrid(string[] headers, Color headerColor)
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None, RowHeadersVisible = false,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9f), GridColor = Color.FromArgb(228, 232, 240),
                ColumnHeadersHeight = 34, RowTemplate = { Height = 30 }
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = headerColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 252, 254);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 230, 250);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(20, 50, 100);
            foreach (var h in headers)
                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = h,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                });
            return dgv;
        }
    }
}
