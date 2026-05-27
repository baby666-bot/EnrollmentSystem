using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EnrollmentSystem.Data;
using EnrollmentSystem.Models;

namespace EnrollmentSystem.Forms
{
    public class ScheduleForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSearch;
        private ComboBox cmbFilterTeacher;
        private Button btnAdd, btnEdit, btnDelete;
        private Label lblCount;

        public ScheduleForm()
        {
            InitializeComponent();
            LoadGrid();
        }

        private void InitializeComponent()
        {
            this.Text = "Class Schedule Management";
            this.Size = new Size(1150, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9f);

            
            var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(23, 115, 115) };
            pnlTitle.Controls.Add(new Label
            {
                Text = "🗓️  Class Schedule Management",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(18, 13)
            });
            this.Controls.Add(dgv);
            this.Controls.Add(pnlTitle);

            
            var pnlBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };

            pnlBar.Controls.Add(new Label { Text = "Search:", AutoSize = true, Location = new Point(10, 17) });
            txtSearch = new TextBox { Location = new Point(66, 13), Width = 180 };
            txtSearch.TextChanged += (s, e) => ApplyFilter();

            pnlBar.Controls.Add(new Label { Text = "Teacher:", AutoSize = true, Location = new Point(260, 17) });
            cmbFilterTeacher = new ComboBox { Location = new Point(318, 13), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFilterTeacher.SelectedIndexChanged += (s, e) => ApplyFilter();

            btnAdd    = Btn("➕ Add Schedule",  Color.FromArgb(23, 115, 115), 535, 10, 135);
            btnEdit   = Btn("✏️ Edit",           Color.FromArgb(41, 128, 185), 680, 10, 78);
            btnDelete = Btn("🗑️ Delete",         Color.FromArgb(192, 57, 43),  768, 10, 90);
            var btnRefresh = Btn("🔄", Color.FromArgb(100, 115, 140), 868, 10, 36);

            btnAdd.Click    += BtnAdd_Click;
            btnEdit.Click   += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => { DataManager.LoadAll(); PopulateFilters(); LoadGrid(); };

            pnlBar.Controls.AddRange(new Control[] { txtSearch, cmbFilterTeacher, btnAdd, btnEdit, btnDelete, btnRefresh });
            this.Controls.Add(pnlBar);

            
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                RowHeadersVisible = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9f), GridColor = Color.FromArgb(228, 232, 240),
                ColumnHeadersHeight = 36, RowTemplate = { Height = 30 }
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(23, 115, 115);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(246, 252, 252);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 235, 235);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(10, 60, 60);
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };

            
            var pnlStatus = new Panel { Dock = DockStyle.Bottom, Height = 28, BackColor = Color.FromArgb(240, 244, 248) };
            lblCount = new Label { AutoSize = true, Location = new Point(10, 7), ForeColor = Color.SlateGray, Font = new Font("Segoe UI", 8.5f) };
            pnlStatus.Controls.Add(lblCount);
            this.Controls.Add(pnlStatus);

            PopulateFilters();
        }

        private Button Btn(string text, Color color, int x, int y, int w)
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

        private void PopulateFilters()
        {
            cmbFilterTeacher.Items.Clear();
            cmbFilterTeacher.Items.Add("All Teachers");
            foreach (var t in DataManager.Teachers.OrderBy(t => t.LastName))
                cmbFilterTeacher.Items.Add($"{t.TeacherID} – {t.FullName}");
            if (cmbFilterTeacher.SelectedIndex < 0) cmbFilterTeacher.SelectedIndex = 0;
        }

        private void LoadGrid() => BindGrid(DataManager.Schedules
            .OrderBy(s => s.Day).ThenBy(s => s.TimeStart).ToList());

        private void ApplyFilter()
        {
            string search = txtSearch.Text.Trim();
            string tid = cmbFilterTeacher.SelectedIndex > 0
                ? cmbFilterTeacher.SelectedItem.ToString().Split('–')[0].Trim()
                : null;

            var list = string.IsNullOrEmpty(search)
                ? DataManager.Schedules.AsEnumerable()
                : DataManager.SearchSchedules(search);

            if (!string.IsNullOrEmpty(tid))
                list = list.Where(s => s.TeacherID == tid);

            BindGrid(list.OrderBy(s => s.Day).ThenBy(s => s.TimeStart).ToList());
        }

        private void BindGrid(System.Collections.Generic.List<Schedule> list)
        {
            dgv.Rows.Clear();
            dgv.Columns.Clear();

            foreach (var h in new[] { "Sched ID", "Course", "Teacher", "Day", "Time", "Room", "Section", "Semester" })
                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = h,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                });

            foreach (var s in list)
            {
                var course  = DataManager.GetCourse(s.CourseID);
                var teacher = DataManager.GetTeacher(s.TeacherID);
                dgv.Rows.Add(
                    s.ScheduleID,
                    course?.CourseName  ?? s.CourseID,
                    teacher?.FullName   ?? s.TeacherID,
                    s.Day,
                    s.TimeDisplay,
                    s.Room,
                    s.Section,
                    s.Semester
                );
            }

            lblCount.Text = $"Showing {list.Count} of {DataManager.Schedules.Count} schedule(s)";
        }

        private string SelectedID() =>
            dgv.SelectedRows.Count == 0 ? null : dgv.SelectedRows[0].Cells[0].Value?.ToString();

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!DataManager.Courses.Any())
            { MessageBox.Show("Please add courses first.", "No Courses", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!DataManager.Teachers.Any())
            { MessageBox.Show("Please add teachers first.", "No Teachers", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            using var dlg = new ScheduleDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                if (!DataManager.AddSchedule(dlg.Schedule))
                    MessageBox.Show("Schedule ID already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                {
                    MessageBox.Show("Schedule added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadGrid();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            var id = SelectedID();
            if (id == null) { MessageBox.Show("Select a schedule to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var sched = DataManager.GetSchedule(id);
            if (sched == null) return;

            using var dlg = new ScheduleDialog(sched);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                DataManager.UpdateSchedule(dlg.Schedule);
                MessageBox.Show("Schedule updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadGrid();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var id = SelectedID();
            if (id == null) { MessageBox.Show("Select a schedule to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            var sched   = DataManager.GetSchedule(id);
            var course  = DataManager.GetCourse(sched?.CourseID);
            var teacher = DataManager.GetTeacher(sched?.TeacherID);

            if (MessageBox.Show(
                    $"Delete schedule '{id}'?\n{course?.CourseName}  |  {teacher?.FullName}  |  {sched?.Day} {sched?.TimeDisplay}",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DataManager.DeleteSchedule(id);
                MessageBox.Show("Schedule deleted.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadGrid();
            }
        }
    }

    

    public class ScheduleDialog : Form
    {
        public Schedule Schedule { get; private set; }

        private TextBox txtID, txtRoom, txtSection;
        private ComboBox cmbCourse, cmbTeacher, cmbDay, cmbSemester;
        private TextBox txtTimeStart, txtTimeEnd;

        public ScheduleDialog(Schedule existing = null)
        {
            Schedule = existing;
            InitializeComponent();
            if (existing != null) Populate(existing);
        }

        private void InitializeComponent()
        {
            bool isEdit = Schedule != null;
            this.Text = isEdit ? "Edit Schedule" : "Add Class Schedule";
            this.Size = new Size(500, 530);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);

            
            var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(23, 115, 115) };
            pnlTitle.Controls.Add(new Label
            {
                Text = isEdit ? "✏️  Edit Class Schedule" : "🗓️  Add Class Schedule",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(16, 14)
            });
            this.Controls.Add(pnlTitle);

            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 16, 24, 16) };
            int y = 16;
            int lw = 130;  
            int fw = 300;  

            void Row(string label, Control ctrl)
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
                y += 40;
            }

            
            txtID = new TextBox { Text = isEdit ? "" : DataManager.GenerateScheduleID() };
            if (isEdit) { txtID.ReadOnly = true; txtID.BackColor = Color.FromArgb(242, 244, 248); }
            Row("Schedule ID *", txtID);

            
            cmbCourse = new ComboBox();
            foreach (var c in DataManager.Courses.OrderBy(c => c.CourseName))
                cmbCourse.Items.Add($"{c.CourseID} – {c.CourseName}");
            Row("Course *", cmbCourse);

            
            cmbTeacher = new ComboBox();
            foreach (var t in DataManager.Teachers.OrderBy(t => t.LastName))
                cmbTeacher.Items.Add($"{t.TeacherID} – {t.FullName}");
            Row("Assign Teacher *", cmbTeacher);

            
            cmbDay = new ComboBox();
            cmbDay.Items.AddRange(new[] {
                "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday",
                "Mon / Wed / Fri", "Tue / Thu", "Mon / Wed", "Tue / Thu / Sat"
            });
            Row("Day *", cmbDay);

            
            pnl.Controls.Add(new Label { Text = "Time *", AutoSize = true, Location = new Point(0, y + 3) });
            txtTimeStart = new TextBox { Location = new Point(lw, y), Width = 130, Text = "07:30" };
            pnl.Controls.Add(new Label { Text = "to", AutoSize = true, Location = new Point(lw + 138, y + 3) });
            txtTimeEnd = new TextBox { Location = new Point(lw + 158, y), Width = 130, Text = "09:00" };
            pnl.Controls.Add(new Label { Text = "(HH:MM)", Font = new Font("Segoe UI", 7.5f), ForeColor = Color.Gray, AutoSize = true, Location = new Point(lw + 295, y + 5) });
            pnl.Controls.Add(txtTimeStart);
            pnl.Controls.Add(txtTimeEnd);
            y += 40;

            
            txtRoom = new TextBox();
            Row("Room *", txtRoom);

            
            txtSection = new TextBox();
            Row("Section *", txtSection);

            
            cmbSemester = new ComboBox();
            int yr = DateTime.Now.Year;
            cmbSemester.Items.AddRange(new[] {
                $"1st Sem {yr}-{yr + 1}",
                $"2nd Sem {yr}-{yr + 1}",
                $"Summer {yr + 1}",
                $"1st Sem {yr + 1}-{yr + 2}",
                $"2nd Sem {yr + 1}-{yr + 2}"
            });
            cmbSemester.SelectedIndex = 0;
            Row("Semester *", cmbSemester);

            
            var btnSave = new Button
            {
                Text = isEdit ? "💾 Update" : "💾 Save Schedule",
                Location = new Point(lw, y + 10), Size = new Size(145, 36),
                BackColor = Color.FromArgb(23, 115, 115), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += Save;

            var btnCancel = new Button
            {
                Text = "Cancel", Location = new Point(lw + 155, y + 10), Size = new Size(100, 36),
                BackColor = Color.FromArgb(149, 165, 166), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            pnl.Controls.Add(btnSave);
            pnl.Controls.Add(btnCancel);
            this.Controls.Add(pnl);
        }

        private void Populate(Schedule s)
        {
            txtID.Text = s.ScheduleID;

            for (int i = 0; i < cmbCourse.Items.Count; i++)
                if (cmbCourse.Items[i].ToString().StartsWith(s.CourseID)) { cmbCourse.SelectedIndex = i; break; }

            for (int i = 0; i < cmbTeacher.Items.Count; i++)
                if (cmbTeacher.Items[i].ToString().StartsWith(s.TeacherID)) { cmbTeacher.SelectedIndex = i; break; }

            cmbDay.SelectedItem = s.Day;
            txtTimeStart.Text   = s.TimeStart;
            txtTimeEnd.Text     = s.TimeEnd;
            txtRoom.Text        = s.Room;
            txtSection.Text     = s.Section;
            cmbSemester.SelectedItem = s.Semester;
            if (cmbSemester.SelectedIndex < 0) cmbSemester.SelectedIndex = 0;
        }

        private void Save(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text) ||
                cmbCourse.SelectedIndex < 0 ||
                cmbTeacher.SelectedIndex < 0 ||
                cmbDay.SelectedIndex < 0 ||
                string.IsNullOrWhiteSpace(txtTimeStart.Text) ||
                string.IsNullOrWhiteSpace(txtTimeEnd.Text) ||
                string.IsNullOrWhiteSpace(txtRoom.Text) ||
                string.IsNullOrWhiteSpace(txtSection.Text))
            {
                MessageBox.Show("Please fill in all required (*) fields.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string courseID  = cmbCourse.SelectedItem.ToString().Split('–')[0].Trim();
            string teacherID = cmbTeacher.SelectedItem.ToString().Split('–')[0].Trim();

            Schedule = new Schedule
            {
                ScheduleID = txtID.Text.Trim(),
                CourseID   = courseID,
                TeacherID  = teacherID,
                Day        = cmbDay.SelectedItem.ToString(),
                TimeStart  = txtTimeStart.Text.Trim(),
                TimeEnd    = txtTimeEnd.Text.Trim(),
                Room       = txtRoom.Text.Trim(),
                Section    = txtSection.Text.Trim(),
                Semester   = cmbSemester.SelectedItem?.ToString() ?? ""
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
