using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EnrollmentSystem.Data;
using EnrollmentSystem.Models;

namespace EnrollmentSystem.Forms
{
    public class TeacherDashboard : Form
    {
        private TabControl tabs;
        private TabPage tabSchedule, tabStudents, tabGrades, tabProfile;
        private DataGridView dgvSchedule, dgvStudents, dgvGrades;
        private ComboBox cmbScheduleFilter;
        private string teacherID => DataManager.CurrentUser.UserID;

        public TeacherDashboard()
        {
            InitializeComponent();
            LoadAll();
        }

        private void InitializeComponent()
        {
            var teacher = DataManager.CurrentUser;
            this.Text = $"Teacher Portal — {teacher.FullName}";
            this.Size = new Size(1060, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9f);

            
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top, Height = 72,
                BackColor = Color.FromArgb(23, 115, 115)
            };

            pnlHeader.Controls.Add(new Label
            {
                Text = $"👨‍🏫  Teacher Portal",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(20, 10)
            });
            pnlHeader.Controls.Add(new Label
            {
                Text = $"{teacher.FullName}   |   ID: {teacher.UserID}   |   Role: Teacher",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(190, 235, 235),
                AutoSize = true, Location = new Point(22, 44)
            });

            var btnLogout = new Button
            {
                Text = "🚪 Logout", Size = new Size(95, 32),
                Location = new Point(940, 20),
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

            tabSchedule = new TabPage("🗓️  My Schedule")   { BackColor = Color.White };
            tabStudents = new TabPage("👨‍🎓  My Students")  { BackColor = Color.White };
            tabGrades   = new TabPage("📊  Input Grades")   { BackColor = Color.White };
            tabProfile  = new TabPage("👤  My Profile")     { BackColor = Color.White };

            BuildScheduleTab();
            BuildStudentsTab();
            BuildGradesTab();
            BuildProfileTab();

            tabs.TabPages.Add(tabSchedule);
            tabs.TabPages.Add(tabStudents);
            tabs.TabPages.Add(tabGrades);
            tabs.TabPages.Add(tabProfile);

            
            this.Controls.Add(tabs);
            this.Controls.Add(pnlHeader);
        }

        

        private void BuildScheduleTab()
        {
            
            var pnlInfo = new Panel
            {
                Dock = DockStyle.Top, Height = 46,
                BackColor = Color.FromArgb(232, 248, 248)
            };
            pnlInfo.Controls.Add(new Label
            {
                Text = "ℹ  Your assigned class schedules for all semesters are listed below.",
                AutoSize = true, Location = new Point(14, 14),
                ForeColor = Color.FromArgb(23, 100, 100),
                Font = new Font("Segoe UI", 9f)
            });
            tabSchedule.Controls.Add(pnlInfo);

            dgvSchedule = MakeGrid(
                new[] { "Sched ID", "Course", "Section", "Day", "Time", "Room", "Semester" },
                Color.FromArgb(23, 115, 115));

            tabSchedule.Controls.Add(dgvSchedule);
        }

        

        private void BuildStudentsTab()
        {
            var pnlBar = new Panel
            {
                Dock = DockStyle.Top, Height = 48,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            pnlBar.Controls.Add(new Label
            {
                Text = "Filter by Course:", AutoSize = true, Location = new Point(12, 15)
            });
            cmbScheduleFilter = new ComboBox
            {
                Location = new Point(120, 11), Width = 280,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f)
            };
            cmbScheduleFilter.SelectedIndexChanged += (s, e) => LoadStudents();
            pnlBar.Controls.Add(cmbScheduleFilter);

            dgvStudents = MakeGrid(
                new[] { "Student ID", "Full Name", "Gender", "Year Level", "Course", "Enroll Status" },
                Color.FromArgb(41, 128, 185));

            tabStudents.Controls.Add(dgvStudents);
            tabStudents.Controls.Add(pnlBar);
        }

        

        private void BuildGradesTab()
        {
            
            var pnlBar = new Panel
            {
                Dock = DockStyle.Top, Height = 50,
                BackColor = Color.White
            };

            var btnAdd    = MakeBtn("➕ Add Grade",    Color.FromArgb(39, 174, 96),  10, 10, 120);
            var btnEdit   = MakeBtn("✏️ Edit Grade",   Color.FromArgb(23, 115, 115), 140, 10, 110);
            var btnDelete = MakeBtn("🗑️ Delete Grade", Color.FromArgb(192, 57, 43),  260, 10, 115);
            var btnRefresh = MakeBtn("🔄 Refresh",     Color.FromArgb(100, 115, 140),385, 10, 90);

            btnAdd.Click    += BtnAddGrade_Click;
            btnEdit.Click   += BtnEditGrade_Click;
            btnDelete.Click += BtnDeleteGrade_Click;
            btnRefresh.Click += (s, e) => { DataManager.LoadAll(); LoadGrades(); };

            pnlBar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });
            tabGrades.Controls.Add(pnlBar);

            dgvGrades = MakeGrid(
                new[] { "Grade ID", "Student ID", "Student Name", "Course", "Grade", "Remarks", "Date Recorded" },
                Color.FromArgb(39, 174, 96));

            
            dgvGrades.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex != 5 || e.RowIndex < 0) return;
                e.CellStyle.ForeColor = e.Value?.ToString() switch
                {
                    "Passed"     => Color.FromArgb(39, 174, 96),
                    "Failed"     => Color.FromArgb(192, 57, 43),
                    "Incomplete" => Color.FromArgb(230, 126, 34),
                    "Dropped"    => Color.FromArgb(149, 165, 166),
                    _            => Color.Black
                };
                e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            };

            dgvGrades.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEditGrade_Click(s, e); };
            tabGrades.Controls.Add(dgvGrades);
        }

        

        private void BuildProfileTab()
        {
            var teacher = DataManager.GetTeacher(teacherID);
            var pnl = new Panel { Dock = DockStyle.Fill };

            
            var card = new Panel
            {
                Size = new Size(500, 320),
                Location = new Point(50, 30),
                BackColor = Color.White
            };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(220, 228, 235));
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                using var b = new SolidBrush(Color.FromArgb(23, 115, 115));
                e.Graphics.FillRectangle(b, 0, 0, 6, card.Height);
            };

            var titleBar = new Panel { Location = new Point(0, 0), Size = new Size(500, 46), BackColor = Color.FromArgb(23, 115, 115) };
            titleBar.Controls.Add(new Label
            {
                Text = "👨‍🏫  Teacher Profile",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(14, 12)
            });
            card.Controls.Add(titleBar);

            int y = 58;
            void AddField(string label, string value)
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
                    AutoSize = true, Location = new Point(200, y)
                });
                y += 38;
            }

            if (teacher != null)
            {
                AddField("Teacher ID",     teacher.TeacherID);
                AddField("Full Name",      teacher.FullName);
                AddField("Department",     teacher.Department);
                AddField("Contact",        teacher.ContactNumber);
                AddField("Schedules",      DataManager.GetSchedulesForTeacher(teacherID).Count + " assigned");
                AddField("Grades Given",   DataManager.GetGradesForTeacher(teacherID).Count + " records");
            }
            else
            {
                card.Controls.Add(new Label
                {
                    Text = "No teacher profile found. Please contact the administrator.",
                    ForeColor = Color.Gray, AutoSize = true, Location = new Point(20, 60)
                });
            }

            pnl.Controls.Add(card);
            tabProfile.Controls.Add(pnl);
        }

        

        private void LoadAll()
        {
            DataManager.LoadAll();
            LoadSchedule();
            PopulateCourseFilter();
            LoadStudents();
            LoadGrades();
        }

        private void LoadSchedule()
        {
            dgvSchedule.Rows.Clear();
            var schedules = DataManager.GetSchedulesForTeacher(teacherID);
            foreach (var s in schedules)
            {
                var course = DataManager.GetCourse(s.CourseID);
                dgvSchedule.Rows.Add(
                    s.ScheduleID,
                    course?.CourseName ?? s.CourseID,
                    s.Section,
                    s.Day,
                    s.TimeDisplay,
                    s.Room,
                    s.Semester
                );
            }

            if (schedules.Count == 0)
            {
                
            }
        }

        private void PopulateCourseFilter()
        {
            cmbScheduleFilter.Items.Clear();
            cmbScheduleFilter.Items.Add("All Courses");

            
            var teacherCourseIDs = DataManager.GetSchedulesForTeacher(teacherID)
                .Select(s => s.CourseID).Distinct().ToList();

            var teacherCourses = DataManager.Courses
                .Where(c => teacherCourseIDs.Contains(c.CourseID))
                .OrderBy(c => c.CourseName)
                .ToList();

            foreach (var c in teacherCourses)
                cmbScheduleFilter.Items.Add($"{c.CourseID} – {c.CourseName}");

            
            if (teacherCourses.Count == 0)
                foreach (var c in DataManager.Courses.OrderBy(c => c.CourseName))
                    cmbScheduleFilter.Items.Add($"{c.CourseID} – {c.CourseName}");

            cmbScheduleFilter.SelectedIndex = 0;
        }

        private void LoadStudents()
        {
            dgvStudents.Rows.Clear();

            string filterCourseID = null;
            if (cmbScheduleFilter != null && cmbScheduleFilter.SelectedIndex > 0)
                filterCourseID = cmbScheduleFilter.SelectedItem.ToString().Split('–')[0].Trim();

            var enrollments = filterCourseID == null
                ? DataManager.Enrollments.AsEnumerable()
                : DataManager.Enrollments.Where(e => e.CourseID == filterCourseID);

            var studentIDs = enrollments.Select(e => e.StudentID).Distinct().ToList();

            var students = DataManager.Students
                .Where(s => studentIDs.Contains(s.StudentID))
                .OrderBy(s => s.LastName)
                .ToList();

            foreach (var s in students)
            {
                var latestEnroll = DataManager.Enrollments
                    .Where(e => e.StudentID == s.StudentID &&
                           (filterCourseID == null || e.CourseID == filterCourseID))
                    .OrderByDescending(e => e.EnrollmentDate)
                    .FirstOrDefault();

                int rowIdx = dgvStudents.Rows.Add(
                    s.StudentID, s.FullName, s.Gender,
                    s.YearLevel, s.Course,
                    latestEnroll?.Status ?? "—");

                
                var cell = dgvStudents.Rows[rowIdx].Cells[5];
                cell.Style.ForeColor = latestEnroll?.Status switch
                {
                    "Enrolled"  => Color.FromArgb(39, 174, 96),
                    "Dropped"   => Color.FromArgb(192, 57, 43),
                    "Completed" => Color.FromArgb(41, 128, 185),
                    "Pending"   => Color.FromArgb(230, 126, 34),
                    _           => Color.Gray
                };
                cell.Style.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            }
        }

        private void LoadGrades()
        {
            dgvGrades.Rows.Clear();
            var grades = DataManager.GetGradesForTeacher(teacherID)
                .OrderByDescending(g => g.DateRecorded)
                .ToList();

            foreach (var g in grades)
            {
                var student = DataManager.GetStudent(g.StudentID);
                var course  = DataManager.GetCourse(g.CourseID);
                dgvGrades.Rows.Add(
                    g.GradeID,
                    g.StudentID,
                    student?.FullName ?? g.StudentID,
                    course?.CourseName ?? g.CourseID,
                    g.SubjectGrade,
                    g.Remarks,
                    g.DateRecorded
                );
            }
        }

        

        private void BtnAddGrade_Click(object sender, EventArgs e)
        {
            
            var teacherCourseIDs = DataManager.GetSchedulesForTeacher(teacherID)
                .Select(s => s.CourseID).Distinct().ToList();

            if (!DataManager.Students.Any())
            { MessageBox.Show("No students found.", "No Students", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            using var dlg = new GradeDialog(teacherID);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                if (!DataManager.AddGrade(dlg.Grade))
                    MessageBox.Show("Grade ID already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                {
                    MessageBox.Show("Grade recorded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadGrades();
                }
            }
        }

        private void BtnEditGrade_Click(object sender, EventArgs e)
        {
            if (dgvGrades.SelectedRows.Count == 0)
            { MessageBox.Show("Select a grade to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            var gradeID = dgvGrades.SelectedRows[0].Cells[0].Value?.ToString();
            var grade   = DataManager.Grades.FirstOrDefault(g => g.GradeID == gradeID);
            if (grade == null) return;

            using var dlg = new GradeDialog(teacherID, grade);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                DataManager.UpdateGrade(dlg.Grade);
                MessageBox.Show("Grade updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadGrades();
            }
        }

        private void BtnDeleteGrade_Click(object sender, EventArgs e)
        {
            if (dgvGrades.SelectedRows.Count == 0)
            { MessageBox.Show("Select a grade to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            var gradeID     = dgvGrades.SelectedRows[0].Cells[0].Value?.ToString();
            var studentName = dgvGrades.SelectedRows[0].Cells[2].Value?.ToString();

            if (MessageBox.Show($"Delete grade for '{studentName}' ({gradeID})?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DataManager.DeleteGrade(gradeID);
                MessageBox.Show("Grade deleted.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadGrades();
            }
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
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 252, 252);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 235, 235);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(10, 60, 60);

            foreach (var h in headers)
                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = h,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                });
            return dgv;
        }

        private Button MakeBtn(string text, Color color, int x, int y, int w)
        {
            var b = new Button
            {
                Text = text, Location = new Point(x, y), Size = new Size(w, 30),
                BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
    }

    

    public class GradeDialog : Form
    {
        public Grade Grade { get; private set; }
        private string teacherID;
        private TextBox txtGradeID, txtGradeValue;
        private ComboBox cmbStudent, cmbCourse, cmbRemarks;
        private Label lblEnrollStatus;

        public GradeDialog(string teacherID, Grade existing = null)
        {
            this.teacherID = teacherID;
            this.Grade = existing;
            InitializeComponent();
            if (existing != null) Populate(existing);
        }

        private void InitializeComponent()
        {
            bool isEdit = Grade != null;
            this.Text = isEdit ? "Edit Grade" : "Input Grade";
            this.Size = new Size(500, 460);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);

            var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(39, 174, 96) };
            pnlTitle.Controls.Add(new Label
            {
                Text = isEdit ? "✏️  Edit Grade Record" : "📊  Input Student Grade",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(16, 14)
            });
            this.Controls.Add(pnlTitle);

            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 16, 24, 16) };
            int y = 16;
            int lw = 155;
            int fw = 290;

            void Row(string label, Control ctrl, string hint = null)
            {
                pnl.Controls.Add(new Label
                {
                    Text = label, AutoSize = true,
                    Location = new Point(0, y + 3),
                    Font = new Font("Segoe UI", 9f)
                });
                ctrl.Location = new Point(lw, y);
                if (ctrl is TextBox tb) tb.Width = fw;
                else if (ctrl is ComboBox cb) { cb.Width = fw; cb.DropDownStyle = ComboBoxStyle.DropDownList; }
                pnl.Controls.Add(ctrl);
                if (hint != null)
                    pnl.Controls.Add(new Label
                    {
                        Text = hint, AutoSize = true,
                        Location = new Point(lw + fw + 6, y + 4),
                        Font = new Font("Segoe UI", 7.5f),
                        ForeColor = Color.Gray
                    });
                y += 40;
            }

            
            txtGradeID = new TextBox { Text = DataManager.GenerateGradeID() };
            if (isEdit) { txtGradeID.ReadOnly = true; txtGradeID.BackColor = Color.FromArgb(242,244,248); }
            Row("Grade ID *", txtGradeID);

            
            cmbStudent = new ComboBox();
            foreach (var s in DataManager.Students.OrderBy(s => s.LastName))
                cmbStudent.Items.Add($"{s.StudentID} – {s.FullName}");
            if (isEdit) cmbStudent.Enabled = false;
            cmbStudent.SelectedIndexChanged += (s, e) => UpdateEnrollStatus();
            Row("Student *", cmbStudent);

           
            lblEnrollStatus = new Label
            {
                Text = "", AutoSize = true,
                Location = new Point(lw, y - 28),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(39, 130, 80)
            };
            pnl.Controls.Add(lblEnrollStatus);

            
            cmbCourse = new ComboBox();
            foreach (var c in DataManager.Courses.OrderBy(c => c.CourseName))
                cmbCourse.Items.Add($"{c.CourseID} – {c.CourseName}");
            if (isEdit) cmbCourse.Enabled = false;
            Row("Course *", cmbCourse);

           
            txtGradeValue = new TextBox();
            Row("Grade *", txtGradeValue, "e.g. 1.00–5.00 or INC");

            
            cmbRemarks = new ComboBox();
            cmbRemarks.Items.AddRange(new[] { "Passed", "Failed", "Incomplete", "Dropped", "Conditional" });
            cmbRemarks.SelectedIndex = 0;
            Row("Remarks *", cmbRemarks);

            
            txtGradeValue.TextChanged += (s, e) =>
            {
                if (double.TryParse(txtGradeValue.Text, out double g))
                    cmbRemarks.SelectedItem = g <= 3.0 ? "Passed" : "Failed";
                else if (txtGradeValue.Text.ToUpper() == "INC")
                    cmbRemarks.SelectedItem = "Incomplete";
            };

            
            var btnSave = new Button
            {
                Text = isEdit ? "💾 Update Grade" : "💾 Save Grade",
                Location = new Point(lw, y + 10), Size = new Size(145, 36),
                BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += Save;

            var btnCancel = new Button
            {
                Text = "Cancel", Location = new Point(lw + 155, y + 10), Size = new Size(110, 36),
                BackColor = Color.FromArgb(149,165,166), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            pnl.Controls.Add(btnSave);
            pnl.Controls.Add(btnCancel);
            this.Controls.Add(pnl);
        }

        private void UpdateEnrollStatus()
        {
            if (cmbStudent.SelectedIndex < 0 || lblEnrollStatus == null) return;
            string sid = cmbStudent.SelectedItem.ToString().Split('–')[0].Trim();
            var enroll = DataManager.Enrollments
                .Where(e => e.StudentID == sid)
                .OrderByDescending(e => e.EnrollmentDate)
                .FirstOrDefault();
            lblEnrollStatus.Text = enroll != null
                ? $"Enrollment status: {enroll.Status} ({enroll.CourseID})"
                : "No enrollment record found";
            lblEnrollStatus.ForeColor = enroll?.Status == "Enrolled"
                ? Color.FromArgb(39, 130, 80)
                : Color.FromArgb(180, 100, 0);
        }

        private void Populate(Grade g)
        {
            txtGradeID.Text = g.GradeID;
            for (int i = 0; i < cmbStudent.Items.Count; i++)
                if (cmbStudent.Items[i].ToString().StartsWith(g.StudentID)) { cmbStudent.SelectedIndex = i; break; }
            for (int i = 0; i < cmbCourse.Items.Count; i++)
                if (cmbCourse.Items[i].ToString().StartsWith(g.CourseID)) { cmbCourse.SelectedIndex = i; break; }
            txtGradeValue.Text = g.SubjectGrade;
            cmbRemarks.SelectedItem = g.Remarks;
        }

        private void Save(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtGradeID.Text) ||
                cmbStudent.SelectedIndex < 0 ||
                cmbCourse.SelectedIndex < 0 ||
                string.IsNullOrWhiteSpace(txtGradeValue.Text) ||
                cmbRemarks.SelectedIndex < 0)
            {
                MessageBox.Show("Please fill in all required (*) fields.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string studentID = cmbStudent.SelectedItem.ToString().Split('–')[0].Trim();
            string courseID  = cmbCourse.SelectedItem.ToString().Split('–')[0].Trim();

            Grade = new Grade
            {
                GradeID      = txtGradeID.Text.Trim(),
                StudentID    = studentID,
                CourseID     = courseID,
                TeacherID    = teacherID,
                SubjectGrade = txtGradeValue.Text.Trim(),
                Remarks      = cmbRemarks.SelectedItem.ToString(),
                DateRecorded = DateTime.Now.ToString("MM/dd/yyyy"),
                EnrollmentID = DataManager.Enrollments
                    .FirstOrDefault(en => en.StudentID == studentID && en.CourseID == courseID)
                    ?.EnrollmentID ?? ""
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
