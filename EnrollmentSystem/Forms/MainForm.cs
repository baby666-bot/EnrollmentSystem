using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using EnrollmentSystem.Data;
using EnrollmentSystem.Forms;

namespace EnrollmentSystem
{
    public class MainForm : Form
    {
        private MenuStrip menuStrip;
        private Panel pnlHeader, pnlStats, pnlRecent;
        private Label lblTotalStudents, lblTotalCourses, lblTotalEnrollments, lblTotalTeachers, lblTotalSchedules;
        private DataGridView dgvRecentStudents, dgvRecentCourses, dgvRecentEnrollments;
        private Label lblFooterTime;
        private System.Windows.Forms.Timer clockTimer;

        public MainForm()
        {
            InitializeComponent();
            RefreshDashboard();
            StartClock();
        }

       

        private void InitializeComponent()
        {
            this.Text = "Enrollment System — Admin Dashboard";
            this.Size = new Size(1100, 750);
            this.MinimumSize = new Size(960, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 243, 249);
            this.Font = new Font("Segoe UI", 9f);

            BuildMenuStrip();
            BuildHeader();
            BuildStatCards();
            BuildRecentTables();
            BuildFooter();
        }

       

        private void BuildMenuStrip()
        {
            menuStrip = new MenuStrip
            {
                BackColor = Color.FromArgb(22, 44, 76),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                Renderer = new DarkMenuRenderer()
            };

            (string Text, Action Click)[] items =
            {
                ("🏠  Dashboard",    () => RefreshDashboard()),
                ("👨‍🎓  Students",   () => OpenStudents()),
                ("📚  Courses",      () => OpenCourses()),
                ("📋  Enrollments",  () => OpenEnrollments()),
                ("👨‍🏫  Teachers",   () => OpenTeachers()),
                ("🗓️  Schedules",  () => OpenSchedules()),
                ("🚪  Logout",       () => Logout()),
            };

            foreach (var (text, click) in items)
            {
                var item = new ToolStripMenuItem(text)
                {
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f),
                    Padding = new Padding(10, 4, 10, 4)
                };
                item.Click += (s, e) => click();
                menuStrip.Items.Add(item);
            }

            
            menuStrip.Items.Add(new ToolStripSeparator());
            var userLabel = new ToolStripLabel($"👤  {DataManager.CurrentUser?.FullName ?? "Admin"}  ")
            {
                ForeColor = Color.FromArgb(160, 200, 240),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                Alignment = ToolStripItemAlignment.Right
            };
            menuStrip.Items.Add(userLabel);

            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;
        }

        

        private void BuildHeader()
        {
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 88,
                BackColor = Color.FromArgb(22, 44, 76)
            };

            pnlHeader.Controls.Add(new Label
            {
                Text = "📘  Enrollment System Management",
                Font = new Font("Segoe UI", 19f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(28, 18)
            });
            pnlHeader.Controls.Add(new Label
            {
                Text = $"Welcome back, {DataManager.CurrentUser?.FullName ?? "Administrator"}  •  Admin Dashboard",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(170, 200, 235),
                AutoSize = true,
                Location = new Point(32, 58)
            });

            this.Controls.Add(pnlHeader);
        }

        

        private void BuildStatCards()
        {
            pnlStats = new Panel
            {
                Height = 120,
                BackColor = Color.Transparent
            };

            
            string[] cardIcons   = { "👨‍🎓", "📚", "📋", "👨‍🏫", "🗓️" };
            string[] cardTitles  = { "Total Students", "Total Courses", "Total Enrollments", "Total Teachers", "Total Schedules" };
            Color[]  cardColors  = { Color.FromArgb(41, 128, 185), Color.FromArgb(39, 174, 96), Color.FromArgb(142, 68, 173), Color.FromArgb(211, 84, 0), Color.FromArgb(23, 115, 115) };

            for (int i = 0; i < 5; i++)
            {
                var card = MakeStatCard(cardIcons[i], cardTitles[i], cardColors[i], out Label numLbl);
                if      (i == 0) lblTotalStudents     = numLbl;
                else if (i == 1) lblTotalCourses      = numLbl;
                else if (i == 2) lblTotalEnrollments  = numLbl;
                else if (i == 3) lblTotalTeachers     = numLbl;
                else             lblTotalSchedules    = numLbl;
                pnlStats.Controls.Add(card);
            }

            this.Controls.Add(pnlStats);
        }

        private Panel MakeStatCard(string icon, string title, Color color, out Label numLabel)
        {
            var card = new Panel
            {
                Size = new Size(230, 90),
                BackColor = Color.White,
                Cursor = Cursors.Default
            };

            
            card.Paint += (s, e) =>
            {
                using var b = new SolidBrush(color);
                e.Graphics.FillRectangle(b, 0, 0, 6, card.Height);
                using var pen = new Pen(Color.FromArgb(220, 224, 232));
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            var lblIcon = new Label
            {
                Text = icon, Font = new Font("Segoe UI", 22f),
                ForeColor = color, AutoSize = true, Location = new Point(16, 14)
            };

            numLabel = new Label
            {
                Text = "0", Font = new Font("Segoe UI", 26f, FontStyle.Bold),
                ForeColor = color, AutoSize = true, Location = new Point(62, 10)
            };

            var lblTitle = new Label
            {
                Text = title, Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(110, 120, 140),
                AutoSize = true, Location = new Point(62, 56)
            };

            card.Controls.Add(lblIcon);
            card.Controls.Add(numLabel);
            card.Controls.Add(lblTitle);
            return card;
        }

        

        private void BuildRecentTables()
        {
            pnlRecent = new Panel
            {
                BackColor = Color.Transparent,
                Dock = DockStyle.None
            };

            
            var (pnlS, dgvS) = MakeTablePanel("👨‍🎓  Recent Students", Color.FromArgb(41, 128, 185),
                new[] { "Student ID", "Name", "Course", "Year" });
            dgvRecentStudents = dgvS;

           
            var (pnlC, dgvC) = MakeTablePanel("📚  Courses", Color.FromArgb(39, 174, 96),
                new[] { "Course ID", "Name", "Department" });
            dgvRecentCourses = dgvC;

            
            var (pnlE, dgvE) = MakeTablePanel("📋  Recent Enrollments", Color.FromArgb(142, 68, 173),
                new[] { "Enrollment ID", "Student", "Course", "Status" });
            dgvRecentEnrollments = dgvE;

            pnlRecent.Controls.Add(pnlS);
            pnlRecent.Controls.Add(pnlC);
            pnlRecent.Controls.Add(pnlE);

            this.Controls.Add(pnlRecent);
        }

        private (Panel outer, DataGridView dgv) MakeTablePanel(string title, Color accentColor, string[] headers)
        {
            var outer = new Panel { BackColor = Color.White };
            outer.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(220, 224, 232));
                e.Graphics.DrawRectangle(pen, 0, 0, outer.Width - 1, outer.Height - 1);
            };

            var pnlTitle = new Panel
            {
                Dock = DockStyle.Top, Height = 36,
                BackColor = accentColor
            };
            pnlTitle.Controls.Add(new Label
            {
                Text = title, Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(10, 9)
            });
            outer.Controls.Add(pnlTitle);

            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 8.5f),
                GridColor = Color.FromArgb(235, 238, 244),
                ColumnHeadersHeight = 28,
                RowTemplate = { Height = 26 },
                ScrollBars = ScrollBars.Vertical
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 251);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(60, 80, 110);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 251, 254);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 232, 248);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 50, 90);

            foreach (var h in headers)
                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = h,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                });

            outer.Controls.Add(dgv);
            return (outer, dgv);
        }

        

        private void BuildFooter()
        {
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom, Height = 30,
                BackColor = Color.FromArgb(22, 44, 76)
            };
            lblFooterTime = new Label
            {
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(160, 200, 240),
                AutoSize = true, Location = new Point(14, 8)
            };
            pnlFooter.Controls.Add(lblFooterTime);
            pnlFooter.Controls.Add(new Label
            {
                Text = "Enrollment System Management  •  Admin",
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(120, 160, 200),
                AutoSize = true,
                Location = new Point(780, 8)
            });
            this.Controls.Add(pnlFooter);
        }

        

        protected override void OnLoad(EventArgs e)    { base.OnLoad(e);    DoLayout(); }
        protected override void OnResize(EventArgs e)  { base.OnResize(e);  DoLayout(); }

        private void DoLayout()
        {
            int menuH   = menuStrip?.Height ?? 24;
            int headerH = 88;
            int statY   = menuH + headerH;
            int statH   = 120;
            int pad     = 18;
            int w       = this.ClientSize.Width;
            int footerH = 30;

            
            if (pnlStats != null)
            {
                pnlStats.Location = new Point(0, statY);
                pnlStats.Size = new Size(w, statH);

                int cardW   = (w - pad * 5) / 4;
                int cardX   = pad;
                for (int i = 0; i < pnlStats.Controls.Count; i++)
                {
                    var card = pnlStats.Controls[i];
                    card.Location = new Point(cardX, 16);
                    card.Size = new Size(cardW, 88);
                    cardX += cardW + pad;
                }
            }

           
            if (pnlRecent != null)
            {
                int tableY = statY + statH + 10;
                int tableH = this.ClientSize.Height - tableY - footerH - 10;
                pnlRecent.Location = new Point(0, tableY);
                pnlRecent.Size = new Size(w, tableH);

                int tblW = (w - pad * 4) / 3;
                int tx = pad;
                for (int i = 0; i < pnlRecent.Controls.Count; i++)
                {
                    var tbl = pnlRecent.Controls[i];
                    tbl.Location = new Point(tx, 0);
                    tbl.Size = new Size(tblW, tableH);
                    tx += tblW + pad;
                }
            }
        }

        

        public void RefreshDashboard()
        {
            DataManager.LoadAll();

            
            if (lblTotalStudents    != null) lblTotalStudents.Text    = DataManager.Students.Count.ToString();
            if (lblTotalCourses     != null) lblTotalCourses.Text     = DataManager.Courses.Count.ToString();
            if (lblTotalEnrollments != null) lblTotalEnrollments.Text = DataManager.Enrollments.Count.ToString();
            if (lblTotalTeachers    != null) lblTotalTeachers.Text    = DataManager.Teachers.Count.ToString();
            if (lblTotalSchedules   != null) lblTotalSchedules.Text   = DataManager.Schedules.Count.ToString();

            
            if (dgvRecentStudents != null)
            {
                dgvRecentStudents.Rows.Clear();
                var recent = DataManager.Students
                    .OrderByDescending(s => s.StudentID)
                    .Take(10)
                    .ToList();
                foreach (var s in recent)
                    dgvRecentStudents.Rows.Add(s.StudentID, s.FullName, s.Course, s.YearLevel);
            }

            
            if (dgvRecentCourses != null)
            {
                dgvRecentCourses.Rows.Clear();
                foreach (var c in DataManager.Courses.OrderBy(c => c.CourseID))
                    dgvRecentCourses.Rows.Add(c.CourseID, c.CourseName, c.Department);
            }

            
            if (dgvRecentEnrollments != null)
            {
                dgvRecentEnrollments.Rows.Clear();
                var recent = DataManager.Enrollments
                    .OrderByDescending(e => e.EnrollmentDate)
                    .Take(10)
                    .ToList();
                foreach (var e in recent)
                {
                    var student = DataManager.GetStudent(e.StudentID);
                    var course  = DataManager.GetCourse(e.CourseID);
                    int row = dgvRecentEnrollments.Rows.Add(
                        e.EnrollmentID,
                        student?.FullName ?? e.StudentID,
                        course?.CourseName ?? e.CourseID,
                        e.Status);

                    
                    var cell = dgvRecentEnrollments.Rows[row].Cells[3];
                    cell.Style.ForeColor = e.Status switch
                    {
                        "Enrolled"  => Color.FromArgb(39, 174, 96),
                        "Dropped"   => Color.FromArgb(192, 57, 43),
                        "Completed" => Color.FromArgb(41, 128, 185),
                        "Pending"   => Color.FromArgb(230, 126, 34),
                        _           => Color.Gray
                    };
                    cell.Style.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                }
            }
        }

        

        private void OpenStudents()
        {
            using var f = new StudentForm();
            f.ShowDialog(this);
            RefreshDashboard();
        }

        private void OpenCourses()
        {
            using var f = new CourseForm();
            f.ShowDialog(this);
            RefreshDashboard();
        }

        private void OpenEnrollments()
        {
            using var f = new EnrollmentForm();
            f.ShowDialog(this);
            RefreshDashboard();
        }

        private void OpenTeachers()
        {
            using var f = new TeacherForm();
            f.ShowDialog(this);
            RefreshDashboard();
        }

        private void OpenSchedules()
        {
            using var f = new ScheduleForm();
            f.ShowDialog(this);
            RefreshDashboard();
        }

        private void Logout()
        {
            var r = MessageBox.Show("Log out and return to the login screen?",
                "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;
            DataManager.CurrentUser = null;
            new LoginForm().Show();
            this.Close();
        }

        

        private void StartClock()
        {
            clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            clockTimer.Tick += (s, e) =>
            {
                if (lblFooterTime != null)
                    lblFooterTime.Text = DateTime.Now.ToString("dddd, MMMM dd yyyy   hh:mm:ss tt");
            };
            clockTimer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            clockTimer?.Stop();
            base.OnFormClosed(e);
        }
    }

    

    public class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            using var b = new SolidBrush(
                e.Item.Selected || e.Item.Pressed
                    ? Color.FromArgb(50, 85, 130)
                    : Color.FromArgb(22, 44, 76));
            e.Graphics.FillRectangle(b, new Rectangle(Point.Empty, e.Item.Size));
        }
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using var b = new SolidBrush(Color.FromArgb(22, 44, 76));
            e.Graphics.FillRectangle(b, e.AffectedBounds);
        }
        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            
        }
    }
}
